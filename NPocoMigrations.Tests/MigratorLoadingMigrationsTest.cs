using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPocoMigrations.Model;

namespace NPocoMigrations.Tests
{
    [TestClass]
    public class MigratorLoadingMigrationsTest
    {

        private static NPocoMigrator _migrator;
        private static bool _loadStatus;

            
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _migrator = new NPocoMigrator();
            _loadStatus = _migrator.LoadConfig() && _migrator.LoadMigrations();
        }


        [TestMethod]
        public void LoadsTwoMigrationsTest()
        {
            Assert.IsTrue(_loadStatus);
            Assert.AreEqual(2, _migrator.MigrationsList.Count);
            Assert.AreEqual("1.0.0", _migrator.MigrationsList[0].Version);
            Assert.AreEqual(new Version("1.0.0"), _migrator.MigrationsList[0].SysVersion);
            Assert.AreEqual("Initial database setup", _migrator.MigrationsList[0].Description);
            Assert.AreEqual("1.0.1", _migrator.MigrationsList[1].Version);
            Assert.AreEqual(new Version("1.0.1"), _migrator.MigrationsList[1].SysVersion);
            Assert.AreEqual("Add Membership Settings and TableColumns-View", _migrator.MigrationsList[1].Description);
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

    }

}
