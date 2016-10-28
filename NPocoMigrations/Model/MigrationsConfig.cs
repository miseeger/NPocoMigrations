using System;
using Newtonsoft.Json;

namespace NPocoMigrations
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MigrationsConfig
    {

        private string _dbVersion;

        [JsonProperty]
        public string DbVersion
        {
            get { return _dbVersion; }
            set
            {
                _dbVersion = value;
                SysDbVersion = new Version(value);
            }

        }

        [JsonProperty]
        public string DbConnection { get; set; }

        public Version SysDbVersion { get; private set; }

        [JsonProperty]
        public string MigrationsDir { get; set; }
    }

}