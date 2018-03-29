using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using NLog.Extensions.Logging;
using YaVipCore.Middleware;
using YaVipCore.Service;
using YaVipCore.Api.Music;
using YaVipCore.Helper;
using YaVipCore.Api.Video;

namespace YaVipCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            loggerFactory.AddConsole(LogLevel.Warning).AddNLog();

            app.UseTokenSign();
            app.UseMvc();
            app.UseStaticFiles();

            InitUserCfg();
        }

        private void InitUserCfg()
        {
            UserService.InitUser();
            CommonHelper.InitJsPool();

            var userCfg = Configuration.GetSection("UserConfig");
            var wyCookie = userCfg["WyCookie"];
            if (!string.IsNullOrEmpty(wyCookie))
            {
                WyMusic.WyNewCookie = wyCookie;
            }

            var ipAddr = userCfg["IpAddress"];
            if (!string.IsNullOrEmpty(ipAddr))
            {
                CommonHelper.IpAddr = ipAddr;
            }

            var signKey = userCfg["SignKey"];
            if (!string.IsNullOrEmpty(signKey))
            {
                CommonHelper.SignKey = signKey;
            }

            var yk = userCfg["YkCookie"];
            if (!string.IsNullOrEmpty(yk))
            {
                YkVip.Cookie = yk;
            }
        }
    }
}
