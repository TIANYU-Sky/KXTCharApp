using IKXTServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KXTNetStruct
{
    public class CloudDatagramDefine
    {
        public const byte CloudRequest = 0x00;
        public const byte CloudResponse = 0x01;
        public const byte CloudResFinish = 0x02;

        public const byte CreateFolder = 0x10;
        public const byte DeleteFolder = 0x11;
        public const byte DeleteFile = 0x12;

        public const byte FileUploadReq = 0x20;
        public const byte FileDownloadReq = 0x21;

        public const byte FileUploadRes = 0x30;
        public const byte FileDownloadRes = 0x31;

        public const byte StreamReq = 0x40;
        public const byte StreamRes = 0x41;

        public const byte FileUploadFinish = 0x50;
        public const byte FileDownloadFinish = 0x51;

        public const byte FileUploadCancel = 0x60;
    }

    public class CloudRequest : IKXTServer.IKXTSerialization
    {
        public string Path;

        public CloudRequest()
        {
            Path = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Path = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return KXTBitConvert.ToBytes(Path);
        }
    }

    public class CloudResponse : IKXTServer.IKXTSerialization
    {
        public readonly List<CloudFile> Files;

        public CloudResponse()
        {
            Files = new List<CloudFile>();
        }

        public void FromBytes(byte[] buffer, int index)
        {
            List<CloudFile> list = new List<CloudFile>();

            try
            {
                int count = BitConverter.ToInt32(buffer, index);
                index += 4;

                for (int i = 0; i < count; ++i) 
                {
                    CloudFile file = new CloudFile();
                    file.FromByte(buffer, index, out int length);
                    index += length;
                    list.Add(file);
                }
            }
            catch
            {

            }

            Files.AddRange(list);
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            for (int i = 0; i < Files.Count; ++i)
            {
                byte[] vs = Files[i].ToByteArray();
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
                int count = 0;
                byte[] temp;
                for (int i = 0; i < Files.Count; ++i)
                {
                    byte[] vs = Files[i].ToByteArray();
                    if (Datagram.DatagramDataMax - 4 < vs.Length + array.Count)
                    {
                        temp = new byte[array.Count + 4];
                        BitConverter.GetBytes(count).CopyTo(temp, 0);
                        array.ToArray().CopyTo(temp, 4);
                        buffer.Add(temp);
                        array.Clear();
                        count = 0;
                    }
                    array.AddRange(vs);
                    ++count;
                }

                temp = new byte[array.Count + 4];
                BitConverter.GetBytes(count).CopyTo(temp, 0);
                if (0 < array.Count)
                {
                    array.ToArray().CopyTo(temp, 4);
                    buffer.Add(temp);
                }
            }

            return buffer.ToArray();
        }
    }

    public class CreateFolder : IKXTServer.IKXTSerialization
    {
        public string Name;
        public string Path;

        public CreateFolder()
        {
            Name = "";
            Path = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Name = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                Path = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(Name));
            buffer.AddRange(KXTBitConvert.ToBytes(Path));

            return buffer.ToArray();
        }
    }

    public class DeleteFolderFile : IKXTServer.IKXTSerialization
    {
        public string Name;
        public string Path;

        public DeleteFolderFile()
        {
            Name = "";
            Path = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Name = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                Path = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(Name));
            buffer.AddRange(KXTBitConvert.ToBytes(Path));

            return buffer.ToArray();
        }
    }

    public class FileUploadReq : IKXTServer.IKXTSerialization
    {
        public string Name;
        public string Path;
        public int Size;

        public FileUploadReq()
        {
            Name = "";
            Path = "";
            Size = 0;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Size = BitConverter.ToInt32(buffer, index);
                index += 4;

                Name = KXTBitConvert.ToString(buffer, index, out int count);
                index += count;

                Path = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(BitConverter.GetBytes(Size));
            buffer.AddRange(KXTBitConvert.ToBytes(Name));
            buffer.AddRange(KXTBitConvert.ToBytes(Path));

            return buffer.ToArray();
        }
    }

    public class FileDownloadReq : IKXTServer.IKXTSerialization
    {
        public string Name;
        public string Path;

        public FileDownloadReq()
        {
            Name = "";
            Path = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Name = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                Path = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(Name));
            buffer.AddRange(KXTBitConvert.ToBytes(Path));

            return buffer.ToArray();
        }
    }

    public class FileUploadRes : IKXTServer.IKXTSerialization
    {
        public Guid UploadID;

        public FileUploadRes()
        {
            UploadID = Guid.Empty;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                UploadID = new Guid(temp);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return UploadID.ToByteArray();
        }
    }

    public class FileDownloadRes : IKXTServer.IKXTSerialization
    {
        public int Size;
        public Guid DownloadID;

        public FileDownloadRes()
        {
            Size = 0;
            DownloadID = Guid.Empty;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                DownloadID = new Guid(temp);

                Size = BitConverter.ToInt32(buffer, index + 16);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(DownloadID.ToByteArray());
            buffer.AddRange(BitConverter.GetBytes(Size));

            return buffer.ToArray();
        }
    }

    public class StreamReq : IKXTServer.IKXTSerialization
    {
        public Guid StreamID;
        public int Block;

        public StreamReq()
        {
            StreamID = Guid.Empty;
            Block = -1;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                StreamID = new Guid(temp);

                Block = BitConverter.ToInt32(buffer, index + 16);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(StreamID.ToByteArray());
            buffer.AddRange(BitConverter.GetBytes(Block));

            return buffer.ToArray();
        }
    }

    public class StreamRes : IKXTServer.IKXTSerialization
    {
        public Guid StreamID;
        public int Block;
        public byte[] Stream;

        public StreamRes()
        {
            StreamID = Guid.Empty;
            Block = -1;
            Stream = new byte[0];
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                StreamID = new Guid(temp);

                Block = BitConverter.ToInt32(buffer, index + 16);

                index += 20;
                Stream = new byte[buffer.Length - index];
                Array.Copy(buffer, index, Stream, 0, Stream.Length);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(StreamID.ToByteArray());
            buffer.AddRange(BitConverter.GetBytes(Block));
            if (null != Stream && 0 < Stream.Length)
                buffer.AddRange(Stream);

            return buffer.ToArray();
        }
    }

    public class FileUploadFinish : IKXTServer.IKXTSerialization
    {
        public Guid UploadID;

        public FileUploadFinish()
        {
            UploadID = Guid.Empty;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                UploadID = new Guid(temp);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return UploadID.ToByteArray();
        }
    }

    public class FileDownloadFinish : IKXTServer.IKXTSerialization
    {
        public Guid DownloadID;

        public FileDownloadFinish()
        {
            DownloadID = Guid.Empty;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                DownloadID = new Guid(temp);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return DownloadID.ToByteArray();
        }
    }

    public class FileUploadCancel : IKXTServer.IKXTSerialization
    {
        public Guid UploadID;

        public FileUploadCancel()
        {
            UploadID = Guid.Empty;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                UploadID = new Guid(temp);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return UploadID.ToByteArray();
        }
    }
}
