using Microsoft.VisualStudio.TestTools.UnitTesting;

using HomeConf;

namespace HomeConfTests {
    [TestClass]
    public class ExtractParamsTest {

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
