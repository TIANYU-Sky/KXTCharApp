using KXTNetStruct.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KXTServiceDBServer
{
    public interface ISQLDataBase
    {
        void Open();
        void Close();

        LoginResult Login(string user, string pw, LoginType type, out string userid);

        bool CheckEmail(string email);

        bool Register(RegisterPackage package);
        bool UpdatePassword(string email, string npw);
        bool UpdatePhone(string id, string nphone);
        bool UpdateEmail(string id, string nemail);
        bool UpdateName(string id, string nname);
        bool UpdateDescribe(string id, string ndescribe);
        bool UpdateProfilePhoto(string id, int nphoto);
        bool SelectUserInfor(string id, out UserInfoPackage info);
        bool SelectUserInfors(string[] ids, out UserInfoPackage[] infos);

        bool CreateGroup(CreateGroupPackage package);
        bool DeleteGroup(string id);
        bool UpdateGroupDescribe(string id, string ndescribe);
        bool UpdateGroupName(string id, string nname);
        bool SelectGroupInfo(string id, out GroupInfoPackage info);
        bool SelectGroupInfos(string[] ids, out GroupInfoPackage[] infos);

        bool SearchFriends(string search, out UserInfoPackage[] infos);
        bool SearchGroups(string search, out GroupInfoPackage[] infos);
    }

    internal interface IDBUserChatReader
    {
        void Close();
        bool GetRelation(string userA, string userB, out string relation_id);
        bool NewRelation(string userA, string userB, out string relation_id);
        bool DelRelation(string userA, string userB);
    }
}
