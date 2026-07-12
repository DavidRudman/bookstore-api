using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.PublicAPI.Middleware
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) 
        {
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken token)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);

            var error = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Instance = ctx.Request.Path
            };

            ctx.Response.StatusCode = error.Status.Value;
            await ctx.Response.WriteAsJsonAsync(error, token);
            return true;
        }
    }
}
