using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Trading_Card_Tradeboard
    {
        public static readonly string tableName = "trading_card_tradeboard";

        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string id_guild = "id_guild";
            public static readonly string id_user = "id_user";
            public static readonly string id_card_looking_for = "id_card_looking_for";
            public static readonly string id_card_have = "id_card_have";
            public static readonly string last_update = "last_update";
        }
    }
}
