using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Database;
using OjamajoBot.Database.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OjamajoBot {
    public static class PatissiereCore
    {
        public static string version = "1.00";
        public static string propertyShop = "pattisier_shop";

        public static Color embedShopColor = new Color(2,19,43);

        public static Dictionary<string, object> getUserData(ulong userId)
        {
            DataTable dt = new DataTable();
            Dictionary<string, object> ret = new Dictionary<string, object>();

            try
            {
                DBC db = new DBC();
                string query = $"SELECT * " +
                $" FROM {DBM_User_Patissiere_Data.tableName} " +
                $" WHERE {DBM_User_Patissiere_Data.Columns.id_user}=@{DBM_User_Patissiere_Data.Columns.id_user}";

                Dictionary<string, object> colSelect = new Dictionary<string, object>();
                colSelect[DBM_Guild_User_Avatar.Columns.id_user] = userId.ToString();

                dt = db.selectAll(query, colSelect);

                if (dt.Rows.Count <= 0)
                    insertUserData(userId);

                dt = db.selectAll(query, colSelect);

                //List<object> colValues = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    ret[DBM_User_Patissiere_Data.Columns.level] = row[DBM_User_Patissiere_Data.Columns.level];
                    ret[DBM_User_Patissiere_Data.Columns.exp] = row[DBM_User_Patissiere_Data.Columns.exp];
                    ret[DBM_User_Patissiere_Data.Columns.point_energy] = row[DBM_User_Patissiere_Data.Columns.point_energy];
                    ret[DBM_User_Patissiere_Data.Columns.point_progress] = row[DBM_User_Patissiere_Data.Columns.point_progress];
                    ret[DBM_User_Patissiere_Data.Columns.contribution_total] = row[DBM_User_Patissiere_Data.Columns.contribution_total];
                    ret[DBM_User_Patissiere_Data.Columns.contribution_point] = row[DBM_User_Patissiere_Data.Columns.contribution_point];
                    ret[DBM_User_Patissiere_Data.Columns.last_venture_time] = row[DBM_User_Patissiere_Data.Columns.last_venture_time];
                    ret[DBM_User_Patissiere_Data.Columns.venture_rate] = row[DBM_User_Patissiere_Data.Columns.venture_rate];
                    ret[DBM_User_Patissiere_Data.Columns.venture_majo] = row[DBM_User_Patissiere_Data.Columns.venture_majo];
                    ret[DBM_User_Patissiere_Data.Columns.created_at] = row[DBM_User_Patissiere_Data.Columns.created_at];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            return ret;
        }

        public static void insertUserData(ulong userId)
        {
            try
            {
                DBC db = new DBC();

                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_User_Patissiere_Data.Columns.id_user] = userId.ToString();
                db.insert(DBM_User_Patissiere_Data.tableName, columns);
            }
            catch
            {

            }
        }

        public static EmbedBuilder printStatusTemplate(SocketCommandContext context, Color color, SocketGuildUser otherUser = null)
        {
            var userId = context.User.Id;
            var username = context.User.Username;
            var thumbnailUrl = context.User.GetAvatarUrl();

            if (otherUser != null)
            {
                userId = otherUser.Id;
                username = otherUser.Username;
                thumbnailUrl = otherUser.GetAvatarUrl();
            }

            var userData = getUserData(userId);

            return new EmbedBuilder()
            .WithAuthor(username,thumbnailUrl)
            .WithTitle($"Patissiere Status | Level: " +
            $"{Convert.ToInt32(userData[DBM_User_Patissiere_Data.Columns.level].ToString())}")
            .WithColor(color)
            .AddField("EXP:", $"**{Convert.ToInt32(userData[DBM_User_Patissiere_Data.Columns.exp].ToString())}**", true)
            .AddField("Contribution:", 
            $"**Total: {Convert.ToInt32(userData[DBM_User_Patissiere_Data.Columns.contribution_total].ToString())}**\n" +
            $"**Point: {Convert.ToInt32(userData[DBM_User_Patissiere_Data.Columns.contribution_point].ToString())}**", true);
        }

        public class RecipeDiary
        {
            public class Ingredients
            {

            }
        }
    }
}
