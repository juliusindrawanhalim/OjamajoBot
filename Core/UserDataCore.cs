using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using OjamajoBot.Database;
using OjamajoBot.Database.Model;

namespace OjamajoBot
{
    public static class UserDataCore
    {
        public static Dictionary<string,object> getUserData(ulong clientId)
        {
            DataTable dt = new DataTable();
            Dictionary<string, object> ret = new Dictionary<string, object>();

            try
            {
                DBC db = new DBC();
                string query = $"SELECT * " +
                $" FROM {DBM_User_Data.tableName} " +
                $" WHERE {DBM_User_Data.Columns.id_user}=@{DBM_User_Data.Columns.id_user}";

                Dictionary<string, object> colSelect = new Dictionary<string, object>();
                colSelect[DBM_User_Data.Columns.id_user] = clientId.ToString();

                dt = db.selectAll(query, colSelect);
                
                if (dt.Rows.Count<=0)
                    insertUserData(clientId);

                dt = db.selectAll(query, colSelect);

                //List<object> colValues = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    ret[DBM_User_Data.Columns.id_user] = row[DBM_User_Data.Columns.id_user];
                    ret[DBM_User_Data.Columns.magic_seeds] = row[DBM_User_Data.Columns.magic_seeds];
                    ret[DBM_User_Data.Columns.royal_seeds] = row[DBM_User_Data.Columns.royal_seeds];
                    ret[DBM_User_Data.Columns.chat_level] = row[DBM_User_Data.Columns.chat_level];
                    ret[DBM_User_Data.Columns.chat_exp] = row[DBM_User_Data.Columns.chat_exp];
                }

                //colValues = dt.AsEnumerable().Select(r => r[DBM_User_Data.Columns.magic_seeds]).ToList();
                //Console.WriteLine(colValues[0]);


                //List<int> ids = new List<int>(dt.Rows.Count);
                //foreach (DataRow row in dt.Rows)
                //{
                //    ids.Add((int)row["magic_seeds"]);
                //}
                //Console.WriteLine(ids[0]);



            } catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ret;
        }

        public static void insertUserData(ulong clientId)
        {
            try
            {
                DBC db = new DBC();

                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_User_Data.Columns.id_user] = clientId.ToString();
                db.insert(DBM_User_Data.tableName, columns);
            }
            catch
            {

            }
            
        }

        public static void updateMagicSeeds(ulong clientId, int amount)
        {
            int maximumCap = 3000;
            //check if user data exists/not
            DBC db = new DBC();
            string query = $"SELECT * " +
            $" FROM {DBM_User_Data.tableName} " +
            $" WHERE {DBM_User_Data.Columns.id_user}=@{DBM_User_Data.Columns.id_user}";

            Dictionary<string, object> colSelect = new Dictionary<string, object>();
            colSelect[DBM_User_Data.Columns.id_user] = clientId.ToString();
            DataTable dt = db.selectAll(query, colSelect);
            if (dt.Rows.Count <= 0)
                insertUserData(clientId);

            //update magic seeds
            query = $"UPDATE {DBM_User_Data.tableName} ";
            if (amount >= 1) //add
                query += $" SET {DBM_User_Data.Columns.magic_seeds} = CASE " +
                    $" WHEN {DBM_User_Data.Columns.magic_seeds}+{amount}>={maximumCap} THEN {maximumCap} " +
                    $" ELSE {DBM_User_Data.Columns.magic_seeds}+{amount} " +
                    $" END ";
            else //negative/substract
                query += $" SET {DBM_User_Data.Columns.magic_seeds} = CASE " +
                    $" WHEN {DBM_User_Data.Columns.magic_seeds}>={amount} THEN  {DBM_User_Data.Columns.magic_seeds}-{amount} " +
                    $" ELSE 0 " +
                    $" END ";
            query+=$" WHERE {DBM_User_Data.Columns.id_user}=@{DBM_User_Data.Columns.id_user}";

            DBC dbUpdate = new DBC();
            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Data.Columns.id_user] = clientId.ToString();

            dbUpdate.update(query, columns);
        }

        public static void updateRoyalSeeds(ulong clientId, int amount)
        {
            int maximumCap = 100;
            //check if user data exists/not
            DBC db = new DBC();
            string query = $"SELECT * " +
            $" FROM {DBM_User_Data.tableName} " +
            $" WHERE {DBM_User_Data.Columns.id_user}=@{DBM_User_Data.Columns.id_user}";

            Dictionary<string, object> colSelect = new Dictionary<string, object>();
            colSelect[DBM_User_Data.Columns.id_user] = clientId.ToString();
            DataTable dt = db.selectAll(query, colSelect);
            if (dt.Rows.Count <= 0)
                insertUserData(clientId);

            //update royal seeds
            query = $"UPDATE {DBM_User_Data.tableName} ";
            if (amount >= 1) //add
                query += $" SET {DBM_User_Data.Columns.royal_seeds} = CASE " +
                    $" WHEN {DBM_User_Data.Columns.royal_seeds}+{amount}>={maximumCap} THEN {maximumCap} " +
                    $" ELSE {DBM_User_Data.Columns.royal_seeds}+{amount} " +
                    $" END ";
            else //negative/substract
                query += $" SET {DBM_User_Data.Columns.royal_seeds} = CASE " +
                    $" WHEN {DBM_User_Data.Columns.royal_seeds}>={amount} THEN  {DBM_User_Data.Columns.royal_seeds}-{amount} " +
                    $" ELSE 0 " +
                    $" END ";
            query += $" WHERE {DBM_User_Data.Columns.id_user}=@{DBM_User_Data.Columns.id_user}";

            DBC dbUpdate = new DBC();
            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Data.Columns.id_user] = clientId.ToString();

            dbUpdate.update(query, columns);
        }

        public static async Task updateChatExp(SocketUser user, int amount)
        {
            //return type: boolean: true if level up
            //update chat exp & level up
            var userId = user.Id;
            var username = user.Username;
            var userAvatarUrl = user.GetAvatarUrl();

            int maxLevel = 200;
            //check if user data exists/not
            DBC db = new DBC();
            string query = $"SELECT * " +
            $" FROM {DBM_User_Data.tableName} " +
            $" WHERE {DBM_User_Data.Columns.id_user}=@{DBM_User_Data.Columns.id_user}";

            Dictionary<string, object> colSelect = new Dictionary<string, object>();
            colSelect[DBM_User_Data.Columns.id_user] = userId.ToString();
            DataTable dt = db.selectAll(query, colSelect);
            if (dt.Rows.Count <= 0)
                insertUserData(userId);

            var userData = getUserData(userId);
            int level = Convert.ToInt32(userData[DBM_User_Data.Columns.chat_level]);
            int exp = Convert.ToInt32(userData[DBM_User_Data.Columns.chat_exp]);
            int nextExp = level * 100;

            if (level <= maxLevel)
            {
                if (exp + amount >= nextExp)
                {
                    //level up
                    query = $"UPDATE {DBM_User_Data.tableName} ";
                    query += $" SET {DBM_User_Data.Columns.chat_level}={DBM_User_Data.Columns.chat_level}+1, " +
                        $" {DBM_User_Data.Columns.chat_exp}=0 ";
                    query += $" WHERE {DBM_User_Data.Columns.id_user}=@{DBM_User_Data.Columns.id_user}";
                }
                else
                { //add exp
                    query = $"UPDATE {DBM_User_Data.tableName} ";
                    query += $" SET {DBM_User_Data.Columns.chat_exp}={DBM_User_Data.Columns.chat_exp}+1 ";
                    query += $" WHERE {DBM_User_Data.Columns.id_user}=@{DBM_User_Data.Columns.id_user}";
                }

                DBC dbUpdate = new DBC();
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_User_Data.Columns.id_user] = userId.ToString();
                dbUpdate.update(query, columns);
            }
        }
    }
}
