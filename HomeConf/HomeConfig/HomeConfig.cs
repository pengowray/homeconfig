using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

namespace HomeConf {
    public class HomeConfig {
        // Terminology: "Path" will generally be used to mean a full path. "Folder" for a node. 
        // e.g. path: "C:\Users\PengoWray\MyApp"
        //      folder: "MyApp"

        public string AppFolder = null; // e.g. "myapp"; //TODO: read only
        public string ConfigSubFolder = null; // "config"; // create a subfolder for config files //TODO: read only

        // configured automatically (hopefully)
        string BaseUserPath; // e.g. "C:\Users\USERNAME\Documents\" or "/home/pengowray/"
        string AppUserDataPath; // e.g. "C:\Users\USERNAME\Documents\myapp" i.e. BaseUserPath + AppFolder
        string ConfigPath; // store config files here. Typically same as AppUserDataPath, or if ConfigSubFolder is not null then, e.g. "C:\Users\PengoWray\myapp\config" 
        string InstallBasePath; // where the app is installed or run from: AppDomain.CurrentDomain.BaseDirectory 
        string OtherInstallConfigPath; // e.g. $"{InstallBasePath}/config"
        string EnvironmentName; // e.g. "Production" or "Testing"
        string OSName; // "linux", "windows", "osx", or "unknownos"

        protected ConfigurationBuilder builder;
        public IConfigurationRoot Configuration { protected set; get; }

        public string this[string key] {
            get { return Configuration[key]; }
            set { Configuration[key] = value; }
        }

        public string ParamConfigFile = null;

        public HomeConfig() {
            //TODO: more ways to configure before this point?
            string name = Assembly.GetEntryAssembly().GetName().Name;
            string[] args = Environment.GetCommandLineArgs();
            Setup(name, args);
        }

        // was originally a convience method for typical usage.
        protected string[] Setup(string appNameAndFolder, string[] paramList, string defaultJson = "", string exampleJson = "") {
            AppFolder = appNameAndFolder;

            if (string.IsNullOrWhiteSpace(AppFolder)) {
                throw new ArgumentException("No app name. HomeConfig requires AppFolder to be set.");
            }

            builder = new ConfigurationBuilder();

            var returnParams = ExtractParams(paramList);
            AddJsonFile(ParamConfigFile); //TODO: check if exists?

            //builder.AddEnvironmentVariables(); // maybe?
            builder.AddEnvironmentVariables(AppFolder); // AppName.Blah becomes Blah (I think)? TODO: Test this

            EnvironmentName = Environment.GetEnvironmentVariable("EnvironmentName");
            OSName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
                   : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx"
                   : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux"
                   : "unknownos";

            FindOrCreateConfigPath();  // c:\users\USERNAME\myapp\appname.json / appname.linux.json / appname.production.json
            FindAppPath(); // where app is run from

            /*
            //Create default json if doesn't exist; e.g. "appname.json"
            //TODO: move to CreateDefaultJson() ?
            string defaultFile = Path.Combine(ConfigPath, $"{AppFolder}.json");
            if (!File.Exists(defaultFile)) {
                try {
                    File.Create(defaultFile);
                    TextWriter tw = new StreamWriter(defaultFile);
                    tw.WriteLine("{}");
                    tw.Close();
                } catch {
                    //TODO: why not?
                    Console.Error.WriteLine("Could not create default config file: " + defaultFile);
                }
            }
            */

            //todo: addition seach spots via env variable $CONFIGPATH or commandline --configpath 
            string[] searchSpots = {
                OtherInstallConfigPath,
                InstallBasePath,
                ConfigPath };

            foreach (var spot in searchSpots) {
                AddJsonFile($"homeconfig.json", spot);
                AddJsonFile($"{AppFolder}.json", spot);
                //AddJsonFile($"{AppFolder}.homeconfig.json", spot); //TODO: decide on preferred config name

                AddJsonFile($"config.{OSName}.json", spot);
                AddJsonFile($"{AppFolder}.{OSName}.json", spot);

                // $EnvironmentName
                if (!string.IsNullOrWhiteSpace(EnvironmentName)) {
                    AddJsonFile($"config.{EnvironmentName}.json", spot);
                    AddJsonFile($"{AppFolder}.{EnvironmentName}.json", spot);
                    AddJsonFile($"config.{EnvironmentName}.{OSName}.json", spot);
                    AddJsonFile($"{AppFolder}.{EnvironmentName}.{OSName}.json", spot);
                }
            }

            // Config files found in environment variables
            //note: "ignoreMissing: false" only triggers here if a file is non-blank and it's missing
            //TODO: should these have higher/lower priority?
            AddJsonEnvFile($"{AppFolder}.config", ignoreMissing: false);
            if (!string.IsNullOrWhiteSpace(EnvironmentName)) {
                AddJsonEnvFile($"{AppFolder}.{EnvironmentName}.config", ignoreMissing: false);
            }

            //TODO:
            //CreateExampleJson(exampleJson);   // appname.json.example 

            //TODO:
            AddSecrets();

            Configuration = builder.Build();

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

            //part A: --config B: = c: filename.json
            var partABC = new Regex(@"^[\t ]*(?<config>((--config|--CONFIG|--Config|-c)([\t ]*\=[\t ]*|[\t ]+|[\t ]*$)))(?<filename>[^\t\n\r\v\f].*?)?[\t ]*$");
            var partBC  = new Regex(@"^(?<equals>[\t ]*\=)?[\t ]*(?<filename>[^\= \t\n\r\v\f].*?)?[\t ]*$");
            var partC   = new Regex(@"^[\t ]*(?<filename>[^\= \t\n\r\v\f].*?)[\t ]*$"); //NOTE: doens't allow filename to start with an "="

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
                        var filename = match.Groups["filename"].Value.Trim(); // should already be trimmed by regex but, to be sure
                        if (filename.StartsWith('"') && filename.EndsWith('"')) {
                            filename = filename.Substring(1, filename.Length - 2); // remove first and last (quotation marks)
                        }
                        ParamConfigFile = filename;

                        //return paramArray.Where((val, index) => index != i).ToArray(); // return all but this param
                        return paramArray.Where((val, index) => index != i && index != aLoc && index != bLoc).ToArray();  // everything but the locations of the parameter that we've handled.

                    } else if (match.Groups["config"].Success) { // only if partABC
                        aLoc = i;
                        //Console.WriteLine("Found <config>");
                        if (match.Groups["config"].Value.TrimEnd().EndsWith("=")) {
                            bLoc = i;
                        }

                    } else if (match.Groups["equals"].Success) { // only if partBC
                        // found equals without filename
                        bLoc = i;

                    } else if (bLoc != -1 || aLoc != -1) {
                    // error: (aLoc != -1) found "--config[=]" previously but filename (or lone equals sign) not found

                    //Console.WriteLine("Configuration filename missing"); //TODO: throw an error
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="envVariable"></param>
        /// <param name="ignoreMissing">if true, will only complain if the environment variable is set to some non-whitespice value and that filename doesn't exist</param>
        /// <returns></returns>
        protected void AddJsonEnvFile(string envVariable, bool ignoreMissing = true) {
            if (string.IsNullOrWhiteSpace(envVariable)) 
                return;

            //Console.WriteLine($"Adding file from env variable: {envVariable}");

            //envConfigFile 
            var path = Environment.GetEnvironmentVariable(envVariable); // e.g. $"{AppFolder}.{EnvironmentName}.config"
            if (string.IsNullOrWhiteSpace(path)) {
                return;
            }

            builder.AddJsonFile(path, ignoreMissing);
            /*
            if (File.Exists(path)) {
                Console.WriteLine($"Adding config file from env variable: {envVariable}=\"{path}\"");
                
                return true;

            } else if (!ignoreMissing) {
                throw new FileNotFoundException($"invalid path found in env variable. {envVariable}=\"{path}\"");
                //return false;
            } else {
                Console.WriteLine($"Adding config file from env variable: {envVariable}=\"{path}\" [file not found]");
                return false;
            }

            //return false;
            */
        }

        protected void AddJsonFile(string filename, string configPath = null, bool ignoreMissing = true) {

            if (string.IsNullOrWhiteSpace(filename)) //todo: cw
                return; // false

            //var path = combineWithConfigPath ? Path.Combine(ConfigPath, filename) : filename;
            var path = !string.IsNullOrWhiteSpace(configPath) ? Path.Combine(ConfigPath, filename) : filename;

            builder.AddJsonFile(path, ignoreMissing);
            /*
            if (File.Exists(path)) {
                Console.WriteLine($"Adding config file: {path}");
                builder.AddJsonFile(path, true);
                return true;

            } else if (!ignoreMissing) {
                throw new FileNotFoundException(filename);
                //return false;

            } else {
                Console.WriteLine($"Adding config file: {path} [not found]");
                return false;
            }
            */
        }

        protected void FindOrCreateConfigPath() {

            if (string.IsNullOrWhiteSpace(AppFolder)) {
                throw new ArgumentException("No app name. HomeConfig requires AppFolder to be set");
            }

            BaseUserPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            AppUserDataPath = Path.Combine(BaseUserPath, AppFolder);
            ConfigPath = AppUserDataPath;
            if (!string.IsNullOrWhiteSpace(ConfigSubFolder)) {
                ConfigPath = Path.Combine(AppUserDataPath, ConfigSubFolder);
            } else {
                ConfigSubFolder = null; // null if whitespace
            }

            new DirectoryInfo(AppUserDataPath).Create(); // If the directory already exists, this method does nothing.
            new DirectoryInfo(ConfigPath).Create(); // If the directory already exists, this method does nothing.

            /*
            var files = Directory.GetFiles(ConfigPath, "*.json");
            foreach (var file in files.OrderBy(f => f)) {
                builder.AddJsonFile(file);
            }
            */

        }

        protected void FindAppPath() {
            // both of these are searched
            InstallBasePath = AppDomain.CurrentDomain.BaseDirectory;
            OtherInstallConfigPath = Path.Combine(InstallBasePath, "config"); // e.g. $"{InstallBasePath}/config"
        }

        /// <summary>
        /// Read in all .json files in $HOME/beastie/config
        /// </summary>
        /// <param name="builder"></param>
        protected void AddHomeConfigFiles() {

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
