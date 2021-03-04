using IKXTServer;
using KXTNetStruct;
using KXTServiceDBServer.Cloud;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace 快迅通_文件服务器.Server
{
    internal class MainConnect
    {
        private readonly string FileRootPath;

        private readonly Socket MainServer;
        private readonly byte[] Buffer;

        private readonly Action<LogLevel, string> Notify;
        private readonly Action CloseServer;

        private readonly Timer Timer;
        private bool HeartTest;

        private readonly CloudInfoReader InfoReader;

        private readonly ConcurrentDictionary<Guid, StreamBase> FileCache;
        private readonly Timer CacheTime;

        private bool IsClosed;

        public MainConnect
            (
            Socket main_server,
            string info_root_path,
            string file_root_path,
            Action<LogLevel, string> notify,
            Action close
            )
        {
            IsClosed = false;

            InfoReader = new CloudInfoReader(info_root_path, Notify);
            FileCache = new ConcurrentDictionary<Guid, StreamBase>();

            FileRootPath = file_root_path;
            Notify = notify;

            CloseServer = close;

            HeartTest = false;
            Timer = new Timer
            {
                Interval = HeartRequestInterval,
                AutoReset = false
            };
            Timer.Elapsed += HeartRequest_Trigger;
            Timer.Start();

            CacheTime = new Timer
            {
                Interval = CacheVaildTime,
                AutoReset = false
            };
            CacheTime.Elapsed += CacheFresh_Trigger;
            CacheTime.Start();

            MainServer = main_server;
            Buffer = new byte[Datagram.DatagramLengthMax];
            MainServer.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
        }

        public void Stop()
        {
            if (!IsClosed)
            {
                MainServer.Shutdown(SocketShutdown.Both);
                MainServer.Dispose();

                Timer.Stop();
                CacheTime.Close();

                InfoReader.Close();
                foreach (var item in FileCache)
                    item.Value.Close();
            }

            IsClosed = true;
        }

        private void Close()
        {
            Stop();
            CloseServer();
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int length = MainServer.EndReceive(ar);
                if (Datagram.DatagramLengthMin <= length)
                {
                    Action<byte[], int> action = OnProcess;
                    byte[] temp = new byte[length];
                    Array.Copy(Buffer, 0, temp, 0, length);
                    action.BeginInvoke(temp, length, EndProcess, action);
                }
            }
            catch
            {
                Notify(IKXTServer.LogLevel.Warning, "消息接收异常");
            }

            try
            {
                if (!IsClosed)
                    MainServer.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
            }
            catch
            {
                Notify(IKXTServer.LogLevel.Error, "套接字启动接收异常");
                Close();
            }
        }
        private void OnProcess(byte[] buffer, int count)
        {
            Datagram datagram = new Datagram();
            if (!datagram.FromBytes_S(buffer, 0))
            {
                Notify(LogLevel.Info, "无效的数据：" + Encoding.UTF8.GetString(buffer));
                return;
            }

            if (DatagramType.Main == datagram.DataType)
                if (MainMessageType.HeartResponse == datagram.MessageType)
                {
                    HeartTest = false;
                    return;
                }

            switch (datagram.MessageType)
            {
                case CloudDatagramDefine.CloudRequest:
                    OnCloudRequest(datagram);
                    break;
                case CloudDatagramDefine.CreateFolder:
                    OnCreateFolder(datagram);
                    break;
                case CloudDatagramDefine.DeleteFolder:
                    OnDeleteFolder(datagram);
                    break;
                case CloudDatagramDefine.DeleteFile:
                    OnDeleteFile(datagram);
                    break;
                case CloudDatagramDefine.FileUploadReq:
                    OnFileUploadReq(datagram);
                    break;
                case CloudDatagramDefine.FileDownloadReq:
                    OnFileDownloadReq(datagram);
                    break;
                case CloudDatagramDefine.StreamReq:
                    OnStreamReq(datagram);
                    break;
                case CloudDatagramDefine.StreamRes:
                    OnStreamRes(datagram);
                    break;
                case CloudDatagramDefine.FileDownloadFinish:
                    OnFileDownloadFinish(datagram);
                    break;
                case CloudDatagramDefine.FileUploadCancel:
                    OnFileUploadCancel(datagram);
                    break;
            }
        }
        private void EndProcess(IAsyncResult ar)
        {
            if (null != ar && null != ar.AsyncState)
                (ar.AsyncState as Action<byte[]>).EndInvoke(ar);
        }

        private bool Send(byte[] datas)
        {
            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    MainServer.Send(datas);
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        private void HeartRequest_Trigger(object sender, ElapsedEventArgs args)
        {
            // 发送心跳连接
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Main,
                MessageType = MainMessageType.HeartRequest,
                Time = DateTime.Now,
                RequestID = Guid.Empty,
                Sender = Guid.Empty,
                Datas = null
            };
            if (Send(datagram.ToByteArray()))
            {
                HeartTest = true;
                Timer.Elapsed -= HeartRequest_Trigger;
                Timer.Elapsed += HeartResponse_Trigger;
                Timer.Interval = HeartResponseInterval;
                Timer.Start();
            }
            else
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void HeartResponse_Trigger(object sender, ElapsedEventArgs args)
        {
            if (HeartTest)
            {
                Notify(IKXTServer.LogLevel.Error, "心跳连接超时");
                Close();
            }
            else
            {
                Timer.Elapsed -= HeartResponse_Trigger;
                Timer.Elapsed += HeartRequest_Trigger;
                Timer.Interval = HeartRequestInterval;
                Timer.Start();
            }
        }

        public const double HeartRequestInterval = 5000;
        public const double HeartResponseInterval = 2000;

        private void CacheFresh_Trigger(object sender, ElapsedEventArgs args)
        {
            if (0 < FileCache.Count)
            {
                foreach (var item in FileCache)
                {
                    item.Value.Flush();

                    if (item.Value.Invaild)
                    {
                        item.Value.Close();
                        FileCache.TryRemove(item.Key, out _);
                    }
                    else if ((DateTime.Now - item.Value.Time).TotalMilliseconds > CacheVaildTime)
                        item.Value.Invaild = true;
                }
            }

            CacheTime.Start();
        }

        private const double CacheVaildTime = 30000;

        private void OnCloudRequest(Datagram datagram)
        {
            CloudRequest request = datagram.UnSerialData<CloudRequest>();

            CloudResponse response = new CloudResponse();
            response.Files.AddRange
                (
                InfoReader.ReadAll
                (
                IKXTServer.DataConvert.GetString(datagram.Sender),
                request.Path
                )
                );

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = CloudDatagramDefine.CloudResponse;

            byte[][] buffer = response.ToByteArrays();
            for (int i = 0; i < buffer.Length; ++i)
            {
                datagram.Datas = buffer[i];
                if (!Send(datagram.ToByteArray()))
                {
                    Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                    Close();
                    return;
                }
            }

            datagram.MessageType = CloudDatagramDefine.CloudResFinish;
            datagram.Datas = null;
            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnCreateFolder(Datagram datagram)
        {
            CreateFolder create = datagram.UnSerialData<CreateFolder>();

            try
            {
                string path = FileRootPath + "\\" + datagram.Sender.ToString();
                path += ("" == create.Path ? "" : create.Path + "\\") + create.Name;
                System.IO.Directory.CreateDirectory(path);
                InfoReader.AddDir
                    (
                    IKXTServer.DataConvert.GetString(datagram.Sender),
                    create.Path,
                    create.Name
                    );
            }
            catch
            {

            }
        }
        private void OnDeleteFolder(Datagram datagram)
        {
            DeleteFolderFile delete = datagram.UnSerialData<DeleteFolderFile>();

            try
            {
                string path = FileRootPath + "\\" + datagram.Sender.ToString();
                path += ("" == delete.Path ? "" : delete.Path + "\\") + delete.Name;
                System.IO.Directory.Delete(path);
                InfoReader.DelDir
                    (
                    IKXTServer.DataConvert.GetString(datagram.Sender),
                    delete.Path,
                    delete.Name
                    );
            }
            catch
            {

            }
        }
        private void OnDeleteFile(Datagram datagram)
        {
            DeleteFolderFile delete = datagram.UnSerialData<DeleteFolderFile>();

            try
            {
                string path = FileRootPath + "\\" + datagram.Sender.ToString();
                path += ("" == delete.Path ? "" : delete.Path + "\\") + delete.Name;
                System.IO.File.Delete(path);
                InfoReader.DelFile
                    (
                    IKXTServer.DataConvert.GetString(datagram.Sender),
                    delete.Path,
                    delete.Name
                    );
            }
            catch
            {

            }
        }
        private void OnFileUploadReq(Datagram datagram)
        {
            FileUploadReq req = datagram.UnSerialData<FileUploadReq>();

            Upload upload = new Upload(FileRootPath + "\\" + datagram.Sender.ToString(), req.Path, req.Name, req.Size);
            Guid upload_id = Guid.NewGuid();

            if (FileCache.TryAdd(upload_id, upload))
            {
                FileUploadRes res = new FileUploadRes
                {
                    UploadID = upload_id
                };
                datagram.DataType = DatagramType.Client;
                datagram.MessageType = CloudDatagramDefine.FileUploadRes;

                datagram.Datas = res.ToByteArray();
                if (!Send(datagram.ToByteArray()))
                {
                    Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                    Close();
                }
            }
            else
                upload.Clear();
        }
        private void OnFileDownloadReq(Datagram datagram)
        {
            FileDownloadReq req = datagram.UnSerialData<FileDownloadReq>();

            string path = FileRootPath + "\\" + datagram.Sender.ToString();
            path += ("" == req.Path ? "" : req.Path + "\\") + req.Name;
            Download download = new Download(path);
            Guid download_id = Guid.NewGuid();

            if (FileCache.TryAdd(download_id, download))
            {
                FileDownloadRes res = new FileDownloadRes
                {
                    DownloadID = download_id,
                    Size = (int)download.Length
                };
                datagram.DataType = DatagramType.Client;
                datagram.MessageType = CloudDatagramDefine.FileDownloadRes;

                datagram.Datas = res.ToByteArray();
                if (!Send(datagram.ToByteArray()))
                {
                    Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                    Close();
                }
            }
            else
                download.Close();
        }
        private void OnStreamReq(Datagram datagram)
        {
            StreamReq req = datagram.UnSerialData<StreamReq>();

            if (FileCache.TryGetValue(req.StreamID, out StreamBase value))
            {
                if (StreamType.Download == value.GetStreamType())
                {
                    Download download = value as Download;

                    StreamRes res = new StreamRes
                    {
                        StreamID = req.StreamID,
                        Block = req.Block,
                        Stream = download.ReadBlock(req.Block)
                    };

                    datagram.DataType = DatagramType.Client;
                    datagram.MessageType = CloudDatagramDefine.StreamRes;
                    datagram.Datas = res.ToByteArray();

                    if (!Send(datagram.ToByteArray()))
                    {
                        Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                        Close();
                    }
                }
            }
        }
        private void OnStreamRes(Datagram datagram)
        {
            StreamRes res = datagram.UnSerialData<StreamRes>();

            if (FileCache.TryGetValue(res.StreamID, out StreamBase value))
            {
                if (StreamType.Upload == value.GetStreamType())
                {
                    Upload upload = value as Upload;

                    int block = upload.ReceiveBlock(res.Block, res.Stream);

                    if (-1 != block)
                    {
                        StreamReq req = new StreamReq
                        {
                            StreamID = res.StreamID,
                            Block = block
                        };

                        datagram.DataType = DatagramType.Client;
                        datagram.MessageType = CloudDatagramDefine.StreamReq;
                        datagram.Datas = req.ToByteArray();
                    }
                    else
                    {
                        upload.FinishUpload();
                        InfoReader.AddFile
                            (
                            IKXTServer.DataConvert.GetString(datagram.Sender),
                            upload.Path,
                            upload.Name,
                            upload.Length
                            );

                        FileCache.TryRemove(res.StreamID, out _);

                        FileUploadFinish finish = new FileUploadFinish
                        {
                            UploadID = res.StreamID
                        };

                        datagram.DataType = DatagramType.Client;
                        datagram.MessageType = CloudDatagramDefine.FileUploadFinish;
                        datagram.Datas = finish.ToByteArray();
                    }

                    if (!Send(datagram.ToByteArray()))
                    {
                        Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                        Close();
                    }
                }
            }
        }
        private void OnFileDownloadFinish(Datagram datagram)
        {
            FileDownloadFinish finish = datagram.UnSerialData<FileDownloadFinish>();

            if (FileCache.TryGetValue(finish.DownloadID, out StreamBase value))
            {
                if (StreamType.Download == value.GetStreamType())
                {
                    Download download = value as Download;

                    download.Close();
                    FileCache.TryRemove(finish.DownloadID, out _);
                }
            }
        }
        private void OnFileUploadCancel(Datagram datagram)
        {
            FileUploadCancel cancel = datagram.UnSerialData<FileUploadCancel>();

            if (FileCache.TryGetValue(cancel.UploadID, out StreamBase value))
            {
                if (StreamType.Upload == value.GetStreamType())
                {
                    (value as Upload).Clear();
                    FileCache.TryRemove(cancel.UploadID, out _);
                }
            }
        }
    }
}
