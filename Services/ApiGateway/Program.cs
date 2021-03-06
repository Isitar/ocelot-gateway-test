using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApiGateway
{
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    var env = ctx.HostingEnvironment;
                    if (env.IsDevelopment())
                    {
                        var jwtSettingsPath = Path.GetFullPath("../../Libraries/ApiAuthLib/jwtSettings.json");
                        config.AddJsonFile(jwtSettingsPath, false, true);
                    }
                    else
                    {
                        config.AddJsonFile(Path.GetFullPath("jwtSettings.json"), false, true);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}