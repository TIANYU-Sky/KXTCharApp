using IKXTServer;
using KXTNetStruct;
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
    public class KXTUserAppliesPackage
    {
        public byte TargetType;
        public string GroupID;
        public string ApplierID;
        public DateTime ApplyTime;
        public string Message;

        public KXTUserAppliesPackage()
        {
            TargetType = TargetType_Friend;
            GroupID = "";
            ApplierID = "";
            ApplyTime = DateTime.Now;
            Message = "";
        }

        public const byte TargetType_Friend = 0x00;
        public const byte TargetType_Group = 0x01;
    }

    internal class KXTUserAppliesFile
    {
        public bool Invaild;
        public DateTime VisitTime;

        private readonly KXTJson Json;

        public KXTUserAppliesFile(string path)
        {
            Json = new KXTJson(path);

            Invaild = false;
            VisitTime = DateTime.Now;
        }

        public void Read(out KXTUserAppliesPackage[] applies)
        {
            VisitTime = DateTime.Now;
            Invaild = false;

            List<KXTUserAppliesPackage> buffer = new List<KXTUserAppliesPackage>();

            foreach (var item in Json["Applies"] as JArray)
            {
                JObject obj = item as JObject;

                if (!DateTime.TryParse(obj["time"].ToString(), out DateTime result))
                    result = DateTime.Now;

                buffer.Add(new KXTUserAppliesPackage
                {
                    TargetType = obj["type"].ToString() == "group" ? KXTUserAppliesPackage.TargetType_Group : KXTUserAppliesPackage.TargetType_Friend,
                    GroupID = obj["group"].ToString(),
                    ApplierID = obj["applicat"].ToString(),
                    Message = obj["message"].ToString(),
                    ApplyTime = result
                });
            }

            applies = buffer.ToArray();
        }

        public void AddApply(Guid sender, ApplyRequest request)
        {
            VisitTime = DateTime.Now;
            Invaild = false;

            string applicat = IKXTServer.DataConvert.GetString(sender);

            JArray array = Json["Applies"] as JArray;
            
            for (int i = 0; i < array.Count; ++i)
            {
                JObject obj = array[i] as JObject;
                if (obj["applicat"].ToString() == applicat)
                {
                    if (obj["type"].ToString() == "group")
                    {
                        if (obj["group"].ToString() == request.TargetID)
                        {
                            array.RemoveAt(i);
                            break;
                        }
                    }
                    else
                    {
                        array.RemoveAt(i);
                        break;
                    }
                }
            }

            array.Add(new JObject
            {
                {"type", request.TargetType == ApplyRequest.TargetType_Friend ? "friend" : "group" },
                {"group", request.TargetID },
                {"applicat", applicat },
                {"message", request.Message },
                {"time", request.ApplyTime.ToString() }
            });
        }
        public void EndApply(string sender)
        {
            VisitTime = DateTime.Now;
            Invaild = false;

            JArray array = Json["Applies"] as JArray;

            for (int i = 0; i < array.Count; ++i)
            {
                JObject obj = array[i] as JObject;
                if (obj["applicat"].ToString() == sender && obj["type"].ToString() == "friend")
                {
                    array.RemoveAt(i);
                    break;
                }
            }
        }
        public void EndApply(string sender, string group)
        {
            VisitTime = DateTime.Now;
            Invaild = false;

            JArray array = Json["Applies"] as JArray;

            for (int i = 0; i < array.Count; ++i)
            {
                JObject obj = array[i] as JObject;
                if (
                    obj["applicat"].ToString() == sender
                    && obj["type"].ToString() == "group"
                    && obj["group"].ToString() == group
                    )
                {
                    array.RemoveAt(i);
                    break;
                }
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

    public class KXTUserAppliesReader
    {
        private readonly string RootPath;
        private readonly Action<LogLevel, string> Notify;

        private readonly ConcurrentDictionary<string, KXTUserAppliesFile> Cache;
        private readonly System.Timers.Timer CacheTime;

        public KXTUserAppliesReader(string root, Action<LogLevel, string> notify)
        {
            RootPath = root;
            Notify = notify;

            Cache = new ConcurrentDictionary<string, KXTUserAppliesFile>();

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
                        "Applies",
                        KXTRootJsonType.Array
                    )
               }
               );
            }
            catch
            {
                Notify(LogLevel.Warning, "用户申请数据操作异常：创建文件异常");
                return false;
            }
        }
        public KXTUserAppliesPackage[] ReadApplies(string user_id)
        {
            try
            {
                if (!Cache.TryGetValue(user_id, out KXTUserAppliesFile file))
                {
                    file = new KXTUserAppliesFile
                        (
                        RootPath + "\\" + user_id + ".json"
                        );
                    Cache.TryAdd(user_id, file);
                }

                file.Read(out KXTUserAppliesPackage[] applies);

                return applies;
            }
            catch
            {
                Notify(LogLevel.Warning, "用户申请数据操作异常：文件异常");
                return new KXTUserAppliesPackage[0];
            }
        }
        public void EndApply(string sender, ApplyResponse response)
        {
            try
            {
                if (!Cache.TryGetValue(sender, out KXTUserAppliesFile file))
                {
                    file = new KXTUserAppliesFile
                        (
                        RootPath + "\\" + sender + ".json"
                        );
                    Cache.TryAdd(sender, file);
                }

                if ("" == response.TargetID)
                    file.EndApply(response.ApplierID);
                else
                    file.EndApply(response.ApplierID, response.TargetID);

                file.Flush();
            }
            catch
            {
                Notify(LogLevel.Warning, "用户申请数据操作异常：文件异常");
            }
        }
        public void AddApply(Guid sender, string target, ApplyRequest request)
        {
            try
            {
                if (!Cache.TryGetValue(target, out KXTUserAppliesFile file))
                {
                    file = new KXTUserAppliesFile
                        (
                        RootPath + "\\" + target + ".json"
                        );
                    Cache.TryAdd(target, file);
                }

                file.AddApply(sender, request);

                file.Flush();
            }
            catch
            {
                Notify(LogLevel.Warning, "用户申请数据操作异常：文件异常");
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
