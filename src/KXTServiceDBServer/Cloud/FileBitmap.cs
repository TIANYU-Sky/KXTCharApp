using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KXTServiceDBServer.Cloud
{
    public enum FileBitmapState
    {
        UnSet,
        Setted
    }

    public class FileBitmap
    {
        private readonly int BitCount;
        private readonly byte[] Bitmap;

        private readonly FileStream Stream;

        public FileBitmap(string path, int bits)
        {
            if (0 > bits)
                bits = 0;

            BitCount = bits;
            Bitmap = new byte[(bits >> 3) + ((bits & 7) != 0 ? 1 : 0)];

            if (System.IO.File.Exists(path))
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                try
                {
                    byte[] temp = new byte[4];
                    file.Read(temp, 0, 4);
                    int count = BitConverter.ToInt32(temp, 0);

                    if (count == bits)
                    {
                        file.Read(Bitmap, 0, Bitmap.Length);
                        Stream = file;
                        return;
                    }
                    else
                        throw new Exception();
                }
                catch
                {
                    file.Close();
                    System.IO.File.Delete(path);
                }
            }

            Stream = System.IO.File.Create(path);
            Stream.Write(BitConverter.GetBytes(BitCount), 0, 4);
            Stream.Write(Bitmap, 0, Bitmap.Length);
            Stream.Flush();
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
        public void Flush()
        {
            Stream.Position = 4;
            Stream.Write(Bitmap, 0, Bitmap.Length);
            Stream.Flush();
        }
        public void Close()
        {
            Flush();
            Stream.Close();
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
