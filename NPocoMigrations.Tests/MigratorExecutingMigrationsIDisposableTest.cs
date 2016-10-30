using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPoco;

namespace NPocoMigrations.Tests
{
    [TestClass]
    public class MigratorExecutingMigrationsIDisposableTest
    {

        [TestInitialize]
        public void ClearTest()
        {
            using (var migrator = new NPocoMigrator("."))
            {
                migrator.LoadConfig();

                using (var db = new Database(migrator.MigrationsConfig.DbConnection))
                {
                    db.Execute("IF OBJECT_ID('dbo.GeoData', 'U') IS NOT NULL DROP TABLE dbo.GeoData");
                    db.Execute("IF OBJECT_ID('dbo.MemberSettings', 'U') IS NOT NULL DROP TABLE dbo.MemberSettings");
                    db.Execute("IF OBJECT_ID('dbo.TableColumns_v', 'V') IS NOT NULL DROP VIEW dbo.TableColumns_v");
                }
            }
        }


        [TestMethod]
        public void ExecuteVersion101OnlyTest()
        {
            // use execution directory (implicitly)
            using (var migrator = new NPocoMigrator())
            {
                migrator.LoadConfig();
                migrator.MigrationsConfig.DbVersion = "1.0.0";
                migrator.LoadMigrations();

                var result = migrator.ExecuteMigrations();

                Assert.IsTrue(result);

                using (var testMigrator = new NPocoMigrator())
                {
                    testMigrator.LoadConfig();
                    Assert.AreEqual("1.0.1", testMigrator.MigrationsConfig.DbVersion);
                    Assert.AreEqual(new Version("1.0.1"), testMigrator.MigrationsConfig.SysDbVersion);
                }

                migrator.MigrationsConfig.DbVersion = "0.0.0";
                migrator.SaveConfig();
            }
        }


        [TestMethod]
        public void ExecutesAllMigrationsTest()
        {
            // use execution directory (explicitly)
            using (var migrator = new NPocoMigrator("."))
            {
                migrator.LoadConfig();
                migrator.LoadMigrations();
                var result = migrator.ExecuteMigrations();

                Assert.IsTrue(result);

                using (var testMigrator = new NPocoMigrator())
                {
                    testMigrator.LoadConfig();
                    Assert.AreEqual("1.0.1", testMigrator.MigrationsConfig.DbVersion);
                    Assert.AreEqual(new Version("1.0.1"), testMigrator.MigrationsConfig.SysDbVersion);
                }

                migrator.MigrationsConfig.DbVersion = "0.0.0";
                migrator.SaveConfig();
            }

        }


        [TestMethod]
        public void ExecutesAllMigrationsShortTest()
        {
            // use execution directory (explicitly)
            using (var migrator = new NPocoMigrator("."))
            {
                var result = migrator.Migrate();

                Assert.IsTrue(result);

                using (var testMigrator = new NPocoMigrator())
                {
                    testMigrator.LoadConfig();
                    Assert.AreEqual("1.0.1", testMigrator.MigrationsConfig.DbVersion);
                    Assert.AreEqual(new Version("1.0.1"), testMigrator.MigrationsConfig.SysDbVersion);
                }

                migrator.MigrationsConfig.DbVersion = "0.0.0";
                migrator.SaveConfig();
            }

        }

    }

}
