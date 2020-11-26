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
    public static class TradingCardGuildCore
    {

        public static Dictionary<string, object> getGuildData(ulong guildId)
        {
            DataTable dt = new DataTable();
            Dictionary<string, object> ret = new Dictionary<string, object>();

            try
            {
                DBC db = new DBC();
                string query = $"SELECT * " +
                $" FROM {DBM_Trading_Card_Guild.tableName} " +
                $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild}";

                Dictionary<string, object> colSelect = new Dictionary<string, object>();
                colSelect[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();

                dt = db.selectAll(query, colSelect);

                //create if not exists
                if (dt.Rows.Count <= 0)
                    insertGuildData(guildId);

                dt = db.selectAll(query, colSelect);

                List<object> colValues = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    ret[DBM_Trading_Card_Guild.Columns.id_guild] = row[DBM_Trading_Card_Guild.Columns.id_guild];
                    ret[DBM_Trading_Card_Guild.Columns.id_channel_spawn] = row[DBM_Trading_Card_Guild.Columns.id_channel_spawn];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_interval] = row[DBM_Trading_Card_Guild.Columns.spawn_interval];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_id] = row[DBM_Trading_Card_Guild.Columns.spawn_id];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_parent] = row[DBM_Trading_Card_Guild.Columns.spawn_parent];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_category] = row[DBM_Trading_Card_Guild.Columns.spawn_category];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_token] = row[DBM_Trading_Card_Guild.Columns.spawn_token];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_is_mystery] = row[DBM_Trading_Card_Guild.Columns.spawn_is_mystery];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_is_badcard] = row[DBM_Trading_Card_Guild.Columns.spawn_is_badcard];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_is_zone] = row[DBM_Trading_Card_Guild.Columns.spawn_is_zone];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_badcard_question] = row[DBM_Trading_Card_Guild.Columns.spawn_badcard_question];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_badcard_answer] = row[DBM_Trading_Card_Guild.Columns.spawn_badcard_answer];
                    ret[DBM_Trading_Card_Guild.Columns.spawn_time] = row[DBM_Trading_Card_Guild.Columns.spawn_time];
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ret;
        }

        public static void insertGuildData(ulong guildId)
        {
            DBC db = new DBC();

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();
            db.insert(DBM_Trading_Card_Guild.tableName, columns);
        }

    }
}
