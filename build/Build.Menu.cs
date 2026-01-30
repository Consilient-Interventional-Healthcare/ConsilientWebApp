using Nuke.Common;
using Spectre.Console;

/// <summary>
/// Interactive menu system for developer operations.
///
/// <para><b>Usage:</b></para>
/// <code>
/// build.cmd                    # Launch interactive menu
/// build.cmd GenerateAllTypes   # Run specific target (non-interactive)
/// </code>
///
/// <para><b>Menu Structure:</b></para>
/// <list type="bullet">
///   <item>Generate Code: OpenAPI, GraphQL, TypeScript, All</item>
///   <item>Database Actions: Add Migration, Apply Migrations, Squash, Rebuild, Docs</item>
///   <item>Docker: Nuclear Reset (complete cleanup)</item>
/// </list>
///
/// <para><b>Adding New Menu Items:</b></para>
/// <list type="number">
///   <item>Add a new MenuItem to the appropriate menu configuration</item>
///   <item>For simple targets: use RunTargetAction(TargetName)</item>
///   <item>For interactive workflows: create a new method and reference it</item>
/// </list>
///
/// <para><b>Non-Interactive Examples (for AI assistants and scripts):</b></para>
/// <code>
/// # Code Generation
/// build.cmd GenerateAllTypes
/// build.cmd GenerateAllTypes --force
/// build.cmd GenerateOpenApiDoc
/// build.cmd GenerateGraphQL
///
/// # Database - Add Migration
/// build.cmd AddMigration --migration-name AddPatientNotes --db-context ConsilientDbContext
///
/// # Database - Generate SQL Script
/// build.cmd GenerateMigrationScript --db-context ConsilientDbContext
///
/// # Database - Apply Migrations
/// build.cmd UpdateLocalDatabase --db-context Both
///
/// # Database - Squash (destructive, requires --force)
/// build.cmd SquashMigrations --db-context ConsilientDbContext --force
///
/// # Database - Rebuild (destructive, requires --force)
/// build.cmd RebuildDatabase --force
/// build.cmd RebuildDatabase --force --backup
///
/// # Database - Documentation
/// build.cmd GenerateDatabaseDocs
/// build.cmd GenerateDatabaseDocs --db-auto-start
/// </code>
/// </summary>
partial class Build
{
    // ============================================
    // MENU CONFIGURATION
    // ============================================
    // To add/remove menu items, modify these collections.
    // Each MenuItem can have either a SubMenu (for nested menus) or an Action.

    static readonly string[] AvailableDbContexts = ["ConsilientDbContext", "UsersDbContext"];
    static readonly string[] AvailableDbContextsWithBoth = ["ConsilientDbContext", "UsersDbContext", "Both"];

    // ============================================
    // INTERACTIVE MENU TARGET
    // ============================================

    Target InteractiveMenu => _ => _
        .Description("Launch interactive developer menu")
        .Executes(() =>
        {
            RunMainMenu();
        });

    void RunMainMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            WriteHeader();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select an option:[/]")
                    .HighlightStyle(Style.Parse("cyan"))
                    .AddChoices(
                        "Generate Code",
                        "Database Actions",
                        "Docker",
                        "Terraform",
                        "Exit"));

            switch (choice)
            {
                case "Generate Code":
                    RunCodeGenMenu();
                    break;
                case "Database Actions":
                    RunDatabaseMenu();
                    break;
                case "Docker":
                    RunDockerMenu();
                    break;
                case "Terraform":
                    RunTerraformMenu();
                    break;
                case "Exit":
                    AnsiConsole.MarkupLine("[grey]Goodbye![/]");
                    return;
            }
        }
    }

    void RunCodeGenMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            WriteHeader("Code Generation");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select code generation target:[/]")
                    .HighlightStyle(Style.Parse("cyan"))
                    .AddChoices(
                        "All Types (Recommended)",
                        "OpenAPI Only",
                        "GraphQL Only",
                        "TypeScript Types Only",
                        "[grey]<- Back[/]"));

            switch (choice)
            {
                case "All Types (Recommended)":
                    ExecuteTargetWithOutput("GenerateAllTypes");
                    break;
                case "OpenAPI Only":
                    ExecuteTargetWithOutput("GenerateOpenApiDoc");
                    break;
                case "GraphQL Only":
                    ExecuteTargetWithOutput("GenerateGraphQL");
                    break;
                case "TypeScript Types Only":
                    ExecuteTargetWithOutput("GenerateApiTypes");
                    break;
                default:
                    return;
            }
        }
    }

    void RunDatabaseMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            WriteHeader("Database Actions");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select database action:[/]")
                    .HighlightStyle(Style.Parse("cyan"))
                    .AddChoices(
                        "Add Migration",
                        "Apply Pending Migrations",
                        "Squash Migrations",
                        "Recreate Local Database",
                        "Recreate Database Documentation",
                        "[grey]<- Back[/]"));

            switch (choice)
            {
                case "Add Migration":
                    RunAddMigrationFlow();
                    break;
                case "Apply Pending Migrations":
                    RunApplyMigrationsFlow();
                    break;
                case "Squash Migrations":
                    RunSquashMigrationsFlow();
                    break;
                case "Recreate Local Database":
                    RunRebuildDatabaseFlow();
                    break;
                case "Recreate Database Documentation":
                    ExecuteTargetWithOutput("GenerateDatabaseDocs", "--db-auto-start");
                    break;
                default:
                    return;
            }
        }
    }

    // ============================================
    // INTERACTIVE WORKFLOWS
    // ============================================

    void RunAddMigrationFlow()
    {
        AnsiConsole.Clear();
        WriteHeader("Add Migration");

        // Step 1: Get migration name
        var migrationName = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Enter migration name:[/]")
                .Validate(name =>
                {
                    if (string.IsNullOrWhiteSpace(name))
                        return ValidationResult.Error("[red]Migration name cannot be empty[/]");
                    if (name.Contains(' '))
                        return ValidationResult.Error("[red]Migration name cannot contain spaces[/]");
                    return ValidationResult.Success();
                }));

        // Step 2: Select DbContext
        var dbContext = PromptForDbContext("Add Migration");

        // Step 3: Run AddMigration
        AnsiConsole.WriteLine();
        var success = ExecuteTargetWithOutput("AddMigration", $"--migration-name {migrationName} --db-context {dbContext}");

        // Step 4: Offer to generate SQL script on success
        if (success)
        {
            AnsiConsole.WriteLine();
            var generateScript = AnsiConsole.Confirm("[green]Generate SQL script for this migration?[/]", defaultValue: true);

            if (generateScript)
            {
                ExecuteTargetWithOutput("GenerateMigrationScript", $"--db-context {dbContext}");
            }
        }

        WaitForKeyPress();
    }

    void RunApplyMigrationsFlow()
    {
        AnsiConsole.Clear();
        WriteHeader("Apply Pending Migrations");

        // Select DbContext (with "Both" option)
        var dbContext = PromptForDbContext("Apply Migrations", includeBoth: true);

        // Run UpdateLocalDatabase
        ExecuteTargetWithOutput("UpdateLocalDatabase", $"--db-context {dbContext}");

        WaitForKeyPress();
    }

    void RunSquashMigrationsFlow()
    {
        AnsiConsole.Clear();
        WriteHeader("Squash Migrations");

        // Step 1: Select DbContext
        var dbContext = PromptForDbContext("Squash Migrations");

        // Step 2: Show warning
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]WARNING: This will DELETE all existing migrations and create a fresh Initial migration.[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]The following will be deleted:[/]");
        AnsiConsole.MarkupLine($"  - All C# migration files for {dbContext}");
        AnsiConsole.MarkupLine($"  - All SQL scripts for {dbContext}");
        AnsiConsole.WriteLine();

        // Step 3: Confirm with explicit YES
        var confirmation = AnsiConsole.Prompt(
            new TextPrompt<string>("[red]Type YES to confirm:[/]"));

        if (confirmation != "YES")
        {
            AnsiConsole.MarkupLine("[grey]Operation cancelled.[/]");
            WaitForKeyPress();
            return;
        }

        // Step 4: Run SquashMigrations
        ExecuteTargetWithOutput("SquashMigrations", $"--db-context {dbContext} --force");

        WaitForKeyPress();
    }

    void RunRebuildDatabaseFlow()
    {
        AnsiConsole.Clear();
        WriteHeader("Recreate Local Database");

        // Step 1: Ask about backup
        var createBackup = AnsiConsole.Confirm("[green]Create backup before rebuilding?[/]", defaultValue: false);

        // Step 2: Show warning
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]WARNING: This will DROP ALL OBJECTS from the local database and rebuild from scripts.[/]");
        AnsiConsole.WriteLine();

        // Step 3: Confirm
        if (!AnsiConsole.Confirm("[red]Are you sure you want to continue?[/]", defaultValue: false))
        {
            AnsiConsole.MarkupLine("[grey]Operation cancelled.[/]");
            WaitForKeyPress();
            return;
        }

        // Step 4: Run RebuildDatabase
        var args = createBackup ? "--force --backup" : "--force";
        ExecuteTargetWithOutput("RebuildDatabase", args);

        WaitForKeyPress();
    }

    void RunDockerMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            WriteHeader("Docker");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select Docker action:[/]")
                    .HighlightStyle(Style.Parse("cyan"))
                    .AddChoices(
                        "Nuclear Docker Reset",
                        "[grey]<- Back[/]"));

            switch (choice)
            {
                case "Nuclear Docker Reset":
                    RunDockerNuclearResetFlow();
                    break;
                default:
                    return;
            }
        }
    }

    void RunDockerNuclearResetFlow()
    {
        AnsiConsole.Clear();
        WriteHeader("Nuclear Docker Reset");

        // Show warning
        AnsiConsole.MarkupLine("[yellow]WARNING: This will perform a complete Docker cleanup:[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  - Stop and remove all Consilient containers");
        AnsiConsole.MarkupLine("  - Remove all Consilient Docker images");
        AnsiConsole.MarkupLine("  - Prune unused volumes and networks");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]This is useful when Docker gets into an inconsistent state.[/]");
        AnsiConsole.WriteLine();

        // Confirm
        if (!AnsiConsole.Confirm("[red]Are you sure you want to continue?[/]", defaultValue: false))
        {
            AnsiConsole.MarkupLine("[grey]Operation cancelled.[/]");
            WaitForKeyPress();
            return;
        }

        // Run DockerNuclearReset
        ExecuteTargetWithOutput("DockerNuclearReset", "--force");

        WaitForKeyPress();
    }

    void RunTerraformMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            WriteHeader("Terraform");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select Terraform action:[/]")
                    .HighlightStyle(Style.Parse("cyan"))
                    .AddChoices(
                        "Plan",
                        "[grey]<- Back[/]"));

            switch (choice)
            {
                case "Plan":
                    RunTerraformPlanFlow();
                    break;
                default:
                    return;
            }
        }
    }

    void RunTerraformPlanFlow()
    {
        AnsiConsole.Clear();
        WriteHeader("Terraform Plan");

        // Step 1: Select environment
        var environment = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Select target environment:[/]")
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices("dev", "prod"));

        // Step 2: Select state source
        var stateSource = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Select state source:[/]")
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices(
                    "Fresh (no state - see what would be created)",
                    "Local (use local terraform.tfstate)",
                    "Remote (use Azure blob storage backend)"));

        // Parse state source selection
        var stateSourceArg = stateSource switch
        {
            var s when s.StartsWith("Fresh") => "Fresh",
            var s when s.StartsWith("Local") => "Local",
            var s when s.StartsWith("Remote") => "Remote",
            _ => "Fresh"
        };

        // Step 3: Run TerraformPlan
        AnsiConsole.WriteLine();
        ExecuteTargetWithOutput("TerraformPlan", $"--terraform-environment {environment} --terraform-state-source {stateSourceArg}");

        WaitForKeyPress();
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    /// <summary>
    /// Prompts user to select a database context.
    /// ConsilientDbContext is always first (default when pressing Enter).
    /// </summary>
    /// <param name="actionDescription">Description shown in the prompt</param>
    /// <param name="includeBoth">Whether to include "Both" option</param>
    /// <returns>Selected context name</returns>
    string PromptForDbContext(string actionDescription, bool includeBoth = false)
    {
        var choices = includeBoth ? AvailableDbContextsWithBoth : AvailableDbContexts;

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[green]Select database context for {actionDescription}:[/]")
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices(choices));
    }

    /// <summary>
    /// Executes a NUKE target and displays output in real-time.
    /// Returns true if successful, false otherwise.
    /// </summary>
    bool ExecuteTargetWithOutput(string targetName, string? additionalArgs = null)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[cyan]Running: {targetName}[/]").LeftJustified());
        AnsiConsole.WriteLine();

        try
        {
            // Build the command arguments
            var args = targetName;
            if (!string.IsNullOrEmpty(additionalArgs))
            {
                args += " " + additionalArgs;
            }

            // Get the path to the build executable
            var buildExePath = Path.Combine(RootDirectory, "build", "bin", "Debug", "_build.exe");

            // Run the process and capture output
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = buildExePath,
                Arguments = args,
                WorkingDirectory = RootDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = startInfo };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(e.Data)}[/]");
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            AnsiConsole.WriteLine();

            if (process.ExitCode == 0)
            {
                AnsiConsole.MarkupLine("[green]Completed successfully.[/]");
                return true;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Failed with exit code {process.ExitCode}[/]");
                return false;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
            return false;
        }
    }

    void WriteHeader(string? subtitle = null)
    {
        AnsiConsole.Write(
            new FigletText("Consilient")
                .Color(Color.Cyan1));

        if (subtitle != null)
        {
            AnsiConsole.MarkupLine($"[grey]> {subtitle}[/]");
        }

        AnsiConsole.WriteLine();
    }

    void WaitForKeyPress()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
    }
}
