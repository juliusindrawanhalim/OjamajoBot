using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_User_Data
    {
        public static readonly string tableName = "user_data";

        public static class Columns
        {
            public static readonly string id_user = "id_user";
            public static readonly string magic_seeds = "magic_seeds";
            public static readonly string royal_seeds = "royal_seeds";
            public static readonly string chat_level = "chat_level";
            public static readonly string chat_exp = "chat_exp";
        }

    }
}
