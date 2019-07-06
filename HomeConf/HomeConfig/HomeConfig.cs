using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

namespace HomeConf {
    public class HomeConfig {
        public string AppFoldername = null; // e.g. "myapp";
        public string ConfigSubFoldername = null; // "config"; // create a subfolder for config files

        protected ConfigurationBuilder builder;
        public IConfigurationRoot Configuration { protected set; get; }

        public string ParamConfigFile = null;

        public HomeConfig() {
        }

        // convience method for typical usage.
        public string[] Setup(string appName, string[] paramList, string defaultJson, string exampleJson) {
            AppFoldername = appName;

            builder = new ConfigurationBuilder();

            //AddEnv(); // TODO
            var returnParams = ExtractParams(paramList);
            AddJsonFile(ParamConfigFile); //TODO: check if exists?

            //CreateExampleJson();   // appname.json.example // TODO
            //CreateDefaultJson();   // appname.json // TODO
            AddHomeConfigFiles();  // c:\users\USERNAME\myapp\appname.json / appname.linux.json / appname.production.json
            AddSecrets();

            return returnParams;
        }


        public string[] ExtractParams(string[] paramArray) {
            // finds and removes --config=filename from params ... 
            // '=' may have spaces on either side, or may be replaced by a space
            // future might also take --SET:<param>=<value> or something

            // TODO: In future perhaps make compatible with Dragonfruit (System.CommandLine)
            // Dragonfruit is too broken to use right as of now (alpha-0.2)
            // https://msdn.microsoft.com/en-us/magazine/mt833289.aspx
            // https://github.com/dotnet/command-line-api/wiki/DragonFruit-overview

            if (paramArray == null || paramArray.Length == 0)
                return paramArray;

            var partABC = new Regex(@"^[\t ]*(?<config>((--config|-c)([\t ]*\=[\t ]*|[\t ]+|[\t ]*$)))(?<filename>[^\t\n\r\v\f].*?)?[\t ]*$", RegexOptions.IgnoreCase);
            var partBC  = new Regex(@"^(?<equals>[\t ]*\=)?[\t ]*(?<filename>[^\= \t\n\r\v\f].*?)?[\t ]*$");
            var partC   = new Regex(@"^[\t ]*(?<filename>[^\= \t\n\r\v\f].*?)[\t ]*$"); //NOTE: doens't allow filename to start with an "="

            //string partC = @"([""'])(?:(?=(\\?))\2.)*?\1"; // anything between quotes
            //var partB = new Regex("=");
            //var partC = new Regex("");

            int aLoc = -1;
            int bLoc = -1;
            //int cLoc = -1; // bit redundant.. wont need to store this because we return once we do.

            for (int i = 0; i < paramArray.Length; i++) {

                var part = partABC; // try to match: --config=blah
                if (aLoc != -1) {
                    part = partBC;  // try to match: =blah  (--config already matched previously)
                    if (bLoc != -1) 
                        part = partC;
                }

                var match = part.Match(paramArray[i]);
                if (match.Success) {
                    if (match.Groups["filename"].Success && !string.IsNullOrWhiteSpace(match.Groups["filename"].Value)) {
                        // Found

                        // TODO: strip quotes
                        var filename = match.Groups["filename"].Value;
                        ParamConfigFile = filename;

                        //return paramArray.Where((val, index) => index != i).ToArray(); // return all but this param
                        return paramArray.Where((val, index) => index != i && index != aLoc && index != bLoc).ToArray();  // everything but the locations of the parameter that we've handled.

                    } else if (match.Groups["config"].Success) { // only if partABC
                        aLoc = i;
                        Console.WriteLine("Found <config>");
                        if (match.Groups["config"].Value.TrimEnd().EndsWith("=")) {
                            bLoc = i;
                        }

                    } else if (match.Groups["equals"].Success) { // only if partBC
                        // found equals without filename
                        bLoc = i;

                    } else if (bLoc != -1 || aLoc != -1) {
                    // error: (aLoc != -1) found "--config[=]" previously but filename (or lone equals sign) not found

                    Console.WriteLine("Configuration filename missing"); //TODO: throw an error
                    return paramArray.Where((val, index) => index != aLoc && index != bLoc).ToArray();  // everything but the locations of the parameter that we've handled.

                }
                }
            }

            return paramArray;
        }

        public IConfigurationRoot Build() {
            //Configuration = BuildConfig();

            return Configuration;
        }

        protected void AddJsonFile(string filename, bool ignoreMissing = false) {
            builder.AddJsonFile(filename);
        }

        /// <summary>
        /// Read in all .json files in $HOME/beastie/config
        /// </summary>
        /// <param name="builder"></param>
        protected void AddHomeConfigFiles() {

            if (string.IsNullOrWhiteSpace(AppFoldername)) {
                return;
            }

            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string appFolder = Path.Combine(baseFolder, AppFoldername);
            string configFolder = appFolder;
            if (!string.IsNullOrWhiteSpace(ConfigSubFoldername)) {
                configFolder = Path.Combine(appFolder, ConfigSubFoldername);
            }

            new DirectoryInfo(appFolder).Create(); // If the directory already exists, this method does nothing.
            new DirectoryInfo(configFolder).Create(); // If the directory already exists, this method does nothing.

            var files = Directory.GetFiles(configFolder, "*.json");

            foreach (var file in files.OrderBy(f => f)) {
                builder.AddJsonFile(file);
            }

        }

        protected void AddSecrets() {
            // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows
        }

        //public static async Task<int> Main(string[] args) {
        /*
        private IConfigurationRoot BuildConfig() {
            //string commandLineConfigFile = (args.Length > 0) ? args[0] : null;


            // Add defaultConfigurationStrings
            builder.AddInMemoryCollection(DefaultConfigurationStrings);

            // test string
            string jsonString = @" { test:OK } ";
            var memoryFileProvider = new InMemoryFileProvider(jsonString);

            AddHomeConfigFiles(builder);

            // poor man's hosting environment name
            string envName = Environment.GetEnvironmentVariable("EnvironmentName");
            //string envName = env.EnvironmentName;  // better version, if we were using an IHost thing: 
            if (!string.IsNullOrWhiteSpace(envName)) {
                builder.AddJsonFile($"{AppName}.{envName}.json", optional: true); // what's root dir?
            }

            // $BEASTIECONFIG
            string envConfigFile = Environment.GetEnvironmentVariable("BEASTIECONFIG");
            if (!String.IsNullOrWhiteSpace(envConfigFile))
                builder.AddJsonFile(envConfigFile, optional: true, reloadOnChange: true);


            //broken?
            //builder.AddEnvironmentVariables();

            var configuration = builder.Build();
            Console.WriteLine($"Hello {configuration["Profile:UserName"]}");

            //ConsoleWindow consoleWindow = Configuration.Get<ConsoleWindow>("AppConfiguration:MainWindow");
            //ConsoleWindow.SetConsoleWindow(consoleWindow);

            return configuration;
        }
        */


        /*
        public static async Task MainWithHost(string[] args)
        {
            builder.ConfigureHostConfiguration(configHost => {
                //  configHost.Add(new FileProvider()); ??
                configHost.AddConfiguration();
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddJsonFile("hostsettings.json", optional: true);
                configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                configHost.AddCommandLine(args);
            });
            IHost host = builder.Build();

            await host.RunAsync();
        }
        */


    }
}
