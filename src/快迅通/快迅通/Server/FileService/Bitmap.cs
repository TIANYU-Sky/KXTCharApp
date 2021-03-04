using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace 快迅通.Server.FileService
{
    internal enum FileBitmapState
    {
        UnSet,
        Setted
    }

    internal class FileBitmap
    {
        private readonly int BitCount;
        private readonly byte[] Bitmap;
        private readonly StorageFolder Path;
        private readonly string Name;

        private Stream Stream;

        public FileBitmap(StorageFolder path, string name, int bits)
        {
            Path = path;
            Name = name + ".bitmap";

            if (0 > bits)
                bits = 0;

            BitCount = (int)((bits >> 12) + ((bits & 4095) != 0 ? 1 : 0));
            Bitmap = new byte[(BitCount >> 3) + ((BitCount & 7) != 0 ? 1 : 0)];

            Open();
        }
        private async void Open()
        {
            StorageFile file;
            if (null != await Path.TryGetItemAsync(Name))
            {
                file = await Path.GetFileAsync(Name);

                bool error = false;
                using (Stream reader = await file.OpenStreamForReadAsync())
                {
                    byte[] temp = new byte[4];
                    await reader.ReadAsync(temp, 0, 4);

                    int count = BitConverter.ToInt32(temp, 0);
                    if (count == BitCount)
                    {
                        reader.Position = 4;
                        await reader.ReadAsync(Bitmap, 0, Bitmap.Length);
                    }
                    else
                        error = true;
                }
                if (error)
                    file = await Path.CreateFileAsync(Name, CreationCollisionOption.ReplaceExisting);
            }
            else
                file = await Path.CreateFileAsync(Name);

            Stream = await file.OpenStreamForWriteAsync();
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
        public int GetBlock(FileBitmapState state = FileBitmapState.UnSet)
        {
            int block = -1;

            switch (state)
            {
                case FileBitmapState.UnSet:
                    for (int i = 0; i < BitCount; ++i)
                    {
                        if ((Bitmap[i >> 8] & (1 << (i & 7))) == 0)
                        {
                            block = i;
                            break;
                        }
                    }
                    break;
                case FileBitmapState.Setted:
                    for (int i = 0; i < BitCount; ++i)
                    {
                        if ((Bitmap[i >> 8] & (1 << (i & 7))) == 1)
                        {
                            block = i;
                            break;
                        }
                    }
                    break;
            }

            return block;
        }
        public async void Flush()
        {
            Stream.Position = 0;
            await Stream.WriteAsync(BitConverter.GetBytes(BitCount), 0, 4);
            Stream.Position = 4;
            await Stream.WriteAsync(Bitmap, 0, Bitmap.Length);
            await Stream.FlushAsync();
        }
        public void Close()
        {
            Flush();
            Stream.Dispose();
        }
        public async void Clear()
        {
            try
            {
                Stream.Dispose();
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

        public FileBitmapState this[int index]
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
    }
}
