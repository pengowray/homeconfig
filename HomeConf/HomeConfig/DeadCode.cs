using System;
using System.Collections.Generic;
using System.Text;

//using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeConf {
    class DeadCode
    {


        protected void A(ConfigurationBuilder builder) {

        }

        /*
    //example
    static public string DefaultConnectionString { get; } =
@"Server=(localdb)\\mssqllocaldb;Database=SampleData-0B3B0919-C8B3-481C-9833-
36C21776A565;Trusted_Connection=True;MultipleActiveResultSets=true";

    //example
    static IReadOnlyDictionary<string, string> DefaultConfigurationStrings { get; } =
      new Dictionary<string, string>() {
          ["Profile:UserName"] = Environment.UserName,
          [$"AppConfiguration:ConnectionString"] = DefaultConnectionString,
          [$"AppConfiguration:MainWindow:Height"] = "400",
          [$"AppConfiguration:MainWindow:Width"] = "600",
          [$"AppConfiguration:MainWindow:Top"] = "0",
          [$"AppConfiguration:MainWindow:Left"] = "0",
      };
    */

        /*
        /// <summary>
        /// Just some dead code that isn't used because non-web host building seems to alpha rihgt now (dotnet core 2.1)
        /// </summary>
        /// <returns></returns>
        private IHost BuildHost() {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseEnvironment(EnvironmentName.Development);

            //The app uses whichever option sets a value last on a given key
            hostBuilder.ConfigureServices((hostContext, services) => {
                services.Configure<HostOptions>(option => {
                    option.ShutdownTimeout = System.TimeSpan.FromSeconds(20);
                });
            });
            //hostBuilder.SetBasePath(env.ContentRootPath);
            return hostBuilder.Build();
        }
        */

    }
}
