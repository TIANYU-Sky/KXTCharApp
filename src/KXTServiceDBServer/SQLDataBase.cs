using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using KXTNetStruct;
using KXTNetStruct.Struct;

namespace KXTServiceDBServer
{
    public class SQLDataBase : ISQLDataBase
    {
        private readonly SqlConnection SQLConnection;

        public SQLDataBase(string conn_str)
        {
            SQLConnection = new SqlConnection(conn_str);
        }

        public void Open()
        {
            try
            {
                SQLConnection.Open();
            }
            catch
            {
                throw new Exception("数据库连接失败");
            }
        }
        public void Close()
        {
            try
            {
                SQLConnection.Close();
            }
            catch
            {
                throw new Exception("数据库操作异常");
            }
        }

        public LoginResult Login(string user, string pw, LoginType type, out string userid)
        {
            string sql = string.Format
                (
                "select {0}, {1}, {2} from {3} where ",
                DBStaticData.DataBaseUserTablePasswordField,
                DBStaticData.DataBaseUserTableTokenField,
                DBStaticData.DataBaseUserTableIDField,
                DBStaticData.DataBaseUserTableName
                );
            userid = "";
            switch (type)
            {
                case LoginType.Email:
                    sql += DBStaticData.DataBaseUserTableEmailField + " = '" + user + "'";
                    break;
                case LoginType.Phone:
                    sql += DBStaticData.DataBaseUserTablePhoneField + " = '" + user + "'";
                    break;
                case LoginType.Identification:
                    sql += DBStaticData.DataBaseUserTableIDField + " = '" + user + "'";
                    break;
                default:
                    return LoginResult.Error_Server;
            }
            if (ExecuteNonDataSet(sql, out DataSet set))
                if (
                    0 < set.Tables.Count 
                    && 0 < set.Tables[0].Rows.Count
                    )
                {
                    string password = set.Tables[0].Rows[0].ItemArray[0] as string;
                    string token = set.Tables[0].Rows[0].ItemArray[1] as string;
                    if (DBStaticMethod.SHA256(pw, token).Equals(password)) 
                    {
                        userid = set.Tables[0].Rows[0].ItemArray[2] as string;
                        return LoginResult.Success;
                    }
                    else
                        return LoginResult.Error_Password;
                }
                else
                {
                    switch (type)
                    {
                        case LoginType.Email:
                            return LoginResult.Error_Email;
                        case LoginType.Phone:
                            return LoginResult.Error_Phone;
                        case LoginType.Identification:
                            return LoginResult.Error_User;
                    }
                }
            return LoginResult.Error_Server;
        }

        public bool Register(RegisterPackage package)
        {
            if (null != package.Phone && "" != package.Phone)
            {
                string sql_checkphone = "select * from " +
                    DBStaticData.DataBaseUserTableName +
                    " where " +
                    DBStaticData.DataBaseUserTablePhoneField + " = '" + package.Phone + "'";
                if (ExecuteNonDataSet(sql_checkphone, out DataSet set))
                {
                    if (0 != set.Tables.Count)
                    {
                        if (0 != set.Tables[0].Rows.Count)
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            string token = DBStaticMethod.GetToken();
            string sql = string.Format
                (
                "insert into {0} values ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8})",
                DBStaticData.DataBaseUserTableName,
                package.ID,
                DBStaticMethod.SHA256(package.Password, token),
                token,
                package.Name,
                package.Email,
                package.Phone,
                package.Describe,
                package.Photo.ToString()
                );
            if (!ExecuteNonQuery(sql))
                return false;
            return true;
        }
        public bool UpdatePassword(string email, string npw)
        {
            string token = DBStaticMethod.GetToken();
            string sql = string.Format
                (
                "update {0} set {1} = '{2}', {3} = '{4}' where {5} = '{6}'",
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTableTokenField,
                token,
                DBStaticData.DataBaseUserTablePasswordField,
                DBStaticMethod.SHA256(npw, token),
                DBStaticData.DataBaseUserTableEmailField,
                email
                );
            return ExecuteNonQuery(sql);
        }
        public bool UpdatePhone(string id, string nphone)
        {
            string sql = string.Format
                (
                "update {0} set {1} = '{2}' where {3} = '{4}'",
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTablePhoneField,
                nphone,
                DBStaticData.DataBaseUserTableIDField,
                id
                );
            return ExecuteNonQuery(sql);
        }
        public bool UpdateEmail(string id, string nemail)
        {
            string sql = string.Format
                (
                "update {0} set {1} = '{2}' where {3} = '{4}'",
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTableEmailField,
                nemail,
                DBStaticData.DataBaseUserTableIDField,
                id
                );
            return ExecuteNonQuery(sql);
        }
        public bool UpdateName(string id, string nname)
        {
            string sql = string.Format
                (
                "update {0} set {1} = '{2}' where {3} = '{4}'",
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTableNameField,
                nname,
                DBStaticData.DataBaseUserTableIDField,
                id
                );
            return ExecuteNonQuery(sql);
        }
        public bool UpdateDescribe(string id, string ndescribe)
        {
            string sql = string.Format
                (
                "update {0} set {1} = '{2}' where {3} = '{4}'",
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTableDescribeField,
                ndescribe,
                DBStaticData.DataBaseUserTableIDField,
                id
                );
            return ExecuteNonQuery(sql);
        }
        public bool UpdateProfilePhoto(string id, int nphoto)
        {
            string sql = string.Format
                (
                "update {0} set {1} = {2} where {3} = '{4}'",
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTableProfilePhotoField,
                nphoto,
                DBStaticData.DataBaseUserTableIDField,
                id
                );
            return ExecuteNonQuery(sql);
        }
        public bool SelectUserInfor(string id, out UserInfoPackage info)
        {
            info = null;
            string sql = string.Format
                (
                "select * from {0} where {1} = '{2}'",
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTableIDField,
                id
                );
            if (ExecuteNonDataSet(sql, out DataSet set))
                if (0 < set.Tables[0].Rows.Count)
                {
                    info = new UserInfoPackage
                    {
                        ID = set.Tables[0].Rows[0].ItemArray[0] as string,
                        Name = set.Tables[0].Rows[0].ItemArray[3] as string,
                        Email = set.Tables[0].Rows[0].ItemArray[4] as string,
                        Phone = set.Tables[0].Rows[0].ItemArray[5] as string,
                        Describe = set.Tables[0].Rows[0].ItemArray[6] as string,
                        Photo = (int)set.Tables[0].Rows[0].ItemArray[7],
                    };
                    return true;
                }
            return false;
        }
        public bool SelectUserInfors(string[] ids, out UserInfoPackage[] infos)
        {
            infos = null;
            if (null != ids && 0 < ids.Length)
            {
                string sql = "select * from " +
                    DBStaticData.DataBaseUserTableName +
                    " where " +
                    DBStaticData.DataBaseUserTableIDField +
                    " in ('" +
                    string.Join("', '", ids) +
                    "')";
                if (ExecuteNonDataSet(sql.ToString(), out DataSet set)) 
                {
                    if (0 < set.Tables[0].Rows.Count)
                    {
                        infos = new UserInfoPackage[set.Tables[0].Rows.Count];
                        for(int i = 0; i < infos.Length; ++i)
                        {
                            infos[i] = new UserInfoPackage
                            {
                                ID = set.Tables[0].Rows[i].ItemArray[0] as string,
                                Name = set.Tables[0].Rows[i].ItemArray[3] as string,
                                Email = set.Tables[0].Rows[i].ItemArray[4] as string,
                                Phone = set.Tables[0].Rows[i].ItemArray[5] as string,
                                Describe = set.Tables[0].Rows[i].ItemArray[6] as string,
                                Photo = (int)set.Tables[0].Rows[i].ItemArray[7],
                            };
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CreateGroup(CreateGroupPackage package)
        {
            string sql = string.Format
                (
                "insert into {0} values('{1}', '{2}', '{3}', '{4}', '{5}')",
                DBStaticData.DataBaseGroupTableName,
                package.ID,
                package.Manager,
                package.Name,
                package.Describe,
                package.Time.ToString()
                );
            return ExecuteNonQuery(sql);
        }
        public bool DeleteGroup(string id)
        {
            string sql = string.Format
                (
                "delete from {0} where {1} = '{2}'",
                DBStaticData.DataBaseGroupTableName,
                DBStaticData.DataBaseGroupTableGroupIDField,
                id
                );
            return ExecuteNonQuery(sql);
        }
        public bool UpdateGroupDescribe(string id, string ndescribe)
        {
            string sql = string.Format
                (
                "update {0} set {1} = '{2}' where {3} = '{4}'",
                DBStaticData.DataBaseGroupTableName,
                DBStaticData.DataBaseGroupTableDescribeField,
                ndescribe,
                DBStaticData.DataBaseGroupTableGroupIDField,
                id
                );
            return ExecuteNonQuery(sql);
        }
        public bool UpdateGroupName(string id, string nname)
        {
            string sql = string.Format
                (
                "update {0} set {1} = '{2}' where {3} = '{4}'",
                DBStaticData.DataBaseGroupTableName,
                DBStaticData.DataBaseGroupTableNameField,
                nname,
                DBStaticData.DataBaseGroupTableGroupIDField,
                id
                );
            return ExecuteNonQuery(sql);
        }
        public bool SelectGroupInfo(string id, out GroupInfoPackage info)
        {
            info = null;
            string sql = string.Format
                (
                "select {0}.{1} as GID, " +
                "{0}.{2} as GName, " +
                "{0}.{3} as Describe, " +
                "{0}.{4} as CTime, " +
                "{5}.{6} as CID, " +
                "{5}.{7} as CName " +
                "from {0}, {5} " +
                "where {0}.{8} = {5}.{6} and {0}.{1} = '{9}'",
                DBStaticData.DataBaseGroupTableName,
                DBStaticData.DataBaseGroupTableGroupIDField,
                DBStaticData.DataBaseGroupTableNameField,
                DBStaticData.DataBaseGroupTableDescribeField,
                DBStaticData.DataBaseGroupTableCreateTimeField,
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTableIDField,
                DBStaticData.DataBaseUserTableNameField,
                DBStaticData.DataBaseGroupTableCreatorIDField,
                id
                );
            if (ExecuteNonDataSet(sql, out DataSet set))
            {
                if (0 < set.Tables[0].Rows.Count)
                {
                    info = new GroupInfoPackage
                    {
                        ID = set.Tables[0].Rows[0].ItemArray[0] as string,
                        Name = set.Tables[0].Rows[0].ItemArray[1] as string,
                        Describe = set.Tables[0].Rows[0].ItemArray[2] as string,
                        Time = (DateTime)set.Tables[0].Rows[0].ItemArray[3],
                        ManagerID = set.Tables[0].Rows[0].ItemArray[4] as string,
                        ManagerName = set.Tables[0].Rows[0].ItemArray[5] as string,
                    };
                    return true;
                }
            }
            return false;
        }
        public bool SelectGroupInfos(string[] ids, out GroupInfoPackage[] infos)
        {
            infos = null;
            string sql = string.Format
                (
                "select {0}.{1} as GID, " +
                "{0}.{2} as GName, " +
                "{0}.{3} as Describe, " +
                "{0}.{4} as CTime, " +
                "{5}.{6} as CID, " +
                "{5}.{7} as CName " +
                "from {0}, {5} " +
                "where {0}.{8} = {5}.{6} and {0}.{1} in ('{9}')",
                DBStaticData.DataBaseGroupTableName,
                DBStaticData.DataBaseGroupTableGroupIDField,
                DBStaticData.DataBaseGroupTableNameField,
                DBStaticData.DataBaseGroupTableDescribeField,
                DBStaticData.DataBaseGroupTableCreateTimeField,
                DBStaticData.DataBaseUserTableName,
                DBStaticData.DataBaseUserTableIDField,
                DBStaticData.DataBaseUserTableNameField,
                DBStaticData.DataBaseGroupTableCreatorIDField,
                string.Join("', '", ids)
                );
            if (ExecuteNonDataSet(sql, out DataSet set))
            {
                if (0 < set.Tables[0].Rows.Count)
                {
                    infos = new GroupInfoPackage[set.Tables[0].Rows.Count];
                    for (int i = 0; i < infos.Length; ++i)
                        infos[i] = new GroupInfoPackage
                        {
                            ID = set.Tables[0].Rows[i].ItemArray[0] as string,
                            Name = set.Tables[0].Rows[i].ItemArray[1] as string,
                            Describe = set.Tables[0].Rows[i].ItemArray[2] as string,
                            Time = (DateTime)set.Tables[i].Rows[0].ItemArray[3],
                            ManagerID = set.Tables[0].Rows[i].ItemArray[4] as string,
                            ManagerName = set.Tables[0].Rows[i].ItemArray[5] as string,
                        };
                    return true;
                }
            }
            return false;
        }

        public bool SearchFriends(string search, out UserInfoPackage[] infos)
        {
            infos = null;
            string sql = string.Format
                (
                "select * from {0} where {2} like '%{1}%' union " +
                "select * from {0} where {3} like '%{1}%' union " +
                "select * from {0} where {4} like '%{1}%' union " +
                "select * from {0} where {5} like '%{1}%'",
                DBStaticData.DataBaseUserTableName,
                search,
                DBStaticData.DataBaseUserTableIDField,
                DBStaticData.DataBaseUserTableNameField,
                DBStaticData.DataBaseUserTableEmailField,
                DBStaticData.DataBaseUserTablePhoneField
                );
            if (ExecuteNonDataSet(sql.ToString(), out DataSet set))
            {
                if (0 < set.Tables[0].Rows.Count)
                {
                    infos = new UserInfoPackage[set.Tables[0].Rows.Count];
                    for (int i = 0; i < infos.Length; ++i)
                    {
                        infos[i] = new UserInfoPackage
                        {
                            ID = set.Tables[0].Rows[i].ItemArray[0] as string,
                            Name = set.Tables[0].Rows[i].ItemArray[3] as string,
                            Email = set.Tables[0].Rows[i].ItemArray[4] as string,
                            Phone = set.Tables[0].Rows[i].ItemArray[5] as string,
                            Describe = set.Tables[0].Rows[i].ItemArray[6] as string,
                            Photo = (int)set.Tables[0].Rows[i].ItemArray[7],
                        };
                    }
                    return true;
                }
            }
            return false;
        }
        public bool SearchGroups(string search, out GroupInfoPackage[] infos)
        {
            infos = null;
            string sql = string.Format
                (
                "select " +
                "SelectedGroup.{2} as GID, " +
                "SelectedGroup.{3} as GName, " +
                "SelectedGroup.{4} as Describe, " +
                "SelectedGroup.{5} as CTime, " +
                "SelectedUser.{7} as CID, " +
                "SelectedUser.{8} as CName " +
                "from {6} as SelectedUser, " +
                "(" +
                "select * from {1} where {2} like '%{0}%' union " +
                "select * from {1} where {3} like '%{0}%' union " +
                "select * from {1} where {4} like '%{0}%'" +
                ") as SelectedGroup " +
                "where SelectedUser.{9} = SelectedGroup.{7}",
                search,                                         // 0
                DBStaticData.DataBaseGroupTableName,            // 1
                DBStaticData.DataBaseGroupTableGroupIDField,    // 2
                DBStaticData.DataBaseGroupTableNameField,       // 3
                DBStaticData.DataBaseGroupTableDescribeField,   // 4
                DBStaticData.DataBaseGroupTableCreateTimeField, // 5

                DBStaticData.DataBaseUserTableName,             // 6
                DBStaticData.DataBaseUserTableIDField,          // 7
                DBStaticData.DataBaseUserTableNameField,        // 8

                DBStaticData.DataBaseGroupTableCreatorIDField   // 9
                );
            if (ExecuteNonDataSet(sql, out DataSet set))
            {
                if (0 < set.Tables[0].Rows.Count)
                {
                    infos = new GroupInfoPackage[set.Tables[0].Rows.Count];
                    for (int i = 0; i < infos.Length; ++i)
                        infos[i] = new GroupInfoPackage
                        {
                            ID = set.Tables[0].Rows[i].ItemArray[0] as string,
                            Name = set.Tables[0].Rows[i].ItemArray[1] as string,
                            Describe = set.Tables[0].Rows[i].ItemArray[2] as string,
                            Time = (DateTime)set.Tables[i].Rows[0].ItemArray[3],
                            ManagerID = set.Tables[0].Rows[i].ItemArray[4] as string,
                            ManagerName = set.Tables[0].Rows[i].ItemArray[5] as string,
                        };
                    return true;
                }
            }
            return false;
        }

        private bool ExecuteNonDataSet(string sql, out DataSet dataSet)
        {
            dataSet = null;
            try
            {
                SqlDataAdapter sqlData = new SqlDataAdapter(sql, SQLConnection);
                dataSet = new DataSet();
                sqlData.Fill(dataSet);
                sqlData.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool ExecuteNonQuery(string sql)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sql, SQLConnection);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CheckEmail(string email)
        {
            string sql_checkphone = "select * from " +
                    DBStaticData.DataBaseUserTableName +
                    " where " +
                    DBStaticData.DataBaseUserTableEmailField + " = '" + email + "'";

            if (ExecuteNonDataSet(sql_checkphone, out DataSet set))
            {
                if (0 != set.Tables.Count)
                    if (0 != set.Tables[0].Rows.Count)
                        return false;
            }
            else
                return false;

            return true;
        }
    }
}
