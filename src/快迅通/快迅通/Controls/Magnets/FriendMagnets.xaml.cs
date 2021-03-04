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
using 快迅通.Views.CommunitySubViews;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace 快迅通.Controls.Magnets
{
    public sealed partial class FriendMagnets : UserControl
    {
        public FriendMagnets()
        {
            this.InitializeComponent();
            FriendMagentsGrid.Background = Utils.Utils.GetViewPre();
            FriendMessageCountGrid.Background = Utils.Utils.GetViewIllustrator();
            FriendMessageCountBox.Foreground = Utils.Utils.GetViewIllustratorForce();
        }

        public string FriendName
        {
            get
            {
                return FriendNameBox.Text;
            }
            set
            {
                FriendNameBox.Text = value ?? "";
            }
        }
        public string FriendID
        {
            get
            {
                return FriendIDBox.Text;
            }
            set
            {
                FriendIDBox.Text = value ?? "";
            }
        }
        public string FriendDescribe
        {
            get
            {
                return FriendDescribeBox.Text;
            }
            set
            {
                FriendDescribeBox.Text = value ?? "";
            }
        }
        public bool FriendOnline
        {
            get
            {
                return FriendOnlineFlag.Visibility == Visibility.Visible;
            }
            set
            {
                FriendOnlineFlag.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public int FriendMessageCount
        {
            set
            {
                if (0 == value)
                    FriendMessageCountGrid.Visibility = Visibility.Collapsed;
                else
                {
                    FriendMessageCountGrid.Visibility = Visibility.Visible;
                    FriendMessageCountBox.Text = 100 > value ? value.ToString() : "99+";
                }
            }
        }
        public ImageSource FriendPhoto
        {
            get
            {
                return FriendPhotoImage.ImageSource;
            }
            set
            {
                FriendPhotoImage.ImageSource = value ?? Utils.Utils.GetPhoto(0);
            }
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FriendMessageCount = 0;
            Utils.RunningDatas.UIStack.Push(new FriendCommunityPage
            {
                FriendID = FriendID,
                FriendName = FriendName,
                FriendDescribe = FriendDescribe,
                FriendPhoto = FriendPhoto
            });
            Utils.RunningDatas.MainPage.UpdateUI();
        }
    }
}
