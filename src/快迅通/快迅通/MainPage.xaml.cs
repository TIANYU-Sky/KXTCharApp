using KXTNetStruct;
using KXTNetStruct.Struct;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Controls;
using 快迅通.Dialogs;
using 快迅通.Utils;
using 快迅通.Views;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace 快迅通
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, IMainPageCall, RequestSender
    {
        private CloudFilePage Cloud;
        private CommunityPage Community;
        private SettingPage Setting;

        private readonly Guid MainPageRequestID;
        private readonly System.Timers.Timer OperationTimer;

        private bool Logined;

        public MainPage()
        {
            OperationTimer = new System.Timers.Timer
            {
                AutoReset = false
            };
            MainPageRequestID = Guid.NewGuid();
            Logined = false;

            this.InitializeComponent();

            RunningDatas.MainPage = this;
            RunningDatas.ErrorNotify = ErrorNotify;
            RunningDatas.InfoNotify = InfoNotify;

            this.Background = Utils.Utils.GetViewBackground();
            this.SelectionColumn.Background = Utils.Utils.GetViewPre();

            LoginSuccess();
        }

        private void LoginButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (0 == UserNameBox.Text.Length)
            {
                ErrorNotify("请输入快迅通ID/邮箱/手机号");
                return;
            }
            if (0 == PassWordBox.Password.Length)
            {
                ErrorNotify("密码不能为空");
                return;
            }

            if (Utils.Utils.LoginUserCheck(UserNameBox.Text, out LoginType type))
            {
                if (8 > PassWordBox.Password.Length)
                {
                    ErrorNotify("密码太短");
                    return;
                }

                LoginButton_login(UserNameBox.Text, PassWordBox.Password, type);
            }
            else
                ErrorNotify("无法识别的用户名格式 请重新输入");
        }
        private void LoginButton_login(string id, string pw, LoginType type)
        {
            LoginButton.Visibility = Visibility.Collapsed;
            LoginProgressRing.Visibility = Visibility.Visible;
            LoginProgressRing.IsActive = true;

            RunningDatas.RequestTable.TryAdd(MainPageRequestID, this);

            RunningDatas.DataSender.Login
                (
                MainPageRequestID,
                new LoginRequest
                {
                    UserID = id,
                    UserPW = Utils.Security.SHA1Encode(pw),
                    UserIDType = type
                }
                );

            OperationTimer.Interval = LoginTimeInterval;
            OperationTimer.Elapsed += LoginTimer_Tigger;
            OperationTimer.Start();
        }

        private void SettingButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Utils.RunningDatas.MainPage.CleanUIStack();
            Utils.RunningDatas.UIStack.Push(Setting);
            Utils.RunningDatas.MainPage.UpdateUI();
        }
        private void CloudFileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Utils.RunningDatas.MainPage.CleanUIStack();
            Utils.RunningDatas.UIStack.Push(Cloud);
            Utils.RunningDatas.MainPage.UpdateUI();
        }
        private void CommunityButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Utils.RunningDatas.MainPage.CleanUIStack();
            Utils.RunningDatas.UIStack.Push(Community);
            Utils.RunningDatas.MainPage.UpdateUI();
        }

        private void NewUserButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = new RegisterNewAccount().ShowAsync();
        }
        private void ForgetPWButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = new ForgetPassword().ShowAsync();
        }

        private void MyDetailInfo_Click(object sender, RoutedEventArgs e)
        {
            Utils.RunningDatas.MainPage.CleanUIStack();
            Utils.RunningDatas.UIStack.Push(Community);
            Utils.RunningDatas.UIStack.Push(new Views.SettingSubViews.UserInformationPage());
            Utils.RunningDatas.MainPage.UpdateUI();
        }
        private void MyRemaind_Click(object sender, RoutedEventArgs e)
        {
            _ = new ContentDialog
            {
                Title = "我的提醒",
                Content = "当前功能测试",
                PrimaryButtonText = "确定"
            }.ShowAsync();
        }

        void IMainPageCall.CleanUIStack()
        {
            Utils.RunningDatas.UIStack.Clear();
        }
        void IMainPageCall.UpdateUI()
        {
            SecondGrid.Children.Clear();
            if (0 != Utils.RunningDatas.UIStack.Count)
                SecondGrid.Children.Add(Utils.RunningDatas.UIStack.Peek());
            this.Background = Utils.Utils.GetViewBackground();
            this.SelectionColumn.Background = Utils.Utils.GetViewPre();
            Cloud.UpdateUI();
            Community.UpdateUI();
            Setting.UpdateUI();
            RunningDatas.CurrentPage = null;
        }
        void IMainPageCall.UpdatePhoto()
        {
            UserButtonPhoto.Source = Utils.Utils.GetPhoto(Utils.RunningDatas.UserPhoto);
        }

        void RequestSender.RequestCallback(object response)
        {
            if (Logined)
            {
                OperationTimer.Stop();

                _ = Dispatcher.RunAsync
                    (Windows.UI.Core.CoreDispatcherPriority.Normal,
                    new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        LoginProgressRing.IsActive = false;
                        LoginProgressRing.Visibility = Visibility.Collapsed;
                        LoginButton.Visibility = Visibility.Visible;
                    }));

                try
                {
                    MainRegistRes res = response as MainRegistRes;
                    if (MainRegistRes.RegResult_Success == res.RegResult)
                        LoginSuccess();
                }
                catch
                {
                    ErrorNotify("登录失败 无法连接到服务器");
                }

                Logined = false;
            }
            else
            {
                LoginResponse res = response as LoginResponse;
                Login(res);
            }
        }
        private void ToRegist()
        {
            RunningDatas.RequestTable.TryAdd(MainPageRequestID, this);

            RunningDatas.DataSender.Regist(MainPageRequestID);
        }
        private void Login(LoginResponse response)
        {
            if (LoginResult.Success == response.LoginResult)
            {
                RunningDatas.UserID = response.UserID;
                RunningDatas.UserName = response.UserName;
                RunningDatas.UserEmail = response.UserEmail;
                RunningDatas.UserPhone = response.UserPhone;
                RunningDatas.UserDescribe = response.UserDesicribe;
                RunningDatas.UserPhoto = response.UserPicture;

                ToRegist();
                Logined = true;
                return;
            }

            if (LoginResult.Error_Server == response.LoginResult)
            {
                ErrorNotify("无法连接到服务 请稍后重试");
                return;
            }
            if (LoginResult.Error_Password == response.LoginResult)
            {
                ErrorNotify("密码错误");
                return;
            }
            ErrorNotify("无法找到用户" + UserNameBox.Text);
        }
        private void LoginSuccess()
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            UserGrid.Visibility = Visibility.Visible;

            Cloud = new CloudFilePage();
            Community = new CommunityPage();
            Setting = new SettingPage();

            Utils.RunningDatas.MainPage = this;
            Utils.RunningDatas.FriendsTable = new ConcurrentDictionary<string, TemporaryFriendsList>();
            Utils.RunningDatas.GroupsTable = new ConcurrentDictionary<string, Utils.TemporaryGroupsList>();
            Utils.RunningDatas.ApplyLists = new ConcurrentBag<AppliesResponse.ApplyPackage>();
        }

        private void ErrorNotify(string str)
        {
            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler
                (
                    () =>
                    {
                        _ = new MessageDialog(str, "错误").ShowAsync();
                    }
                    )
                );
        }
        private void InfoNotify(string str)
        {
            _ = MainPageGrid.Dispatcher.RunAsync
               (
               Windows.UI.Core.CoreDispatcherPriority.Normal,
               new Windows.UI.Core.DispatchedHandler
               (
                   () =>
                   {
                       MainPageGrid.Children.Add(new NotifyTip(EndNotify)
                       {
                           Message = str,
                           Icon = Symbol.Message,
                           VerticalAlignment = VerticalAlignment.Top,
                           HorizontalAlignment = HorizontalAlignment.Right,
                           Margin = new Thickness(0,10,0,0)
                       });
                   }
               )
               );
        }
        private void EndNotify(UIElement element)
        {
            _ = MainPageGrid.Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler
                (
                    () =>
                    {
                        MainPageGrid.Children.Remove(element);
                    }
                )
                );
        }

        private void LoginTimer_Tigger(object sender, System.Timers.ElapsedEventArgs args)
        {
            _ = Dispatcher.RunAsync
                (Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    LoginProgressRing.IsActive = false;
                    LoginProgressRing.Visibility = Visibility.Collapsed;
                    LoginButton.Visibility = Visibility.Visible;
                }));

            RunningDatas.RequestTable.TryRemove(MainPageRequestID, out _);

            InfoNotify("登录超时 请重试");
        }

        private const double LoginTimeInterval = 20000;
    }
}
