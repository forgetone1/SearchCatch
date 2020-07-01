using System;
using System.Collections.Generic;
using System.IO;

namespace SearchCatch
{
    public class CatchNoError
    {

        public void run()
        {
            var file = getNoError(@"D:\projects\");
            save(file);
        }


        public void save(List<string> filepath)
        {
            var fs = new FileStream(@"d:\tmp\noerror.txt", FileMode.OpenOrCreate);
            var sw = new StreamWriter(fs);
            foreach (var item in filepath)
            {
                sw.WriteLine(item);
            }
            sw.Close();
        }

        public List<string> getNoError(string path)
        {
            var files = FileHelper.getDictories(path);
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

        public bool isNotError(string filePath)
        {
            if (filePath.ToLower().IndexOf("test") >= 0)
            {
                return false;
            }
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Extension.Equals(".java") || fileInfo.Extension.Equals(".cs"))
            {
                var str = File.ReadAllText(filePath);
                var list = getCatchs(str);
                foreach (var item in list)
                {
                    if (item.ToLower().IndexOf("error") < 0
                        && item.ToLower().IndexOf("throw") < 0
                        && item.ToLower().IndexOf("logs.write") < 0
                        && item.ToLower().IndexOf("filehelper.append") < 0
                        && item.ToLower().IndexOf("executeerr") < 0
                        && item.ToLower().IndexOf("errormanage.saveerror") < 0
                        && item.ToLower().IndexOf("monitorhelp.sendmonitor") < 0
                        && item.ToLower().IndexOf("_monitor.senderror") < 0
                        && item.ToLower().IndexOf("handleexternalerror") < 0
                        && item.ToLower().IndexOf("handleinternalerror") < 0
                        && item.ToLower().IndexOf("handleexception") < 0
                        && item.ToLower().IndexOf("writeerror") < 0
                        && item.ToLower().IndexOf("fatal") < 0
                        && item.ToLower().IndexOf("orderexception") < 0
                        && item.ToLower().IndexOf("ywmonitordealernotexist") < 0
                        && item.ToLower().IndexOf("webactionlog") < 0
                        && item.ToLower().IndexOf("logbyfilenamefatal") < 0
                           && item.ToLower().IndexOf("apirequesterror") < 0
                           && item.ToLower().IndexOf("rejectmessageandsleep") < 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<string> getCatchs(string txt)
        {
            var cpList = new CatchMatch(txt).getCatchesText();
            return cpList;
        }
    }
}
