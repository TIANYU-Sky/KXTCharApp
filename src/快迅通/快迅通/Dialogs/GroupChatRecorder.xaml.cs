using KXTNetStruct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
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
    public sealed partial class GroupChatRecorder : ContentDialog, RequestSender
    {
        private readonly Guid RequestID;
        private readonly string GroupID;
        private readonly Timer OperationTimer;

        private readonly ImageSource UserPhoto;

        public GroupChatRecorder(string group_id, ImageSource user)
        {
            RequestID = Guid.NewGuid();

            GroupID = group_id;
            UserPhoto = user;

            this.InitializeComponent();

            OperationTimer = new Timer
            {
                Interval = 10000,
                AutoReset = false
            };
            OperationTimer.Elapsed += Trigger;

            RunningDatas.RequestTable.TryAdd(RequestID, this);

            Request();
        }

        private void Request()
        {
            OperationTimer.Start();

            WaitingBar.IsIndeterminate = true;
            WaitingBar.Visibility = Visibility.Visible;
            Next.Visibility = Visibility.Collapsed;

            RunningDatas.DataSender.GroupChatReq(RequestID, new KXTNetStruct.GroupChatsReq
            {
                Group = GroupID
            });
        }

        public void RequestCallback(object response)
        {
            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    WaitingBar.IsIndeterminate = false;
                    WaitingBar.Visibility = Visibility.Collapsed;
                    Next.Visibility = Visibility.Visible;

                    foreach (var item in (response as FriendChatsRes).Chat)
                    {
                        if (item.Sender == RunningDatas.UserID)
                            ChatList.Items.Add(new UserMessageMagnets
                            {
                                Text = item.Message,
                                Time = item.Time.ToString(),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Photo = UserPhoto
                            });
                        else
                            ChatList.Items.Add(new GroupMessageMagnets
                            {
                                Text = item.Message,
                                Time = item.Time.ToString(),
                                HorizontalAlignment = HorizontalAlignment.Left
                            });
                    }
                }));
        }

        private void Trigger(object sender, ElapsedEventArgs args)
        {
            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    WaitingBar.IsIndeterminate = false;
                    WaitingBar.Visibility = Visibility.Collapsed;
                    Next.Visibility = Visibility.Visible;
                }));

            RunningDatas.InfoNotify("请求超时");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RunningDatas.RequestTable.TryRemove(RequestID, out _);

            RunningDatas.DataSender.ChatsFinish(new ChatsFinishPackage
            {
                ID = RequestID.ToString(),
                ChatType = ChatsFinishPackage.ChatType_Group
            });

            Hide();
        }
    }
}
