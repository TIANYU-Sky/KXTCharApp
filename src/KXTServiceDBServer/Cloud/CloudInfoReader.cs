using IKXTServer;
using KXTNetStruct;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KXTServiceDBServer.Cloud
{
    internal class CloudInfoFile
    {
        public bool Invaild;
        public DateTime VisitTime;

        private readonly KXTJson Json;

        public CloudInfoFile(string path)
        {
            Json = new KXTJson(path);

            Invaild = false;
            VisitTime = DateTime.Now;
        }

        public List<CloudFile> GetFiles(string[] paths)
        {
            List<CloudFile> files = new List<CloudFile>();

            try
            {
                JObject obj;

                if (null == paths || 0 == paths.Length)
                    obj = Json["files"] as JObject;
                else
                {
                    obj = Json["dirs"] as JObject;

                    for (int i = 0; i < paths.Length - 1; ++i)
                    {
                        obj = obj[paths[i]] as JObject;
                        obj = obj["dirs"] as JObject;
                    }
                    obj = obj[paths[paths.Length - 1]] as JObject;
                    obj = obj["files"] as JObject;
                }

                foreach (var item in obj)
                {
                    CloudFile file = new CloudFile
                    {
                        SetName = item.Key,
                        Size = item.Value["size"].ToObject<int>()
                    };
                    DateTime.TryParse(item.Value["time"].ToString(), out file.Time);
                    files.Add(file);
                }
            }
            catch
            {
                
            }

            return files;
        }
        public List<CloudFile> GetDirs(string[] paths)
        {
            List<CloudFile> dirs = new List<CloudFile>();

            try
            {
                JObject obj;

                if (null == paths || 0 == paths.Length)
                    obj = Json["dirs"] as JObject;
                else
                {
                    obj = Json["dirs"] as JObject;

                    for (int i = 0; i < paths.Length; ++i)
                    {
                        obj = obj[paths[i]] as JObject;
                        obj = obj["dirs"] as JObject;
                    }
                }

                foreach (var item in obj)
                {
                    CloudFile dir = new CloudFile
                    {
                        SetName = item.Key,
                        Folder = 0x01
                    };
                    DateTime.TryParse(item.Value["time"].ToString(), out dir.Time);
                    dirs.Add(dir);
                }
            }
            catch
            {

            }

            return dirs;
        }

        public void AddFile(string[] paths, string name, int size)
        {
            try
            {
                JObject obj;

                if (null == paths || 0 == paths.Length)
                    obj = Json["files"] as JObject;
                else
                {
                    obj = Json["dirs"] as JObject;

                    for (int i = 0; i < paths.Length - 1; ++i)
                    {
                        obj = obj[paths[i]] as JObject;
                        obj = obj["dirs"] as JObject;
                    }
                    obj = obj[paths[paths.Length - 1]] as JObject;
                    obj = obj["files"] as JObject;
                }

                obj.Add(name, new JObject
                {
                    { "time", DateTime.Now.ToString() },
                    { "size", JsonConvert.SerializeObject(size) }
                });

                Flush();
            }
            catch
            {

            }
        }
        public void AddDir(string[] paths, string name)
        {
            try
            {
                JObject obj;

                if (null == paths || 0 == paths.Length)
                    obj = Json["dirs"] as JObject;
                else
                {
                    obj = Json["dirs"] as JObject;

                    for (int i = 0; i < paths.Length; ++i)
                    {
                        obj = obj[paths[i]] as JObject;
                        obj = obj["dirs"] as JObject;
                    }
                }

                obj.Add(name, new JObject
                {
                    { "time", DateTime.Now.ToString() },
                    { "files", new JObject() },
                    { "dirs", new JObject() }
                });

                Flush();
            }
            catch
            {

            }
        }

        public void DelFile(string[] paths, string name)
        {
            try
            {
                JObject obj;

                if (null == paths || 0 == paths.Length)
                    obj = Json["files"] as JObject;
                else
                {
                    obj = Json["dirs"] as JObject;

                    for (int i = 0; i < paths.Length - 1; ++i)
                    {
                        obj = obj[paths[i]] as JObject;
                        obj = obj["dirs"] as JObject;
                    }
                    obj = obj[paths[paths.Length - 1]] as JObject;
                    obj = obj["files"] as JObject;
                }

                obj.Remove(name);

                Flush();
            }
            catch
            {

            }
        }
        public void DelDir(string[] paths, string name)
        {
            try
            {
                JObject obj;

                if (null == paths || 0 == paths.Length)
                    obj = Json["dirs"] as JObject;
                else
                {
                    obj = Json["dirs"] as JObject;

                    for (int i = 0; i < paths.Length; ++i)
                    {
                        obj = obj[paths[i]] as JObject;
                        obj = obj["dirs"] as JObject;
                    }
                }

                obj.Remove(name);

                Flush();
            }
            catch
            {

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

    public class CloudInfoReader
    {
        private readonly string RootPath;
        private readonly Action<LogLevel, string> Notify;

        private readonly ConcurrentDictionary<string, CloudInfoFile> Cache;
        private readonly System.Timers.Timer CacheTime;

        public CloudInfoReader(string root, Action<LogLevel, string> notify)
        {
            RootPath = root;
            Notify = notify;

            Cache = new ConcurrentDictionary<string, CloudInfoFile>();

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

        public void AddUser(string user_id)
        {
            KXTJson.CreateFile
                (
                RootPath + "\\" + user_id + ".json",
                new KeyValuePair<string, KXTRootJsonType>[]
                {
                    new KeyValuePair<string, KXTRootJsonType>("files", KXTRootJsonType.Object),
                    new KeyValuePair<string, KXTRootJsonType>("dirs", KXTRootJsonType.Object)
                }
                );
        }

        public List<CloudFile> ReadAll(string user, string path)
        {
            List<CloudFile> buffer = new List<CloudFile>();

            if (!Cache.TryGetValue(user, out CloudInfoFile file))
            {
                file = new CloudInfoFile
                    (
                    RootPath + "\\" + user + ".json"
                    );
                Cache.TryAdd(user, file);
            }

            string[] paths = path.Split('\\');
            List<string> actuall_path = new List<string>();
            for (int i = 1; i < paths.Length - 1; ++i)
                actuall_path.Add(paths[i]);
            buffer.AddRange(file.GetFiles(actuall_path.ToArray()));
            buffer.AddRange(file.GetDirs(actuall_path.ToArray()));

            return buffer;
        }
        public void AddFile(string user, string path, string name, int size)
        {
            if (!Cache.TryGetValue(user, out CloudInfoFile file))
            {
                file = new CloudInfoFile
                    (
                    RootPath + "\\" + user + ".json"
                    );
                Cache.TryAdd(user, file);
            }

            string[] paths = path.Split('\\');
            file.AddFile(paths, name, size);
        }
        public void AddDir(string user, string path, string name)
        {
            if (!Cache.TryGetValue(user, out CloudInfoFile file))
            {
                file = new CloudInfoFile
                    (
                    RootPath + "\\" + user + ".json"
                    );
                Cache.TryAdd(user, file);
            }

            string[] paths = path.Split('\\');
            file.AddDir(paths, name);
        }
        public void DelFile(string user, string path, string name)
        {
            if (!Cache.TryGetValue(user, out CloudInfoFile file))
            {
                file = new CloudInfoFile
                    (
                    RootPath + "\\" + user + ".json"
                    );
                Cache.TryAdd(user, file);
            }

            string[] paths = path.Split('\\');
            file.DelFile(paths, name);
        }
        public void DelDir(string user, string path, string name)
        {
            if (!Cache.TryGetValue(user, out CloudInfoFile file))
            {
                file = new CloudInfoFile
                    (
                    RootPath + "\\" + user + ".json"
                    );
                Cache.TryAdd(user, file);
            }

            string[] paths = path.Split('\\');
            file.DelDir(paths, name);
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
