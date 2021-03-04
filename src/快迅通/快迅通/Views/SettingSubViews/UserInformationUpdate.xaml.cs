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
using 快迅通.Dialogs;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 快迅通.Views.SettingSubViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserInformationUpdate : Page
    {
        public UserInformationUpdate()
        {
            this.InitializeComponent();
            TurnBackBar.Tapped = TurnBackButton_Tapped;

            UserNameTitleBox.Foreground = Utils.Utils.GetViewIllustratorForce();
            UserDescribeTitleBox.Foreground = Utils.Utils.GetViewIllustratorForce();
            UserPhoneTitleBox.Foreground = Utils.Utils.GetViewIllustratorForce();
            SaveButton.Foreground = Utils.Utils.GetViewIllustratorForce();
            SaveButton.Background = Utils.Utils.GetViewIllustrator();
            ChangeEmailBind.Foreground = Utils.Utils.GetViewIllustratorForce();
            ChangeEmailBind.Background = Utils.Utils.GetViewIllustrator();

            UserNameBox.Text = RunningDatas.UserName;
            UserDescribeBox.Text = RunningDatas.UserDescribe;
            UserPhoneBox.Text = RunningDatas.UserPhone;
        }

        private void TurnBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RunningDatas.UIStack.Pop();
            RunningDatas.MainPage.UpdateUI();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            RunningDatas.DataSender.UpdateDescribe(new KXTNetStruct.UpdateDescribe
            {
                UserID = RunningDatas.UserID,
                UserDescribe = UserDescribeBox.Text
            });
            RunningDatas.DataSender.UpdateName(new KXTNetStruct.UpdateName
            {
                UserID = RunningDatas.UserID,
                UserName = UserNameBox.Text
            });
            RunningDatas.DataSender.UpdatePhone(new KXTNetStruct.UpdatePhone
            {
                UserID = RunningDatas.UserID,
                UserPhone = UserPhoneBox.Text
            });
        }

        private void ChangeEmailBind_Click(object sender, RoutedEventArgs e)
        {
            _ = new UpdateEmail().ShowAsync();
        }
    }
}
