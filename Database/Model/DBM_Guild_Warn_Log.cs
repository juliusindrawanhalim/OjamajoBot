using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Guild_Warn_Log
    {
        public static readonly string tableName = "guild_warn_log";

        public static class Columns
        {
            public static readonly string id_guild = "id_guild";
            public static readonly string id_user = "id_user";
            public static readonly string message = "message";
            public static readonly string created_at = "created_at";
        }

    }
}
