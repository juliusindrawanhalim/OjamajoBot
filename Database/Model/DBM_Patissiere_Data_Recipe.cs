using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Patissiere_Data_Recipe
    {
        public static readonly string tableName = "patissiere_data_recipe";

        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string level = "level";
            public static readonly string name = "name";
            public static readonly string description = "description";
            public static readonly string ingredient = "ingredient";
            public static readonly string point_progress = "point_progress";
            public static readonly string point_quality = "point_quality";
            public static readonly string contribution_point = "contribution_point";
        }

    }
}
