using OjamajoBot.Bot;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            new Onpu().RunBotAsync().GetAwaiter().GetResult();
            new Momoko().RunBotAsync().GetAwaiter().GetResult();
        }
    }
}
