using SensoringAPI.Services.Interfaces;

namespace SensoringAPI.Middleware
{
    public class TestModeMiddleware
    {
        private readonly RequestDelegate _next;

        public TestModeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITestModeService testModeService)
        {
            if (context.Request.Headers.TryGetValue("X-Test-Mode", out var headerValue) &&
                bool.TryParse(headerValue, out var isTest) &&
                isTest)
            {
                testModeService.IsTestMode = true;
            }

            await _next(context);
        }
    }
}
