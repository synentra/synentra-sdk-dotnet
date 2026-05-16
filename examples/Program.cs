using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vectra.Client.Examples;
using Vectra.Client.Extensions;

// ╔═════════════════════════════════════════════════════════════════╗
// ║            Vectra SDK for .NET — Interactive Examples           ║
// ║                                                                 ║
// ║  Configure the gateway URL and optional token below, then       ║
// ║  pick any example from the menu to run it against a live        ║
// ║  Vectra instance.                                               ║
// ║                                                                 ║
// ║  Quick-start:                                                   ║
// ║    docker run -p 7080:7080 ghcr.io/cortexiumlabs/vectra:latest  ║
// ╚═════════════════════════════════════════════════════════════════╝

const string GatewayUrl   = "http://localhost:7080";
const string BearerToken  = "";           // leave empty to auth via example 02
const string AgentSecret  = "changeme";   // used by auth + lifecycle examples

// ── Host / DI setup ────────────────────────────────────────────────────────────
var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o =>
        {
            o.SingleLine  = true;
            o.TimestampFormat = "HH:mm:ss ";
        });
        logging.SetMinimumLevel(LogLevel.Warning); // keep examples output clean
    })
    .ConfigureServices(services =>
    {
        services.AddVectraClient(options =>
        {
            options.BaseUrl     = GatewayUrl;
            options.BearerToken = string.IsNullOrWhiteSpace(BearerToken) ? null : BearerToken;
        });

        // Register examples so they can inject IVectraClient + ILogger
        services.AddTransient<QuickStartExample>();
        services.AddTransient<TokenAuthenticationExample>();
        services.AddTransient<AgentManagementExample>();
        services.AddTransient<PolicyInspectionExample>();
        services.AddTransient<HitlWorkflowExample>();
        services.AddTransient<ErrorHandlingExample>();
        services.AddTransient<BackgroundHitlMonitorExample>();
        services.AddTransient<AgentLifecycleExample>();
        services.AddTransient<AdvancedConfigurationExample>();
        services.AddTransient<PaginationAndBulkExample>();
    })
    .Build();

// ── Menu ────────────────────────────────────────────────────────────────────────
var examples = new (string Label, Func<Task> Run)[]
{
    ("Quick Start",                    () => host.Services.GetRequiredService<QuickStartExample>().RunAsync()),
    ("Token Authentication",           () => host.Services.GetRequiredService<TokenAuthenticationExample>().RunAsync(AgentSecret)),
    ("Agent Management",               () => host.Services.GetRequiredService<AgentManagementExample>().RunAsync()),
    ("Policy Inspection",              () => host.Services.GetRequiredService<PolicyInspectionExample>().RunAsync()),
    ("HITL Review Workflow",           () => host.Services.GetRequiredService<HitlWorkflowExample>().RunAsync()),
    ("Error Handling Patterns",        () => host.Services.GetRequiredService<ErrorHandlingExample>().RunAsync()),
    ("Background HITL Monitor",        () => host.Services.GetRequiredService<BackgroundHitlMonitorExample>().RunAsync()),
    ("Full Agent Lifecycle",           () => host.Services.GetRequiredService<AgentLifecycleExample>().RunAsync(AgentSecret)),
    ("Advanced Configuration",         () => host.Services.GetRequiredService<AdvancedConfigurationExample>().RunAsync()),
    ("Pagination & Bulk Operations",   () => host.Services.GetRequiredService<PaginationAndBulkExample>().RunAsync()),
};

while (true)
{
    Console.WriteLine();
    Banner("Vectra SDK for .NET — Examples");
    Console.WriteLine($"  Gateway : {GatewayUrl}");
    Console.WriteLine();

    for (var i = 0; i < examples.Length; i++)
        Console.WriteLine($"  {i + 1,2}. {examples[i].Label}");

    Console.WriteLine();
    Console.WriteLine("   0. Exit");
    Console.WriteLine();
    Console.Write("  Select an example: ");

    var input = Console.ReadLine()?.Trim();

    if (input is "0" or "q" or "quit" or "exit")
        break;

    if (!int.TryParse(input, out var choice) || choice < 1 || choice > examples.Length)
    {
        WriteError("Invalid choice. Enter a number between 1 and " + examples.Length + ".");
        continue;
    }

    Console.WriteLine();
    Banner(examples[choice - 1].Label);

    try
    {
        await examples[choice - 1].Run();
    }
    catch (Exception ex)
    {
        WriteError($"Unhandled exception: {ex.GetType().Name}: {ex.Message}");
    }

    Console.WriteLine();
    Console.Write("  Press any key to return to the menu...");
    Console.ReadKey(intercept: true);
}

Console.WriteLine("\n  Goodbye!\n");

// ── Helpers ─────────────────────────────────────────────────────────────────────
static void Banner(string title)
{
    var line = new string('─', Math.Min(title.Length + 4, 60));
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"  ┌{line}┐");
    Console.WriteLine($"  │  {title.PadRight(line.Length - 2)}│");
    Console.WriteLine($"  └{line}┘");
    Console.ResetColor();
}

static void WriteError(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n  ✗ {msg}");
    Console.ResetColor();
}
