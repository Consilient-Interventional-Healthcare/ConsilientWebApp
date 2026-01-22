using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Nuke.Common.IO;
using Serilog;

/// <summary>
/// Organizes NSwag-generated TypeScript interfaces into module namespaces based on OpenAPI tags.
/// Port of Organize-ApiTypes.ps1 to C#.
/// </summary>
public static class TypeScriptNamespaceOrganizer
{
    private const string DefaultNamespace = "Common";

    /// <summary>
    /// Organizes TypeScript interfaces into namespaces based on OpenAPI spec tags.
    /// </summary>
    /// <param name="typeScriptFile">Path to the generated TypeScript file</param>
    /// <param name="openApiFile">Path to the OpenAPI JSON file</param>
    public static void Organize(AbsolutePath typeScriptFile, AbsolutePath openApiFile)
    {
        if (!File.Exists(typeScriptFile))
        {
            throw new FileNotFoundException($"TypeScript file not found: {typeScriptFile}");
        }

        Log.Information("Organizing TypeScript types into module namespaces...");

        // Read the TypeScript content
        var content = File.ReadAllText(typeScriptFile);

        // Get controller tags from OpenAPI spec
        var controllerTags = GetControllerTagsFromOpenApi(openApiFile);

        // Extract all types from TypeScript
        var allTypes = ExtractTypeNames(content);
        Log.Debug("Found {Count} types to organize", allTypes.Count);

        // Build type-to-namespace mapping
        var mapping = BuildNamespaceMapping(controllerTags, allTypes);

        // Parse and organize the TypeScript content
        var result = OrganizeIntoNamespaces(content, mapping);

        // Write back
        File.WriteAllText(typeScriptFile, result.Content);

        Log.Information("Types organized into {Count} namespaces", result.Namespaces.Count);
        foreach (var ns in result.Namespaces.OrderBy(x => x.Key))
        {
            Log.Debug("  {Namespace}: {Count} types", ns.Key, ns.Value.Count);
        }
    }

    /// <summary>
    /// Parses the OpenAPI spec to extract tags and their associated schema types.
    /// </summary>
    private static Dictionary<string, HashSet<string>> GetControllerTagsFromOpenApi(AbsolutePath openApiFile)
    {
        var tags = new Dictionary<string, HashSet<string>>();

        if (!File.Exists(openApiFile))
        {
            Log.Warning("OpenAPI spec file not found: {File}. Using fallback pattern matching.", openApiFile);
            return tags;
        }

        try
        {
            var json = JObject.Parse(File.ReadAllText(openApiFile));

            // Initialize tags from the tags array
            var tagsArray = json["tags"] as JArray;
            if (tagsArray != null)
            {
                foreach (var tag in tagsArray)
                {
                    var tagName = tag["name"]?.ToString();
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        tags[tagName] = new HashSet<string>();
                    }
                }
            }

            // Extract schema associations from paths
            var paths = json["paths"] as JObject;
            if (paths != null)
            {
                foreach (var path in paths.Properties())
                {
                    var pathObj = path.Value as JObject;
                    if (pathObj == null) continue;

                    foreach (var method in pathObj.Properties())
                    {
                        var operation = method.Value as JObject;
                        if (operation == null) continue;

                        var operationTags = operation["tags"] as JArray;
                        if (operationTags == null || operationTags.Count == 0) continue;

                        var tag = operationTags[0]?.ToString();
                        if (string.IsNullOrEmpty(tag)) continue;

                        if (!tags.ContainsKey(tag))
                        {
                            tags[tag] = new HashSet<string>();
                        }

                        // Extract request body schemas
                        ExtractSchemaRefs(operation["requestBody"]?["content"], tags[tag]);

                        // Extract response schemas
                        var responses = operation["responses"] as JObject;
                        if (responses != null)
                        {
                            foreach (var response in responses.Properties())
                            {
                                ExtractSchemaRefs(response.Value?["content"], tags[tag]);
                            }
                        }

                        // Extract parameter schemas
                        var parameters = operation["parameters"] as JArray;
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                var schemaRef = param["schema"]?["$ref"]?.ToString();
                                if (!string.IsNullOrEmpty(schemaRef))
                                {
                                    tags[tag].Add(ExtractSchemaName(schemaRef));
                                }
                            }
                        }
                    }
                }
            }

            Log.Debug("Extracted {Count} tags from OpenAPI spec", tags.Count);
            return tags;
        }
        catch (Exception ex)
        {
            Log.Warning("Failed to parse OpenAPI spec: {Message}. Using fallback.", ex.Message);
            return tags;
        }
    }

    private static void ExtractSchemaRefs(JToken? contentToken, HashSet<string> typeSet)
    {
        var content = contentToken as JObject;
        if (content == null) return;

        foreach (var mediaType in content.Properties())
        {
            var schema = mediaType.Value?["schema"];
            if (schema == null) continue;

            var refValue = schema["$ref"]?.ToString();
            if (!string.IsNullOrEmpty(refValue))
            {
                typeSet.Add(ExtractSchemaName(refValue));
            }

            // Handle array types
            if (schema["type"]?.ToString() == "array")
            {
                var itemsRef = schema["items"]?["$ref"]?.ToString();
                if (!string.IsNullOrEmpty(itemsRef))
                {
                    typeSet.Add(ExtractSchemaName(itemsRef));
                }
            }
        }
    }

    private static string ExtractSchemaName(string refPath)
    {
        // "#/components/schemas/TypeName" -> "TypeName"
        return refPath.Replace("#/components/schemas/", "");
    }

    private static List<string> ExtractTypeNames(string content)
    {
        var pattern = new Regex(@"export\s+(interface|enum)\s+(\w+)");
        return pattern.Matches(content)
            .Select(m => m.Groups[2].Value)
            .ToList();
    }

    private static Dictionary<string, string> BuildNamespaceMapping(
        Dictionary<string, HashSet<string>> controllerTags,
        List<string> allTypes)
    {
        var mapping = new Dictionary<string, string>();

        // First priority: Map based on OpenAPI spec
        foreach (var tag in controllerTags)
        {
            foreach (var typeName in tag.Value)
            {
                mapping[typeName] = tag.Key;
            }
        }

        // Second/Third priority: Infer from naming conventions
        foreach (var typeName in allTypes)
        {
            if (!mapping.ContainsKey(typeName))
            {
                mapping[typeName] = InferNamespaceFromTypeName(typeName);
            }
        }

        return mapping;
    }

    private static string InferNamespaceFromTypeName(string typeName)
    {
        // GraphQL types
        if (typeName.StartsWith("GraphQl") || typeName.StartsWith("Query"))
        {
            return "GraphQl";
        }

        // Common utility types
        if (typeName == "FileParameter" || typeName == "PaginationInfo" ||
            typeName == "ApiError" || typeName == "ValidationError")
        {
            return "Common";
        }

        // CRUD naming convention: Create/Update/Delete/Get/List + EntityName + Request/Response/Dto
        var crudMatch = Regex.Match(typeName, @"^(Create|Update|Delete|Get|List)(\w+?)(Request|Response|Dto)?$");
        if (crudMatch.Success)
        {
            var entityName = crudMatch.Groups[2].Value;
            return Pluralize(entityName);
        }

        return DefaultNamespace;
    }

    private static string Pluralize(string word)
    {
        if (word.EndsWith("y") && word.Length > 1 && !IsVowel(word[word.Length - 2]))
        {
            return word.Substring(0, word.Length - 1) + "ies";
        }
        if (word.EndsWith("s"))
        {
            return word;
        }
        return word + "s";
    }

    private static bool IsVowel(char c) => "aeiouAEIOU".Contains(c);

    /// <summary>
    /// Extracts a short type name from types with namespace prefixes.
    /// E.g., "Consilient_Api_Controllers_AuthenticateUserApiResponse" -> "AuthenticateUserApiResponse"
    /// Returns null if no transformation is needed.
    /// </summary>
    private static string? GetShortTypeName(string typeName)
    {
        // Match patterns like Consilient_Namespace_ClassName
        var match = Regex.Match(typeName, @"^(?:Consilient_)?(?:\w+_)+(\w+(?:Request|Response|Result|Dto))$");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return null;
    }

    private static OrganizeResult OrganizeIntoNamespaces(string content, Dictionary<string, string> mapping)
    {
        // Remove any existing namespace wrappers (from previous runs)
        content = Regex.Replace(content, @"namespace\s+[\w\.]+\s*\{\s*", "");
        content = Regex.Replace(content, @"\s*\}\s*$", "");

        // Parse types from content
        var types = ParseTypes(content);

        // Expand mapping for referenced types (so dependencies stay together)
        var allTypeNames = types.Select(t => t.Name).ToHashSet();
        foreach (var type in types)
        {
            var references = Regex.Matches(type.Code, @"\b([A-Z][A-Za-z0-9_]+)\b")
                .Select(m => m.Groups[1].Value)
                .Distinct();

            foreach (var refName in references)
            {
                if (allTypeNames.Contains(refName))
                {
                    if (!mapping.ContainsKey(refName) || mapping[refName] == DefaultNamespace)
                    {
                        if (mapping.TryGetValue(type.Name, out var typeNamespace))
                        {
                            mapping[refName] = typeNamespace;
                        }
                        else
                        {
                            mapping[refName] = DefaultNamespace;
                        }
                    }
                }
            }
        }

        // Group by namespace
        var namespaceGroups = new Dictionary<string, List<TypeDefinition>>();
        foreach (var type in types)
        {
            var ns = mapping.TryGetValue(type.Name, out var value) ? value : DefaultNamespace;
            if (!namespaceGroups.ContainsKey(ns))
            {
                namespaceGroups[ns] = new List<TypeDefinition>();
            }
            namespaceGroups[ns].Add(type);
        }

        // Build output
        var sb = new StringBuilder();
        sb.AppendLine("//----------------------");
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("//     Generated using NSwag toolchain (http://NSwag.org)");
        sb.AppendLine("//     Post-processed to organize into module namespaces based on OpenAPI tags");
        sb.AppendLine("//");
        sb.AppendLine("//     This file was automatically generated. Do not modify manually.");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine("//----------------------");
        sb.AppendLine();
        sb.AppendLine("/* tslint:disable */");
        sb.AppendLine("/* eslint-disable */");
        sb.AppendLine("// ReSharper disable InconsistentNaming");

        foreach (var ns in namespaceGroups.OrderBy(x => x.Key))
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"export namespace {ns.Key} {{");

            // Track types that need aliases (types with namespace prefixes like Consilient_Api_Controllers_XXX)
            var typeAliases = new List<(string ShortName, string FullName)>();

            foreach (var type in ns.Value)
            {
                sb.AppendLine();
                // Indent each line
                foreach (var line in type.Code.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        sb.AppendLine("  " + line.TrimEnd());
                    }
                }

                // Check if this type has a namespace prefix that should get an alias
                var shortName = GetShortTypeName(type.Name);
                if (shortName != null && shortName != type.Name)
                {
                    typeAliases.Add((shortName, type.Name));
                }
            }

            // Add type aliases at the end of the namespace
            if (typeAliases.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("  // Type aliases for shorter names");
                foreach (var (shortName, fullName) in typeAliases)
                {
                    sb.AppendLine($"  export type {shortName} = {fullName};");
                }
            }

            sb.AppendLine();
            sb.AppendLine("}");
        }

        return new OrganizeResult
        {
            Content = sb.ToString(),
            Namespaces = namespaceGroups.ToDictionary(x => x.Key, x => x.Value.Select(t => t.Name).ToList())
        };
    }

    private static List<TypeDefinition> ParseTypes(string content)
    {
        var types = new List<TypeDefinition>();
        var lines = content.Split('\n');
        var i = 0;

        while (i < lines.Length)
        {
            var line = lines[i];
            var match = Regex.Match(line, @"^\s*export\s+(interface|enum)\s+(\w+)(?:\s+extends\s+\w+)?\s*\{");

            if (match.Success)
            {
                var typeKind = match.Groups[1].Value;
                var typeName = match.Groups[2].Value;
                var codeBuilder = new StringBuilder();
                codeBuilder.AppendLine(line);
                var braceCount = 1;
                i++;

                while (i < lines.Length && braceCount > 0)
                {
                    var currentLine = lines[i];
                    codeBuilder.AppendLine(currentLine);
                    braceCount += currentLine.Count(c => c == '{');
                    braceCount -= currentLine.Count(c => c == '}');
                    i++;
                }

                types.Add(new TypeDefinition
                {
                    Kind = typeKind,
                    Name = typeName,
                    Code = codeBuilder.ToString().TrimEnd()
                });
            }
            else
            {
                i++;
            }
        }

        return types;
    }

    private class TypeDefinition
    {
        public required string Kind { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
    }

    private class OrganizeResult
    {
        public required string Content { get; set; }
        public required Dictionary<string, List<string>> Namespaces { get; set; }
    }
}
