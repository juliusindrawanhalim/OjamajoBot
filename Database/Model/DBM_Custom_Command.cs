using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Custom_Command
    {
        public static readonly string tableName = "guild_custom_command";

        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string id_guild = "id_guild";
            public static readonly string command = "command";
            public static readonly string content = "content";
            public static readonly string created_at = "created_at";
        }
    }
}
