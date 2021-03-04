using KXTNetStruct;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using 快迅通.Utils;
using 快迅通.Views.CloudSubViews;

namespace 快迅通.Server.FileService
{
    internal class FileService
    {
        public int WorkingCount => FileCache.Count;

        private readonly TransPage FileStreamPage;

        private readonly ConcurrentDictionary<Guid, StreamBase> FileCache;
        private readonly System.Timers.Timer CacheTimer;
        private readonly System.Timers.Timer UpdateTimer;

        public FileService(TransPage page)
        {
            FileStreamPage = page;

            FileCache = new ConcurrentDictionary<Guid, StreamBase>();

            CacheTimer = new Timer
            {
                Interval = CacheVaildTime,
                AutoReset = false
            };
            CacheTimer.Elapsed += CacheFresh_Trigger;
            CacheTimer.Start();

            UpdateTimer = new Timer
            {
                Interval = UpdateUITime,
                AutoReset = false
            };
            UpdateTimer.Elapsed += UpdateUI_Trigger;
            UpdateTimer.Start();
        }

        public TemporaryStreamItem[] GetStreams
        {
            get
            {
                List<TemporaryStreamItem> buffer = new List<TemporaryStreamItem>();

                foreach (var item in FileCache)
                {
                    buffer.Add(new TemporaryStreamItem
                    {
                        StreamID = item.Key,
                        Name = item.Value.GetName(),
                        Size = item.Value.GetSize(),
                        Schedult = item.Value.GetSchedult(),
                        Time = item.Value.GetTime(),
                        Type = item.Value.GetStreamType() == StreamType.Download ? "下载" : "上传"
                    });
                }

                return buffer.ToArray();
            }
        }

        public void CreateDownload(Guid stream_id, StorageFolder path, string name, int length)
        {
            Download download;
            try
            {
                download = new Download(path, name, length);
            }
            catch
            {
                RunningDatas.ErrorNotify
                            (
                            string.Format
                            (
                                " 创建下载任务失败：{0} \n\r 无法写入到文件",
                                name
                                )
                            );
                return;
            }

            if (!FileCache.TryAdd(stream_id, download))
            {
                download.Clear();
                RunningDatas.ErrorNotify
                            (
                            string.Format
                            (
                                " 创建下载任务失败：{0} ",
                                name
                                )
                            );
            }
        }
        public void CreateUpload(Guid stream_id, StorageFile file)
        {
            Upload upload = new Upload(file);

            if (!FileCache.TryAdd(stream_id, upload))
            {
                upload.Close();
                RunningDatas.ErrorNotify
                            (
                            string.Format
                            (
                                " 创建上传任务失败：{0} ",
                                file.Path + "\\" + file.Name
                                )
                            );
            }
        }
        public void FinishUpload(Guid upload_id)
        {
            if (FileCache.TryGetValue(upload_id, out StreamBase value))
            {
                if (StreamType.Upload == value.GetStreamType())
                {
                    value.Close();
                    FileCache.TryRemove(upload_id, out _);
                }
            }
        }

        public void Receive(Guid stream_id, int block, byte[] datas)
        {
            if (FileCache.TryGetValue(stream_id, out StreamBase value))
            {
                if (StreamType.Download == value.GetStreamType())
                {
                    int next = (value as Download).ReceiveBlock(block, datas);

                    if (-1 == next)
                    {
                        RunningDatas.DataSender.FileDownloadFinish(new KXTNetStruct.FileDownloadFinish
                        {
                            DownloadID = stream_id
                        });
                    }
                    else
                    {
                        RunningDatas.DataSender.FileStreamReq(new KXTNetStruct.StreamReq
                        {
                            StreamID = stream_id,
                            Block = next
                        });
                    }
                }
            }
        }
        public async void Request(Guid stream_id, int block)
        {
            if (FileCache.TryGetValue(stream_id, out StreamBase value))
            {
                if (StreamType.Upload == value.GetStreamType())
                {
                    RunningDatas.DataSender.FileStreamRes(new KXTNetStruct.StreamRes
                    {
                        StreamID = stream_id,
                        Block = block,
                        Stream = await (value as Upload).ReadBlock(block)
                    });
                }
            }
        }

        public void Cancel(Guid stream_id)
        {
            if (FileCache.TryRemove(stream_id, out StreamBase value))
            {
                if (StreamType.Download == value.GetStreamType())
                    CancelDownload(stream_id, value);
                else
                    CancelUpload(stream_id, value);
            }
        }
        private void CancelDownload(Guid stream_id, StreamBase value)
        {
            (value as Download).Clear();

            RunningDatas.DataSender.FileDownloadFinish(new KXTNetStruct.FileDownloadFinish
            {
                DownloadID = stream_id
            });
        }
        private void CancelUpload(Guid stream_id, StreamBase value)
        {
            value.Close();

            RunningDatas.DataSender.FileUploadCancel(new KXTNetStruct.FileUploadCancel
            {
                UploadID = stream_id
            });
        }

        private void CacheFresh_Trigger(object sender, System.Timers.ElapsedEventArgs args)
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

                        RunningDatas.ErrorNotify
                            (
                            string.Format
                            (
                                " {0}超时 \n\r 文件：{1} ",
                                item.Value.GetStreamType() == StreamType.Download ? "下载" : "上传",
                                item.Value.GetName()
                                )
                            );
                    }
                    else if ((DateTime.Now - item.Value.Time).TotalMilliseconds > CacheVaildTime)
                        item.Value.Invaild = true;
                }
            }

            CacheTimer.Start();
        }
        private void UpdateUI_Trigger(object sender, System.Timers.ElapsedEventArgs args)
        {
            FileStreamPage.UpdateUI(GetStreams);

            UpdateTimer.Start();
        }

        private const double CacheVaildTime = 30000;
        private const double UpdateUITime = 2000;
    }

    internal class FileReqPackage
    {
        public string FileName;
        public StorageFolder Path;
        public StreamType Type;
        public int Size;
    }

    internal class FileWaiting : FileWaiting.IFileWaitingForObject
    {
        private readonly TransPage FileStreamPage;
        private readonly ConcurrentDictionary<Guid, WaitingObject> StreamRequest;
        private readonly System.Timers.Timer CheckTimer;

        public FileWaiting(TransPage page)
        {
            FileStreamPage = page;
            StreamRequest = new ConcurrentDictionary<Guid, WaitingObject>();

            CheckTimer = new Timer
            {
                Interval = 2000,
                AutoReset = false
            };
            CheckTimer.Elapsed += Timer_Trigger;
            CheckTimer.Start();
        }

        public void Add(string cloud_path, string cloud_name, FileReqPackage file)
        {
            Guid id = Guid.NewGuid();

            StreamRequest.TryAdd(id, new WaitingObject(id, cloud_path, cloud_name, file, this));

            UpdateUI();
        }
        public void Del(Guid id)
        {
            if (StreamRequest.TryRemove(id, out WaitingObject value))
            {
                value.Cancel();
                UpdateUI();
            }
        }

        void IFileWaitingForObject.ToStartDownload(Guid stream_id, FileReqPackage file, Guid waiting)
        {
            StreamRequest.TryRemove(waiting, out _);

            RunningDatas.FileService.CreateDownload(stream_id, file.Path, file.FileName, file.Size);

            UpdateUI();
        }
        async void IFileWaitingForObject.ToStartUpload(Guid stream_id, FileReqPackage file, Guid waiting)
        {
            StreamRequest.TryRemove(waiting, out _);

            RunningDatas.FileService.CreateUpload(stream_id, await file.Path.GetFileAsync(file.FileName));

            UpdateUI();
        }

        private void UpdateUI()
        {
            List<TemporaryWaitingObject> buffer = new List<TemporaryWaitingObject>();
            foreach (var item in StreamRequest)
            {
                buffer.Add(new TemporaryWaitingObject
                {
                    ID = item.Key,
                    Name = item.Value.File.FileName,
                    FileType = CloudFile.GetTypeFromExtension(item.Value.File.FileName),
                    Size = item.Value.File.Size,
                    Type = StreamType.Download == item.Value.File.Type ? Symbol.Download : Symbol.Upload
                });
            }
            FileStreamPage.UpdateUI(buffer.ToArray());
        }

        private void Timer_Trigger(object sender, ElapsedEventArgs args)
        {
            if (0 < StreamRequest.Count && 5 > RunningDatas.FileService.WorkingCount)
            {
                int i = RunningDatas.FileService.WorkingCount;
                
                foreach (var item in StreamRequest)
                {
                    if (5 <= i)
                        break;

                    if (!item.Value.IsActive)
                        item.Value.ToStart();

                    ++i;
                }
            }
        }

        internal interface IFileWaitingForObject
        {
            void ToStartDownload(Guid stream_id, FileReqPackage file, Guid waiting);
            void ToStartUpload(Guid stream_id, FileReqPackage file, Guid waiting);
        }

        internal class WaitingObject : RequestSender
        {
            private static FileUploadRes FileUploadResTemp = new FileUploadRes();
            private static FileDownloadRes FileDownloadResTemp = new FileDownloadRes();

            public bool IsActive;
            public readonly FileReqPackage File;

            private readonly Guid ID;
            private readonly string CloudPath;
            private readonly string CloudName;

            private readonly Timer WaitTimer;

            private readonly IFileWaitingForObject Root;

            public WaitingObject(Guid id, string cloud_path, string cloud_name, FileReqPackage file, IFileWaitingForObject root)
            {
                ID = id;
                CloudPath = cloud_path;
                CloudName = cloud_name;
                File = file;
                Root = root;

                IsActive = false;

                WaitTimer = new Timer
                {
                    Interval = 30000,
                    AutoReset = false
                };
                WaitTimer.Elapsed += Timer_Trigger;
            }

            public void ToStart()
            {
                IsActive = true;
                WaitTimer.Start();

                RunningDatas.RequestTable.TryAdd(ID, this);

                if (StreamType.Download == File.Type)
                    RunningDatas.DataSender.FileDownload(ID, new KXTNetStruct.FileDownloadReq
                    {
                        Path = CloudPath,
                        Name = CloudName
                    });
                else
                    RunningDatas.DataSender.FileUpload(ID, new KXTNetStruct.FileUploadReq
                    {
                        Name = File.FileName,
                        Path = CloudPath,
                        Size = File.Size
                    });
            }
            public void Cancel()
            {
                RunningDatas.RequestTable.TryRemove(ID, out _);
            }

            public void RequestCallback(object response)
            {
                if (null != response)
                {
                    if (response.GetType() == FileUploadResTemp.GetType())
                    {
                        WaitTimer.Stop();

                        FileUploadRes res = response as FileUploadRes;

                        Root.ToStartUpload(res.UploadID, File, ID);
                    }
                    else if (response.GetType() == FileDownloadResTemp.GetType())
                    {
                        WaitTimer.Stop();

                        FileDownloadRes res = response as FileDownloadRes;

                        File.Size = res.Size;
                        Root.ToStartDownload(res.DownloadID, File, ID);
                    }
                }
            }

            private void Timer_Trigger(object sender, ElapsedEventArgs args)
            {
                IsActive = false;
                RunningDatas.InfoNotify("文件请求超时：" + File.FileName);
            }
        }
    }
}
