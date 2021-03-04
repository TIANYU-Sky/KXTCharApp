using System;
using System.Collections.Generic;
using System.Text;

namespace IKXTServer
{
    public class KXTBitConvert
    {
        public static byte[] ToBytes(DateTime time)
        {
            return BitConverter.GetBytes(time.Ticks);
        }
        public static byte[] ToBytes(string str)
        {
            List<byte> buffer = new List<byte>();
            byte[] temp = Encoding.UTF8.GetBytes(str);

            buffer.AddRange(BitConverter.GetBytes(temp.Length));
            buffer.AddRange(temp);

            return buffer.ToArray();
        }

        public static DateTime ToDateTime(byte[] bytes, int offset)
        {
            if (null != bytes && offset + 8 <= bytes.Length)
            {
                long ticks = BitConverter.ToInt64(bytes, offset);
                if (DateTime.MinValue.Ticks <= ticks 
                    && DateTime.MaxValue.Ticks >= ticks)
                {
                    return new DateTime(ticks);
                }
            }

            return DateTime.MinValue;
        }
        public static string ToString(byte[] bytes, int offset)
        {
            try
            {
                int length = BitConverter.ToInt32(bytes, offset);
                if (0 < length)
                {
                    byte[] temp = new byte[length];
                    Array.Copy(bytes, offset + 4, temp, 0, length);
                    return Encoding.UTF8.GetString(temp);
                }
            }
            catch
            {

            }

            return "";
        }
        public static string ToString(byte[] bytes, int offset, out int count)
        {
            count = 0;
            try
            {
                int length = BitConverter.ToInt32(bytes, offset);
                if (0 < length)
                {
                    byte[] temp = new byte[length];
                    Array.Copy(bytes, offset + 4, temp, 0, length);
                    count = length + 4;
                    return Encoding.UTF8.GetString(temp);
                }
            }
            catch
            {

            }

            return "";
        }
    }
}
