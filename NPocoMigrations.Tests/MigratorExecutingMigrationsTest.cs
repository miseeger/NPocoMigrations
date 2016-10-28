using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPoco;

namespace NPocoMigrations.Tests
{
    [TestClass]
    public class MigratorExecutingMigrationsTest
    {

        private static NPocoMigrator _migrator;
        private static bool _loadStatus;

            
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _migrator = new NPocoMigrator();
            _loadStatus = _migrator.LoadConfig() && _migrator.LoadMigrations();
        }


        [TestInitialize]
        public void ClearTest()
        {
            if (_loadStatus)
            {

                _migrator.MigrationsConfig.DbVersion = "0.0.0";
                _migrator.SaveConfig();

                using (var db = new Database(_migrator.MigrationsConfig.DbConnection))
                {
                    db.Execute("IF OBJECT_ID('dbo.GeoData', 'U') IS NOT NULL DROP TABLE dbo.GeoData");
                    db.Execute("IF OBJECT_ID('dbo.MemberSettings', 'U') IS NOT NULL DROP TABLE dbo.MemberSettings");
                    db.Execute("IF OBJECT_ID('dbo.TableColumns_v', 'V') IS NOT NULL DROP VIEW dbo.TableColumns_v");
                }
            }
        }


        [TestMethod]
        public void LoadsVersion100Test()
        {
            Assert.IsTrue(_loadStatus);
            Assert.AreEqual(3, _migrator.MigrationsList[0].Tasks.Count);

            Assert.IsFalse(_migrator.MigrationsList[0].Tasks[0].Execute == string.Empty);
            Assert.IsNotNull(_migrator.MigrationsList[0].Tasks[0].Test);

            Assert.IsFalse(_migrator.MigrationsList[0].Tasks[2].Execute == string.Empty);
            Assert.IsNull(_migrator.MigrationsList[0].Tasks[2].Test);
        }


        [TestMethod]
        public void LoadsVersion101Test()
        {
            Assert.IsTrue(_loadStatus);
            Assert.AreEqual(2, _migrator.MigrationsList[1].Tasks.Count);

            Assert.IsFalse(_migrator.MigrationsList[1].Tasks[0].Execute == string.Empty);
            Assert.IsNotNull(_migrator.MigrationsList[1].Tasks[0].Test);

            Assert.IsFalse(_migrator.MigrationsList[1].Tasks[1].Execute == string.Empty);
            Assert.IsNull(_migrator.MigrationsList[1].Tasks[1].Test);
        }


        [TestMethod]
        public void ExecuteVersion101OnlyTest()
        {
            Assert.IsTrue(_loadStatus);

            _migrator.MigrationsConfig.DbVersion = "1.0.0";
            var result = _migrator.ExecuteMigrations();

            Assert.IsTrue(result);

            var testMigrator = new NPocoMigrator();
            testMigrator.LoadConfig();

            Assert.AreEqual("1.0.1", testMigrator.MigrationsConfig.DbVersion);
            Assert.AreEqual(new Version("1.0.1"), testMigrator.MigrationsConfig.SysDbVersion);
        }


        [TestMethod]
        public void ExecutesAllMigrationsTest()
        {
            Assert.IsTrue(_loadStatus);

            var result = _migrator.ExecuteMigrations();

            Assert.IsTrue(result);

            var testMigrator = new NPocoMigrator();
            testMigrator.LoadConfig();

            Assert.AreEqual("1.0.1", testMigrator.MigrationsConfig.DbVersion);
            Assert.AreEqual(new Version("1.0.1"), testMigrator.MigrationsConfig.SysDbVersion);
        }

    }

}
