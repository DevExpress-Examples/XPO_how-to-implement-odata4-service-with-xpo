using System;
using System.Diagnostics;
using System.IO;
using Default;
using NUnit.Framework;

namespace Tests {
    public abstract class ODataTestsBase {

        const string ODataServiceUrl = "http://localhost:5000/";
        Process iisProcess = null;

        protected Container GetODataContainer() {
            return new Container(new Uri(ODataServiceUrl));
        }

        [OneTimeSetUp]
        public void OneTimeSetup() {
            string iisExpressPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "IIS Express", "iisexpress.exe");
            string appPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            appPath = Path.GetFullPath(Path.Combine(appPath, "..", "..", "..", "ODataService"));
            string args = string.Format("/path:\"{0}\" /port:5000", appPath);
            iisProcess = Process.Start(iisExpressPath, args);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() {
            if(iisProcess != null) {
                iisProcess.Kill();
            }
        }

        [SetUp]
        public void Setup() {
            Container container = GetODataContainer();
            container.InitializeDatabase().Execute();
        }
    }
}
