using System;
using System.Collections.Generic;
using NPocoMigrations.Model;

namespace NPocoMigrations
{

    public interface IMigrator
    {
        MigrationsConfig MigrationsConfig { get; set; }
        List<Migrations> MigrationsList { get; set; }
        string MigrationsBaseDir { get; }
        string MigrationsConfigFilePath { get; }
        string MigrationsScriptFilePath { get; }

        bool LoadConfig();
        void SaveConfig();
        bool LoadMigrations();
        bool ExecuteMigrations();
        bool Migrate();
    }

}