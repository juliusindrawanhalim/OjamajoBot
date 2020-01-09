using OjamajoBot.Bot;
using System;

namespace OjamajoBot
{
    class Program
    {

        public static void Main(string[] args)
        {
            new Config.Core(); //init core    
            new Doremi().RunBotAsync().GetAwaiter().GetResult();
            new Hazuki().RunBotAsync().GetAwaiter().GetResult();
            new Aiko().RunBotAsync().GetAwaiter().GetResult();
        }

    }
}
