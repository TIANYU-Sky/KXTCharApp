using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 快迅通.Views.CloudSubViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TransPage : Page
    {
        public TransPage()
        {
            this.InitializeComponent();
            ToolOperationBar.Background = Utils.Utils.GetViewPre();
            FilesListViewHead.Background = Utils.Utils.GetViewPre();

            RunningDatas.FileService = new Server.FileService.FileService(this);
            RunningDatas.FileWaiting = new Server.FileService.FileWaiting(this);
        }

        public void UpdateUI(TemporaryStreamItem[] items)
        {
            if (null != items)
                _ = Dispatcher.RunAsync
                    (
                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                    new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        FilesListView.ItemsSource = items;
                    }));
        }
        public void UpdateUI(TemporaryWaitingObject[] items)
        {
            if (null != items)
                _ = Dispatcher.RunAsync
                    (
                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                    new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        WaitingGridView.ItemsSource = items;
                    }));
        }

        private void SymbolIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TextBlock temp = new TextBlock();
            string stream_id = "";
            foreach (var item in (sender as Grid).Children)
            {
                if (item.GetType() == temp.GetType())
                {
                    stream_id = (item as TextBlock).Text;
                    break;
                }
            }

            if ("" == stream_id)
                return;

            RunningDatas.FileService.Cancel(new Guid(stream_id));
        }
        private void FreshButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FilesListView.ItemsSource = RunningDatas.FileService.GetStreams;
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TextBlock temp = new TextBlock();
            string stream_id = "";
            foreach (var item in (sender as Grid).Children)
            {
                if (item.GetType() == temp.GetType())
                {
                    stream_id = (item as TextBlock).Text;
                    break;
                }
            }

            if ("" == stream_id)
                return;

            RunningDatas.FileWaiting.Del(new Guid(stream_id));
        }
    }
}
