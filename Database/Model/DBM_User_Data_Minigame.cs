using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_User_Data_Minigame
    {
        public static readonly string tableName = "user_data_minigame";


        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string id_guild = "id_guild";
            public static readonly string id_user = "id_user";
            public static readonly string score = "score";
        }

    }
}
