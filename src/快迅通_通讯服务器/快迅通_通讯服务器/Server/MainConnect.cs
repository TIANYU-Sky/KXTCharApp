using IKXTServer;
using KXTServiceDBServer.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using KXTNetStruct;

namespace 快迅通_通讯服务器.Server
{
    internal class MainConnect
    {
        private readonly Socket MainServer;
        private readonly byte[] Buffer;

        private readonly Action<LogLevel, string> Notify;
        private readonly Action CloseServer;

        private readonly Timer Timer;
        private bool HeartTest;

        private readonly KXTChatGroupInfoReader GroupInfoReader;
        private readonly KXTUserChatReader UserChatReader;

        private bool IsClosed;

        public MainConnect
            (
            Socket main_server,
            string group_info_path,
            string chat_path,
            Action<LogLevel, string> notify,
            Action close
            )
        {
            IsClosed = false;

            Notify = notify;
            GroupInfoReader = new KXTChatGroupInfoReader(group_info_path, Notify);
            UserChatReader = new KXTUserChatReader(chat_path, Notify);

            CloseServer = close;

            HeartTest = false;
            Timer = new Timer
            {
                Interval = HeartRequestInterval,
                AutoReset = false
            };
            Timer.Elapsed += HeartRequest_Trigger;
            Timer.Start();

            MainServer = main_server;
            Buffer = new byte[Datagram.DatagramLengthMax];
            MainServer.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int length = MainServer.EndReceive(ar);
                if (Datagram.DatagramLengthMin <= length)
                {
                    Action<byte[], int> action = OnProcess;
                    byte[] temp = new byte[length];
                    Array.Copy(Buffer, 0, temp, 0, length);
                    action.BeginInvoke(temp, length, null, null);
                }
            }
            catch
            {
                Notify(IKXTServer.LogLevel.Warning, "消息接收异常");
            }

            try
            {
                if (!IsClosed)
                    MainServer.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
            }
            catch
            {
                Notify(IKXTServer.LogLevel.Error, "套接字启动接收异常");
                Close();
            }
        }
        private void OnProcess(byte[] buffer, int count)
        {
            Datagram datagram = new Datagram();
            if (!datagram.FromBytes_S(buffer, 0))
            {
                Notify(LogLevel.Info, "无效的数据：" + Encoding.UTF8.GetString(buffer));
                return;
            }

            if (DatagramType.Main == datagram.DataType)
                if (MainMessageType.HeartResponse == datagram.MessageType)
                {
                    HeartTest = false;
                    return;
                }

            switch (datagram.MessageType)
            {
                case ChatDatagramDefine.FriendChat:
                    OnFriendChat(datagram);
                    break;
                case ChatDatagramDefine.GroupChat:
                    OnGroupChat(datagram);
                    break;
                case ChatDatagramDefine.FriendChatsReq:
                    OnFriendChatsReq(datagram);
                    break;
                case ChatDatagramDefine.GroupChatsReq:
                    OnGroupChatsReq(datagram);
                    break;
                case ChatDatagramDefine.ChatsFinish:
                    OnChatsFinish(datagram);
                    break;
            }
        }

        public void Stop()
        {
            if (!IsClosed)
            {
                MainServer.Shutdown(SocketShutdown.Both);
                MainServer.Close();
                MainServer.Dispose();

                CloseServer();
            }

            IsClosed = true;
        }

        private void Close()
        {
            Stop();
            CloseServer();
        }

        private bool Send(byte[] datas)
        {
            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    MainServer.Send(datas);
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        private void HeartRequest_Trigger(object sender, ElapsedEventArgs args)
        {
            // 发送心跳连接
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Main,
                MessageType = MainMessageType.HeartRequest,
                Time = DateTime.Now,
                RequestID = Guid.Empty,
                Sender = Guid.Empty,
                Datas = null
            };
            if (Send(datagram.ToByteArray()))
            {
                HeartTest = true;
                Timer.Elapsed -= HeartRequest_Trigger;
                Timer.Elapsed += HeartResponse_Trigger;
                Timer.Interval = HeartResponseInterval;
                Timer.Start();
            }
            else
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void HeartResponse_Trigger(object sender, ElapsedEventArgs args)
        {
            if (HeartTest)
            {
                Notify(IKXTServer.LogLevel.Error, "心跳连接超时");
                Close();
            }
            else
            {
                Timer.Elapsed -= HeartResponse_Trigger;
                Timer.Elapsed += HeartRequest_Trigger;
                Timer.Interval = HeartRequestInterval;
                Timer.Start();
            }
        }

        public const double HeartRequestInterval = 5000;
        public const double HeartResponseInterval = 3000;

        private void OnFriendChat(Datagram datagram)
        {
            ChatMessage chat = datagram.UnSerialData<ChatMessage>();

            UserChatReader.SaveFriendChat(chat);

            datagram.DataType = DatagramType.Client;

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnGroupChat(Datagram datagram)
        {
            ChatMessage chat = datagram.UnSerialData<ChatMessage>();

            List<string> members = GroupInfoReader.ReadMembers(chat.Target);
            if (members.Contains(chat.Sender))
            {
                UserChatReader.SaveGroupChat(chat);

                datagram.DataType = DatagramType.Client;

                foreach (string i in members)
                {
                    if (i != chat.Sender)
                    {
                        if (!Send(datagram.ToByteArray()))
                        {
                            Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                            Close();
                        }
                    }
                }
            }
        }
        private void OnFriendChatsReq(Datagram datagram)
        {
            FriendChatsReq req = datagram.UnSerialData<FriendChatsReq>();

            FriendChatsRes res = new FriendChatsRes();
            ChatPackage[] chats = UserChatReader.ReadFriendChat(req.Sender, req.Friend, datagram.RequestID);

            byte[][] buffer;
            if (null != chats)
            {
                for (int i = 0; i < chats.Length; ++i)
                    res.Chat.Add(chats[i] as FriendChatPackage);

                buffer = res.ToByteArrays();
            }
            else
                buffer = new byte[1][]
                {
                    new byte[0]
                };

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = ChatDatagramDefine.FriendChatsRes;
            for (int i = 0; i < buffer.Length; ++i)
            {
                datagram.Datas = buffer[i];
                if (!Send(datagram.ToByteArray()))
                {
                    Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                    Close();
                }
            }
        }
        private void OnGroupChatsReq(Datagram datagram)
        {
            GroupChatsReq req = datagram.UnSerialData<GroupChatsReq>();

            ChatPackage[] chats = UserChatReader.ReadGroupChat(req.Group, datagram.RequestID);

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = ChatDatagramDefine.GroupChatsRes;
            if (null != chats)
            {
                for (int i = 0; i < chats.Length; ++i)
                {
                    datagram.Datas = (chats[i] as GroupChatPackage).ToByteArray();
                    if (!Send(datagram.ToByteArray()))
                    {
                        Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                        Close();
                    }
                }
            }
            else
            {
                datagram.Datas = new byte[0];
                if (!Send(datagram.ToByteArray()))
                {
                    Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                    Close();
                }
            }
        }
        private void OnChatsFinish(Datagram datagram)
        {
            ChatsFinishPackage req = datagram.UnSerialData<ChatsFinishPackage>();

            if (ChatsFinishPackage.ChatType_Friend == req.ChatType)
                UserChatReader.FinishReadFriend
                    (
                    IKXTServer.DataConvert.GetString(datagram.Sender),
                    req.ID,
                    datagram.RequestID
                    );
            else
                UserChatReader.FinishReadGroup
                    (
                    req.ID,
                    datagram.RequestID
                    );
        }
    }
}
