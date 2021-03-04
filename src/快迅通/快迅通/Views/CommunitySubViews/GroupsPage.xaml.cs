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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 快迅通.Views.CommunitySubViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GroupsPage : Page
    {
        public GroupsPage()
        {
            this.InitializeComponent();
        }

        public void UpdateList()
        {
            foreach (var item in RunningDatas.GroupLists)
            {
                GroupGridView.Items.Add(new GroupMagnets
                {
                    GroupName = item.Name,
                    GroupID = item.ID,
                    GroupDescribe = item.Describe,
                    GroupMessageCount = item.Message.Count,
                    CreateID = item.ManagerID,
                    CreateName = item.ManagerName,
                    CreateTime = item.Time
                });
            }
        }
    }
}
