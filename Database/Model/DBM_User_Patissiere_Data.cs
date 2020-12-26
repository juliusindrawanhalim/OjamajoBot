using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_User_Patissiere_Data
    {
        public static readonly string tableName = "user_patissiere_data";

        public static class Columns
        {
            public static readonly string id_user = "id_user";
            public static readonly string level = "level";
            public static readonly string exp = "exp";
            public static readonly string point_energy = "point_energy";
            public static readonly string point_progress = "point_progress";
            public static readonly string contribution_total = "contribution_total";
            public static readonly string contribution_point = "contribution_point";
            public static readonly string last_venture_time = "last_venture_time";
            public static readonly string venture_rate = "venture_rate";
            public static readonly string venture_majo = "venture_majo";
            public static readonly string created_at = "created_at";
        }

    }
}
