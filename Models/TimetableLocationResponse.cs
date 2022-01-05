using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimetableServer.Models
{
    public class TimetableLocationResponse
    {
        public string MD5 { get; set; }

        public string SHA512 { get; set; }

        public string Location { get; set; }
    }
}
