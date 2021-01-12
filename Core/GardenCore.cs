using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using OjamajoBot.Database;
using OjamajoBot.Database.Model;
using System.Data;

namespace OjamajoBot
{
    public static class GardenCore
    {
        public static string headCoreConfigFolder = "core/garden/";
        public static string headUserConfigFolder = "garden";

        public static string imgMagicSeeds = "https://cdn.discordapp.com/attachments/706770454697738300/738695405558038578/magic_seeds.jpg";
        public static string imgRoyalSeeds = "https://cdn.discordapp.com/attachments/706770454697738300/726137300052082728/royal_seeds.gif";
        public static string[] weather = { $"☀️", "sunny", "A perfect time to water the plant!", "4","5" };//current weather/initialize it
        public static string[,] arrRandomWeather = {
            {$"☀️", "sunny","A perfect time to water the plant!","4","5"},
            {$"☁️", "cloudy","There will be a chance to rain...","2","4"},
            {$"🌧️", "raining","Not sure if it's a good time to water the plant.","1","3"},
            {$"⛈️", "thunder storm","I don't think it's the best time to water the plant now...","1","2"}
        };

        //revalidate if user data exists/not
        public static Dictionary<string,object> getUserGardenData(ulong clientId)
        {
            DataTable dt = new DataTable();
            Dictionary<string, object> ret = new Dictionary<string, object>();

            try
            {
                DBC db = new DBC();
                string query = $"SELECT * " +
                $" FROM {DBM_User_Garden_Data.tableName} " +
                $" WHERE {DBM_User_Garden_Data.Columns.id_user}=@{DBM_User_Garden_Data.Columns.id_user}";

                Dictionary<string, object> colSelect = new Dictionary<string, object>();
                colSelect[DBM_User_Garden_Data.Columns.id_user] = clientId.ToString();

                dt = db.selectAll(query, colSelect);

                if (dt.Rows.Count <= 0)
                    insertUserGardenData(clientId);

                dt = db.selectAll(query, colSelect);

                List<object> colValues = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    ret[DBM_User_Garden_Data.Columns.id_user] = row[DBM_User_Garden_Data.Columns.id_user];
                    ret[DBM_User_Garden_Data.Columns.last_water_time] = row[DBM_User_Garden_Data.Columns.last_water_time];
                    ret[DBM_User_Garden_Data.Columns.plant_growth] = row[DBM_User_Garden_Data.Columns.plant_growth];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ret;
        }

        public static void insertUserGardenData(ulong clientId)
        {
            DBC db = new DBC();

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Garden_Data.Columns.id_user] = clientId.ToString();
            db.insert(DBM_User_Garden_Data.tableName, columns);
        }

        public static void createGardenUserData(string playerDataDirectory)
        {
            File.Copy($@"{Config.Core.headConfigFolder}{headCoreConfigFolder}garden_template_data.json", $@"{playerDataDirectory}");
        }

        public static void updatePlantProgress(ulong clientId, int growth)
        {
            string query = $"UPDATE {DBM_User_Garden_Data.tableName} " +
                $" SET {DBM_User_Garden_Data.Columns.plant_growth}={growth}, " +
                $" {DBM_User_Garden_Data.Columns.last_water_time}=@{DBM_User_Garden_Data.Columns.last_water_time} " +
                $" WHERE {DBM_User_Garden_Data.Columns.id_user}=@{DBM_User_Garden_Data.Columns.id_user}";

            DBC db = new DBC();

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Garden_Data.Columns.id_user] = clientId.ToString();
            columns[DBM_User_Garden_Data.Columns.last_water_time] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            db.update(query, columns);
        }

        public static void waterPlant(ulong clientId, int growth)
        {
            string query = $"UPDATE {DBM_User_Garden_Data.tableName} " +
                $" SET {DBM_User_Garden_Data.Columns.plant_growth}={DBM_User_Garden_Data.Columns.plant_growth}+{growth}, " +
                $" {DBM_User_Garden_Data.Columns.last_water_time}=@{DBM_User_Garden_Data.Columns.last_water_time} " +
                $" WHERE {DBM_User_Garden_Data.Columns.id_user}=@{DBM_User_Garden_Data.Columns.id_user}";

            DBC db = new DBC();

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Garden_Data.Columns.id_user] = clientId.ToString();
            columns[DBM_User_Garden_Data.Columns.last_water_time] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            db.update(query, columns);
        }

        public static void insertGardenData(ulong clientId)
        {
            DBC db = new DBC();

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Data.Columns.id_user] = clientId.ToString();
            db.insert(DBM_User_Data.tableName, columns);
        }

    }
}
