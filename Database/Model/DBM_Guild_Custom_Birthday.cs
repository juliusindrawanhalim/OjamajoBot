using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Guild_Custom_Birthday
    {
        public static readonly string tableName = "guild_custom_birthday";

        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string id_guild = "id_guild";
            public static readonly string message = "message";
            public static readonly string img_url = "img_url";
            public static readonly string created_at = "created_at";
        }

    }
}
