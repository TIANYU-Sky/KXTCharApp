using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KXTServiceDBServer.Files
{
    internal class UserRelationDB : IDBUserChatReader
    {
        private readonly SqlConnection SQLConnection;

        public UserRelationDB()
        {
            SQLConnection = new SqlConnection(DBStaticData.UserChatDBConnString);
            SQLConnection.Open();
        }

        public void Close()
        {
            try
            {
                SQLConnection.Close();
            }
            catch
            {
                
            }
        }

        public bool DelRelation(string userA, string userB)
        {
            if (
                null != userA && "" != userA
                && null != userB && "" != userB
              )
            {
                string sql = string.Format
                    (
                    "delete from {0} " +
                    "where ({1} + '_' + {2}) " +
                    "in ('{3}', '{4}')",
                    DBStaticData.UserChatDBTableName,
                    DBStaticData.UserChatDBTableFieldA,
                    DBStaticData.UserChatDBTableFieldB,
                    userA + "_" + userB,
                    userB + "_" + userA
                    );

                return ExecuteNonQuery(sql);
            }
            return false;
        }
        public bool GetRelation(string userA, string userB, out string relation_id)
        {
            

            if (
                null != userA && "" != userA
                && null != userB && "" != userB
              )
            {
                string sql = string.Format
                    (
                    "select * from {0} " +
                    "where ({1} + '_' + {2}) " +
                    "in ('{3}', '{4}')",
                    DBStaticData.UserChatDBTableName,
                    DBStaticData.UserChatDBTableFieldA,
                    DBStaticData.UserChatDBTableFieldB,
                    userA + "_" + userB,
                    userB + "_" + userA
                    );

                if (ExecuteNonDataSet(sql, out DataSet data))
                    if (0 < data.Tables[0].Rows.Count)
                    {
                        relation_id = data.Tables[0].Rows[0].ItemArray[2] as string;
                        return true;
                    }
            }
            relation_id = "";
            return false;
        }
        public bool NewRelation(string userA, string userB, out string relation_id)
        {
            if (
                null != userA && "" != userA
                && null != userB && "" != userB
              )
            {
                string id = Guid.NewGuid().ToString();
                string sql = string.Format
                    (
                    "insert into {0} values ('{1}', '{2}', '{3}') ",
                    DBStaticData.UserChatDBTableName,
                    userA,
                    userB,
                    id
                    );

                if (ExecuteNonQuery(sql))
                {
                    relation_id = id;
                    return true;
                }
            }
            relation_id = "";
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
    }
}
