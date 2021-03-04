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
    public sealed partial class FriendCommunityPage : Page, IPageReceive
    {
        public string FriendID { get; set; }
        public string FriendName
        {
            get
            {
                return TurnBackBar.Text;
            }
            set
            {
                TurnBackBar.Text = value;
            }
        }
        public string FriendDescribe { get; set; }
        public ImageSource FriendPhoto { get; set; }

        public string GetID => FriendID;

        public FriendCommunityPage()
        {
            this.InitializeComponent();
            TurnBackBar.Tapped = TurnBackButton_Tapped;

            SenderButton.Background = Utils.Utils.GetViewBackground();

            RunningDatas.CurrentPage = this;
            LoadChats();
        }
        private void LoadChats()
        {
            if (RunningDatas.FriendsTable.TryGetValue(FriendID, out TemporaryFriendsList value))
            {
                foreach (var item in value.Message)
                    ChatPanel.Items.Add(new FriendMessageMagnets
                    {
                        Text = item.Message,
                        Time = item.Time.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Photo = FriendPhoto
                    });

                value.Message.Clear();
            }
        }
        private void TurnBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RunningDatas.UIStack.Pop();
            RunningDatas.MainPage.UpdateUI();
        }
        private void FriendDetailInfo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = new FriendDetailInfo
            {
                UserID = FriendID,
                UserName = FriendName,
                UserDescribe = FriendDescribe,
                UserPhoto = FriendPhoto
            }.ShowAsync();
        }
        private void SenderButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MessageEnter.TextDocument.GetText(Windows.UI.Text.TextGetOptions.None, out string message);
            if (RunningDatas.DataSender.FriendChat(new ChatMessage
            {
                Message = message,
                Sender = RunningDatas.UserID,
                Target = FriendID
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
                    ChatPanel.Items.Add(new FriendMessageMagnets
                    {
                        Text = message.Message,
                        Time = message.Time.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Photo = FriendPhoto
                    });
                }));
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = new UserChatRecorder
                (
                FriendID,
                Utils.Utils.GetPhoto(RunningDatas.UserPhoto),
                FriendPhoto
                )
            {
                Title = FriendName
            }.ShowAsync();
        }
    }
}
