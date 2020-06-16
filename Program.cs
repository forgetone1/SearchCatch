using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SearchCatch
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = getNoError(@"D:\projects\");
            save(file);
        }

        static void save(List<string> filepath)
        {
            var fs = new FileStream(@"d:\tmp\noerror.txt", FileMode.OpenOrCreate);
            var sw = new StreamWriter(fs);
            foreach (var item in filepath)
            {
                sw.WriteLine(item);
            }
            sw.Close();
        }

        static List<string> getNoError(string path)
        {
            var files = getDictories(path);
            Console.WriteLine(string.Format("总共{0}条。", files.Count));
            var ret = new List<string>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                Console.WriteLine(string.Format("当前执行的文件：{0}", i));
                if (isNotError(file))
                {
                    ret.Add(file);
                }
            }

            return ret;
        }

        static bool isNotError(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Extension.Equals(".java") || fileInfo.Extension.Equals(".cs"))
            {
                var str = File.ReadAllText(filePath);
                var list = getCatchs(str);
                foreach (var item in list)
                {
                    if (item.IndexOf("error") < 0 && item.IndexOf("throw") < 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static List<string> getCatchs(string txt)
        {
            var cpList = new CatchMatch(txt).getCatchesText();
            return cpList;
        }



        static List<string> getDictories(string path)
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

        static void getfiles(string path, List<string> ret)
        {
            var list = System.IO.Directory.GetFiles(path);
            if (list != null || list.Count() > 0)
            {
                ret.AddRange(list);
            }
        }
    }
}
