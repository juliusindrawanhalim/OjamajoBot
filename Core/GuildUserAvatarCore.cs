using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OjamajoBot.Database;
using OjamajoBot.Database.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OjamajoBot.Core
{
    public static class GuildUserAvatarCore
    {
        public static Dictionary<string, object> getUserData(ulong guildId, ulong clientId)
        {
            DataTable dt = new DataTable();
            Dictionary<string, object> ret = new Dictionary<string, object>();
            
            try
            {
                DBC db = new DBC();
                string query = $"SELECT * " +
                $" FROM {DBM_Guild_User_Avatar.tableName} " +
                $" WHERE {DBM_Guild_User_Avatar.Columns.id_guild}=@{DBM_Guild_User_Avatar.Columns.id_guild} AND " +
                $"{DBM_Guild_User_Avatar.Columns.id_user}=@{DBM_Guild_User_Avatar.Columns.id_user}";

                Dictionary<string, object> colSelect = new Dictionary<string, object>();
                colSelect[DBM_Guild_User_Avatar.Columns.id_guild] = guildId.ToString();
                colSelect[DBM_Guild_User_Avatar.Columns.id_user] = clientId.ToString();

                dt = db.selectAll(query, colSelect);

                if (dt.Rows.Count <= 0)
                    insertUserData(guildId, clientId);

                dt = db.selectAll(query, colSelect);

                //List<object> colValues = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    ret[DBM_Guild_User_Avatar.Columns.id_guild] = row[DBM_Guild_User_Avatar.Columns.id_guild];
                    ret[DBM_Guild_User_Avatar.Columns.id_user] = row[DBM_Guild_User_Avatar.Columns.id_user];
                    ret[DBM_Guild_User_Avatar.Columns.nickname] = row[DBM_Guild_User_Avatar.Columns.nickname];
                    ret[DBM_Guild_User_Avatar.Columns.color] = row[DBM_Guild_User_Avatar.Columns.color];
                    ret[DBM_Guild_User_Avatar.Columns.chat_level] = row[DBM_Guild_User_Avatar.Columns.chat_level];
                    ret[DBM_Guild_User_Avatar.Columns.chat_exp] = row[DBM_Guild_User_Avatar.Columns.chat_exp];
                    ret[DBM_Guild_User_Avatar.Columns.info] = row[DBM_Guild_User_Avatar.Columns.info];
                    ret[DBM_Guild_User_Avatar.Columns.image_url] = row[DBM_Guild_User_Avatar.Columns.image_url];
                    ret[DBM_Guild_User_Avatar.Columns.created_at] = row[DBM_Guild_User_Avatar.Columns.created_at];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ret;
        }

        public static void insertUserData(ulong guildId,ulong clientId)
        {
            try
            {
                DBC db = new DBC();

                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_User_Avatar.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_User_Avatar.Columns.id_user] = clientId.ToString();
                db.insert(DBM_Guild_User_Avatar.tableName, columns);
            }
            catch
            {

            }
        }

        public static async Task updateChatExp(DiscordSocketClient client,
            SocketTextChannel textChannel, ulong guildId, SocketUser user, int amount)
        {
            //return type: boolean: true if level up
            //update chat exp & level up
            var userId = user.Id;
            SocketGuildUser guildUser = (SocketGuildUser)user;
            var username = user.Username;
            var userAvatarUrl = user.GetAvatarUrl();

            int maxLevel = 200;
            //check if user data exists/not
            DBC db = new DBC();
            string query = $"SELECT * " +
            $" FROM {DBM_Guild_User_Avatar.tableName} " +
            $" WHERE {DBM_Guild_User_Avatar.Columns.id_guild}=@{DBM_Guild_User_Avatar.Columns.id_guild} AND " +
            $" {DBM_Guild_User_Avatar.Columns.id_user}=@{DBM_Guild_User_Avatar.Columns.id_user}";

            Dictionary<string, object> colSelect = new Dictionary<string, object>();
            colSelect[DBM_Guild_User_Avatar.Columns.id_guild] = guildId.ToString();
            colSelect[DBM_Guild_User_Avatar.Columns.id_user] = userId.ToString();
            DataTable dt = db.selectAll(query, colSelect);
            if (dt.Rows.Count <= 0)
                insertUserData(guildId, userId);

            var userData = getUserData(guildId, userId);
            int level = Convert.ToInt32(userData[DBM_Guild_User_Avatar.Columns.chat_level]);
            int exp = Convert.ToInt32(userData[DBM_Guild_User_Avatar.Columns.chat_exp]);
            int nextExp = (level+1) * 100;

            if (level <= maxLevel)
            {
                if (exp + amount >= nextExp)
                {
                    //Check if level up notification channel is existed
                    var guildData = Config.Guild.getGuildData(guildId);
                    if (guildData[DBM_Guild.Columns.id_channel_notification_chat_level_up].ToString() != "")
                    {
                        //check autorole if exists
                        query = $"SELECT * " +
                            $" FROM {DBM_Guild_Autorole_Level.tableName} " +
                            $" WHERE {DBM_Guild_Autorole_Level.Columns.id_guild}=@{DBM_Guild_Autorole_Level.Columns.id_guild} AND " +
                            $" {DBM_Guild_Autorole_Level.Columns.level_min}=@{DBM_Guild_Autorole_Level.Columns.level_min}";
                        Dictionary<string, object> column = new Dictionary<string, object>();
                        column[DBM_Guild_Autorole_Level.Columns.id_guild] = guildId.ToString();
                        column[DBM_Guild_Autorole_Level.Columns.level_min] = level+1;
                        var autoroleData = new DBC().selectAll(query,column);
                        ulong idAutorole = 0;
                        foreach(DataRow row in autoroleData.Rows)
                        {
                            idAutorole = Convert.ToUInt64(row[DBM_Guild_Autorole_Level.Columns.id_role].ToString());
                        }

                        EmbedBuilder eb = new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithTitle("Avatar Level Up!")
                        .WithDescription($"{MentionUtils.MentionUser(userId)} has leveled up into level {level + 1}!")
                        .WithThumbnailUrl(userAvatarUrl);

                        if (idAutorole != 0)
                        {
                            var roleMaster = textChannel.Guild.Roles.FirstOrDefault(x => x.Id == idAutorole);
                            if (roleMaster != null)
                            {
                                await guildUser.AddRoleAsync(roleMaster);
                                eb.AddField("Role unlocked:",roleMaster.Name,true);
                            }
                        }

                        eb.AddField("Next EXP:", $"{(level + 2)*100}", true);

                        //level up notification
                        ulong channelNotificationId = Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_notification_chat_level_up]);
                        var systemChannel = client.GetChannel(channelNotificationId) as SocketTextChannel; //Gets the channel to send the message in
                        await systemChannel.SendMessageAsync(embed: eb.Build());
                    }

                    //level up
                    query = $"UPDATE {DBM_Guild_User_Avatar.tableName} ";
                    query += $" SET {DBM_Guild_User_Avatar.Columns.chat_level}={DBM_Guild_User_Avatar.Columns.chat_level}+1, " +
                        $" {DBM_Guild_User_Avatar.Columns.chat_exp}=0 ";
                    query += $" WHERE {DBM_Guild_User_Avatar.Columns.id_guild}=@{DBM_Guild_User_Avatar.Columns.id_guild} AND " +
                        $" {DBM_Guild_User_Avatar.Columns.id_user}=@{DBM_Guild_User_Avatar.Columns.id_user}";

                }
                else
                { //add exp
                    query = $"UPDATE {DBM_Guild_User_Avatar.tableName} ";
                    query += $" SET {DBM_Guild_User_Avatar.Columns.chat_exp}={DBM_Guild_User_Avatar.Columns.chat_exp}+1 ";
                    query += $" WHERE {DBM_Guild_User_Avatar.Columns.id_guild}=@{DBM_Guild_User_Avatar.Columns.id_guild} AND " +
                        $"{DBM_Guild_User_Avatar.Columns.id_user}=@{DBM_Guild_User_Avatar.Columns.id_user}";
                }

                DBC dbUpdate = new DBC();
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_User_Avatar.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_User_Avatar.Columns.id_user] = userId.ToString();
                dbUpdate.update(query, columns);
            }

        }

        public static EmbedBuilder printAvatarStatus(SocketCommandContext context, SocketGuildUser otherUser = null)
        {
            ulong guildId = context.Guild.Id;
            ulong userId = context.User.Id;
            string userAvatar = context.User.GetAvatarUrl();
            string username = context.User.Username;
            
            if (otherUser != null)
            {
                userId = otherUser.Id;
                userAvatar = otherUser.GetAvatarUrl();
                username = otherUser.Username;
            }

            var userAvatarData = GuildUserAvatarCore.getUserData(guildId, userId);
            int userLevel = Convert.ToInt32(userAvatarData[DBM_Guild_User_Avatar.Columns.chat_level].ToString());
            int nextExp = (userLevel+1) * 100;
            int userExp = Convert.ToInt32(userAvatarData[DBM_Guild_User_Avatar.Columns.chat_exp].ToString());
            Discord.Color color = Config.Doremi.EmbedColor; //default color
            int customColorR = -1;  int customColorG = -1; int customColorB = -1; //custom color
            if (userAvatarData[DBM_Guild_User_Avatar.Columns.color].ToString() != "")
            {
                string[] splittedColor = userAvatarData[DBM_Guild_User_Avatar.Columns.color].ToString().Split(',');
                customColorR = Convert.ToInt32(splittedColor[0]); customColorG = Convert.ToInt32(splittedColor[1]);
                customColorB = Convert.ToInt32(splittedColor[2]);
            }

            string nickname = "";
            if (userAvatarData[DBM_Guild_User_Avatar.Columns.nickname].ToString() != "")
                nickname = $" ({userAvatarData[DBM_Guild_User_Avatar.Columns.nickname].ToString()})";
            
            EmbedBuilder eb = new EmbedBuilder()
            .WithTitle($"{username}{nickname}")
            .WithThumbnailUrl(userAvatar);
            if (customColorR != -1)
                eb.WithColor(new Discord.Color(customColorR, customColorG, customColorB));
            else
                eb.WithColor(color);
            
            //information
            if (userAvatarData[DBM_Guild_User_Avatar.Columns.info].ToString() != "")
                eb.Description = $"{userAvatarData[DBM_Guild_User_Avatar.Columns.info].ToString()}";
            
            eb.AddField("Level:", $"{userLevel}", true);
            eb.AddField("Exp:", $"{userExp}/{nextExp}", true);

            //search birthday if exists
            string query = $"SELECT * " +
                $" FROM {DBM_Guild_User_Birthday.tableName} " +
                $" WHERE {DBM_Guild_User_Birthday.Columns.id_guild}=@{DBM_Guild_User_Birthday.Columns.id_guild} AND " +
                $" {DBM_Guild_User_Birthday.Columns.id_user}=@{DBM_Guild_User_Birthday.Columns.id_user}";
            Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
            columnsFilter[DBM_Guild_User_Birthday.Columns.id_guild] = guildId.ToString();
            columnsFilter[DBM_Guild_User_Birthday.Columns.id_user] = userId.ToString();
            var result = new DBC().selectAll(query, columnsFilter);
            foreach (DataRow row in result.Rows)
            {
                if (row[DBM_Guild_User_Birthday.Columns.birthday_date].ToString()!="")
                {
                    var parsedBirthdayDate = DateTime.Parse(row[DBM_Guild_User_Birthday.Columns.birthday_date].ToString());
                    string birthdayDate = parsedBirthdayDate.ToString("dd MMMM");//default

                    if (Convert.ToBoolean(row[DBM_Guild_User_Birthday.Columns.show_year]))
                    {
                        birthdayDate = parsedBirthdayDate.ToString("dd-MMMM-yyyy");//default
                    }

                    eb.AddField("Birthday Date:",birthdayDate, true);
                }
            }

            //image_url for banner
            if (userAvatarData[DBM_Guild_User_Avatar.Columns.image_url].ToString() != "")
                eb.WithImageUrl(userAvatarData[DBM_Guild_User_Avatar.Columns.image_url].ToString());

            eb.WithFooter(@$"Created at:{DateTime.Parse(userAvatarData[DBM_Guild_User_Avatar.Columns.created_at].ToString())
                .ToString("MM/dd/yyyy")}");

            return eb;

        }

    }
}
