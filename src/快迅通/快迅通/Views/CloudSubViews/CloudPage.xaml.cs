using KXTNetStruct;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Timers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Dialogs;
using 快迅通.Server.FileService;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 快迅通.Views.CloudSubViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CloudPage : Page, RequestSender
    {
        private readonly Guid RequestID;
        private readonly List<string> FolderTree;
        private readonly Timer OperationTimer;

        private bool Waiting;

        public CloudPage()
        {
            RequestID = Guid.NewGuid();
            FolderTree = new List<string>();

            this.InitializeComponent();
            ToolOperationBar.Background = Utils.Utils.GetViewPre();

            OperationTimer = new Timer
            {
                Interval = OperationTimerInterval,
                AutoReset = false
            };
            OperationTimer.Elapsed += Operation_Trigger;

            RequestFiles();
        }

        private void RequestFiles()
        {
            RunningDatas.RequestTable.TryAdd(RequestID, this);
            OperationTimer.Start();
            OperationWaiting.IsIndeterminate = true;
            RunningDatas.DataSender.CloudRequest(RequestID, new CloudRequest
            {
                Path = FolderPath.Text
            });
        }

        private void FilesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 < FilesGridView.SelectedItems.Count)
            {
                int size = 0;
                foreach (var i in FilesGridView.SelectedItems)
                    size += (i as TemporaryFilesList).Size;
                _File_Size_Units_ unit = Utils.Utils.PraseFileSizeUnit(size, out double s);
                string str = "";
                switch (unit)
                {
                    case _File_Size_Units_._G_Bytes_:
                        str = "GB";
                        break;
                    case _File_Size_Units_._K_Bytes_:
                        str = "KB";
                        break;
                    case _File_Size_Units_._M_Bytes_:
                        str = "MB";
                        break;
                    default:
                        str = "字节";
                        break;
                }
                SelectedFileSize.Text = s.ToString("f6").Substring(0,4) + str;
            }
            else
                SelectedFileSize.Text = "0字节";
        }

        private void BulletsButton_Click(object sender, RoutedEventArgs e)
        {
            FilesGridView.IsMultiSelectCheckBoxEnabled = !FilesGridView.IsMultiSelectCheckBoxEnabled;
            FilesGridView.SelectionMode = FilesGridView.IsMultiSelectCheckBoxEnabled ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!OperationWaiting.IsIndeterminate)
            {
                if (ListViewSelectionMode.Single == FilesGridView.SelectionMode)
                {
                    if (-1 != FilesGridView.SelectedIndex)
                    {
                        TemporaryFilesList file = FilesGridView.SelectedItem as TemporaryFilesList;
                        if (FileType.Folder == file.Type)
                            RunningDatas.DataSender.DeleteFolder(new DeleteFolderFile
                            {
                                Path = FolderPath.Text,
                                Name = file.Name
                            });
                        else
                            RunningDatas.DataSender.DeleteFile(new DeleteFolderFile
                            {
                                Path = FolderPath.Text,
                                Name = file.Name
                            });
                        RequestFiles();
                    }
                    else
                        RunningDatas.ErrorNotify("请选择需要删除的文件或文件夹");
                }
                else
                {
                    if (0 != FilesGridView.SelectedItems.Count)
                    {
                        foreach (var i in FilesGridView.SelectedItems)
                        {
                            TemporaryFilesList item = i as TemporaryFilesList;

                            if (FileType.Folder == item.Type)
                                RunningDatas.DataSender.DeleteFolder(new DeleteFolderFile
                                {
                                    Path = FolderPath.Text,
                                    Name = item.Name
                                });
                            else
                                RunningDatas.DataSender.DeleteFile(new DeleteFolderFile
                                {
                                    Path = FolderPath.Text,
                                    Name = item.Name
                                });
                            RequestFiles();
                        }
                    }
                    else
                        RunningDatas.ErrorNotify("请选择需要删除的文件或文件夹");
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!OperationWaiting.IsIndeterminate)
                RequestFiles();
        }
        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");

            IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();

            if (null != files && 0 < files.Count)
            {
                foreach (var file in files)
                {
                    int size = 0;
                    using (var reader = await file.OpenStreamForReadAsync())
                    {
                        size = (int)reader.Length;
                    }

                    RunningDatas.FileWaiting.Add
                        (
                        FolderPath.Text,
                        file.Name,
                        new FileReqPackage
                        {
                            FileName = file.Name,
                            Path = await file.GetParentAsync(),
                            Size = size,
                            Type = StreamType.Upload
                        }
                        );
                }
            }
        }
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!OperationWaiting.IsIndeterminate)
            {
                if (ListViewSelectionMode.Single == FilesGridView.SelectionMode)
                {
                    if (-1 != FilesGridView.SelectedIndex)
                    {
                        TemporaryFilesList file = FilesGridView.SelectedItem as TemporaryFilesList;
                        if (FileType.Folder == file.Type)
                        {
                            RunningDatas.ErrorNotify("不能下载文件夹");
                            return;
                        }

                        StorageFolder folder = await GetFolder();

                        if (null == folder)
                            return;

                        RunningDatas.FileWaiting.Add
                            (
                            FolderPath.Text,
                            file.Name,
                            new FileReqPackage
                            {
                                FileName = file.Name,
                                Path = folder,
                                Size = file.Size,
                                Type = StreamType.Download
                            }
                            );
                    }
                    else
                        RunningDatas.ErrorNotify("请选择需要下载的文件");
                }
                else
                {
                    if (0 != FilesGridView.SelectedItems.Count)
                    {
                        StorageFolder folder = await GetFolder();

                        if (null == folder)
                            return;

                        foreach (var i in FilesGridView.SelectedItems)
                        {
                            TemporaryFilesList item = i as TemporaryFilesList;

                            if (FileType.Folder == item.Type)
                            {
                                RunningDatas.InfoNotify("不能下载文件夹：" + item.Name);
                                return;
                            }

                            RunningDatas.FileWaiting.Add
                                (
                                FolderPath.Text,
                                item.Name,
                                new FileReqPackage
                                {
                                    FileName = item.Name,
                                    Path = folder,
                                    Size = item.Size,
                                    Type = StreamType.Download
                                }
                                );
                        }
                    }
                    else
                        RunningDatas.ErrorNotify("请选择需要下载的文件");
                }
            }
        }
        private async Task<StorageFolder> GetFolder()
        {
            FolderPicker folder = new FolderPicker();
            folder.FileTypeFilter.Add("*");

            return await folder.PickSingleFolderAsync();
        }
        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!OperationWaiting.IsIndeterminate)
            {
                FolderNamePakcage name_package = new FolderNamePakcage
                {
                    Name = "",
                    OKState = false
                };
                _ = new CloudNewFolder(name_package, FolderNameCheck).ShowAsync();

                if (name_package.OKState)
                {
                    RunningDatas.DataSender.CreateFolder(new CreateFolder
                    {
                        Path = FolderPath.Text,
                        Name = name_package.Name
                    });

                    RequestFiles();
                }
            }
        }
        private bool FolderNameCheck(string name)
        {
            foreach (var item in FilesGridView.Items)
            {
                TemporaryFilesList file = item as TemporaryFilesList;

                if (FileType.Folder == file.Type)
                    if (name == file.Name)
                        return false;
            }

            return true;
        }

        private void BehindButton_Click(object sender, RoutedEventArgs e)
        {
            if (!OperationWaiting.IsIndeterminate)
            {
                FolderTree.RemoveAt(FolderTree.Count - 1);

                string path = "";
                foreach (string i in FolderTree)
                    path += i + "\\";
                FolderPath.Text = path;
                if (0 == FolderTree.Count)
                    BehindButton.IsEnabled = false;

                RequestFiles();
            }
        }
        private void FilesGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!OperationWaiting.IsIndeterminate && null != e.OriginalSource)
            {
                FrameworkElement element= (FrameworkElement)e.OriginalSource;
                TemporaryFilesList file = (TemporaryFilesList)element.DataContext;
                if (FileType.Folder == file.Type)
                {
                    BehindButton.IsEnabled = true;
                    FolderTree.Add(file.Name);
                    string path = "\\";
                    foreach (string i in FolderTree)
                        path += i + "\\";
                    FolderPath.Text = path;

                    RequestFiles();
                }
            }
        }

        public void RequestCallback(object response)
        {
            if (null == response)
            {
                _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    OperationWaiting.IsIndeterminate = false;
                }));
                return;
            }
            else
            {

            }
        }

        private void Operation_Trigger(object sender, ElapsedEventArgs args)
        {
            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    OperationWaiting.IsIndeterminate = false;
                }));

            RunningDatas.InfoNotify("数据请求超时");
        }

        private const double OperationTimerInterval = 30000;
    }
}
