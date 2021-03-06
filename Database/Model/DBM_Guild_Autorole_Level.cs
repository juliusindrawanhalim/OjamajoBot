﻿using System;
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
            public static readonly string level_min = "level_min";
            public static readonly string id_role = "id_role";
            public static readonly string id_role_remove = "id_role_remove";
        }
    }
}
