using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;
using System.Diagnostics;

/// <summary>
/// Terraform infrastructure management targets.
/// Handles: Plan, Apply, and state management for Azure infrastructure.
/// </summary>
/// <remarks>
/// <para><b>State Source Options:</b></para>
/// <list type="bullet">
///   <item><c>Fresh</c> (default): No state - shows what would be created from scratch</item>
///   <item><c>Local</c>: Uses local terraform.tfstate file</item>
///   <item><c>Remote</c>: Connects to Azure blob storage backend</item>
/// </list>
///
/// <para><b>Windows Usage:</b></para>
/// <code>
/// .\build.ps1 TerraformPlan
/// .\build.ps1 TerraformPlan --terraform-environment prod
/// .\build.ps1 TerraformPlan --terraform-state-source Local
/// .\build.ps1 TerraformPlan --terraform-environment prod --terraform-state-source Remote
/// </code>
/// </remarks>
partial class Build
{
    // Terraform paths
    static AbsolutePath TerraformDirectory => RootDirectory / "infra" / "terraform";

    // Parameters
    [Parameter("Target environment (dev, prod)")]
    readonly string TerraformEnvironment = "dev";

    [Parameter("State source: Fresh (default), Local, Remote")]
    readonly string TerraformStateSource = "Fresh";

    // ============================================
    // RESOLVED PROPERTIES
    // ============================================

    string ResolvedArmClientId => EnvLocal.GetValueOrDefault("ARM_CLIENT_ID", "");
    string ResolvedArmClientSecret => EnvLocal.GetValueOrDefault("ARM_CLIENT_SECRET", "");
    string ResolvedArmTenantId => EnvLocal.GetValueOrDefault("ARM_TENANT_ID", "");
    string ResolvedAzureSubscriptionId => EnvLocal.GetValueOrDefault("AZURE_SUBSCRIPTION_ID", "");
    string ResolvedAzureRegion => EnvLocal.GetValueOrDefault("AZURE_REGION", DefaultAzureRegion);
    string ResolvedAzureResourceGroupName => EnvLocal.GetValueOrDefault("AZURE_RESOURCE_GROUP_NAME", "");
    string ResolvedSqlAdminUsername => EnvLocal.GetValueOrDefault("SQL_ADMIN_USERNAME", "sa");
    string ResolvedSqlAdminPassword => EnvLocal.GetValueOrDefault("SQL_ADMIN_PASSWORD", "");
    string ResolvedJwtSigningSecret => EnvLocal.GetValueOrDefault("JWT_SIGNING_SECRET", "");
    string ResolvedOAuthClientSecret => EnvLocal.GetValueOrDefault("OAUTH_CLIENT_SECRET", "");
    string ResolvedCaeNameTemplate => EnvLocal.GetValueOrDefault("CAE_NAME_TEMPLATE", "consilient-cae-{environment}");

    // Remote state configuration
    string ResolvedTerraformStateRg => EnvLocal.GetValueOrDefault("TERRAFORM_STATE_RG", DefaultTerraformStateRg);
    string ResolvedTerraformStateSa => EnvLocal.GetValueOrDefault("TERRAFORM_STATE_SA", DefaultTerraformStateSa);
    string ResolvedTerraformStateContainer => EnvLocal.GetValueOrDefault("TERRAFORM_STATE_CONTAINER", DefaultTerraformStateContainer);

    // ============================================
    // TERRAFORM TARGETS
    // ============================================

    Target TerraformPlan => _ => _
        .Description("Generate Terraform execution plan")
        .Executes(() =>
        {
            Log.Information("=== TERRAFORM PLAN ===");
            Log.Information("Environment: {Environment}", TerraformEnvironment);
            Log.Information("State Source: {StateSource}", TerraformStateSource);
            Log.Information("");

            // Validate Terraform is installed
            ValidateTerraformInstalled();

            // Validate required environment variables
            ValidateTerraformEnvironmentVariables();

            // Set up environment variables for Terraform
            SetTerraformEnvironmentVariables();

            // Initialize Terraform based on state source
            InitializeTerraform();

            // Run terraform plan and capture output
            var outputFile = RunTerraformPlan();

            Log.Information("");
            Log.Information("=== TERRAFORM PLAN COMPLETED ===");
            Log.Information("Plan output saved to: {OutputFile}", outputFile);
        });

    // ============================================
    // HELPER METHODS
    // ============================================

    void ValidateTerraformInstalled()
    {
        Log.Information("Checking Terraform installation...");

        var process = ProcessTasks.StartProcess(
            "terraform",
            "version",
            workingDirectory: RootDirectory);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception("Terraform is not installed or not in PATH. Please install Terraform.");
        }

        var version = string.Join("", process.Output.Take(1).Select(o => o.Text)).Trim();
        Log.Information("  {Version}", version);
    }

    void ValidateTerraformEnvironmentVariables()
    {
        Log.Information("Validating required environment variables...");

        var requiredVars = new[]
        {
            ("ARM_CLIENT_ID", ResolvedArmClientId),
            ("ARM_CLIENT_SECRET", ResolvedArmClientSecret),
            ("ARM_TENANT_ID", ResolvedArmTenantId),
            ("AZURE_SUBSCRIPTION_ID", ResolvedAzureSubscriptionId),
            ("SQL_ADMIN_USERNAME", ResolvedSqlAdminUsername),
            ("SQL_ADMIN_PASSWORD", ResolvedSqlAdminPassword),
            ("JWT_SIGNING_SECRET", ResolvedJwtSigningSecret),
            ("AZURE_REGION", ResolvedAzureRegion),
            ("AZURE_RESOURCE_GROUP_NAME", ResolvedAzureResourceGroupName)
        };

        var missingVars = requiredVars
            .Where(v => string.IsNullOrWhiteSpace(v.Item2))
            .Select(v => v.Item1)
            .ToList();

        if (missingVars.Count > 0)
        {
            Log.Error("Missing required environment variables:");
            foreach (var v in missingVars)
            {
                Log.Error("  - {Variable}", v);
            }
            throw new Exception($"Missing {missingVars.Count} required environment variable(s). Configure them in .nuke/.env.local");
        }

        Log.Information("  All required variables present");
    }

    void SetTerraformEnvironmentVariables()
    {
        Log.Information("Setting Terraform environment variables...");

        // ARM provider authentication
        System.Environment.SetEnvironmentVariable("ARM_CLIENT_ID", ResolvedArmClientId);
        System.Environment.SetEnvironmentVariable("ARM_CLIENT_SECRET", ResolvedArmClientSecret);
        System.Environment.SetEnvironmentVariable("ARM_TENANT_ID", ResolvedArmTenantId);

        // Terraform variables (TF_VAR_*)
        System.Environment.SetEnvironmentVariable("TF_VAR_environment", TerraformEnvironment);
        System.Environment.SetEnvironmentVariable("TF_VAR_subscription_id", ResolvedAzureSubscriptionId);
        System.Environment.SetEnvironmentVariable("TF_VAR_sql_admin_username", ResolvedSqlAdminUsername);
        System.Environment.SetEnvironmentVariable("TF_VAR_sql_admin_password", ResolvedSqlAdminPassword);
        System.Environment.SetEnvironmentVariable("TF_VAR_jwt_signing_secret", ResolvedJwtSigningSecret);
        System.Environment.SetEnvironmentVariable("TF_VAR_oauth_client_secret", ResolvedOAuthClientSecret);
        System.Environment.SetEnvironmentVariable("TF_VAR_enable_local_firewall", "false");
        System.Environment.SetEnvironmentVariable("TF_VAR_region", ResolvedAzureRegion);
        System.Environment.SetEnvironmentVariable("TF_VAR_resource_group_name", ResolvedAzureResourceGroupName);
        System.Environment.SetEnvironmentVariable("TF_VAR_container_app_environment_name_template", ResolvedCaeNameTemplate);

        Log.Information("  Environment variables set");
    }

    void InitializeTerraform()
    {
        Log.Information("Initializing Terraform ({StateSource} state)...", TerraformStateSource);

        string initArgs;
        switch (TerraformStateSource.ToLower())
        {
            case "fresh":
                // No backend - shows what would be created from scratch
                initArgs = "init -backend=false -reconfigure";
                Log.Information("  Using fresh state (no backend)");
                break;

            case "local":
                // Use local terraform.tfstate file
                initArgs = "init -reconfigure";
                Log.Information("  Using local state file");
                break;

            case "remote":
                // Connect to Azure blob storage backend
                var stateKey = $"{TerraformEnvironment}.terraform.tfstate";
                initArgs = $"init -reconfigure " +
                    $"-backend-config=\"resource_group_name={ResolvedTerraformStateRg}\" " +
                    $"-backend-config=\"storage_account_name={ResolvedTerraformStateSa}\" " +
                    $"-backend-config=\"container_name={ResolvedTerraformStateContainer}\" " +
                    $"-backend-config=\"key={stateKey}\" " +
                    $"-backend-config=\"use_oidc=false\" " +
                    $"-backend-config=\"use_azuread_auth=false\"";
                Log.Information("  Using remote state: {Rg}/{Sa}/{Container}/{Key}",
                    ResolvedTerraformStateRg, ResolvedTerraformStateSa, ResolvedTerraformStateContainer, stateKey);
                break;

            default:
                throw new Exception($"Invalid state source: {TerraformStateSource}. Use Fresh, Local, or Remote.");
        }

        RunTerraformCommand(initArgs);
        Log.Information("  Terraform initialized");
    }

    string RunTerraformPlan()
    {
        Log.Information("Running terraform plan...");
        Log.Information("");

        // Generate output file path
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var outputFileName = $"terraform-plan-{TerraformEnvironment}-{timestamp}.txt";
        var outputFilePath = Path.Combine(Path.GetTempPath(), outputFileName);

        // Run terraform plan and capture output
        var startInfo = new ProcessStartInfo
        {
            FileName = "terraform",
            Arguments = "plan -no-color",
            WorkingDirectory = TerraformDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        var output = new List<string>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                output.Add(e.Data);
                Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                output.Add(e.Data);
                Console.WriteLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        // Write output to file
        var header = $@"Terraform Plan Output
=====================
Environment: {TerraformEnvironment}
State Source: {TerraformStateSource}
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
=====================

";
        File.WriteAllText(outputFilePath, header + string.Join(System.Environment.NewLine, output));

        if (process.ExitCode != 0)
        {
            Log.Error("Terraform plan failed with exit code {ExitCode}", process.ExitCode);
            Log.Error("Output saved to: {OutputFile}", outputFilePath);
            throw new Exception($"Terraform plan failed with exit code {process.ExitCode}");
        }

        return outputFilePath;
    }

    void RunTerraformCommand(string arguments)
    {
        var process = ProcessTasks.StartProcess(
            "terraform",
            arguments,
            workingDirectory: TerraformDirectory);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var output = string.Join("\n", process.Output.Select(o => o.Text));
            throw new Exception($"Terraform command failed: terraform {arguments}\nOutput: {output}");
        }
    }
}
