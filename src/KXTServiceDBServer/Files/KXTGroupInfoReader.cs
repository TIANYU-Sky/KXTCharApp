using IKXTServer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KXTServiceDBServer.Files
{
    internal class KXTGroupInfoFile
    {
        public bool Invaild;
        public DateTime VisitTime;

        private readonly KXTJson Json;

        public KXTGroupInfoFile(string path)
        {
            Json = new KXTJson(path);

            Invaild = false;
            VisitTime = DateTime.Now;
        }

        public void ReadMembers(out string[] members)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            List<string> list = new List<string>();

            JArray array = Json["member"] as JArray;
            foreach (var item in array)
                list.Add(item.ToString());

            members = list.ToArray();
        }
        public void AddMember(string member)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            JArray array = Json["member"] as JArray;

            foreach (var item in array)
                if (item.ToString() == member)
                    return;

            array.Add(member);

            Flush();
        }
        public void DelMember(string member)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            JArray array = Json["member"] as JArray;

            for (int i = 0; i < array.Count; ++i)
                if (array[i].ToString() == member)
                {
                    array.RemoveAt(i);
                    Flush();
                }
        }

        public void Flush()
        {
            Json.Write();
        }
        public void Close()
        {
            Flush();
        }
    }

    public class KXTGroupInfoReader
    {
        private readonly string RootPath;
        private readonly Action<LogLevel, string> Notify;

        private readonly ConcurrentDictionary<string, KXTGroupInfoFile> Cache;
        private readonly System.Timers.Timer CacheTime;

        public KXTGroupInfoReader(string root, Action<LogLevel, string> notify)
        {
            RootPath = root;
            Notify = notify;

            Cache = new ConcurrentDictionary<string, KXTGroupInfoFile>();

            CacheTime = new System.Timers.Timer
            {
                AutoReset = false,
                Interval = CacheTimeInterval
            };
            CacheTime.Elapsed += CacheScan_Trigger;
            CacheTime.Start();
        }

        public void Close()
        {
            CacheTime.Stop();
            foreach (var item in Cache)
                item.Value.Close();
        }

        public bool CreateGroup(string group_id)
        {
            try
            {
                return KXTJson.CreateFile
               (
               RootPath + "\\" + group_id + ".json",
               new KeyValuePair<string, KXTRootJsonType>[]
               {
                    new KeyValuePair<string, KXTRootJsonType>
                    (
                        "member",
                        KXTRootJsonType.Array
                    )
               }
               );
            }
            catch
            {
                Notify(LogLevel.Warning, "群组信息数据操作异常：创建文件异常");
                return false;
            }
        }
        public string[] ReadMembers(string group_id)
        {
            try
            {
                if (!Cache.TryGetValue(group_id, out KXTGroupInfoFile file))
                {
                    file = new KXTGroupInfoFile
                        (
                        RootPath + "\\" + group_id + ".json"
                        );
                    Cache.TryAdd(group_id, file);
                }

                file.ReadMembers(out string[] friends);

                return friends;
            }
            catch
            {
                Notify(LogLevel.Warning, "群组信息数据操作异常：文件异常");
                return new string[0];
            }
        }
        public void AddMember(string group_id, string member)
        {
            try
            {
                if (!Cache.TryGetValue(group_id, out KXTGroupInfoFile file))
                {
                    file = new KXTGroupInfoFile
                        (
                        RootPath + "\\" + group_id + ".json"
                        );
                    Cache.TryAdd(group_id, file);
                }

                file.AddMember(member);
            }
            catch
            {
                Notify(LogLevel.Warning, "群组信息数据操作异常：文件异常");
            }
        }
        public void DelMember(string group_id, string member)
        {
            try
            {
                if (!Cache.TryGetValue(group_id, out KXTGroupInfoFile file))
                {
                    file = new KXTGroupInfoFile
                        (
                        RootPath + "\\" + group_id + ".json"
                        );
                    Cache.TryAdd(group_id, file);
                }

                file.DelMember(member);
            }
            catch
            {
                Notify(LogLevel.Warning, "群组信息数据操作异常：文件异常");
            }
        }
        public void DelGroup(string group_id)
        {
            try
            {
                File.Delete(RootPath + "\\" + group_id + ".json");
            }
            catch
            {
                Notify(LogLevel.Warning, "群组信息数据操作异常：移除文件异常（" + RootPath + "\\" + group_id + ".json" + "）");
            }
        }

        private void CacheScan_Trigger(object sender, ElapsedEventArgs e)
        {
            foreach (var item in Cache)
            {
                if (item.Value.Invaild)
                {
                    Cache.TryRemove(item.Key, out _);
                    item.Value.Close();
                }
                else if ((DateTime.Now - item.Value.VisitTime).TotalMilliseconds > CacheTimeInterval)
                    item.Value.Invaild = true;
            }

            CacheTime.Start();
        }

        private const double CacheTimeInterval = 600000;
    }
}
