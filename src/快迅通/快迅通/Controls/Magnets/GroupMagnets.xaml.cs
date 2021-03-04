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
    public sealed partial class GroupMagnets : UserControl
    {
        public GroupMagnets()
        {
            this.InitializeComponent();
            GroupMagnetGrid.Background = Utils.Utils.GetViewPre();
            GroupMessageCountGrid.Background = Utils.Utils.GetViewIllustrator();
            GroupMessageCountBox.Foreground = Utils.Utils.GetViewIllustratorForce();
        }
        public string GroupName
        {
            get
            {
                return GroupNameBox.Text;
            }
            set
            {
                GroupNameBox.Text = value ?? "";
            }
        }
        public string GroupID
        {
            get
            {
                return GroupIDBox.Text;
            }
            set
            {
                GroupIDBox.Text = value ?? "";
            }
        }
        public string GroupDescribe
        {
            get
            {
                return GroupDescribeBox.Text;
            }
            set
            {
                GroupDescribeBox.Text = value ?? "";
            }
        }
        public int GroupMessageCount
        {
            set
            {
                if (0 == value)
                    GroupMessageCountGrid.Visibility = Visibility.Collapsed;
                else
                {
                    GroupMessageCountGrid.Visibility = Visibility.Visible;
                    GroupMessageCountBox.Text = 100 > value ? value.ToString() : "99+";
                }
            }
        }
        public string CreateID { get; set; }
        public string CreateName { get; set; }
        public DateTime CreateTime { get; set; }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GroupMessageCount = 0;
            Utils.RunningDatas.UIStack.Push(new GroupCommunityPage
            {
                GroupID = GroupID,
                GroupName = GroupName,
                GroupDescribe = GroupDescribe,
                CreateID = CreateID,
                CreateName = CreateName,
                CreateTime = CreateTime
            });
            Utils.RunningDatas.MainPage.UpdateUI();
        }
    }
}
