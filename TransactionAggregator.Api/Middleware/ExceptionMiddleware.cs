using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Net;
using TransactionAggregator.Application.Exceptions;
using TransactionAggregator.Application.Telemetry;

namespace TransactionAggregator.Api.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionMiddleware> _logger;
		
		public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}
		
		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			int statusCode = (int)HttpStatusCode.InternalServerError;
			var problem = new ProblemDetails
			{
				Title = "An unexpected error occurred.",
				Detail = exception.Message,
				Instance = context.Request.Path
			};

			switch (exception)
			{
				case AggregatorValidationException vex:
					statusCode = StatusCodes.Status400BadRequest;
					problem.Title = "Validation error";
					problem.Detail = vex.Message;
					if (vex.Errors?.Count > 0)
					{
						problem.Extensions["errors"] = vex.Errors;
					}
					break;
				case AggregatorServiceException _:
					statusCode = StatusCodes.Status503ServiceUnavailable;
					problem.Title = "External service error";
					break;
				case AggregatorException _:
					statusCode = StatusCodes.Status400BadRequest;
					break;
				default:
					statusCode = StatusCodes.Status500InternalServerError;
					break;
			}

			_logger.LogError(exception, "Handling exception, returning status {StatusCode}", statusCode);

			var activity = Activity.Current ?? ApplicationTelemetry.ActivitySource.StartActivity("UncaughtActivityException", ActivityKind.Internal);

			if (activity != null)
			{
				activity.SetTag("otel.status_code", statusCode);
				activity.SetTag("error", true);
				activity.AddException(exception);
			}

			context.Response.ContentType = "application/problem+json";
			context.Response.StatusCode = statusCode;

			await context.Response.WriteAsJsonAsync(problem);
		}
	}
}
