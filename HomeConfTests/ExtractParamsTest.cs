using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

using HomeConf;

namespace HomeConfTests {
    [TestClass]
    public class ExtractParamsTest {

        [TestMethod]
        // normal usage:
        [DataRow("--config=config.json", "config.json", "")]
        [DataRow("--config|config.json", "config.json", "")]
        [DataRow("-c|config.json", "config.json", "")]

        // normal usage plus spaces
        [DataRow("--config|my space folder\\config.json", "my space folder\\config.json", "")]
        [DataRow("-c|con space fig.json", "con space fig.json", "")]

        // normal usage + other params
        [DataRow("--click|-clack|--config=config.json", "config.json", "--click|-clack")]
        [DataRow("--config=config.json|--click|-clack", "config.json", "--click|-clack")]
        [DataRow("-pre|--config=config.json|-post", "config.json", "-pre|-post")]

        // normal with quotation marks
        [DataRow("--config=\"config.json\"", "config.json", "")]
        [DataRow("--config|\"config.json\"", "config.json", "")]
        [DataRow("-c|\"config.json\"", "config.json", "")]

        // weird caps
        [DataRow("--CONFIG=Config.json", "Config.json", "")]
        // "-C" is ignored, only "-c" allowed
        [DataRow("-C|Config.json", null, "-C|Config.json")]
        [DataRow("-c|Config.json", "Config.json", "")]

        //unlikely usage:
        [DataRow("--config|=|config.json", "config.json", "")]
        [DataRow("--config |=|config.json", "config.json", "")]
        [DataRow("--config|= |config.json", "config.json", "")]
        [DataRow("--config|=| config.json", "config.json", "")]
        [DataRow("--config |=|config.json", "config.json", "")]
        [DataRow("--config | = | config.json", "config.json", "")]
        [DataRow("--config \t|\t =  |\t config.json", "config.json", "")]
        [DataRow("--config=|config.json", "config.json", "")]
        [DataRow("--config= |config.json", "config.json", "")]
        [DataRow("--config=| config.json  ", "config.json", "")]
        [DataRow("--config|=config.json", "config.json", "")]
        [DataRow("--config| =config.json", "config.json", "")]
        [DataRow("  --config | =config.json", "config.json", "")]

        [DataRow("x|-y|hi|--config=config.json", "config.json", "x|-y|hi")]
        [DataRow("--config=config.json|x|-y|hi", "config.json", "x|-y|hi")]

        [DataRow("-c config.json", "config.json", "")]
        [DataRow("-c=config.json", "config.json", "")]
        [DataRow("-c   config.json", "config.json", "")]

        public void ConfigTestGeneric(string paramsPipeSeparated, string expectedFilename, string expectedRemainingParams) {
            var conf = new HomeConfig();
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = paramsPipeSeparated.Split('|');
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(expectedFilename, conf.ParamConfigFile);

            var expectedRem = expectedRemainingParams == ""
                ? new string[0] // if we expect "" then expect empty array
                : expectedRemainingParams.Split('|');

            Assert.AreEqual(expectedRem.Length, result.Length);
            for (int i=0; i< expectedRem.Length; i++) {
                Assert.AreEqual(expectedRem[i], result[i]);
            }
        }

        [TestMethod]
        public void ConfigTest1Value() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { $"--config={filename}"};
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void ConfigTest1ValueExtraArgs() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { "arg0", $"--config={filename}", "arg1" };
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("arg0", result[0]);
            Assert.AreEqual("arg1", result[1]);
        }


        [TestMethod]
        public void ConfigTest1ValueNoEquals() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { $"--config {filename}" };
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(0, result.Length);
        }


        [TestMethod]
        public void ConfigTest1ValueEqualsAndSpaces() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { $"--config = {filename}" };
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void ConfigTest2ValuesEquals() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { "--config=", filename };
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(0, result.Length);
        }




        [TestMethod]
        public void ConfigTest2ValuesNoEquals() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { "--config", filename };
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(0, result.Length);
        }


        [TestMethod]
        public void ConfigTest2ValuesEqualsOnOtherSide() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { "--config", $"={filename}" };
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(0, result.Length);
        }


        [TestMethod]
        public void ConfigTest3Values() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { "--config", "=", filename };
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(0, result.Length);
        }



        [TestMethod]
        public void ConfigTest3ValuesSpaces() {
            var conf = new HomeConfig();
            string filename = "test.json";
            conf.AppFoldername = "HomeConfigLibraryTests";
            string[] paramList = { "--config ", " = ", filename };
            var result = conf.ExtractParams(paramList);
            Assert.AreEqual(filename, conf.ParamConfigFile);
            Assert.AreEqual(0, result.Length);
        }

    }
}
