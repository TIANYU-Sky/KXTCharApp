using System;
using System.Collections.Generic;
using System.Text;

namespace IKXTServer
{
    public class DataConvert
    {
        public static Guid GetGuid(string id)
        {
            byte[] temp = new byte[16]
            {
                0xAA, 0xAA, 0xAA, 0xAA,
                0xAA, 0xAA, 0xAA, 0xAA,
                0xAA, 0xAA, 0xAA, 0xAA,
                0xAA, 0xAA, 0xAA, 0xAA
            };

            for (int i = 0; i < id.Length && i < temp.Length; ++i)
                if ('0' <= id[i] && '9' >= id[i])
                    temp[temp.Length - 1 - i] = (byte)(id[i] - '0');

            return new Guid(temp);
        }
        public static string GetString(Guid guid)
        {
            byte[] temp = guid.ToByteArray();

            StringBuilder builder = new StringBuilder();

            for (int i = temp.Length - 1; i >= 0; --i) 
            {
                if (0xAA == temp[i])
                    break;
                builder.Append(temp[i].ToString());
            }

            return builder.ToString();
        }
    }
}
