using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using OjamajoBot.Database;
using OjamajoBot.Database.Model;

namespace OjamajoBot
{
    public static class UserTradingCardDataCore
    {

        public static Dictionary<string, object> getUserData(ulong userId)
        {
            DataTable dt = new DataTable();
            Dictionary<string, object> ret = new Dictionary<string, object>();

            try
            {
                DBC db = new DBC();
                string query = $"SELECT * " +
                $" FROM {DBM_User_Trading_Card_Data.tableName} " +
                $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user}";

                Dictionary<string, object> colSelect = new Dictionary<string, object>();
                colSelect[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();

                dt = db.selectAll(query, colSelect);

                //create if not exists
                if (dt.Rows.Count <= 0)
                    insertUserData(userId);

                dt = db.selectAll(query, colSelect);

                foreach (DataRow row in dt.Rows)
                {
                    ret[DBM_User_Trading_Card_Data.Columns.id_user] = row[DBM_User_Trading_Card_Data.Columns.id_user];
                    ret[DBM_User_Trading_Card_Data.Columns.rank] = row[DBM_User_Trading_Card_Data.Columns.rank];
                    ret[DBM_User_Trading_Card_Data.Columns.exp] = row[DBM_User_Trading_Card_Data.Columns.exp];
                    ret[DBM_User_Trading_Card_Data.Columns.catch_attempt] = row[DBM_User_Trading_Card_Data.Columns.catch_attempt];
                    ret[DBM_User_Trading_Card_Data.Columns.catch_token] = row[DBM_User_Trading_Card_Data.Columns.catch_token];
                    ret[DBM_User_Trading_Card_Data.Columns.card_zone] = row[DBM_User_Trading_Card_Data.Columns.card_zone];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = row[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum] = row[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal] = row[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos] = row[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos];

                    ret[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = row[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum] = row[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal] = row[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos] = row[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos];

                    ret[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = row[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum] = row[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal] = row[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos] = row[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos];

                    ret[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal] = row[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum] = row[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal] = row[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos] = row[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos];

                    ret[DBM_User_Trading_Card_Data.Columns.boost_momoko_normal] = row[DBM_User_Trading_Card_Data.Columns.boost_momoko_normal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum] = row[DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_momoko_metal] = row[DBM_User_Trading_Card_Data.Columns.boost_momoko_metal];
                    ret[DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos] = row[DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos];

                    ret[DBM_User_Trading_Card_Data.Columns.boost_other_special] = row[DBM_User_Trading_Card_Data.Columns.boost_other_special];
                    ret[DBM_User_Trading_Card_Data.Columns.created_at] = row[DBM_User_Trading_Card_Data.Columns.created_at];

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ret;
        }

        //public static Dictionary<string,object> getUserData(ulong clientId)
        //{
        //    DataTable dt = new DataTable();
        //    Dictionary<string, object> ret = new Dictionary<string, object>();

        //    try
        //    {
        //        DBC db = new DBC();
        //        string query = $"SELECT * " +
        //        $" FROM {DBM_User_Trading_Card_Data.tableName} " +
        //        $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user}";

        //        Dictionary<string, object> colSelect = new Dictionary<string, object>();
        //        colSelect[DBM_User_Trading_Card_Data.Columns.id_user] = clientId.ToString();

        //        dt = db.selectAll(query, colSelect);
                
        //        if (dt.Rows.Count<=0)
        //            insertUserData(clientId);

        //        dt = db.selectAll(query, colSelect);

        //        //List<object> colValues = new List<object>();
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            ret[DBM_User_Data.Columns.id_user] = row[DBM_User_Data.Columns.id_user];
        //            ret[DBM_User_Data.Columns.magic_seeds] = row[DBM_User_Data.Columns.magic_seeds];
        //            ret[DBM_User_Data.Columns.royal_seeds] = row[DBM_User_Data.Columns.royal_seeds];
        //        }


        //    } catch(Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }

        //    return ret;
        //}

        public static void insertUserData(ulong clientId)
        {
            DBC db = new DBC();

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Data.Columns.id_user] = clientId.ToString();
            db.insert(DBM_User_Data.tableName, columns);
        }

        public static void addCatchAttempt(ulong clientId,string guildCaptureToken = "")
        {
            //guildCaptureToken: optional parameter if want to be added so user cannot catch card anymore
            //check if user data exists/not
            DBC db = new DBC();
            string query = $"SELECT * " +
            $" FROM {DBM_User_Trading_Card_Data.tableName} " +
            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user}";

            Dictionary<string, object> colSelect = new Dictionary<string, object>();
            colSelect[DBM_User_Trading_Card_Data.Columns.id_user] = clientId.ToString();
            DataTable dt = db.selectAll(query, colSelect);
            if (dt.Rows.Count <= 0)
                insertUserData(clientId);

            //update magic seeds
            query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
            $" SET {DBM_User_Trading_Card_Data.Columns.catch_attempt} = {DBM_User_Trading_Card_Data.Columns.catch_attempt}+1 ";
            if (guildCaptureToken != "")
                query += $",{DBM_User_Trading_Card_Data.Columns.catch_token}=@{DBM_User_Trading_Card_Data.Columns.catch_token} ";
            query += $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user}";

            DBC dbUpdate = new DBC();
            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Data.Columns.id_user] = clientId.ToString();
            if (guildCaptureToken != "")
                columns[DBM_User_Trading_Card_Data.Columns.catch_token] = guildCaptureToken;

            dbUpdate.update(query, columns);
        }



        public static Boolean checkCardCompletion(ulong userId,string pack)
        {
            DBC db = new DBC();
            Dictionary<string, object> colSelect = new Dictionary<string, object>();

            int totalTarget = 0;
            switch (pack)
            {
                case "doremi":
                    totalTarget = TradingCardCore.Doremi.maxNormal + TradingCardCore.Doremi.maxPlatinum + 
                        TradingCardCore.Doremi.maxMetal + TradingCardCore.Doremi.maxOjamajos;
                    break;
                case "hazuki":
                    totalTarget = TradingCardCore.Hazuki.maxNormal + TradingCardCore.Hazuki.maxPlatinum +
                        TradingCardCore.Hazuki.maxMetal + TradingCardCore.Hazuki.maxOjamajos;
                    break;
                case "aiko":
                    totalTarget = TradingCardCore.Aiko.maxNormal + TradingCardCore.Aiko.maxPlatinum +
                        TradingCardCore.Hazuki.maxMetal + TradingCardCore.Aiko.maxOjamajos;
                    break;
                case "onpu":
                    totalTarget = TradingCardCore.Onpu.maxNormal + TradingCardCore.Onpu.maxPlatinum +
                        TradingCardCore.Onpu.maxMetal + TradingCardCore.Onpu.maxOjamajos;
                    break;
                case "momoko":
                    totalTarget = TradingCardCore.Momoko.maxNormal + TradingCardCore.Momoko.maxPlatinum +
                        TradingCardCore.Momoko.maxMetal + TradingCardCore.Momoko.maxOjamajos;
                    break;
                case "other":
                    totalTarget = TradingCardCore.maxSpecial;
                    break;
            }

            bool ret = false;

            if (pack != "other")
            {
                try
                {
                    string query = @$"select (select count(*) 
                    from {DBM_User_Trading_Card_Inventory.tableName} inv, {DBM_Trading_Card_Data.tableName} tc
                    where inv.{DBM_User_Trading_Card_Inventory.Columns.id_user} = @{DBM_User_Trading_Card_Inventory.Columns.id_user} and 
                    tc.{DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} and 
                    inv.{DBM_User_Trading_Card_Inventory.Columns.id_card} = tc.{DBM_Trading_Card_Data.Columns.id_card}) + 
                    (select count(distinct(inv.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card})) 
                    from {DBM_User_Trading_Card_Inventory_Ojamajos.tableName} inv, {DBM_Trading_Card_Data_Ojamajos.tableName} tc 
                    where inv.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user}=@{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user} and 
                    tc.{DBM_Trading_Card_Data_Ojamajos.Columns.pack} like @{DBM_Trading_Card_Data_Ojamajos.Columns.pack}_ojamajos and 
                    inv.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card} = tc.{DBM_Trading_Card_Data_Ojamajos.Columns.id_card}) as total";

                    colSelect[DBM_User_Trading_Card_Inventory.Columns.id_user] = userId.ToString();
                    colSelect[$"{DBM_Trading_Card_Data.Columns.pack}"] = pack;
                    colSelect[$"{DBM_Trading_Card_Data_Ojamajos.Columns.pack}_ojamajos"] = $"%{pack}%";

                    foreach (DataRow rows in new DBC().selectAll(query, colSelect).Rows)
                    {
                        Console.WriteLine($"{pack} : {Convert.ToInt32(rows["total"]).ToString()}");
                        if (Convert.ToInt32(rows["total"]) >= totalTarget)
                            ret = true;
                    }
                }
                catch(Exception e)
                {
                    Console.Write(e.ToString());
                }
                
            } else
            {
                //other card pack
                string query = @$"select (select count(*) 
                from {DBM_User_Trading_Card_Inventory.tableName} inv, {DBM_Trading_Card_Data.tableName} tc
                where inv.{DBM_User_Trading_Card_Inventory.Columns.id_user} = @{DBM_User_Trading_Card_Inventory.Columns.id_user} and 
                tc.{DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} and 
                inv.{DBM_User_Trading_Card_Inventory.Columns.id_card} = tc.{DBM_Trading_Card_Data.Columns.id_card}) as total";

                colSelect[DBM_User_Trading_Card_Inventory.Columns.id_user] = userId.ToString();
                colSelect[DBM_Trading_Card_Data.Columns.pack] = pack;
                foreach (DataRow rows in db.selectAll(query, colSelect).Rows)
                {
                    if (Convert.ToInt32(rows["total"]) >= totalTarget)
                        ret = true;
                }
            }

            return ret;
        }

    }
}
