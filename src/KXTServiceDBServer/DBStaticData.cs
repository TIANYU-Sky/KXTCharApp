using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KXTServiceDBServer
{
    public class DBStaticData
    {
        public static string DataBaseConnString = "server=192.168.2.4;database=KXT;uid=kxt;pwd=kxt123456;";

        public static string FileDataBaseUsers                  = "User";
        public static string FileDataBaseGroups                 = "Group";
        public static string FileDataBaseRelation               = "Relation";

        public static string DataBaseUserTableName              = "Users";
        public static string DataBaseUserTableIDField           = "ID";
        public static string DataBaseUserTablePasswordField     = "Password";
        public static string DataBaseUserTableTokenField        = "Token";
        public static string DataBaseUserTableNameField         = "Name";
        public static string DataBaseUserTableEmailField        = "Email";
        public static string DataBaseUserTablePhoneField        = "Phone";
        public static string DataBaseUserTableDescribeField     = "Describe";
        public static string DataBaseUserTableProfilePhotoField = "ProfilePhoto";
        public static string DataBaseUserTableOnlineField       = "Online";

        public static string DataBaseGroupTableName             = "Groups";
        public static string DataBaseGroupTableGroupIDField     = "GID";
        public static string DataBaseGroupTableCreatorIDField   = "CID";
        public static string DataBaseGroupTableNameField        = "Name";
        public static string DataBaseGroupTableDescribeField    = "Describe";
        public static string DataBaseGroupTableCreateTimeField  = "CreateTime";

        public static string UserProfilePhotoDefault            = "0";


        internal const string UserChatDBConnString = "server=192.168.2.4;database=KXT;uid=kxt;pwd=kxt123456;";

        internal const string UserChatDBTableName   = "UserMap";
        internal const string UserChatDBTableFieldA = "UserIDA";
        internal const string UserChatDBTableFieldB = "UserIDB";
        internal const string UserChatDBTableMap    = "MAP";


        public const string UserInfoPath    = @"UserInfo";
        public const string GroupInfoPath   = @"GroupInfo";
        public const string AppliesInfoPath = @"Applies";
        public const string ChatPath        = @"Chats";
    }
}
