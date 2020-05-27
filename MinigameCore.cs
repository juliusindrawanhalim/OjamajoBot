using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OjamajoBot
{
    public class MinigameCore
    {
        public class rockPaperScissor{

            public static Tuple<string,EmbedBuilder> rpsResults(Color color, string embedIcon, int randomGuess, string guess, string parent, string username,
                string[] arrWinReaction, string[] arrLoseReaction, string[] arrDrawReaction, 
                ulong guildId, ulong userId)
            {
                //IDictionary<string, string> arrResult = new Dictionary<string, string>();
                string randomResult; string gameState;
                string picReactionFolderDir = $"config/rps_reaction/{parent}/";
                string embedTitle; string textTemplate = "";

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

                return Tuple.Create($"{randomPathFile}", eb);

                //return arrResult;
            }
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
