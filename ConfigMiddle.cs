using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SearchCatch
{
    public class ConfigMiddle
    {
        public void run()
        {
            Console.WriteLine("开始执行");
            var projectPaths = FileHelper.getDirectoiesFirst(@"D:\projects\autohome");

            foreach (var project in projectPaths)
            {
                var projectPath = project.ToLower();
                analysisProject(projectPath);
            }
            Console.WriteLine("执行完成");
            Console.ReadLine();
        }

        private void analysisProject(string projectPath)
        {
            ConfigMiddleLogs configMiddleLogs = new ConfigMiddleLogs();
            var allFiles = FileHelper.getDictories(projectPath);
            var configPaths = allFiles.Where(x => (x.IndexOf(".config", StringComparison.OrdinalIgnoreCase) >= 0 && x.IndexOf("online", StringComparison.OrdinalIgnoreCase) >= 0)
            || x.IndexOf("application-online.properties", StringComparison.OrdinalIgnoreCase) >= 0
            || x.IndexOf("application-online.yml", StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();
            configMiddleLogs.gitPath = gitAddress(allFiles);


            foreach (var config in configPaths)
            {
                configMiddleLogs.path = config;
                configMiddleLogs.dbItems = getConfigs(config, ConfigType.db);
                configMiddleLogs.rabbitItems = getConfigs(config, ConfigType.rabbitMq);
                configMiddleLogs.httpItems = getConfigs(config, ConfigType.http);
                configMiddleLogs.codisItems = getConfigs(config, ConfigType.codis);
                configMiddleLogs.kylinItems = getConfigs(config, ConfigType.kylin);
                configMiddleLogs.hbaseItems = getConfigs(config, ConfigType.hbase);
                configMiddleLogs.ftpItems = getConfigs(config, ConfigType.ftp);
            }
            saveLogs(configMiddleLogs);
        }

        private string gitAddress(IList<string> filepath)
        {
            var tmp = filepath.Where(x => x.IndexOf(@".git\config", StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();
            var match = Regex.Match(FileHelper.getFileText(tmp), "(https://.*)");
            if (match != null && match.Groups.Count == 2)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }


        private void saveLogs(ConfigMiddleLogs log)
        {
            var fs = new FileStream(@"d:\tmp\middle.txt", FileMode.OpenOrCreate | FileMode.Append);
            var sw = new StreamWriter(fs);
            saveItems(sw, log.codisItems, log.gitPath);
            saveItems(sw, log.rabbitItems, log.gitPath);
            saveItems(sw, log.httpItems, log.gitPath);
            saveItems(sw, log.dbItems, log.gitPath);
            saveItems(sw, log.ftpItems, log.gitPath);
            saveItems(sw, log.kylinItems, log.gitPath);
            saveItems(sw, log.hbaseItems, log.gitPath);
            sw.WriteLine();
            sw.Close();
        }

        private void saveItems(StreamWriter sw, List<string> list, string gitpath)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    sw.WriteLine("{0}\t{1}", gitpath, item);
                }
            }
        }



        private List<string> getConfigs(string filePath, ConfigType type)
        {
            List<DbConfig> configs = new List<DbConfig>();
            if (filePath.IndexOf(".config", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return getDotnetconfig(filePath, type);
            }
            else if (filePath.IndexOf("application-online.properties", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return getSpringProconfig(filePath, type);
            }
            else if (filePath.IndexOf("application-online.yml", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return getSpringbootconfig(filePath, type);
            }
            return null;
        }

        #region dotnet

        private List<string> getDotnetconfig(string filepath, ConfigType type)
        {
            var doc = new XmlDocument();
            doc.Load(filepath);
            if (type == ConfigType.db)
            {
                return getDotNetDbConfig(doc);
            }
            else if (type == ConfigType.codis)
            {
                return getDotnetCodis(doc);
            }
            else if (type == ConfigType.http)
            {
                return getDotNethttp(doc);
            }
            else if (type == ConfigType.rabbitMq)
            {
                return getDotNetRabbitmq(doc);
            }
            return null;
        }

        private List<string> getDotnetCodis(XmlDocument document)
        {
            var list = document.SelectNodes("//configuration/appSettings/add/@value");
            List<string> ret = new List<string>();
            if (list != null)
            {
                foreach (XmlNode item in list)
                {
                    if (item.Value.IndexOf("codis", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ret.Add(item.Value);
                    }
                }
            }

            return ret;
        }

        private List<string> getDotNetDbConfig(XmlDocument document)
        {
            var list = document.SelectNodes("//configuration/connectionStrings/add/@connectionString");
            List<string> ret = new List<string>();
            if (list != null)
            {
                foreach (XmlNode item in list)
                {
                    SqlConnection sqlConnection = new SqlConnection(item.Value);
                    ret.Add(string.Format("{0}\t{1}", sqlConnection.DataSource, sqlConnection.Database));
                }
            }
            return ret;
        }

        private List<string> getDotNethttp(XmlDocument document)
        {
            var list = document.SelectNodes("//configuration/appSettings/add/@value");
            List<string> ret = new List<string>();
            if (list != null)
            {
                foreach (XmlNode item in list)
                {
                    if (item.Value.IndexOf("http", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ret.Add(item.Value);
                    }
                }
            }
            return ret;
        }

        private List<string> getDotNetRabbitmq(XmlDocument document)
        {
            var list = document.SelectNodes("//configuration/appSettings/add/@value");
            List<string> ret = new List<string>();

            if (list != null)
            {
                foreach (XmlNode item in list)
                {
                    if (item.Value.IndexOf("amqp", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ret.Add(item.Value);
                    }
                }
            }

            list = document.SelectNodes("//configuration/lightRabbitMQ/mqs/add");
            if (list != null)
            {
                foreach (XmlNode item in list)
                {
                    string amqp = string.Format("amqp://{0}:{1}@{2}:{3}/{4} {5}", item.Attributes["userName"].Value
                        , item.Attributes["password"].Value
                        , item.Attributes["host"].Value
                        , item.Attributes["port"].Value
                        , item.Attributes["virtualHost"].Value
                        , item.Attributes["queueName"].Value);
                    ret.Add(amqp);
                }
            }
            return ret;
        }
        #endregion

        #region springProperties

        private List<string> getSpringProValues(string prev, string filePath)
        {
            var ret = new List<string>();
            var list = FileHelper.getfileRows(filePath);

            var matchs = prev == "" ? list : list.Where(x => x.IndexOf(prev, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            foreach (var item in matchs)
            {
                var index = item.IndexOf('=');
                if (index >= 0)
                {
                    ret.Add(item.Substring(index + 1));
                }
            }
            return ret;
        }


        private List<string> getSpringProconfig(string filepath, ConfigType type)
        {
            if (type == ConfigType.db)
            {
                return getSpringProDbConfig(filepath);
            }
            else if (type == ConfigType.codis)
            {
                return getSpringProCodis(filepath);
            }
            else if (type == ConfigType.http)
            {
                return getSpringProhttp(filepath);
            }
            else if (type == ConfigType.rabbitMq)
            {
                return getSpringProRabbitmq(filepath);
            }
            return null;
        }

        private List<string> getSpringProCodis(string filePath)
        {
            List<string> ret = new List<string>();
            var host = getSpringProValues("spring.redis.host", filePath).FirstOrDefault();
            var port = getSpringProValues("spring.redis.port", filePath).FirstOrDefault();
            ret.Add(string.Format("{0}:{1}", host, port));
            return ret;
        }

        private List<string> getSpringProDbConfig(string filePath)
        {
            List<string> ret = new List<string>();

            var db = getSpringProValues("spring.datasource.druid", filePath).Where(x => x.IndexOf("jdbc", StringComparison.OrdinalIgnoreCase) >= 0
            && x.IndexOf("Driver", StringComparison.OrdinalIgnoreCase) < 0).ToList();
            ret.AddRange(db);
            return ret;
        }

        private List<string> getSpringProhttp(string filePath)
        {
            List<string> ret = new List<string>();
            var http = getSpringProValues("", filePath).Where(x => x.IndexOf("http", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            ret.AddRange(http);
            return ret;
        }

        private List<string> getSpringProRabbitmq(string filePath)
        {
            List<string> ret = new List<string>();
            var host = getSpringProValues("spring.rabbitmq.host", filePath).FirstOrDefault();
            var port = getSpringProValues("spring.rabbitmq.port", filePath).FirstOrDefault();
            ret.Add(string.Format("{0}:{1}", host, port));
            return ret;
        }
        #endregion

        #region springPropertiesYaml

        private List<string> getSpringbootValues(string prev, string filePath)
        {
            var ret = new List<string>();
            var list = FileHelper.getfileRows(filePath);

            var matchs = prev == "" ? list : list.Where(x => x.IndexOf(prev, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            foreach (var item in matchs)
            {
                var index = item.IndexOf(':');
                if (index >= 0)
                {
                    ret.Add(item.Substring(index + 1));
                }
            }
            return ret;
        }


        private List<string> getSpringbootconfig(string filepath, ConfigType type)
        {
            if (type == ConfigType.db)
            {
                return getSpringbootDbConfig(filepath);
            }
            else if (type == ConfigType.codis)
            {
                return getSpringbootCodis(filepath);
            }
            else if (type == ConfigType.http)
            {
                return getSpringboothttp(filepath);
            }
            else if (type == ConfigType.rabbitMq)
            {
                return getSpringbootRabbitmq(filepath);
            }
            return null;
        }

        private List<string> getSpringbootCodis(string filePath)
        {
            List<string> ret = new List<string>();
            var list = getSpringbootValues("", filePath).Where(x => x.IndexOf("codis", StringComparison.OrdinalIgnoreCase) >= 0);
            foreach (var item in list)
            {
                ret.Add(item);
            }
            return ret;
        }

        private List<string> getSpringbootDbConfig(string filePath)
        {
            List<string> ret = new List<string>();

            var db = getSpringbootValues("", filePath).Where(x => x.IndexOf("jdbc", StringComparison.OrdinalIgnoreCase) >= 0
            && x.IndexOf("Driver", StringComparison.OrdinalIgnoreCase) < 0).ToList();
            ret.AddRange(db);
            return ret;
        }

        private List<string> getSpringboothttp(string filePath)
        {
            List<string> ret = new List<string>();
            var http = getSpringbootValues("", filePath).Where(x => x.IndexOf("http", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            ret.AddRange(http);
            return ret;
        }

        private List<string> getSpringbootRabbitmq(string filePath)
        {
            List<string> ret = new List<string>();
            var list = getSpringbootValues("", filePath).Where(x => x.IndexOf("mq") >= 0);
            ret.AddRange(list);
            return ret;
        }
        #endregion

    }
}
