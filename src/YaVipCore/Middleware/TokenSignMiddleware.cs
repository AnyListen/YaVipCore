using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using YaVipCore.Service;

namespace YaVipCore.Middleware
{
    public class TokenSignMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenSignMiddleware(RequestDelegate next)
        {
            _next = next;
            //_logger = logger ?? new LoggerFactory().CreateLogger<TokenSignMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value;
            
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith("/music/") && !path.Contains("/music/ymusic/"))
                {
                    var sign = context.Request.Headers["Token"];
                    if (sign.Count <= 0 || !UserService.CheckSign(sign[0]))
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }
                }
            }
            
            await _next.Invoke(context);
        }
    }

    public static class SignExtensions
    {
        public static IApplicationBuilder UseTokenSign(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenSignMiddleware>();
        }
    }
}