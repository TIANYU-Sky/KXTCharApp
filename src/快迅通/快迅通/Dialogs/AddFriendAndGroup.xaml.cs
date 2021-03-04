using KXTNetStruct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Controls.Magnets;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 快迅通.Dialogs
{
    public sealed partial class AddFriendAndGroup : ContentDialog, RequestSender
    {
        private readonly Guid RequestID;
        private readonly System.Timers.Timer OperationTimer;

        public AddFriendAndGroup()
        {
            RequestID = Guid.NewGuid();

            this.InitializeComponent();

            OperationTimer = new System.Timers.Timer
            {
                Interval = OperationTimeInterval,
                AutoReset = false
            };
            OperationTimer.Elapsed += OperationTimer_Trigger;

            RunningDatas.RequestTable.TryAdd(RequestID, this);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            RunningDatas.RequestTable.TryRemove(RequestID, out _);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FriendAndGroupList.Items.Clear();

            if (0 == SearchBox.Text.Length)
            {
                RunningDatas.ErrorNotify("查询条件不能为空！");
                return;
            }

            KXTNetStruct.SearchReq req = new KXTNetStruct.SearchReq
            {
                SearchString = SearchBox.Text
            };
            if ((bool)FriendRadioButton.IsChecked)
                req.SearchType = KXTNetStruct.SearchReq.SearchType_User;
            else if ((bool)GroupRadioButton.IsChecked)
                req.SearchType = KXTNetStruct.SearchReq.SearchType_Group;

            RunningDatas.DataSender.SearchReq(RequestID, req);
        }

        public void RequestCallback(object response)
        {
            SearchBox.IsEnabled = true;
            OperationTimer.Stop();

            SearchRes res = response as SearchRes;

            if (SearchRes.SearchType_User == res.SearchType)
                UserCallBack((SearchRes.UserSearchResult[])res.SearchResult);
            else
                UserCallBack((SearchRes.GroupSearchResult[])res.SearchResult);
        }
        private void UserCallBack(SearchRes.UserSearchResult[] results)
        {
            for (int i = 0; i < results.Length; ++i)
            {
                _ = Dispatcher.RunAsync
                    (
                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                    new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        FriendAndGroupList.Items.Add(new AddFriendGroupMagnets(results[i]));
                    }
                    ));
            }
        }
        private void UserCallBack(SearchRes.GroupSearchResult[] results)
        {
            for (int i = 0; i < results.Length; ++i)
            {
                _ = Dispatcher.RunAsync
                    (
                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                    new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        FriendAndGroupList.Items.Add(new AddFriendGroupMagnets(results[i]));
                    }
                    ));
            }
        }

        private void OperationTimer_Trigger(object sender, System.Timers.ElapsedEventArgs args)
        {
            _ = Dispatcher.RunAsync
                  (Windows.UI.Core.CoreDispatcherPriority.Normal,
                  new Windows.UI.Core.DispatchedHandler(() =>
                  {
                      SearchBox.IsEnabled = true;
                  }));
        }
        private const double OperationTimeInterval = 30000;
    }
}
