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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using 快迅通.Utils;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace 快迅通.Controls.Magnets
{
    public sealed partial class AddFriendGroupMagnets : UserControl
    {
        private readonly bool IsFriend;

        public AddFriendGroupMagnets()
        {
            IsFriend = false;

            this.InitializeComponent();
        }
        public AddFriendGroupMagnets(SearchRes.UserSearchResult result)
        {
            IsFriend = true;

            this.InitializeComponent();

            FriendAndGroupPhoto.Background = new ImageBrush
            {
                ImageSource = Utils.Utils.GetPhoto(result.UserPicture)
            };
            FriendAndGroupNameBox.Text = result.UserName;
            FriendAndGroupIDBox.Text = result.UserID;
            FriendAndGroupDescribe.Text = result.UserDescribe;

            if (RunningDatas.FriendsTable.ContainsKey(result.UserID))
            {
                NextViewButton.Visibility = Visibility.Collapsed;
                AddedLabel.Visibility = Visibility.Visible;
            }
            else
            {
                NextViewButton.Visibility = Visibility.Visible;
                AddedLabel.Visibility = Visibility.Collapsed;
            }
        }
        public AddFriendGroupMagnets(SearchRes.GroupSearchResult result)
        {
            IsFriend = false;

            this.InitializeComponent();

            FriendAndGroupNameBox.Text = result.GroupName;
            FriendAndGroupIDBox.Text = result.GroupID;
            FriendAndGroupDescribe.Text = result.GroupDescribe;

            if (RunningDatas.GroupsTable.ContainsKey(result.GroupID))
            {
                NextViewButton.Visibility = Visibility.Collapsed;
                AddedLabel.Visibility = Visibility.Visible;
            }
            else
            {
                NextViewButton.Visibility = Visibility.Visible;
                AddedLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void NextViewButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Visibility.Collapsed == SecondScreen.Visibility) 
            {
                SecondScreen.Visibility = Visibility.Visible;
                NextViewButton.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///ICONS/uplist_icon.png"))
                };
            }
            else
            {
                SecondScreen.Visibility = Visibility.Collapsed;
                NextViewButton.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///ICONS/downlist_icon.png"))
                };
            }
        }

        private void AddFriendAndGroupButton_Click(object sender, RoutedEventArgs e)
        {
            RunningDatas.DataSender.ApplyUserReq(new ApplyRequest
            {
                TargetID = FriendAndGroupIDBox.Text,
                Message = ApplyMessageBox.Text,
                TargetType = IsFriend ? ApplyRequest.TargetType_Friend : ApplyRequest.TargetType_Group
            });
        }
    }
}
