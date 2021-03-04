using IKXTServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace KXTNetStruct
{
    public class ChatDatagramDefine
    {
        public const byte FriendChat = 0x00;
        public const byte GroupChat = 0x01;

        public const byte FriendChatsReq = 0x10;
        public const byte FriendChatsRes = 0x11;
        public const byte GroupChatsReq = 0x12;
        public const byte GroupChatsRes = 0x13;

        public const byte ChatsFinish = 0xF0;
    }

    public class ChatMessage : IKXTServer.IKXTSerialization
    {
        public string Sender;   // 发送者的ID
        public string Target;   // 接收者的ID（群消息为群ID）
        public string Message;
        public DateTime Time;

        public ChatMessage()
        {
            Sender = "";
            Target = "";
            Message = "";
            Time = DateTime.Now;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Time = KXTBitConvert.ToDateTime(buffer, index);
                index += 8;

                Sender = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                Target = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                Message = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(Time));
            buffer.AddRange(KXTBitConvert.ToBytes(Sender));
            buffer.AddRange(KXTBitConvert.ToBytes(Target));
            buffer.AddRange(KXTBitConvert.ToBytes(Message));

            return buffer.ToArray();
        }
    }

    public class FriendChatsReq : IKXTServer.IKXTSerialization
    {
        public string Sender;
        public string Friend;

        public FriendChatsReq()
        {
            Sender = "";
            Friend = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Sender = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                Friend = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(Sender));
            buffer.AddRange(KXTBitConvert.ToBytes(Friend));

            return buffer.ToArray();
        }
    }

    public class GroupChatsReq : IKXTServer.IKXTSerialization
    {
        public string Group;

        public GroupChatsReq()
        {
            Group = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Group = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return KXTBitConvert.ToBytes(Group);
        }
    }

    public class FriendChatsRes : IKXTServer.IKXTSerialization
    {
        public readonly List<FriendChatPackage> Chat;

        public FriendChatsRes()
        {
            Chat = new List<FriendChatPackage>();
        }

        public void FromBytes(byte[] buffer, int index)
        {
            List<FriendChatPackage> list = new List<FriendChatPackage>();

            try
            {
                int count = BitConverter.ToInt32(buffer, index);
                index += 4;

                for (int i = 0; i < count; ++i)
                {
                    FriendChatPackage chat = new FriendChatPackage();
                    chat.FromBytes(buffer, index, out int length);
                    index += length;
                    list.Add(chat);
                }
            }
            catch
            {

            }

            Chat.AddRange(list);
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            for (int i = 0; i < Chat.Count; ++i)
            {
                byte[] vs = Chat[i].ToByteArray();
                if (Datagram.DatagramDataMax < vs.Length + buffer.Count)
                    break;
                buffer.AddRange(vs);
            }

            return buffer.ToArray();
        }
        public byte[][] ToByteArrays()
        {
            List<byte[]> buffer = new List<byte[]>();

            {
                List<byte> array = new List<byte>();
                int j = 0;
                for (int i = 0; i < Chat.Count; ++i)
                {
                    byte[] vs = Chat[i].ToByteArray();
                    if (Datagram.DatagramDataMax - 4 < vs.Length + array.Count)
                    {
                        byte[] temp = new byte[array.Count + 4];
                        BitConverter.GetBytes(j).CopyTo(temp, 0);
                        array.ToArray().CopyTo(temp, 4);
                        buffer.Add(temp);
                        array.Clear();
                        j = 0;
                    }
                    array.AddRange(vs);
                    ++j;
                }

                if (0 < array.Count)
                {
                    byte[] temp = new byte[array.Count + 4];
                    BitConverter.GetBytes(j).CopyTo(temp, 0);
                    array.ToArray().CopyTo(temp, 4);
                    buffer.Add(temp);
                }
            }

            return buffer.ToArray();
        }
    }

    public class ChatsFinishPackage : IKXTServer.IKXTSerialization
    {
        public byte ChatType;
        public string ID;

        public ChatsFinishPackage()
        {
            ChatType = ChatType_Friend;
            ID = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                ChatType = buffer[index++];
                ID = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add(ChatType);
            buffer.AddRange(KXTBitConvert.ToBytes(ID));

            return buffer.ToArray();
        }

        public const byte ChatType_Friend = 0x00;
        public const byte ChatType_Group = 0x01;
    }
}
