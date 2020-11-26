using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Guild_Autorole_User_Join
    {
        public static readonly string tableName = "guild_autorole_user_join";

        public static class Columns
        {
            public static readonly string id_guild = "id_guild";
            public static readonly string autorole = "autorole";
        }
    }
}
