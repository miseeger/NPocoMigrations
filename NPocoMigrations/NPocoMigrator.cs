using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using NPoco;
using NPocoMigrations.Model;

namespace NPocoMigrations
{

    public class NPocoMigrator : IMigrator
    {

        private bool _isDisposed;

        public MigrationsConfig MigrationsConfig { get; set; }
        public List<Migrations> MigrationsList { get; set; }
        public string MigrationsBaseDir { get; }
        public string MigrationsConfigFilePath { get; }
        public string MigrationsScriptFilePath { get; private set; }


        public NPocoMigrator(string migrationsBaseDir)
        {
            MigrationsList = new List<Migrations>();
            MigrationsBaseDir = migrationsBaseDir;
            MigrationsConfigFilePath = $"{MigrationsBaseDir}\\{NPocoMigrationsConstants.MigrationsConfigFile}";
        }


        public NPocoMigrator() : this(AppDomain.CurrentDomain.BaseDirectory)
        {
        }


        ~NPocoMigrator()
        {
            Dispose(false);
        }


        public bool LoadConfig()
        {
            // ----- Initialize default logger
            var nLogCconfig = new LoggingConfiguration();

            var migratorFileTarget = new FileTarget
                {
                    FileName = $"{MigrationsBaseDir}\\migrator.log",
                    Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss} " +
                        @"${pad:padding=5:inner=${level:uppercase=true}} ${message}"
            };

            nLogCconfig.AddTarget("file", migratorFileTarget);
            nLogCconfig.LoggingRules.Add(
                new LoggingRule("Migrator", LogLevel.Info, migratorFileTarget));

            LogManager.Configuration = nLogCconfig;

            // ----- Load migrations configuration
            if (File.Exists(MigrationsConfigFilePath))
            {
                MigrationsConfig = JsonConvert.DeserializeObject<MigrationsConfig>(
                    File.ReadAllText(MigrationsConfigFilePath));
                MigrationsScriptFilePath = $"{MigrationsBaseDir}\\{MigrationsConfig.MigrationsDir}";

            }
            else
            {
                LogManager.GetLogger("Migrator")
                    .Error($"Config file not found ({MigrationsConfigFilePath}).");
                return false;
            }

            // ----- Initialize migration logger and re-assign NLog config
            var migrationsFileTarget = new FileTarget
            {
                FileName = $"{MigrationsScriptFilePath}\\migrations.log",
                Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss} ${pad:padding=5:inner=${level:uppercase=true}} ${message}"
            };

            var migrationsRule = new LoggingRule("Migration", LogLevel.Debug, migrationsFileTarget);

            nLogCconfig.AddTarget("file", migrationsFileTarget);
            nLogCconfig.LoggingRules.Add(migrationsRule);

            LogManager.Configuration = nLogCconfig;

            return true;
        }


        public void SaveConfig()
        {
            File.WriteAllText(MigrationsConfigFilePath,
                JsonConvert.SerializeObject(MigrationsConfig, Formatting.Indented));
        }


        public bool LoadMigrations()
        {
            var logger = LogManager.GetLogger("Migrator");
            var migrationFiles = Directory.GetFiles(MigrationsScriptFilePath, "*.json");
            MigrationsList.Clear();

            foreach (string migrationsFile in migrationFiles)
            {
                try
                {
                    var migration = JsonConvert.DeserializeObject<Migrations>(
                        File.ReadAllText(migrationsFile));
                    if (MigrationsConfig.SysDbVersion.CompareTo(migration.SysVersion) == -1)
                    {
                        MigrationsList.Add(migration);
                        logger.Info($"{Path.GetFileName(migrationsFile)} ({migration.Description}) loaded.");
                    }

                }
                catch (Exception e)
                {
                    logger.Error($"Loading {Path.GetFileName(migrationsFile)} - {e.Message} ({e.StackTrace})");
                    return false;
                }
            }

            return true;
        }


        public bool ExecuteMigrations()
        {
            var migrationsToUse = MigrationsList.OrderBy(x => x.SysVersion).ToList();
            var migLogger = LogManager.GetLogger("Migrator");

            if (!MigrationsList.Any())
            {
                migLogger.Info($"No migrations to execute. Current database version is {MigrationsConfig.DbVersion}.");
                return true;
            }

            using (var db = new Database(MigrationsConfig.DbConnection))
            {
                var logger = LogManager.GetLogger("Migration");
                var targetMigration = migrationsToUse.LastOrDefault();

                migLogger.Info($"Migrating from current database version {MigrationsConfig.DbVersion} to {targetMigration.Version}");

                foreach (var migrations in migrationsToUse)
                {
                    using (var transaction = db.GetTransaction())
                    {
                        logger.Info($"Migration {migrations.Version} started.");

                        try
                        {
                            foreach (var task in migrations.Tasks)
                            {
                                db.Execute(task.Execute);

                                if (task.Test != null)
                                {
                                    var result = db.ExecuteScalar<int>(task.Test);

                                    if (result != 1)
                                    {
                                        logger.Error($"Migration {migrations.Version} - Test failed! ({task.Test}) = {result})");
                                        transaction.Dispose();
                                        logger.Info($"Migration {migrations.Version} aborted and rolled back.");
                                        migLogger.Info($"Migration process aborted on failed Test. The current database version is {MigrationsConfig.DbVersion}");
                                        return false;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Migration {migrations.Version} - {e.Message} ({e.StackTrace})");
                            transaction.Dispose();
                            logger.Info($"Migration {migrations.Version} aborted and rolled back.");
                            migLogger.Info($"Migration process aborted on failed Task. The current database version is {MigrationsConfig.DbVersion}");

                            return false;
                        }

                        transaction.Complete();

                        MigrationsConfig.DbVersion = migrations.Version;

                        try
                        {
                            SaveConfig();
                        }
                        catch (Exception e)
                        {
                            logger.Info($"Migration {migrations.Version} completed but database version " +
                                $"could not be updated in {MigrationsConfigFilePath}.\r\n" +
                                $"{e.Message}\r\n{e.StackTrace}");
                            migLogger.Info($"Migration {migrations.Version} completed but {MigrationsConfigFilePath} could not be updated.");

                            return false;
                        }

                        logger.Info($"Migration {migrations.Version} completed. Database version updated.");
                        migLogger.Info($"Migration {migrations.Version} successfully completed.");
                    }
                }

                migLogger.Info($"Migration successfully completed. The current database version is {MigrationsConfig.DbVersion}");

                return true;
            }

        }


        public bool Migrate()
        {
            return LoadConfig() && LoadMigrations() && ExecuteMigrations();
        }


        // ----- Implement IDisposable ----------------------------------------

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // dispose IDisposable members
            }

            MigrationsList = null;
            MigrationsConfig = null;

            _isDisposed = true;
        }

    }

}