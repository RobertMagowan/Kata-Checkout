using CheckoutKata.Console;

try
{
    var pricingRules = ConsolePricingRuleSource.Load("pricing-rules.json");
    var app = CheckoutConsoleApp.Create(pricingRules);

    app.Run();
}
catch (Exception ex)
{
    System.Console.Error.WriteLine($"Failed to start checkout console: {ex.Message}");
}
