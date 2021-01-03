using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Trading_Card_Marketboard
    {
        public static readonly string tableName = "trading_card_marketboard";

        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string id_guild = "id_guild";
            public static readonly string id_user = "id_user";
            public static readonly string id_card = "id_card";
            public static readonly string price = "price";
            public static readonly string last_update = "last_update";
        }
    }
}
