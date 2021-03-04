using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace 快迅通.Server.FileService
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

        public abstract string GetTime();
        public abstract string GetName();
        public abstract int GetSize();
        public abstract string GetSchedult();
        public abstract StreamType GetStreamType();
        public abstract void Close();
        public abstract void Flush();
    }

    internal class Upload : StreamBase
    {
        public long Length => Reader.Length;

        private readonly StorageFile File;

        private string StartTime;
        private Stream Reader;
        private int BitCount;
        private byte[] Bitmap;

        public Upload(StorageFile file) : base()
        {
            File = file;
            Open();
        }
        private async void Open()
        {
            Reader = await File.OpenStreamForReadAsync();
            StartTime = DateTime.Now.ToString();
            BitCount = (int)((Reader.Length >> 12) + ((Reader.Length & 4095) != 0 ? 1 : 0));
            Bitmap = new byte[(BitCount >> 3) + ((BitCount & 7) != 0 ? 1 : 0)];
        }

        public override void Close()
        {
            Reader.Close();
        }
        public override void Flush()
        {

        }
        public async Task<byte[]> ReadBlock(int block)
        {
            this[block] = FileBitmapState.Setted;

            Invaild = false;
            Time = DateTime.Now;

            Reader.Position = Math.Abs(block) << 12;

            byte[] buffer = new byte[4096];
            int count = await Reader.ReadAsync(buffer, 0, buffer.Length);

            byte[] result = new byte[count];
            Array.Copy(buffer, 0, result, 0, count);
            return result;
        }
        public double FinishRate()
        {
            int setted = 0;
            for (int i = 0; i < Bitmap.Length; ++i)
            {
                int temp = Bitmap[i];
                while (0 != temp)
                {
                    temp &= temp - 1;
                    ++setted;
                }
            }

            return (double)setted / BitCount;
        }

        private FileBitmapState this[int index]
        {
            get
            {
                if (BitCount <= index)
                    return FileBitmapState.Setted;

                return (Bitmap[index >> 8] & (1 << (index & 7))) != 0 ?
                    FileBitmapState.Setted :
                    FileBitmapState.UnSet;
            }
            set
            {
                if (BitCount <= index)
                    return;

                Bitmap[index >> 8] = (byte)
                    (
                    value == FileBitmapState.Setted ?
                    Bitmap[index >> 8] | (1 << (index & 7)) :
                    Bitmap[index >> 8] & (~(1 << (index & 7)))
                    );
            }
        }

        public override StreamType GetStreamType() => StreamType.Upload;
        public override string GetTime() => StartTime;
        public override string GetName() => File.Name;
        public override int GetSize() => (int)Length;
        public override string GetSchedult() => string.Format("{0:P}", FinishRate());
    }

    internal class Download : StreamBase
    {
        private readonly string StartTime;
        public readonly StorageFolder Path;
        public readonly string Name;
        public readonly int Length;

        private Stream FileWriter;
        private FileBitmap Bitmap;

        public Download(StorageFolder path, string name, int length) : base()
        {
            Path = path;
            Name = name;
            Length = length;
            StartTime = DateTime.Now.ToString();

            Open();
        }
        private async void Open()
        {
            Bitmap = new FileBitmap(Path, Name, Length);

            StorageFile file;
            if (null != await Path.TryGetItemAsync(Name))
                file = await Path.GetFileAsync(Name);
            else
                file = await Path.CreateFileAsync(Name);

            FileWriter = await file.OpenStreamForWriteAsync();
        }

        public async void Clear()
        {
            Close();
            Bitmap.Clear();

            try
            {
                FileWriter.Dispose();
            }
            catch
            {

            }
            try
            {
                StorageFile file = await Path.GetFileAsync(Name);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch
            {

            }
        }
        public void FinishUpload()
        {
            Close();
            Bitmap.Clear();
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
        private async void WriteBlock(int block, byte[] buffer)
        {
            FileWriter.Position = Math.Abs(block) << 12;
            await FileWriter.WriteAsync(buffer, 0, buffer.Length);
        }

        public override StreamType GetStreamType() => StreamType.Download;
        public override string GetTime() => StartTime;
        public override string GetName() => Name;
        public override int GetSize() => Length;
        public override string GetSchedult() => string.Format("{0:P}", Bitmap.FinishRate());
    }
}
