using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Guild_User_Avatar
    {
        public static readonly string tableName = "guild_user_avatar";

        public static class Columns
        {
            public static readonly string id_guild = "id_guild";
            public static readonly string id_user = "id_user";
            public static readonly string nickname = "nickname";
            public static readonly string color = "color";
            public static readonly string chat_level = "chat_level";
            public static readonly string chat_exp = "chat_exp";
            public static readonly string info = "info";
            public static readonly string created_at = "created_at";
        }
    }
}
