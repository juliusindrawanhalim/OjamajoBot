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
using System.Linq;
using System.Text;

namespace OjamajoBot
{
    public class MinigameCore
    {
        public enum GAME_STATE
        {
            LOSE,
            WIN,
            DRAW
        }

        public class rockPaperScissor{

            public static Tuple<string,EmbedBuilder,Boolean> rpsResults(Color color, string embedIcon, int randomGuess, string guess, string parent, string username,
                string[] arrWinReaction, string[] arrLoseReaction, string[] arrDrawReaction, 
                ulong guildId, ulong userId)
            {
                //IDictionary<string, string> arrResult = new Dictionary<string, string>();
                string randomResult; 
                /*string gameState;*/
                string picReactionFolderDir = $"config/rps_reaction/{parent}/";
                string embedTitle; string textTemplate = "";
                Boolean isWin = false;
                GAME_STATE gameState = GAME_STATE.LOSE;

                guess = guess.ToLower();

                switch (randomGuess)
                {
                    case 0: //rock
                        randomResult = ":rock: rock";
                        switch (guess)
                        {
                            case "rock":
                                gameState = GAME_STATE.DRAW;
                                break;
                            case "paper":
                                gameState = GAME_STATE.WIN;
                                break;
                            default:
                                gameState = GAME_STATE.LOSE;
                                break;
                        }
                        break;

                    case 1: //paper
                        randomResult = "📜 paper";
                        switch (guess)
                        {
                            case "paper":
                                gameState = GAME_STATE.DRAW;
                                break;
                            case "scissor":
                                gameState = GAME_STATE.WIN;
                                break;
                            default:
                                gameState = GAME_STATE.LOSE;
                                break;
                        }
                        break;

                    default: //scissor
                        randomResult = "✂️ scissor";
                        switch (guess)
                        {
                            case "scissor":
                                gameState = GAME_STATE.DRAW;
                                break;
                            case "rock":
                                gameState = GAME_STATE.WIN;
                                break;
                            default:
                                gameState = GAME_STATE.LOSE;
                                break;
                        }
                        break;
                }

                switch (gameState)
                {
                    case GAME_STATE.WIN:
                        int rndIndex = new Random().Next(0, arrLoseReaction.Length);

                        picReactionFolderDir += "lose";
                        embedTitle = $"{username} has win the game!";
                        textTemplate = $"\"{arrLoseReaction[rndIndex]}\" You got 10 minigame score points & 1 magic seeds!";

                        //save the data
                        updateScore(guildId, userId, 10);

                        //update player garden data
                        UserDataCore.updateMagicSeeds(userId, 1);

                        isWin = true;
                        break;
                    case GAME_STATE.DRAW:
                        rndIndex = new Random().Next(0, arrDrawReaction.Length);
                        embedTitle = "❌ The game is draw!";
                        picReactionFolderDir += "draw";
                        textTemplate = $"\"{arrDrawReaction[rndIndex]}\"";
                        break;
                    default:
                        rndIndex = new Random().Next(0, arrWinReaction.Length);
                        picReactionFolderDir += "win";
                        embedTitle = $"❌ {username} has lose the game!";
                        textTemplate = $"\"{arrWinReaction[rndIndex]}\"";
                        break;
                }

                switch (guess)
                {
                    case "scissor":
                        guess = guess.Replace("scissor", "✂️ scissor");
                        break;
                    case "paper":
                        guess = guess.Replace("paper", "📜 paper");
                        break;
                    case "rock":
                        guess = guess.Replace("rock", ":rock: rock");
                        break;
                }
                    

                string randomPathFile = GlobalFunctions.getRandomFile(picReactionFolderDir, new string[] { ".png", ".jpg", ".gif", ".webm" });
                EmbedBuilder eb = new EmbedBuilder
                {
                    Color = color,
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Rock Paper Scissor!",
                        IconUrl = embedIcon
                    },
                    Title = embedTitle,
                    Description = textTemplate,
                    ThumbnailUrl = $"attachment://{Path.GetFileName(randomPathFile)}"
                };
                eb.AddField(GlobalFunctions.UppercaseFirst(username) + " use:", guess, true);
                eb.AddField(GlobalFunctions.UppercaseFirst(parent) + " use:", randomResult, true);

                return Tuple.Create($"{randomPathFile}", eb, isWin);
            }
        }

        public static EmbedBuilder printScore(SocketCommandContext Context, Color color)
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            int score = 0; //set default

            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = color;

            DBC db = new DBC();
            string query = $"SELECT * " +
            $" FROM {DBM_User_Data_Minigame.tableName} " +
            $" WHERE {DBM_User_Data_Minigame.Columns.id_guild}=@{DBM_User_Data_Minigame.Columns.id_guild} AND " +
            $" {DBM_User_Data_Minigame.Columns.id_user}=@{DBM_User_Data_Minigame.Columns.id_user}";

            Dictionary<string, object> colFilter = new Dictionary<string, object>();
            colFilter[DBM_User_Data_Minigame.Columns.id_guild] = guildId.ToString();
            colFilter[DBM_User_Data_Minigame.Columns.id_user] = userId.ToString();

            DataTable dt = db.selectAll(query, colFilter);

            if (dt.Rows.Count >= 1)
                foreach (DataRow row in dt.Rows)
                    score = Convert.ToInt32(row[DBM_User_Data_Minigame.Columns.score]);
            
            builder.Description = $"Your minigame score points are: {score}";
            
            return builder;

        }

        public static void insertUserMinigameData(ulong guildId, ulong userId)
        {
            DBC db = new DBC();

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Data_Minigame.Columns.id_guild] = guildId.ToString();
            columns[DBM_User_Data_Minigame.Columns.id_user] = userId.ToString();
            db.insert(DBM_User_Data_Minigame.tableName, columns);
        }

        public static EmbedBuilder printLeaderboard(SocketCommandContext Context, Color color)
        {
            var guildId = Context.Guild.Id;

            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "\uD83C\uDFC6 Minigame Leaderboard";
            builder.Color = color;

            DBC db = new DBC();
            string query = $"SELECT * " +
            $" FROM {DBM_User_Data_Minigame.tableName} " +
            $" WHERE {DBM_User_Data_Minigame.Columns.id_guild}=@{DBM_User_Data_Minigame.Columns.id_guild} " +
            $" ORDER BY {DBM_User_Data_Minigame.Columns.score} desc " +
            $" LIMIT 10 ";

            Dictionary<string, object> colSelect = new Dictionary<string, object>();
            colSelect[DBM_User_Data_Minigame.Columns.id_guild] = guildId.ToString();

            DataTable dt = db.selectAll(query, colSelect);

            if (dt.Rows.Count >= 1)
            {
                string finalText = "";
                int ctrRank = 1;
                builder.Description = "Here are the top 10 player with the highest score points:";
                
                foreach (DataRow row in dt.Rows)
                {
                    finalText += $"{ctrRank}. " +
                        $"{MentionUtils.MentionUser(Convert.ToUInt64(row[DBM_User_Data_Minigame.Columns.id_user]))} : " +
                        $"{row[DBM_User_Data_Minigame.Columns.score]} \n";
                    ctrRank++;
                }

                builder.AddField("[Rank]. Name & Score", finalText);
            }
            else
            {
                builder.Description = "Currently there are no leaderboard yet.";
            }
            return builder;
        }

        public static void updateScore(ulong guildId, ulong userId, int scoreValue)
        {
            //create if user minigame data is not exists
            DBC db = new DBC();
            string query = $"SELECT * " +
            $" FROM {DBM_User_Data_Minigame.tableName} " +
            $" WHERE {DBM_User_Data_Minigame.Columns.id_guild}=@{DBM_User_Data_Minigame.Columns.id_guild} AND " +
            $" {DBM_User_Data_Minigame.Columns.id_user}=@{DBM_User_Data_Minigame.Columns.id_user}";

            Dictionary<string, object> colFilter = new Dictionary<string, object>();
            colFilter[DBM_User_Data_Minigame.Columns.id_guild] = guildId.ToString();
            colFilter[DBM_User_Data_Minigame.Columns.id_user] = userId.ToString();

            DataTable dt = db.selectAll(query, colFilter);

            if (dt.Rows.Count<=0)
                insertUserMinigameData(guildId, userId);

            //START UPDATE
            query = $"UPDATE {DBM_User_Data_Minigame.tableName}" +
                $" SET {DBM_User_Data_Minigame.Columns.score}={DBM_User_Data_Minigame.Columns.score}+{scoreValue} " +
                $" WHERE {DBM_User_Data_Minigame.Columns.id_guild}=@{DBM_User_Data_Minigame.Columns.id_guild} AND " +
                $" {DBM_User_Data_Minigame.Columns.id_user}=@{DBM_User_Data_Minigame.Columns.id_user}";

            DBC dbUpdate = new DBC();
            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Data_Minigame.Columns.id_guild] = guildId.ToString();
            columns[DBM_User_Data_Minigame.Columns.id_user] = userId.ToString();

            dbUpdate.update(query, columns);
        }

    }
}
