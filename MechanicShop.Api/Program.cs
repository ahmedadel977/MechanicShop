using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.MapGet("/test-email", async (
    [FromServices] INotificationService notificationService,
    CancellationToken cancellationToken) =>
{
    await notificationService.SendEmailAsync(
        "YOUR_EMAIL@gmail.com",
        cancellationToken);

    return Results.Ok("Email sending request completed.");
});
app.Run();
