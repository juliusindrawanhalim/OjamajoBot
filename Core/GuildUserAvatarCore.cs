using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OjamajoBot.Database;
using OjamajoBot.Database.Model;
using System;
using System.Collections.Generic;
using System.Data;
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
                    ret[DBM_Guild_User_Avatar.Columns.chat_level] = row[DBM_Guild_User_Avatar.Columns.chat_level];
                    ret[DBM_Guild_User_Avatar.Columns.chat_exp] = row[DBM_Guild_User_Avatar.Columns.chat_exp];
                    ret[DBM_Guild_User_Avatar.Columns.info] = row[DBM_Guild_User_Avatar.Columns.info];
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
            int nextExp = level * 100;

            if (level <= maxLevel)
            {
                if (exp + amount >= nextExp)
                {
                    //Check if level up notification channel is existed
                    var guildData = Config.Guild.getGuildData(guildId);
                    if (guildData[DBM_Guild.Columns.id_channel_notification_chat_level_up].ToString() != "")
                    {
                        //level up notification
                        ulong channelNotificationId = Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_notification_chat_level_up]);
                        var systemChannel = client.GetChannel(channelNotificationId) as SocketTextChannel; // Gets the channel to send the message in
                        await systemChannel.SendMessageAsync(
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithTitle("Level Up!")
                        .WithDescription($"{MentionUtils.MentionUser(userId)} has leveled up into level {level + 1}!")
                        .WithThumbnailUrl(userAvatarUrl)
                        .Build());
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

        public static EmbedBuilder printAvatarStatus(SocketCommandContext context, Discord.Color color, 
            SocketGuildUser otherUser = null)
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
            int nextExp = userLevel * 100;
            int userExp = Convert.ToInt32(userAvatarData[DBM_Guild_User_Avatar.Columns.chat_exp].ToString());

            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"{username}")
                .WithThumbnailUrl(userAvatar);

            if (userAvatarData[DBM_Guild_User_Avatar.Columns.info].ToString() != "")
            {
                eb.Description = $"{userAvatarData[DBM_Guild_User_Avatar.Columns.info].ToString()}";
            }

            if (userAvatarData[DBM_Guild_User_Avatar.Columns.nickname].ToString() != "")
            {
                eb.AddField("Nickname:", userAvatarData[DBM_Guild_User_Avatar.Columns.nickname].ToString());
            }
            eb.AddField("Level:",$"{userLevel}",true);
            eb.AddField("Exp:",$"{userExp}/{nextExp}",true);
            eb.WithFooter(@$"Created at:{DateTime.Parse(userAvatarData[DBM_Guild_User_Avatar.Columns.created_at].ToString())
                .ToString("MM/dd/yyyy")}");

            return eb;

        }

    }
}
