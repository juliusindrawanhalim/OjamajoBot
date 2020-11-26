using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_User_Garden_Data
    {
        public static readonly string tableName = "user_garden_data";

        public static class Columns
        {
            public static readonly string id_user = "id_user";
            public static readonly string last_water_time = "last_water_time";
            public static readonly string plant_growth = "plant_growth";
        }

    }
}
