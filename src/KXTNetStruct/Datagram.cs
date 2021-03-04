using System;
using System.Collections.Generic;
using System.Text;

namespace KXTNetStruct
{
    public enum DatagramType : byte
    {
        Client,
        Main,
        Login,
        Cloud,
        Chat,
    }

    public class Datagram : IKXTServer.IKXTSerialization
    {
        public Guid Sender;
        public Guid RequestID;
        public DateTime Time;
        public DatagramType DataType;
        public byte MessageType;

        public int DatasLength => null == Datas ? 0 : Datas.Length;
        public byte[] Datas;

        public Datagram()
        {
            Time = DateTime.Now;
            DataType = DatagramType.Main;
            MessageType = 0;
            Sender = Guid.Empty;
            RequestID = Guid.Empty;

            Datas = new byte[0];
        }

        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(RequestID.ToByteArray());
            buffer.AddRange(Sender.ToByteArray());
            buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(Time));
            buffer.Add((byte)DataType);
            buffer.Add(MessageType);
            if (0 < DatasLength)
                buffer.AddRange(Datas);

            return buffer.ToArray();
        }
        public bool FromBytes_S(byte[] buffer, int index)
        {
            if (null == buffer)
                return false;

            if (DatagramLengthMin > buffer.Length)
                return false;

            if (index + DatagramLengthMin > buffer.Length)
                return false;

            try
            {
                RequestID = new Guid(buffer);

                {
                    byte[] temp = new byte[16];
                    Array.Copy(buffer, 16, temp, 0, 16);
                    Sender = new Guid(temp);
                }

                Time = IKXTServer.KXTBitConvert.ToDateTime(buffer, 32);
                DataType = (DatagramType)buffer[24];
                MessageType = buffer[25];

                int length = buffer.Length - DatagramLengthMin;
                if (0 < length)
                {
                    Datas = new byte[length];
                    Array.Copy(buffer, DatagramLengthMin, Datas, 0, Datas.Length);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        public void FromBytes(byte[] buffer, int index)
        {
            FromBytes_S(buffer, index);
        }

        public T UnSerialData<T>()
            where T : IKXTServer.IKXTSerialization, new()
        {
            return UnSerialData<T>(0);
        }
        public T UnSerialData<T>(int offset)
            where T : IKXTServer.IKXTSerialization, new()
        {
            T data = new T();
            data.FromBytes(Datas, offset);
            return data;
        }

        public const int DatagramLengthMin = 42;
        public const int DatagramLengthMax = 4272;
        public const int DatagramDataHead = 128;
        public const int DatagramDataMax = 4096;
    }
}
