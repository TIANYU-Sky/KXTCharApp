using IKXTServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace KXTNetStruct
{
    public enum ChatPackageType
    {
        Friend,
        Group
    }

    public abstract class ChatPackage
    {
        public abstract ChatPackageType GetChatType();
    }

    public class FriendChatPackage : ChatPackage
    {
        public string Sender;
        public DateTime Time;
        public string Message;

        public FriendChatPackage()
        {
            Sender = "";
            Time = DateTime.Now;
            Message = "";
        }

        public void FromBytes(byte[] buffer, int index, out int length)
        {
            length = 0;
            try
            {
                length = BitConverter.ToInt32(buffer, index);
                index += 4;

                Time = KXTBitConvert.ToDateTime(buffer, index);
                index += 8;

                Sender = KXTBitConvert.ToString(buffer, index, out int count);
                index += count;

                Message = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            List<byte> temp = new List<byte>();
            {
                temp.AddRange(KXTBitConvert.ToBytes(Time));
                temp.AddRange(KXTBitConvert.ToBytes(Sender));
                temp.AddRange(KXTBitConvert.ToBytes(Message));
            }
            buffer.AddRange(BitConverter.GetBytes(temp.Count));
            buffer.AddRange(temp);

            return buffer.ToArray();
        }
        public string ToJson()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{");
            {
                builder.Append("\"");
                builder.Append(Time.Ticks.ToString());
                builder.Append("\"");

                builder.Append(":{");

                {
                    builder.Append("\"sender\":");
                    builder.Append("\"");
                    builder.Append(Sender);
                    builder.Append("\"");

                    builder.Append(",");

                    builder.Append("\"message\":");
                    builder.Append("\"");
                    builder.Append(Message);
                    builder.Append("\"");
                }

                builder.Append("}");
            }
            builder.Append("}");

            return builder.ToString();
        }

        public override ChatPackageType GetChatType() => ChatPackageType.Friend;
    }

    public class GroupChatPackage : ChatPackage, IKXTServer.IKXTSerialization
    {
        public string Sender;
        public string Group;
        public DateTime Time;
        public string Message;

        public GroupChatPackage()
        {
            Sender = "";
            Group = "";
            Time = DateTime.Now;
            Message = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            FromBytes(buffer, index, out _);
        }
        public void FromBytes(byte[] buffer, int index, out int length)
        {
            length = 0;
            try
            {
                length = BitConverter.ToInt32(buffer, index);
                index += 4;

                Time = KXTBitConvert.ToDateTime(buffer, index);
                index += 8;

                Sender = KXTBitConvert.ToString(buffer, index, out int count);
                index += count;

                Group = KXTBitConvert.ToString(buffer, index, out count);
                index += count;

                Message = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            List<byte> temp = new List<byte>();
            {
                temp.AddRange(KXTBitConvert.ToBytes(Time));
                temp.AddRange(KXTBitConvert.ToBytes(Sender));
                temp.AddRange(KXTBitConvert.ToBytes(Group));
                temp.AddRange(KXTBitConvert.ToBytes(Message));
            }
            buffer.AddRange(BitConverter.GetBytes(temp.Count));
            buffer.AddRange(temp);

            return buffer.ToArray();
        }
        public string ToJson()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{");
            {
                builder.Append("\"");
                builder.Append(Time.Ticks.ToString());
                builder.Append("\"");

                builder.Append(":{");

                {
                    builder.Append("\"sender\":");
                    builder.Append("\"");
                    builder.Append(Sender);
                    builder.Append("\"");

                    builder.Append(",");

                    builder.Append("\"message\":");
                    builder.Append("\"");
                    builder.Append(Message);
                    builder.Append("\"");
                }

                builder.Append("}");
            }
            builder.Append("}");

            return builder.ToString();
        }

        public override ChatPackageType GetChatType() => ChatPackageType.Group;

    }
}
