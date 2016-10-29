using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NPocoMigrations.Tests
{
    [TestClass]
    public class MigratorLoadingConfigTest
    {

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            if (File.Exists($"_{NPocoMigrationsConstants.MigrationsConfigFile}"))
            {
                File.Delete($"_{NPocoMigrationsConstants.MigrationsConfigFile}");
            }
        }


        [TestMethod]
        public void LoadingConfigFailsTest()
        {
            File.Move(NPocoMigrationsConstants.MigrationsConfigFile,
                $"_{NPocoMigrationsConstants.MigrationsConfigFile}");

            var migrator = new NPocoMigrator(".");
            var result = migrator.LoadConfig();

            File.Move($"_{NPocoMigrationsConstants.MigrationsConfigFile}",
                NPocoMigrationsConstants.MigrationsConfigFile);

            Assert.IsFalse(result);
        }


        [TestMethod]
        public void LoadingConfigSucceedsTest()
        {
            var migrator = new NPocoMigrator();

            Assert.IsTrue(migrator.LoadConfig());
        }


        [TestMethod]
        public void MigrationsConfigCorrectlyLoaded()
        {
            var migrator = new NPocoMigrator();
            migrator.LoadConfig();

            Assert.AreEqual("DevDb", migrator.MigrationsConfig.DbConnection);
            Assert.AreEqual("0.0.0", migrator.MigrationsConfig.DbVersion);
            Assert.AreEqual(new Version("0.0.0"), migrator.MigrationsConfig.SysDbVersion);
            Assert.AreEqual("Migrations", migrator.MigrationsConfig.MigrationsDir);
            Assert.AreEqual($"{AppDomain.CurrentDomain.BaseDirectory}\\{NPocoMigrationsConstants.MigrationsConfigFile}", 
                migrator.MigrationsConfigFilePath);
        }

    }

}
