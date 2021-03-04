using KXTNetStruct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
    public sealed partial class UserInformationPage : Page, RequestSender
    {
        private readonly Guid RequestID;
        private readonly System.Timers.Timer OperationTimer;

        public UserInformationPage()
        {
            RequestID = Guid.NewGuid();
            OperationTimer = new System.Timers.Timer
            {
                Interval = OperationTimeInterval,
                AutoReset = false
            };
            OperationTimer.Elapsed += LoginTimer_Tigger;

            this.InitializeComponent();
            TurnBackBar.Tapped = TurnBackButton_Tapped;

            FrushUI();
        }
        private void FrushUI()
        {
            UpdateUserInfo.Background = Utils.Utils.GetViewPre();
            UserPicture.Background = new ImageBrush
            {
                ImageSource = Utils.Utils.GetPhoto(RunningDatas.UserPhoto)
            };
            UserNameBox.Text = Utils.UserData.UserName;
            UserIDBox.Text = Utils.UserData.UserID;
            UserDecribeBox.Text = Utils.UserData.UserDescribe;
        }

        private void TurnBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OperationTimer.Stop();
            RunningDatas.RequestTable.TryRemove(RequestID, out _);

            RunningDatas.UIStack.Pop();
            RunningDatas.MainPage.UpdateUI();
        }

        private void ReflushButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RunningDatas.RequestTable.TryAdd(RequestID, this);

            ReflushProgressRing.IsActive = true;
            ReflushProgressRing.Visibility = Visibility.Visible;
            ReflushButton.Visibility = Visibility.Collapsed;

            RunningDatas.DataSender.UserInfoReq(RequestID, new KXTNetStruct.UserGroupInfoReq
            {
                ID = RunningDatas.UserID
            });

            OperationTimer.Start();
        }

        private void LoginTimer_Tigger(object sender, System.Timers.ElapsedEventArgs args)
        {
            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    ReflushProgressRing.IsActive = false;
                    ReflushProgressRing.Visibility = Visibility.Collapsed;
                    ReflushButton.Visibility = Visibility.Visible;
                }));

            RunningDatas.RequestTable.TryRemove(RequestID, out _);

            RunningDatas.InfoNotify("请求超时");
        }

        public void RequestCallback(object response)
        {
            OperationTimer.Stop();

            RunningDatas.RequestTable.TryRemove(RequestID, out _);

            UserInfoRes res = response as UserInfoRes;

            RunningDatas.UserName = res.UserName;
            RunningDatas.UserEmail = res.UserEmail;
            RunningDatas.UserDescribe = res.UserDescribe;
            RunningDatas.UserPhone = res.UserPhone;
            RunningDatas.UserPhoto = res.UserPicture;

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(()=>
                {
                    ReflushProgressRing.IsActive = false;
                    ReflushProgressRing.Visibility = Visibility.Collapsed;
                    ReflushButton.Visibility = Visibility.Visible;

                    FrushUI();
                }));
        }

        private const double OperationTimeInterval = 5000;

        private void UpdateUserInfo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Utils.RunningDatas.UIStack.Push(new UserInformationUpdate());
            Utils.RunningDatas.MainPage.UpdateUI();
        }

        private void UserPicture_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = new PhotoSelect().ShowAsync();
        }
    }
}
