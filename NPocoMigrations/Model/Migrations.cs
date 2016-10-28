using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NPocoMigrations.Model
{

    public class Migrations
    {
        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                SysVersion = new Version(value);
            }

        }

        public Version SysVersion { get; private set; }
        public string Description { get; set; }
        public List<Task> Tasks { get; set; }
    }

}