using IKXTServer;
using KXTNetStruct;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace KXTServiceDBServer.Files
{
    internal class KXTUserChatHead
    {
        public long EndRecorder;
        public int RecorderSize;

        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(BitConverter.GetBytes(EndRecorder));
            buffer.AddRange(BitConverter.GetBytes(RecorderSize));

            return buffer.ToArray();
        }

        public static KXTUserChatHead Parse(byte[] buffer)
        {
            KXTUserChatHead head = new KXTUserChatHead();

            try
            {
                head.EndRecorder = BitConverter.ToInt64(buffer, 0);
                head.RecorderSize = BitConverter.ToInt32(buffer, 8);
            }
            catch
            {

            }

            return head;
        }

        public const int KXTUserChatHeadLength = 12;
    }

    internal class KXTUserChatItem
    {
        public Guid Sender;
        public long PreRecorder;
        public int PreSize;
        public DateTime Time;
        public string Message;
        
        public KXTUserChatItem()
        {
            PreRecorder = 0;
            Sender = Guid.Empty;
            Time = DateTime.Now;
            Message = "";
        }

        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(Sender.ToByteArray());
            buffer.AddRange(BitConverter.GetBytes(PreRecorder));
            buffer.AddRange(BitConverter.GetBytes(PreSize));
            buffer.AddRange(KXTBitConvert.ToBytes(Time));
            buffer.AddRange(KXTBitConvert.ToBytes(Message));

            return buffer.ToArray();
        }

        public static KXTUserChatItem Parse(byte[] buffer)
        {
            KXTUserChatItem item = new KXTUserChatItem();

            try
            {
                item.Sender = new Guid(buffer);
                item.PreRecorder = BitConverter.ToInt64(buffer, 16);
                item.PreSize = BitConverter.ToInt32(buffer, 24);
                item.Time = IKXTServer.KXTBitConvert.ToDateTime(buffer, 28);
                item.Message = IKXTServer.KXTBitConvert.ToString(buffer, 36);
            }
            catch
            {

            }

            return item;
        }
    }

    internal class KXTUCCacheKey : IEquatable<KXTUCCacheKey>
    {
        public readonly string UserA;
        public readonly string UserB;

        public KXTUCCacheKey(string userA, string userB)
        {
            UserA = userA;
            UserB = userB;
        }

        public bool Equals(KXTUCCacheKey other)
        {
            if (UserA == other.UserA)
            {
                if (UserB == other.UserB)
                    return true;
            }
            else if (UserA == other.UserB)
            {
                if (UserB == other.UserA)
                    return true;
            }

            return false;
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;

            return base.Equals((KXTUCCacheKey)obj);
        }
        public override int GetHashCode()
        {
            return UserA.GetHashCode() ^ UserB.GetHashCode();
        }
    }

    internal class KXTUCCacheFile
    {
        public bool Invaild;
        public DateTime VisitTime;

        private readonly bool ChatType;

        private readonly FileStream Stream;
        private readonly Action<LogLevel, string> Notify;

        private KXTUserChatHead Write;
        private Dictionary<Guid, KXTUserChatHead> RequestList;

        private int ReadLock;
        private int WriteLock;

        private int SaveTimes;

        public KXTUCCacheFile
            (
            string path,
            Action<LogLevel, string> notify,
            bool chat_type = FriendType
            )
        {
            Invaild = false;
            VisitTime = DateTime.Now;

            Notify = notify;
            ChatType = chat_type;

            Stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);

            byte[] buffer = new byte[KXTUserChatHead.KXTUserChatHeadLength];
            Stream.Read(buffer, 0, buffer.Length);

            Write = KXTUserChatHead.Parse(buffer);
            RequestList = new Dictionary<Guid, KXTUserChatHead>();

            ReadLock = 0;
            WriteLock = 0;

            SaveTimes = 0;
        }

        public void Save(ChatMessage chat)
        {
            ToLockWrite();

            VisitTime = DateTime.Now;
            Invaild = false;

            byte[] temp = new KXTUserChatItem
            {
                PreRecorder = Write.EndRecorder,
                PreSize = Write.RecorderSize,
                Sender = IKXTServer.DataConvert.GetGuid(chat.Sender),
                Time = chat.Time,
                Message = chat.Message
            }.ToByteArray();

            try
            {
                long position = (0 == Write.EndRecorder ? KXTUserChatHead.KXTUserChatHeadLength : Write.EndRecorder)
                    + Write.RecorderSize;

                Stream.Position = position;
                Stream.Write(temp, 0, temp.Length);

                Write.EndRecorder = position;
                Write.RecorderSize = temp.Length;

                ++SaveTimes;
            }
            catch
            {

            }

            ToUnLockWrite();

            if (20 < SaveTimes)
                Flush();
        }

        public void Read(Guid request_id, out ChatPackage[] chats)
        {
            ToLockRead();

            VisitTime = DateTime.Now;
            Invaild = false;

            List<ChatPackage> buffer = new List<ChatPackage>();

            KXTUserChatHead value;
            if (!RequestList.TryGetValue(request_id, out value))
            {
                ToLockWrite();

                value = new KXTUserChatHead
                {
                    EndRecorder = Write.EndRecorder,
                    RecorderSize = Write.RecorderSize
                };

                ToUnLockWrite();

                RequestList.Add(request_id, value);
            }

            for (int i = 0; i < 10; ++i)
            {
                if (0 == value.EndRecorder)
                    break;

                byte[] temp = new byte[value.RecorderSize];

                Stream.Position = value.EndRecorder;
                Stream.Read(temp, 0, temp.Length);

                KXTUserChatItem item = KXTUserChatItem.Parse(temp);
                value.EndRecorder = item.PreRecorder;
                value.RecorderSize = item.PreSize;

                if (ChatType)
                    buffer.Add(new FriendChatPackage
                    {
                        Message = item.Message,
                        Sender = IKXTServer.DataConvert.GetString(item.Sender),
                        Time = item.Time
                    });
                else
                    buffer.Add(new FriendChatPackage
                    {
                        Message = item.Message,
                        Sender = IKXTServer.DataConvert.GetString(item.Sender),
                        Time = item.Time
                    });
            }

            ToUnLockRead();

            chats = buffer.ToArray();
        }
        public void EndRead(Guid request_id)
        {
            RequestList.Remove(request_id);
        }

        public void Flush()
        {
            ToLockWrite();
            ToLockRead();

            Stream.Position = 0;
            Stream.Write(Write.ToByteArray(), 0, KXTUserChatHead.KXTUserChatHeadLength);
            Stream.Flush();

            ToUnLockRead();
            ToUnLockWrite();
        }
        public void Close()
        {
            Stream.Flush();
            Stream.Close();
        }

        private void ToLockWrite()
        {
            if (0 != Interlocked.Exchange(ref WriteLock, 1))
                Thread.Sleep(10);
        }
        private void ToLockRead()
        {
            if (0 != Interlocked.Exchange(ref ReadLock, 1))
                Thread.Sleep(10);
        }

        private void ToUnLockWrite()
        {
            Interlocked.Exchange(ref WriteLock, 0);
        }
        private void ToUnLockRead()
        {
            Interlocked.Exchange(ref ReadLock, 0);
        }

        public const bool FriendType = false;
        public const bool GroupType = true;
    }

    public class KXTUserChatReader
    {
        private readonly string RootDirectory;
        private readonly IDBUserChatReader DBReader;

        private readonly ConcurrentDictionary<KXTUCCacheKey, KXTUCCacheFile> Cache;
        private readonly System.Timers.Timer CacheTime;

        private readonly Action<LogLevel, string> Notify;

        public KXTUserChatReader(string path, Action<LogLevel, string> notify)
        {
            RootDirectory = path;
            Notify = notify;

            Cache = new ConcurrentDictionary<KXTUCCacheKey, KXTUCCacheFile>();
            DBReader = new UserRelationDB();

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
            DBReader.Close();
            foreach (var item in Cache)
                item.Value.Close();
        }

        public bool AddFriend(string userA, string userB)
        {
            if (DBReader.NewRelation(userA, userB, out string relation_id))
            {
                FileStream stream = File.Create(RootDirectory + "\\Relations\\" + relation_id + ".kxtchat");

                KXTUserChatHead head = new KXTUserChatHead
                {
                    EndRecorder = 0,
                    RecorderSize = 0
                };

                stream.Write(head.ToByteArray(), 0, KXTUserChatHead.KXTUserChatHeadLength);
                stream.Flush();
                stream.Close();

                return true;
            }

            return false;
        }
        public bool DelFriend(string userA, string userB)
        {
            if (DBReader.GetRelation(userA, userB, out string relation_id))
            {
                DBReader.DelRelation(userA, userB);

                File.Delete(RootDirectory + "\\Relations\\" + relation_id + ".kxtchat");

                return true;
            }

            return false;
        }

        public void AddGroup(string group_id)
        {
            FileStream stream = File.Create(RootDirectory + "\\Groups\\" + group_id + ".kxtchat");

            KXTUserChatHead head = new KXTUserChatHead
            {
                EndRecorder = 0,
                RecorderSize = 0
            };

            stream.Write(head.ToByteArray(), 0, KXTUserChatHead.KXTUserChatHeadLength);
            stream.Flush();
            stream.Close();
        }
        public void DelGroup(string group_id)
        {
            try
            {
                File.Delete(RootDirectory + "\\Groups\\" + group_id + ".kxtchat");
            }
            catch
            {
                
            }
        }

        public void SaveFriendChat(ChatMessage chat)
        {
            if (!Cache.TryGetValue(new KXTUCCacheKey(chat.Sender, chat.Target), out KXTUCCacheFile value))
            {
                // Cann`t find opened file object
                if (!DBReader.GetRelation(chat.Sender, chat.Target, out string relation_id))
                    return;
                try
                {
                    value = new KXTUCCacheFile(RootDirectory + "\\Relations\\" + relation_id + ".kxtchat", Notify);
                }
                catch
                {
                    return;
                }
                if (!Cache.TryAdd(new KXTUCCacheKey(chat.Sender, chat.Target), value))
                    return;
            }
            try
            {
                value.Save(chat);
            }
            catch
            {
                Cache.TryRemove(new KXTUCCacheKey(chat.Sender, chat.Target), out _);
            }
        }
        public void SaveGroupChat(ChatMessage chat)
        {
            string group = chat.Target;

            if (!Cache.TryGetValue(new KXTUCCacheKey(group, ""), out KXTUCCacheFile value))
            {
                // Cann`t find opened file object
                try
                {
                    value = new KXTUCCacheFile(RootDirectory + "\\Groups\\" + group + ".kxtchat", Notify, KXTUCCacheFile.GroupType); ;
                }
                catch
                {
                    return;
                }
                if (!Cache.TryAdd(new KXTUCCacheKey(group, ""), value))
                    return;
            }
            try
            {
                value.Save(chat);
            }
            catch
            {
                Cache.TryRemove(new KXTUCCacheKey(group, ""), out _);
            }
        }

        public void FinishReadFriend(string userA, string userB, Guid request_id)
        {
            if (Cache.TryGetValue(new KXTUCCacheKey(userA, userB), out KXTUCCacheFile value))
                value.EndRead(request_id);
        }
        public void FinishReadGroup(string group, Guid request_id)
        {
            if (Cache.TryGetValue(new KXTUCCacheKey(group, ""), out KXTUCCacheFile value))
                value.EndRead(request_id);
        }

        public ChatPackage[] ReadFriendChat(string userA, string userB, Guid request_id)
        {
            if (!Cache.TryGetValue(new KXTUCCacheKey(userA, userB), out KXTUCCacheFile value))
            {
                // Cann`t find opened file object
                if (!DBReader.GetRelation(userA, userB, out string relation_id))
                    return null;
                try
                {
                    value = new KXTUCCacheFile(RootDirectory + "\\Relations\\" + relation_id + ".kxtchat", Notify);
                }
                catch
                {
                    return null;
                }
                if (!Cache.TryAdd(new KXTUCCacheKey(userA, userB), value))
                    return null;
            }
            try
            {
                value.Read(request_id, out ChatPackage[] packages);
                return packages;
            }
            catch
            {
                Cache.TryRemove(new KXTUCCacheKey(userA, userB), out _);
                return null;
            }
        }
        public ChatPackage[] ReadGroupChat(string group, Guid request_id)
        {
            if (!Cache.TryGetValue(new KXTUCCacheKey(group, ""), out KXTUCCacheFile value))
            {
                // Cann`t find opened file object
                if (!DBReader.GetRelation(group, "", out string relation_id))
                    return null;
                try
                {
                    value = new KXTUCCacheFile(RootDirectory + "\\Groups\\" + relation_id + ".kxtchat", Notify);
                }
                catch
                {
                    return null;
                }
                if (!Cache.TryAdd(new KXTUCCacheKey(group, ""), value))
                    return null;
            }
            try
            {
                value.Read(request_id, out ChatPackage[] packages);
                return packages;
            }
            catch
            {
                Cache.TryRemove(new KXTUCCacheKey(group, ""), out _);
                return null;
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
