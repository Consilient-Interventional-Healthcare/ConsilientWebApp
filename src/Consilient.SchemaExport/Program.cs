using Consilient.Data;
using Consilient.Data.GraphQL;
using EntityGraphQL.Schema;

var schema = new SchemaProvider<ConsilientDbContext>();
GraphQlSchemaConfigurator.ConfigureSchema(schema);

var sdl = schema.ToGraphQLSchemaString();
var outputPath = args.Length > 0 ? args[0] : "schema.graphql";
File.WriteAllText(outputPath, sdl);

Console.WriteLine($"Schema exported to {outputPath}");
Console.WriteLine($"Schema size: {sdl.Length} characters");
