using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OjamajoBot
{
    public class MinigameCore
    {
        public class rockPaperScissor{

            public static Tuple<string,EmbedBuilder,Boolean> rpsResults(Color color, string embedIcon, int randomGuess, string guess, string parent, string username,
                string[] arrWinReaction, string[] arrLoseReaction, string[] arrDrawReaction, 
                ulong guildId, ulong userId)
            {
                //IDictionary<string, string> arrResult = new Dictionary<string, string>();
                string randomResult; string gameState;
                string picReactionFolderDir = $"config/rps_reaction/{parent}/";
                string embedTitle; string textTemplate = "";
                Boolean isWin = false;

                guess = guess.ToLower();

                if (randomGuess == 0)
                { //rock
                    randomResult = "🥌 rock";

                    if (guess == "rock")
                        gameState = "draw";
                    else if (guess == "paper")
                        gameState = "win";
                    else
                        gameState = "lose";
                }
                else if (randomGuess == 1)
                {  //paper
                    randomResult = "📜 paper";

                    if (guess == "paper")
                        gameState = "draw";
                    else if (guess == "scissor")
                        gameState = "win";
                    else
                        gameState = "lose";
                }
                else
                { //scissor
                    randomResult = "✂️ scissor";

                    if (guess == "scissor")
                        gameState = "draw";
                    else if (guess == "rock")
                        gameState = "win";
                    else
                        gameState = "lose";
                }

                if (gameState == "win")
                { // player win
                    int rndIndex = new Random().Next(0, arrLoseReaction.Length);

                    picReactionFolderDir += "lose";
                    embedTitle = $"👏 {username} win the game!";
                    textTemplate = $"\"{arrLoseReaction[rndIndex]}\" You got 10 minigame score points!";

                    //save the data
                    MinigameCore.updateScore(guildId.ToString(), userId.ToString(), 10);
                    isWin = true;

                }
                else if (gameState == "draw")
                { // player draw
                    int rndIndex = new Random().Next(0, arrDrawReaction.Length);
                    embedTitle = "❌ The game is draw!";
                    picReactionFolderDir += "draw";
                    textTemplate = $"\"{arrDrawReaction[rndIndex]}\"";
                }
                else
                { //player lose
                    int rndIndex = new Random().Next(0, arrWinReaction.Length);
                    picReactionFolderDir += "win";
                    embedTitle = $"❌ {username} lose the game!";
                    textTemplate = $"\"{arrWinReaction[rndIndex]}\"";
                }

                if (guess == "scissor")
                    guess = guess.Replace("scissor", "✂️ scissor");
                else if (guess == "paper")
                    guess = guess.Replace("paper", "📜 paper");
                else if (guess == "rock")
                    guess = guess.Replace("rock", "🥌 rock");

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
                eb.AddField(GlobalFunctions.UppercaseFirst(username) + " used:", guess, true);
                eb.AddField(GlobalFunctions.UppercaseFirst(parent) + " used:", randomResult, true);

                return Tuple.Create($"{randomPathFile}", eb, isWin);

                //return arrResult;
            }
        }

        public static EmbedBuilder printLeaderboard(SocketCommandContext Context, Color color, string guildId, string userId)
        {
            var quizJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.minigameDataFileName}")).GetValue("score");
            string finalText = "";
            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "\uD83C\uDFC6 Minigame Leaderboard";

            builder.Color = color;

            if (quizJsonFile.Count >= 1)
            {
                builder.Description = "Here are the top 10 player score points for minigame leaderboard:";

                var convertedToList = quizJsonFile.Properties().OrderByDescending(p => (int)p.Value).ToList();
                int ctrExists = 0;
                for (int i = 0; i < quizJsonFile.Count; i++)
                {
                    SocketGuildUser userExists = Context.Guild.GetUser(Convert.ToUInt64(convertedToList[i].Name));
                    if (userExists != null)
                    {
                        finalText += $"{i + 1}. {MentionUtils.MentionUser(Convert.ToUInt64(convertedToList[i].Name))} : {convertedToList[i].Value} \n";
                        ctrExists++;
                    }
                    if (ctrExists >= 9) break;
                }
                builder.AddField("[Rank]. Name & Score", finalText);
            }
            else
            {
                builder.Description = "Currently there's no minigame leaderboard yet.";
            }
            return builder;
        }

        public static void updateScore(string guildId, string userId, int scoreValue)
        {
            //save the data
            var quizJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.minigameDataFileName}"));
            var jobjscore = (JObject)quizJsonFile.GetValue("score");

            if (!jobjscore.ContainsKey(userId.ToString()))
            {
                jobjscore.Add(new JProperty(userId.ToString(), scoreValue.ToString()));
            }
            else
            {
                int tempScore = Convert.ToInt32(jobjscore[userId.ToString()]) + scoreValue;
                jobjscore[userId.ToString()] = tempScore.ToString();
            }

            File.WriteAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.minigameDataFileName}", quizJsonFile.ToString());

        }

    }
}
