using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Guild_Role_React
    {
        public static readonly string tableName = "guild_role_react";

        public static class Columns
        {
            public static readonly string id_guild = "id_guild";
            public static readonly string id_message = "id_message";
            public static readonly string emoji = "emoji";
            public static readonly string id_role = "id_role";
            public static readonly string created_at = "created_at";
        }
    }
}
