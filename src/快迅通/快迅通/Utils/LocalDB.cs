using KXTNetStruct;
using KXTNetStruct.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using UserDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.KeyValuePair<string, KXTNetStruct.Struct.LoginType>>;

namespace 快迅通.Utils
{
    internal class LocalDB
    {
        public static void SetAccount(string id, string pw, LoginType type)
        {
            ApplicationDataContainer setting = ApplicationData.Current.LocalSettings;
            setting.Values["CurrentUserID"] = id;
            setting.Values["CurrentUserPW"] = pw;
            setting.Values["CurrentUserType"] = (int)type;
        }
        public static void GetAccount(out KXTNetStruct.LoginRequest package)
        {
            ApplicationDataContainer setting = ApplicationData.Current.LocalSettings;
            object usertype = setting.Values["CurrentUserType"];
            if (null != usertype)
            {
                package = new KXTNetStruct.LoginRequest
                {
                    UserID = setting.Values["CurrentUserID"].ToString(),
                    UserPW = setting.Values["CurrentUserPW"].ToString(),
                    UserIDType = (LoginType)(int)usertype
                };
                return;
            }
            package = null;
        }
        public static void AddAccount(string id, string pw, LoginType type)
        {
            ApplicationDataContainer setting = ApplicationData.Current.LocalSettings;
            object obj = setting.Values["AccountList"];
            if (null == obj)
                obj = new ApplicationDataCompositeValue();
            ApplicationDataCompositeValue accounts = (ApplicationDataCompositeValue)obj;

            accounts[id] = pw + ";" + ((int)type);

            setting.Values["AccountList"] = accounts;
        }
        public static UserDictionary GetAccounts()
        {
            ApplicationDataContainer setting = ApplicationData.Current.LocalSettings;
            object obj = setting.Values["AccountList"];
            if (null == obj)
                return null;
            UserDictionary accounts = new UserDictionary();
            foreach (var i in obj as ApplicationDataCompositeValue) 
            {
                string[] vs = (i.Value as string).Split(";");
                int.TryParse(vs[1], out int result);
                accounts.Add(i.Key, new KeyValuePair<string, LoginType>(vs[0], (LoginType)result));
            }
            return accounts;
        }
        public static void GetAccount(string id, out KXTNetStruct.LoginRequest package)
        {
            package = null;
            ApplicationDataContainer setting = ApplicationData.Current.LocalSettings;
            object obj = setting.Values["AccountList"];
            if (null != obj)
            {
                ApplicationDataCompositeValue accounts = (ApplicationDataCompositeValue)obj;
                string[] vs = (accounts[id] as string).Split(";");
                int.TryParse(vs[1], out int result);
                package = new KXTNetStruct.LoginRequest
                {
                    UserID = id,
                    UserPW = vs[0],
                    UserIDType = (LoginType)result
                };
            }
        }

        public static void SetTheme(int theme, bool acr, double op)
        {
            ApplicationDataContainer setting = ApplicationData.Current.LocalSettings;
            setting.Values["ApplicationThemeBackground"] = theme;
            setting.Values["ApplicationThemeAcrylicStyle"] = acr;
            setting.Values["ApplicationThemeOpacity"] = op;
        }
        public static void GetTheme(out int theme, out bool acr, out double op)
        {
            ApplicationDataContainer setting = ApplicationData.Current.LocalSettings;
            theme = (int)(setting.Values["ApplicationThemeBackground"] ?? 0);
            acr = (bool)(setting.Values["ApplicationThemeAcrylicStyle"] ?? true);
            op = (double)(setting.Values["ApplicationThemeOpacity"] ?? 0.7);
        }
    }
}
