using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MovieProject.Logic.Exceptions;
using System;
using System.Threading.Tasks;

namespace MovieProject.Web
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<Startup> logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<Startup> logger)
        {
            _next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BadRequestException e)
            {
                // request (user) errors

                string message = $"{e.Message} = [{e.InvalidValue}]";
                logger.LogWarning($"Bad request={message}");

                await WriteOutput(context, StatusCodes.Status400BadRequest, message);
                return;
            }
            catch (Exception e)
            {
                // only errors in our own app should result here

                logger.LogCritical(e, e.Message);

                string message = e is DownstreamException ? Constants.DownstreamErrorMessage : Constants.DefaultErrorMessage;

                await WriteOutput(context, StatusCodes.Status500InternalServerError, message);
                return;
            }
        }

        private async Task WriteOutput(HttpContext context, int httpStatus, string message)
        {
            context.Response.Clear();
            context.Response.StatusCode = httpStatus;
            await context.Response.WriteAsync(message);
        }
    }
}
