using CheckoutKata.Api.Composition;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCheckoutKataApiServices(builder.Configuration, TimeProvider.System);

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();
app.Run();

public partial class Program;
