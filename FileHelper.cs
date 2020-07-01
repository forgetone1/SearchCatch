using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SearchCatch
{
    public class FileHelper
    {
        public static List<string> getDictories(string path)
        {
            var paths = System.IO.Directory.GetDirectories(path);
            List<string> ret = new List<string>();
            getfiles(path, ret);

            foreach (var item in paths)
            {
                ret.AddRange(getDictories(item));
            }
            return ret;
        }

        public static void getfiles(string path, List<string> ret)
        {
            var list = System.IO.Directory.GetFiles(path);
            if (list != null || list.Count() > 0)
            {
                ret.AddRange(list);
            }
        }

        public static string getFileText(string filepath)
        {
            return File.ReadAllText(filepath);
        }
    }
}
