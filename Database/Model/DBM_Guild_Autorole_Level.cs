using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Guild_Autorole_Level
    {
        public static readonly string tableName = "guild_autorole_level";

        public static class Columns
        {
            public static readonly string id_guild = "id_guild";
            public static readonly string id_user = "id_user";
            public static readonly string level_min = "level_min";
            public static readonly string autorole = "autorole";
        }
    }
}
