
using System;
using System.Collections.Generic;
using System.Text;

namespace KXTNetStruct
{
    public class MainMessageType
    {
        public const byte HeartRequest = 0x01;
        public const byte HeartResponse = 0x02;

        public const byte RegistReq = 0x10;
        public const byte RegistRes = 0x11;

        public const byte LogoutMessage = 0xF0;
    }

    public class MainRegistRes : IKXTServer.IKXTSerialization
    {
        public byte RegResult;

        public MainRegistRes()
        {
            RegResult = RegResult_Success;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                RegResult = buffer[index];
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return new byte[] { RegResult };
        }

        public const byte RegResult_Success = 0x00;
        public const byte RegResult_Failed = 0x01;
    }
    public class MainRegistReq : IKXTServer.IKXTSerialization
    {
        public Guid UserID;

        public MainRegistReq()
        {
            UserID = Guid.Empty;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                UserID = new Guid(temp);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return UserID.ToByteArray();
        }
    }
}
