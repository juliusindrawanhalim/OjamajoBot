using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OjamajoBot
{
    public class MinigameCore
    {
        public static IDictionary<string,string> rockPaperScissor(int randomGuess, string guess)
        {
            IDictionary<string, string> arrResult = new Dictionary<string, string>();
            guess = guess.ToLower();

            if (randomGuess == 0) { //rock
                arrResult["randomResult"] = "rock";
                
                if (guess == "rock")
                    arrResult["gameState"] = "draw";
                else if (guess == "paper")
                    arrResult["gameState"] = "win";
                else
                    arrResult["gameState"] = "lose";
            } else if (randomGuess == 1) {  //paper
                arrResult["randomResult"] = "paper";

                if (guess == "paper")
                    arrResult["gameState"] = "draw";
                else if (guess == "scissor")
                    arrResult["gameState"] = "win";
                else
                    arrResult["gameState"] = "lose";

            } else { //scissor
                arrResult["randomResult"] = "scissor";

                if (guess == "scissor")
                    arrResult["gameState"] = "draw";
                else if (guess == "rock")
                    arrResult["gameState"] = "win";
                else
                    arrResult["gameState"] = "lose";
            }

                return arrResult;
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
