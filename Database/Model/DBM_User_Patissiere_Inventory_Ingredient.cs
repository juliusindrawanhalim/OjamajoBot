using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Patissiere_Inventory_Ingredient
    {
        public static readonly string tableName = "patissiere_inventory_ingeredient";

        public static class Columns
        {
            public static readonly string id = "id";
            public static readonly string id_user = "id_user";
            public static readonly string id_item = "id_item";
            public static readonly string qty = "qty";
        }
    }
}
