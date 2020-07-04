using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot
{
    public static class GardenCore
    {
        public static string imgMagicSeeds = "https://cdn.discordapp.com/attachments/706770454697738300/716673416479899729/magic_seeds.jpg";
        public static string imgRoyalSeeds = "https://cdn.discordapp.com/attachments/706770454697738300/726137300052082728/royal_seeds.gif";
        public static string[] weather = { $"☀️", "sunny", "It's a sunny day!","4","5" };//current weather/initialize it
        public static string[,] arrRandomWeather = {
            {$"☀️", "sunny","A perfect time to water the plant~","4","5"},
            {$"☁️", "cloudy","There might be a chance to rain soon...","2","4"},
            {$"🌧️", "raining","Not sure if it's a good time to water the plant.","1","3"},
            {$"⛈️", "thunder storm","I don't think it's the best time to water the plant now...","1","2"}
        };
    }
}
