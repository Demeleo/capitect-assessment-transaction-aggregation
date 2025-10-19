using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionAggregator.Api.Middleware;
using TransactionAggregator.Application.Exceptions;

namespace TransactionAggregator.Tests
{
	[TestFixture]
	public class ExceptionMiddlewareTests
	{
		private static DefaultHttpContext CreateContext()
		{
			var context = new DefaultHttpContext();
			context.Request.Path = "/test";
			context.Response.Body = new MemoryStream();
			return context;
		}

		private static string ReadResponseBody(HttpContext context)
		{
			context.Response.Body.Seek(0, SeekOrigin.Begin);
			using var doc = JsonDocument.Parse(context.Response.Body);
			return doc.RootElement.GetRawText();
		}

		[Test]
		public async Task Middleware_Returns_400_WithValidationErrors_ForValidationException()
		{
			var logger = new Mock<ILogger<ExceptionMiddleware>>().Object;
			RequestDelegate next = _ => Task.FromException(new AggregatorValidationException("foo", new System.Collections.Generic.Dictionary<string, string[]>
			{
				["Page"] = new[] { "must be >= 1" }
			}));
			var middleware = new ExceptionMiddleware(next, logger);

			var context = CreateContext();
			await middleware.InvokeAsync(context);

			context.Response.StatusCode.Should().Be(400);

			context.Response.Body.Seek(0, SeekOrigin.Begin);
			using var doc = JsonDocument.Parse(context.Response.Body);
			var root = doc.RootElement;
			root.GetProperty("title").GetString().Should().Be("Validation error");
			root.TryGetProperty("errors", out var errors).Should().BeTrue();
			errors.GetProperty("Page").EnumerateArray().Should().NotBeEmpty();
		}


		[Test]
		public async Task Middleware_Returns_503_ForExternalServiceException()
		{
			var logger = new Mock<ILogger<ExceptionMiddleware>>().Object;
			RequestDelegate next = _ => Task.FromException(new AggregatorServiceException("foo"));
			var middleware = new ExceptionMiddleware(next, logger);

			var context = CreateContext();
			await middleware.InvokeAsync(context);

			context.Response.StatusCode.Should().Be(503);
			context.Response.Body.Seek(0, SeekOrigin.Begin);
			using var doc = JsonDocument.Parse(context.Response.Body);
			doc.RootElement.GetProperty("title").GetString().Should().Be("External service error");
		}

		[Test]
		public async Task Middleware_Returns_500_ForGenericException()
		{
			var logger = new Mock<ILogger<ExceptionMiddleware>>().Object;
			RequestDelegate next = _ => Task.FromException(new Exception("foo"));
			var middleware = new ExceptionMiddleware(next, logger);

			var context = CreateContext();
			await middleware.InvokeAsync(context);

			context.Response.StatusCode.Should().Be(500);
			context.Response.Body.Seek(0, SeekOrigin.Begin);
			using var doc = JsonDocument.Parse(context.Response.Body);
			doc.RootElement.GetProperty("title").GetString().Should().Be("An unexpected error occurred.");
		}
	}
}
