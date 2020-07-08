using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCatch
{
    public class ConfigMiddleLogs
    {
        public string path { get; set; }
        public List<string> dbItems { get; set; }
        public List<string> rabbitItems { get; set; }
        public List<string> httpItems { get; set; }
        public List<string> codisItems { get; set; }
        public List<string> kylinItems { get; set; }
        public List<string> hbaseItems { get; set; }
        public List<string> ftpItems { get; set; }
        public string gitPath { get; set; }
    }
}
