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
using 快迅通.Dialogs;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 快迅通.Views.CommunitySubViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GroupCommunityPage : Page, IPageReceive
    {
        public string GroupID { get; set; }
        public string GroupName
        {
            get
            {
                return TurnBackBar.Text;
            }
            set
            {
                TurnBackBar.Text = value ?? "";
            }
        }
        public string GroupDescribe { get; set; }
        public string CreateID { get; set; }
        public string CreateName { get; set; }
        public DateTime CreateTime { get; set; }

        public string GetID => GroupID;

        public GroupCommunityPage()
        {
            this.InitializeComponent();
            TurnBackBar.Tapped = TurnBackButton_Tapped;

            SenderButton.Background = Utils.Utils.GetViewBackground();

            RunningDatas.CurrentPage = this;
            LoadChats();
        }
        private void LoadChats()
        {
            if (RunningDatas.GroupsTable.TryGetValue(GroupID, out TemporaryGroupsList value))
            {
                foreach (var item in value.Message)
                    ChatPanel.Items.Add(new GroupMessageMagnets
                    {
                        Sender = item.UserID,
                        Text = item.Message,
                        Time = item.Time.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Left
                    });

                value.Message.Clear();
            }
        }

        private void TurnBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RunningDatas.UIStack.Pop();
            RunningDatas.MainPage.UpdateUI();
        }
        private void GroupDetailInfo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = new GroupDetailInfo
            {
                GID = GroupID,
                GName = GroupName,
                GDescribe = GroupDescribe,
                GCreateTime = CreateTime.ToString(),
                GCreateID = CreateID,
                GCreateName = CreateName
            }.ShowAsync();
        }
        private void SenderButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MessageEnter.TextDocument.GetText(Windows.UI.Text.TextGetOptions.None, out string message);
            if (RunningDatas.DataSender.GroupChat(new ChatMessage
            {
                Message = message,
                Sender = RunningDatas.UserID,
                Target = GroupID
            }))
            {
                MessageEnter.TextDocument.SetText(Windows.UI.Text.TextSetOptions.None, "");
                ChatPanel.Items.Add(new UserMessageMagnets
                {
                    Text = message,
                    Time = DateTime.Now.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Photo = Utils.Utils.GetPhoto(RunningDatas.UserPhoto)
                });
            }
            else
                RunningDatas.InfoNotify("发送失败 网络错误");
        }

        public void Receive(ChatMessage message)
        {
            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    ChatPanel.Items.Add(new GroupMessageMagnets
                    {
                        Sender = message.Sender,
                        Text = message.Message,
                        Time = message.Time.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Left
                    });
                }));
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = new GroupChatRecorder(GroupID, Utils.Utils.GetPhoto(RunningDatas.UserPhoto))
            {
                Title = GroupName
            }.ShowAsync();
        }
    }
}
