using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_User_Trading_Card_Inventory
    {
        public static readonly string tableName = "user_trading_card_inventory";


        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string id_user = "id_user";
            public static readonly string id_card = "id_card";
            public static readonly string created_at = "created_at";
        }

    }
}
