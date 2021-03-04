using KXTNetStruct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 快迅通.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CommunityPage : Page, RequestSender
    {
        private static FriendsResponse FriendsResponseTemp = new FriendsResponse();
        private static GroupsResponse GroupsResponseTemp = new GroupsResponse();
        private static UserInfoRes UserInfoResTemp = new UserInfoRes();
        private static GroupInfoRes GroupInfoResTemp = new GroupInfoRes();

        private readonly Guid RequestID;
        private readonly System.Timers.Timer OperationTimer;

        public CommunityPage()
        {
            RunningDatas.ReceiveMessage = ReceiveMessage;

            RequestID = Guid.NewGuid();
            OperationTimer = new System.Timers.Timer
            {
                Interval = OperationTimeInterval,
                AutoReset = false
            };

            this.InitializeComponent();

            CommunityPivot.Foreground = Utils.Utils.GetViewIllustratorForce();

            OperationTimer.Elapsed += LoginTimer_Tigger;
            RunningDatas.RequestTable.TryAdd(RequestID, this);

            FlushFriendAndGroup_Tapped(null, null);
        }

        public void UpdateUI()
        {
            _ = Dispatcher.RunAsync
                (Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    FriendView.UpdateList();
                    GroupView.UpdateList();
                }));
        }

        private void AddFriendAndGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = new Dialogs.AddFriendAndGroup().ShowAsync();
        }
        private void FlushFriendAndGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlushFriendAndGroup.Visibility = Visibility.Collapsed;
            FlushFriendAndGroupRing.IsActive = true;
            FlushFriendAndGroupRing.Visibility = Visibility.Visible;

            OperationTimer.Start();

            RunningDatas.DataSender.FriendsRequest(RequestID, new KXTNetStruct.FriendGroupApplyRequest
            {
                UserID = RunningDatas.UserID
            });
            RunningDatas.DataSender.GroupsRequest(RequestID, new KXTNetStruct.FriendGroupApplyRequest
            {
                UserID = RunningDatas.UserID
            });
        }

        public void RequestCallback(object response)
        {
            if (response.GetType() == FriendsResponseTemp.GetType())
            {
                FriendsCallBack(response as FriendsResponse);
                return;
            }

            if (response.GetType() == GroupsResponseTemp.GetType())
            {
                GroupsCallBack(response as GroupsResponse);
                return;
            }

            if (response.GetType() == UserInfoResTemp.GetType())
            {
                FriendInfoCallBack(response as UserInfoRes);
                return;
            }

            if (response.GetType() == GroupInfoResTemp.GetType())
            {
                GroupInfoCallBack(response as GroupInfoRes);
                return;
            }
        }
        private void FriendsCallBack(FriendsResponse response)
        {
            for (int i = 0; i < response.FriendsID.Length; ++i)
                RunningDatas.DataSender.UserInfoReq(RequestID, new UserGroupInfoReq
                {
                    ID = response.FriendsID[i]
                });
        }
        private void GroupsCallBack(GroupsResponse response)
        {
            for (int i = 0; i < response.GroupsID.Length; ++i)
                RunningDatas.DataSender.GroupInfoReq(RequestID, new UserGroupInfoReq
                {
                    ID = response.GroupsID[i]
                });
        }
        private void FriendInfoCallBack(UserInfoRes friend)
        {
            OperationTimer.Stop();

            if (RunningDatas.FriendsTable.TryGetValue(friend.UserID, out TemporaryFriendsList value))
            {
                value.Email = friend.UserEmail;
                value.Describe = friend.UserDescribe;
                value.Name = friend.UserName;
                value.Phone = friend.UserPhone;
                value.PhotoIndex = friend.UserPicture;
            }
            else
            {
                RunningDatas.FriendsTable.TryAdd(friend.UserID, new TemporaryFriendsList
                {
                    ID = friend.UserID,
                    Describe = friend.UserDescribe,
                    Email = friend.UserEmail,
                    Name = friend.UserName,
                    Phone = friend.UserPhone,
                    PhotoIndex = friend.UserPicture
                });
            }

            UpdateUI();
        }
        private void GroupInfoCallBack(GroupInfoRes group)
        {
            OperationTimer.Stop();

            if (RunningDatas.GroupsTable.TryGetValue(group.GroupID, out TemporaryGroupsList value))
            {
                value.Describe = group.GroupDescribe;
                value.ManagerID = group.CreatorID;
                value.ManagerName = group.CreatorName;
                value.Name = group.GroupName;
                value.Time = group.CreateTime;
            }
            else
            {
                RunningDatas.GroupsTable.TryAdd(group.GroupID, new TemporaryGroupsList
                {
                    ID = group.GroupID,
                    Describe = group.GroupDescribe,
                    ManagerID = group.CreatorID,
                    ManagerName = group.CreatorName,
                    Name = group.GroupName,
                    Time = group.CreateTime
                });
            }

            UpdateUI();
        }

        private void LoginTimer_Tigger(object sender, System.Timers.ElapsedEventArgs args)
        {
            _ = Dispatcher.RunAsync
                (Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    FlushFriendAndGroup.Visibility = Visibility.Visible;
                    FlushFriendAndGroupRing.IsActive = false;
                    FlushFriendAndGroupRing.Visibility = Visibility.Collapsed;
                }));

            RunningDatas.RequestTable.TryRemove(RequestID, out _);

            RunningDatas.InfoNotify("请求超时");
        }

        private const double OperationTimeInterval = 20000;

        private void ReceiveMessage(ChatPackageType type, ChatMessage message)
        {
            if (null!=RunningDatas.CurrentPage && RunningDatas.CurrentPage.GetID == message.Target)
            {
                RunningDatas.CurrentPage.Receive(message);
                return;
            }

            if (ChatPackageType.Friend == type)
                ReceiveFriendChat(message);
            else
                ReceiveGroupChat(message);
        }
        private void ReceiveFriendChat(ChatMessage message)
        {
            if (RunningDatas.FriendsTable.TryGetValue(message.Sender, out TemporaryFriendsList value))
            {
                value.Message.Add(new TemporaryFriendCommunity
                {
                    Message = message.Message,
                    Time = message.Time
                });

                UpdateUI();
            }
        }
        private void ReceiveGroupChat(ChatMessage message)
        {
            if (RunningDatas.GroupsTable.TryGetValue(message.Target, out TemporaryGroupsList value))
            {
                value.Message.Add(new TemporaryGroupMessage
                {
                    UserID = message.Sender,
                    Message = message.Message,
                    Time = message.Time
                });

                UpdateUI();
            }
        }
    }
}
