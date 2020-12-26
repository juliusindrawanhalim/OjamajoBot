using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Patissiere_Ingredient
    {
        public static readonly string tableName = "patissiere_ingeredient";

        public enum CATEGORY {NORMAL,RARE}

        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string level = "level";
            public static readonly string name = "name";
            public static readonly string category = "category";
            public static readonly string img_src = "img_src";
            public static readonly string duration_minute = "duration_minute";
            public static readonly string venture_majo = "venture_majo";
        }
    }
}
