using System;

namespace NPocoMigrations
{

    public interface INPocoMigrator
    {
        MigrationsConfig MigrationsConfig { get; set; }

        bool LoadConfig();
        void SaveConfig();
        bool LoadMigrations();
        bool ExecuteMigrations();
        bool Migrate();
    }

}