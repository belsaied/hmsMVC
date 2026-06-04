using HMS.BLL.ServicesAbstraction.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace HMS.PL.Authorization
{
    public class RedisCacheAttribute (int durationInSeconds=120) : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices
                .GetRequiredService<IServiceManager>().CacheService;

            string key = GenerateKey(context.HttpContext.Request);
            var result = await cacheService.GetCachedValueAsync(key);

            if (result != null)
            {
                context.Result = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK
                };
                return;
            }

            var resultContext = await next.Invoke();

            if (resultContext.Result is OkObjectResult okObjResult)
            {
                await cacheService.SetCacheValueAsync(
                    key, okObjResult.Value!,
                    TimeSpan.FromSeconds(durationInSeconds));
            }
        }

        public static string GenerateKey(HttpRequest request)
        {
            var key = new StringBuilder();
            key.Append(request.Path.ToString().ToLowerInvariant());

            foreach (var item in request.Query
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                key.Append($":{item.Key.ToLowerInvariant()}={item.Value}");
            }

            return key.ToString();
        }
    }
}
