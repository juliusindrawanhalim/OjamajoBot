using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Trading_Card_Leaderboard
    {
        public static readonly string tableName = "trading_card_leaderboard";

        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string id_guild = "id_guild";
            public static readonly string card_pack = "card_pack";
            public static readonly string id_user = "id_user";
            public static readonly string complete_date = "complete_date";
        }
    }
}
