using System;
using System.Collections.Generic;
using System.Text;

namespace IKXTServer
{
    public interface IKXTSerialization
    {
        byte[] ToByteArray();
        void FromBytes(byte[] buffer, int index);
    }
}
