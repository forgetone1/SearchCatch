using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SearchCatch
{
    public class ConfigMiddle
    {
        public void run()
        {
            Console.WriteLine("开始执行");
            var files = FileHelper.getDictories(@"D:\projects\");
            List<ConfigMiddleLogs> logs = new List<ConfigMiddleLogs>();
            foreach (var file in files)
            {
                var fileLow = file.ToLower();

                if (onlineMatch(fileLow) && configMatch(fileLow))
                {
                    var item = midleCompent(fileLow);
                    if (item != null)
                    {
                        logs.Add(item);
                    }
                }
            }
            saveLogs(logs);
            Console.WriteLine("执行完成");
            Console.ReadLine();
        }

        private void saveLogs(List<ConfigMiddleLogs> logs)
        {
            var fs = new FileStream(@"d:\tmp\middle.txt", FileMode.OpenOrCreate);
            var sw = new StreamWriter(fs);
            foreach (var item in logs)
            {

                sw.WriteLine(string.Format("{0}\r\n{1}\r\n{2}", item.path, item.Items[0], item.Items[1]));
            }
            sw.Close();
        }

        private ConfigMiddleLogs midleCompent(string filepath)
        {
            var text = FileHelper.getFileText(filepath);
            var redis = redisGet(text);
            var rabbitmq = rabbitmqGet(text);


            if (!string.IsNullOrEmpty(redis) || !string.IsNullOrEmpty(rabbitmq))
            {
                ConfigMiddleLogs configMiddleLogs = new ConfigMiddleLogs();
                configMiddleLogs.path = filepath;
                configMiddleLogs.Items = new List<string>() { redis, rabbitmq };
                return configMiddleLogs;
            }
            return null;
        }

        private bool configMatch(string filepath)
        {
            return filepath.Contains("web.config") || filepath.Contains("application-online");
        }

        private bool onlineMatch(string filepath)
        {
            return filepath.Contains("online") || filepath.Contains("pmo");
        }

        private string redisGet(string text)
        {
            var pattern = "redis|codis";
            if (Regex.IsMatch(text, pattern))
            {
                return Regex.Match(text, pattern).Groups[1].Value;
            }
            return "";
        }

        private string rabbitmqGet(string text)
        {
            var pattern = "rabbitmq|mq|amqp|:5672|S7!CUy3lZ5";
            if (Regex.IsMatch(text, pattern))
            {
                return Regex.Match(text, pattern).Groups[0].Value;
            }
            return "";
        }
    }
}
