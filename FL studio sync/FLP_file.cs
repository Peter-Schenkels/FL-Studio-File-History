using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FL_studio_sync
{
    class History
    {
        public History(string file_name)
        {
            name = file_name + " - " + DateTime.Now.ToString();
            date_of_file = DateTime.Now;
        }
        public string name { get; }
        public DateTime date_of_file { get; }
    }
    class FLP_file
    {
        public ulong hash { get; set; }
        public string filename { get; set; }
        public string filepath { get; set; }
        public List<History> history = new List<History>();
    }

    class Application
    {
        public Application() { }
        public string dir { get; set; } = null;
        public List<string> directories { get; set; } = new List<string>();
        public List<FLP_file> files { get; set; } = new List<FLP_file>();
        public string destination { get; set; } = null;
    }
}
