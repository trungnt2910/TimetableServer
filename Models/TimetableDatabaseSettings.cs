using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimetableServer.Models
{
    public class TimetableDatabaseSettings : ITimetableDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface ITimetableDatabaseSettings
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
