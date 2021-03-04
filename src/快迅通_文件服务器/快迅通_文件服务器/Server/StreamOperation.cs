using KXTServiceDBServer.Cloud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 快迅通_文件服务器.Server
{
    internal enum StreamType
    {
        Download,
        Upload
    }

    internal abstract class StreamBase
    {
        public bool Invaild;
        public DateTime Time;

        public StreamBase()
        {
            Invaild = false;
            Time = DateTime.Now;
        }

        public abstract StreamType GetStreamType();
        public abstract void Close();
        public abstract void Flush();
    }

    internal class Download : StreamBase
    {
        public long Length => Reader.Length;

        private readonly FileStream Reader;

        public Download(string path) : base()
        {
            Reader = new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public override void Close()
        {
            Reader.Close();
        }
        public override void Flush()
        {

        }
        public byte[] ReadBlock(int block)
        {
            Invaild = false;
            Time = DateTime.Now;

            Reader.Position = Math.Abs(block) << 12;

            byte[] buffer = new byte[4096];
            int count = Reader.Read(buffer, 0, buffer.Length);

            byte[] result = new byte[count];
            Array.Copy(buffer, 0, result, 0, count);
            return result;
        }

        public override StreamType GetStreamType() => StreamType.Download;
    }

    internal class Upload : StreamBase
    {
        public readonly string Path;
        public readonly string Name;
        public readonly int Length;

        private readonly FileStream FileWriter;
        private readonly FileBitmap Bitmap;

        public Upload(string root, string path, string name, int length) : base()
        {
            Path = path;
            Name = name;
            Length = length;

            string npath = root + "\\" + ("" == path ? "" : path + "\\") + name;

            Bitmap = new FileBitmap(npath + ".bitmap", length);
            FileWriter = new FileStream(npath, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public void Clear()
        {
            Close();
            System.IO.File.Delete(Path + "\\" + Name + ".bitmap");
            System.IO.File.Delete(Path + "\\" + Name);
        }
        public void FinishUpload()
        {
            Close();

            System.IO.File.Delete(Path + "\\" + Name + ".bitmap");
        }
        public override void Close()
        {
            FileWriter.Flush();
            FileWriter.Close();

            Bitmap.Close();
        }
        public override void Flush()
        {
            try
            {
                FileWriter.Flush();
                Bitmap.Flush();
            }
            catch
            {

            }
        }

        public int ReceiveBlock(int block, byte[] buffer)
        {
            Invaild = false;
            Time = DateTime.Now;

            if (null != buffer)
            {
                try
                {
                    WriteBlock(block, buffer);
                    Bitmap[block] = FileBitmapState.Setted;
                }
                catch
                {

                }
            }
            return GetFreeBlock();
        }
        private int GetFreeBlock()
        {
            return Bitmap.GetBlock();
        }
        private void WriteBlock(int block, byte[] buffer)
        {
            FileWriter.Position = Math.Abs(block) << 12;
            FileWriter.Write(buffer, 0, buffer.Length);
        }

        public override StreamType GetStreamType() => StreamType.Upload;
    }
}
