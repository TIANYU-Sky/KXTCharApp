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
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 快迅通.Dialogs
{
    public sealed partial class FriendDetailInfo : ContentDialog
    {
        public string UserID
        {
            get
            {
                return FriendID.Text;
            }
            set
            {
                FriendID.Text = value ?? "";
            }
        }
        public string UserName
        {
            get
            {
                return FriendName.Text;
            }
            set
            {
                FriendName.Text = value ?? "";
            }
        }
        public string UserDescribe
        {
            get
            {
                return FriendDescribe.Text;
            }
            set
            {
                FriendDescribe.Text = value ?? "";
            }
        }
        public ImageSource UserPhoto
        {
            set
            {
                FriendIcon.Source = value ?? Utils.Utils.GetPhoto(0);
            }
        }

        public FriendDetailInfo()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void DeleteFriend_Click(object sender, RoutedEventArgs e)
        {
            RunningDatas.DataSender.DeleteUser(new KXTNetStruct.DeleteUser
            {
                UserID = RunningDatas.UserID,
                FriendID = UserID
            });

            RunningDatas.FriendsTable.TryRemove(UserID, out _);

            Utils.RunningDatas.UIStack.Pop();
            Utils.RunningDatas.MainPage.UpdateUI();

            Hide();
        }
    }
}
