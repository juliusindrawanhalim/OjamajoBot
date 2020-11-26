using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Guild_User_Birthday
    {
        public static readonly string tableName = "guild_user_birthday";

        public static class Columns
        {
            public static readonly string id_guild = "id_guild";
            public static readonly string id_user = "id_user";
            public static readonly string birthday_date = "birthday_date";
            public static readonly string show_year = "show_year";
        }
    }
}
