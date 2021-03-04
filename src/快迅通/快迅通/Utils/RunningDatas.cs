using KXTNetStruct;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using 快迅通.Server;
using 快迅通.Server.FileService;
using static KXTNetStruct.AppliesResponse;

namespace 快迅通.Utils
{
    internal class RunningDatas
    {
        public static string UserID = "000000";
        public static string UserName = "测试用户";
        public static string UserEmail = "test@mitutec.com";
        public static string UserPhone = "00000000000";
        public static string UserDescribe = "当前用户为测试用户";
        public static int UserPhoto = 0;

        public static Stack<UIElement> UIStack;
        public static ConcurrentDictionary<string, TemporaryFriendsList> FriendsTable;
        public static ICollection<TemporaryFriendsList> FriendLists => FriendsTable.Values;
        public static ConcurrentDictionary<string, TemporaryGroupsList> GroupsTable;
        public static ICollection<TemporaryGroupsList> GroupLists => GroupsTable.Values;
        public static ConcurrentBag<ApplyPackage> ApplyLists;

        public static ConcurrentDictionary<Guid, RequestSender> RequestTable;

        public static ClientConnect Connection;
        public static IDataSender DataSender => Connection;
        public static FileService FileService;
        public static FileWaiting FileWaiting;

        public static IMainPageCall MainPage;

        public static Action<string> ErrorNotify;
        public static Action<string> InfoNotify;
        public static Action<ChatPackageType, ChatMessage> ReceiveMessage;

        public static IPageReceive CurrentPage;
    }
}
