using IKXTServer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KXTServiceDBServer.Files
{
    internal class KXTUserInfoFile
    {
        public bool Invaild;
        public DateTime VisitTime;

        private readonly KXTJson Json;

        public KXTUserInfoFile(string path)
        {
            Json = new KXTJson(path);

            Invaild = false;
            VisitTime = DateTime.Now;
        }

        public void ReadFriends(out string[] friends)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            List<string> list = new List<string>();

            JArray array = Json["friends"] as JArray;
            foreach (var item in array)
                list.Add(item.ToString());

            friends = list.ToArray();
        }
        public void ReadGroups(out string[] groups)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            List<string> list = new List<string>();

            JArray array = Json["groups"] as JArray;
            foreach (var item in array)
                list.Add(item.ToString());

            groups = list.ToArray();
        }

        public void AddFriend(string friend)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            JArray array = Json["friends"] as JArray;

            foreach (var item in array)
                if (item.ToString() == friend)
                    return;

            array.Add(friend);

            Flush();
        }
        public void AddGroup(string group)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            JArray array = Json["groups"] as JArray;

            foreach (var item in array)
                if (item.ToString() == group)
                    return;

            array.Add(group);

            Flush();
        }

        public void DelFriend(string friend)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            JArray array = Json["friends"] as JArray;

            for (int i = 0; i < array.Count; ++i) 
                if (array[i].ToString() == friend)
                {
                    array.RemoveAt(i);
                    Flush();
                }
        }
        public void DelGroup(string group)
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            JArray array = Json["groups"] as JArray;

            for (int i = 0; i < array.Count; ++i) 
                if (array[i].ToString() == group)
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
    public class KXTUserInfoReader
    {
        private readonly string RootPath;
        private readonly Action<LogLevel, string> Notify;

        private readonly ConcurrentDictionary<string, KXTUserInfoFile> Cache;
        private readonly System.Timers.Timer CacheTime;

        public KXTUserInfoReader(string root, Action<LogLevel, string> notify)
        {
            RootPath = root;
            Notify = notify;

            Cache = new ConcurrentDictionary<string, KXTUserInfoFile>();

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

        public bool CreateUser(string user_id)
        {
            try
            {
                return KXTJson.CreateFile
               (
               RootPath + "\\" + user_id + ".json",
               new KeyValuePair<string, KXTRootJsonType>[]
               {
                    new KeyValuePair<string, KXTRootJsonType>
                    (
                        "friends",
                        KXTRootJsonType.Array
                    ),
                    new KeyValuePair<string, KXTRootJsonType>
                    (
                        "groups",
                        KXTRootJsonType.Array
                    )
               }
               );
            }
            catch
            {
                Notify(LogLevel.Warning, "用户信息数据操作异常：创建文件异常");
                return false;
            }
        }
        public string[] ReadFriends(string user_id)
        {
            try
            {
                if (!Cache.TryGetValue(user_id, out KXTUserInfoFile file))
                {
                    file = new KXTUserInfoFile
                        (
                        RootPath + "\\" + user_id + ".json"
                        );
                    Cache.TryAdd(user_id, file);
                }

                file.ReadFriends(out string[] friends);

                return friends;
            }
            catch
            {
                Notify(LogLevel.Warning, "用户信息数据操作异常：文件异常");
                return new string[0];
            }
        }
        public string[] ReadGroups(string user_id)
        {
            try
            {
                if (!Cache.TryGetValue(user_id, out KXTUserInfoFile file))
                {
                    file = new KXTUserInfoFile
                        (
                        RootPath + "\\" + user_id + ".json"
                        );
                    Cache.TryAdd(user_id, file);
                }

                file.ReadGroups(out string[] friends);

                return friends;
            }
            catch
            {
                Notify(LogLevel.Warning, "用户信息数据操作异常：文件异常");
                return new string[0];
            }
        }
        public void AddFriend(string user_id, string friend)
        {
            try
            {
                if (!Cache.TryGetValue(user_id, out KXTUserInfoFile file))
                {
                    file = new KXTUserInfoFile
                        (
                        RootPath + "\\" + user_id + ".json"
                        );
                    Cache.TryAdd(user_id, file);
                }

                file.AddFriend(friend);
            }
            catch
            {
                Notify(LogLevel.Warning, "用户信息数据操作异常：文件异常");
            }
        } 
        public void AddGroup(string user_id, string group)
        {
            try
            {
                if (!Cache.TryGetValue(user_id, out KXTUserInfoFile file))
                {
                    file = new KXTUserInfoFile
                        (
                        RootPath + "\\" + user_id + ".json"
                        );
                    Cache.TryAdd(user_id, file);
                }

                file.AddGroup(group);
            }
            catch
            {
                Notify(LogLevel.Warning, "用户信息数据操作异常：文件异常");
            }
        } 
        public void DelFriend(string user_id, string friend)
        {
            try
            {
                if (!Cache.TryGetValue(user_id, out KXTUserInfoFile file))
                {
                    file = new KXTUserInfoFile
                        (
                        RootPath + "\\" + user_id + ".json"
                        );
                    Cache.TryAdd(user_id, file);
                }

                file.DelFriend(friend);
            }
            catch
            {
                Notify(LogLevel.Warning, "用户信息数据操作异常：文件异常");
            }
        } 
        public void DelGroup(string user_id, string group)
        {
            try
            {
                if (!Cache.TryGetValue(user_id, out KXTUserInfoFile file))
                {
                    file = new KXTUserInfoFile
                        (
                        RootPath + "\\" + user_id + ".json"
                        );
                    Cache.TryAdd(user_id, file);
                }

                file.DelGroup(group);
            }
            catch
            {
                Notify(LogLevel.Warning, "用户信息数据操作异常：文件异常");
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
