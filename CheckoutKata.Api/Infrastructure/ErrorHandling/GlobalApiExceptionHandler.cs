using System.Text.Json;
using CheckoutKata.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CheckoutKata.Api.Infrastructure.ErrorHandling;

public sealed class GlobalApiExceptionHandler : IExceptionHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            CartCapacityExceededException => (StatusCodes.Status429TooManyRequests, "Cart Capacity Reached"),
            CartNotFoundException => (StatusCodes.Status404NotFound, "Cart Not Found"),
            UnknownPricingVersionException => (StatusCodes.Status404NotFound, "Pricing Version Not Found"),
            PricingVersionMismatchException => (StatusCodes.Status409Conflict, "Pricing Version Mismatch"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        if (exception is PricingVersionMismatchException mismatch)
        {
            problemDetails.Extensions["cartId"] = mismatch.CartId;
            problemDetails.Extensions["expectedPricingVersionId"] = mismatch.ExpectedPricingVersionId.Value;
            problemDetails.Extensions["requestedPricingVersionId"] = mismatch.RequestedPricingVersionId.Value;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(
            httpContext.Response.Body,
            problemDetails,
            SerializerOptions,
            cancellationToken);

        return true;
    }
}
