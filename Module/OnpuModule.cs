using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OjamajoBot.Module
{
    class OnpuModule : ModuleBase<SocketCommandContext>
    {
        [Command("Help")]
        public async Task showHelp()
        {
            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Onpu.EmbedColor)
                .WithAuthor(Config.Onpu.EmbedName, Config.Onpu.EmbedAvatarUrl)
                .WithTitle("Command List:")
                .WithDescription("Pretty Witchy Onpu Chi~ " +
                $"You can tell me what to do with {MentionUtils.MentionUser(Config.Onpu.Id)} or **onpu!** or **on!** as the starting command prefix.")
                .AddField("Basic Commands",
                "**change** or **henshin** : Change into the ojamajo form\n" +
                "**fairy** : I will show you my fairy\n" +
                "**hello** : Hello, I will greet you up\n" +
                "**random** or **moments** : Show any random Onpu moments\n" +
                "**sign** : I will give you my autograph signatures\n" +
                "**sing** : I will sing a random song~\n" +
                "**stats** or **bio** : I will show you my biography info\n" +
                "**turn** or **transform <username> <wishes>** : Turn <username> into <wishes>\n" +
                "**wish <wishes>** : I will grant you a <wishes>")
                .Build());
        }

        [Command("change"), Alias("henshin")]
        public async Task transform()
        {
            await ReplyAsync("Pretty Witchy Onpu Chi~\n");
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Onpu.EmbedColor)
                .WithImageUrl("http://pa1.narvii.com/6537/e69c6922e788f8ce3c15b0ddf10401e79df73a92_00.gif")
                .Build());
        }

        [Command("fairy")]
        public async Task showFairy()
        {
            await ReplyAsync("This is my elegant fairy, Roro.",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Onpu.EmbedName, Config.Onpu.EmbedAvatarUrl)
            .WithDescription("Roro has fair skin with pink blushed cheeks and Onpu's eyes. Her light purple hair frames her face and she has a thick strand sticking up on the left to resemble Onpu's side-tail. She wears a light purple dress with a lilac collar.\n" +
            "In teen form, the only change to her hair is that her side - tail now sticks down, rather than curling up.She gains a developed body and now wears a lilac dress with the shoulder cut out and a white - collar, where a purple gem rests.A lilac top is worn under this, and she gains white booties and a white witch hat with a lilac rim.")
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/84/No.079.jpg/revision/latest?cb=20190429130114")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Roro)")
            .Build());
        }

        [Command("hello")]
        public async Task onpuHello()
        {
            string tempReply = "";
            List<string> listRandomRespond = new List<string>() {
                    $"Hello {MentionUtils.MentionUser(Context.User.Id)}. ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            tempReply = listRandomRespond[rndIndex] + Config.Onpu.arrRandomActivity[Config.Onpu.indexCurrentActivity, 1];

            await ReplyAsync(tempReply);
        }

        [Command("sing")]
        public async Task sing()
        {
            string[,] arrRandom = {
                {"Half Point" , "Tanoshii yume wo miteru ichiban ii toko de\n"+
                "Itsumo me ga samechau noha doushite nandarou\n"+
                "Okiniiri no fuku kite kagami no mae ni tatsu\n"+
                "Totteoki no SMILE ni WINK ga dekinai\n\n"+
                "Atosukoshi nano mōsukoshi\n"+
                "Mitsukaranakute wakan'nai\n"+
                "Itsuka min'na omotta tōrini\n"+
                "Dekiruyou ni naru no kana\n\n"+
                "Ashita ga watashi wo matte iru\n"+
                "Kitto chigau watashi ga iru\n"+
                "Atarashii nanika to deau tabi ni yume mo kawaru\n\n"+
                "Kimagure wagamama koneko no\n"+
                "You na nana iro no hitomi ha\n"+
                "Dokoka ni aru suteki na monotachi wo oikaketeru\n\n"+
                "Tenki yohou wo mireba yakusoku no hi ha ame\n"+
                "Sore demo akiramenai yo zettai shiroi kutsu\n\n"+
                "Atosukoshi nano mōsukoshi\n"+
                "Tarinai mono ga wakan'nai\n"+
                "Tsukurikake no JIGSAW PUZZLE no\n"+
                "Mannaka de mayou mitai\n\n"+
                "Mirai ha watashi ga tsukuru no\n"+
                "Ima ha mada tochuu dakeredo\n"+
                "Honto ha dai suki na kimochi chanto ieruyou ni\n\n"+
                "Kokoro ha itsumo yure nagara\n"+
                "Soshite nani ga daiji nanoka\n"+
                "Shiranai furi wo shiteru kedo demo ne mitsumete iru\n\n"+
                "Ashita ga watashi wo matte iru\n"+
                "Kitto chigau watashi ga iru\n"+
                "Atarashii nanika to deau tabi ni yume mo kawaru\n\n"+
                "Kokoro ha itsumo yure nagara\n"+
                "Soshite nani ga daiji nanoka\n"+
                "Shiranai furi wo shiteru kedo demo ne oikaketeru"},
                {"Lupinasu no Komoriuta", "Chitchana te no hira wa\n"+
                "NEMOPHILA no hana\n"+
                "Pukkuri hoppeta wa\n"+
                "ERICA no tsubomi\n"+
                "Atatakai haru no\n"+
                "Soyokaze mitai na matsuge\n\n"+
                "Saa oyasumi no jikan da yo\n"+
                "Suteki na LADY ni naru\n"+
                "Yume wo mite hoshii yo\n\n"+
                "Suyasuya ude no naka\n"+
                "Kawaii negao\n"+
                "Mamoritai zutto\n"+
                "LUPINUS no hana no you ni sotto."},
                {"WE CAN DO","We can do anything if we do it together\n"+
                "Kitto deaeru yo\n"+
                "Can do !nanika ga kawari hajimeru\n"+
                "Atarashii watashi ni\n\n"+
                "Kakikake no daiarii\n"+
                "Tame iki wo tsuiteru\n"+
                "Kokoro no kotoba mitsukara nakute\n\n"+
                "Daisuki na ribon demo\n"+
                "Chiguhagu shiteru kibun ne\n"+
                "Itsumo mitai ni waraenai\n\n"+
                "Motto gyutto\n"+
                "Te no hira tsunai danara\n"+
                "Ameagari no niji ni aeru yo ne\n\n"+
                "We can do anything if we do it together\n"+
                "Minna issho nara\n"+
                "Can do !Namida mo yuuki ni kawaru\n"+
                "Egao ni naru yo\n"+
                "We can do anything if we do it together\n"+
                "Kitto deaeru yo\n"+
                "Can do !nanika ga kawari hajimeru\n"+
                "Atarashii watashi ni\n\n"+
                "Nemurenai yoru ni wa\n"+
                "Te no hira wo sotto ne\n"+
                "Haato no ue ni kasanetemiru no\n\n"+
                "Tanoshikute shikatanai\n"+
                "Omoidetachi ga hirogaru\n"+
                "Kurayami nanka kowakunai\n\n"+
                "Motto zutto\n"+
                "Yasashisa tsunaida nara\n"+
                "Mitakotonai sora ga matterune\n\n"+
                "We can do anything if we do it together\n"+
                "Minna issho nara\n"+
                "Can do !nandemo dekiru ki ga suru\n"+
                "Kiseki ni naru yo\n"+
                "We can do anything if we do it together\n"+
                "Kitto sagaseru yo\n"+
                "Can do !kokoro ga hashagi hajimeru\n\n"+
                "We can do anything if we do it together\n"+
                "Minna issho nara\n"+
                "Can do !Namida mo yuuki ni kawaru\n"+
                "Egao ni naru yo\n"+
                "We can do anything if we do it together\n"+
                "Kitto deaeru yo\n"+
                "Can do !nanika ga kawari hajimeru\n"+
                "Atarashii watashi ni"}
            };
        }

        [Command("sign"), Summary("I will give you my autograph signatures")]
        public async Task sign()
        {
            string[] arrRandom = {
                $"The idol {MentionUtils.MentionUser(Config.Onpu.Id)} has give you a big smiles and her autograph**",
                "Here is my autograph sign. I hope you're happy with it~",
                "Oh, you want my autograph signatures? Here you go \uD83D\uDE09 ~",
                "\uD83D\uDE09",
                $"**You have been given the autograph signatures by the idol {MentionUtils.MentionUser(Config.Onpu.Id)} herself**"
            };

            string[] arrRandomImg =
            {
                "https://i.4pcdn.org/s4s/1529207416921.jpg",
                "https://i.4pcdn.org/s4s/1512681025683.jpg",
                "https://i.4pcdn.org/s4s/1517845652473.png",
                "https://cdn.myanimelist.net/images/characters/15/130799.jpg",
                "https://i.4pcdn.org/s4s/1510961801004.jpg",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRd6Ujy4Q-7TTGcRrGc-X7Pg1v3SmfrqQryB5iu6_gVKNyuNI06&s",
                "https://i.4pcdn.org/s4s/1553625652304.jpg",
                "https://i.4pcdn.org/s4s/1502212001406.jpg",
            };

            await ReplyAsync(arrRandom[new Random().Next(0, arrRandom.Length)],
            embed: new EmbedBuilder()
            .WithAuthor(Config.Onpu.EmbedName, Config.Onpu.EmbedAvatarUrl)
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/84/No.079.jpg/revision/latest?cb=20190429130114")
            .WithFooter($"Signed by: Onpu Segawa [{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}]")
            .Build());

        }

        [Command("random"), Summary("Show any random Onpu moments")]
        public async Task randomMoments()
        {
            string[] arrRandom =
            {"https://i.4pcdn.org/s4s/1497673945986.png","https://i.4pcdn.org/s4s/1525384969179.png","https://i.4pcdn.org/s4s/1512504487962.jpg",
            "https://i.4pcdn.org/s4s/1533339681414.gif","https://i.4pcdn.org/s4s/1570615443509.jpg","https://i.4pcdn.org/s4s/1518545486532.jpg",
            "https://i.4pcdn.org/s4s/1514591399443.jpg","https://i.4pcdn.org/s4s/1509945691680.png","https://i.4pcdn.org/s4s/1529560439463.png",
            "https://i.4pcdn.org/s4s/1502212001406.jpg","https://i.4pcdn.org/s4s/1529025004211.png","https://i.4pcdn.org/s4s/1520879617340.png",
            "https://i.4pcdn.org/s4s/1519707043077.png","https://i.4pcdn.org/s4s/1513806749960.jpg","https://i.4pcdn.org/s4s/1516083449768.jpg",
            "https://i.4pcdn.org/s4s/1508469660333.jpg","https://i.4pcdn.org/s4s/1507062419193.jpg","https://i.4pcdn.org/s4s/1510380798821.png",
            "https://pbs.twimg.com/media/EOXplGsWkAAyg0s?format=png&name=small","https://pbs.twimg.com/media/EOWsaAxW4AETPv1?format=png&name=small",
            "https://pbs.twimg.com/media/EOMuKHnWoAE2aFw?format=png&name=small","https://pbs.twimg.com/media/EOI6pfmX0AA4BPN?format=png&name=small",
            "https://pbs.twimg.com/media/EOEfdIJX4AAHORN?format=png&name=small","https://pbs.twimg.com/media/EOAvJH5XUAAyso4?format=png&name=small",
            "https://pbs.twimg.com/media/EN_hqAQX0AA1SJf?format=png&name=small","https://pbs.twimg.com/media/EN-aX9OX4AErhax?format=png&name=small",
            "https://pbs.twimg.com/media/EN-G2YgX0AAk499?format=png&name=small","https://pbs.twimg.com/media/EN9klu-WsAA9h23?format=png&name=small",
            "https://pbs.twimg.com/media/EN8R0pHWsAAUAhh?format=png&name=small","https://pbs.twimg.com/media/EN7PUhkXUAA5wh_?format=png&name=small",
            "https://pbs.twimg.com/media/EN5a94DXkAA1-a7?format=png&name=small","https://pbs.twimg.com/media/EN2vN9VX0AAqz8S?format=png&name=small",
            "https://pbs.twimg.com/media/ENy61PTW4AEVHJL?format=png&name=small","https://pbs.twimg.com/media/ENywQseXkAAZORC?format=png&name=small",
            "https://pbs.twimg.com/media/ENyCSNTWoAAupEh?format=png&name=small","https://pbs.twimg.com/media/ENwUIldWwAAIDle?format=png&name=small",
            "https://pbs.twimg.com/media/ENt_lzDX0AAbJQY?format=png&name=small","https://pbs.twimg.com/media/ENpRsDsWsAABTxZ?format=png&name=small",
            "https://pbs.twimg.com/media/ENkPW8HXYAE4Jpa?format=png&name=small","https://pbs.twimg.com/media/ENgoScqX0AAotuK?format=png&name=small",
            "https://pbs.twimg.com/media/ENeLg5CWoAAIpCZ?format=png&name=small","https://pbs.twimg.com/media/ENbwEzpWwAEhzaa?format=png&name=small",
            "https://pbs.twimg.com/media/ENacmC-W4AAxFOm?format=png&name=small","https://pbs.twimg.com/media/ENZWO0HW4AEBrRe?format=png&name=small",
            "https://pbs.twimg.com/media/ENZL4vHXYAMDnS9?format=png&name=small","https://pbs.twimg.com/media/ENV9mlQXYAAo0Y9?format=png&name=small",
            "https://pbs.twimg.com/media/ENVTRWNXkAEV60U?format=png&name=small","https://pbs.twimg.com/media/ENTOJASWoAEGzz8?format=png&name=small",
            "https://pbs.twimg.com/media/ENLogA5XkAAO_Go?format=png&name=small","https://pbs.twimg.com/media/ENKdy17WoAAkANp?format=png&name=small",
            "https://pbs.twimg.com/media/ENJtDOfWsAUx4VR?format=png&name=small","https://pbs.twimg.com/media/ENGBY_bXkAAHehk?format=png&name=small",
            "https://pbs.twimg.com/media/ENFI1CjW4AE3QJK?format=png&name=small","https://pbs.twimg.com/media/EM8JwLkWoAErgE5?format=png&name=small",
            "https://pbs.twimg.com/media/EM7SXeDXYAUSt-U?format=png&name=small","https://pbs.twimg.com/media/EM6aQAqWwAAq1kG?format=png&name=small",
            "https://pbs.twimg.com/media/EM6QJOYX0AI9JO6?format=png&name=small","https://pbs.twimg.com/media/EM59S_nWoAEUFCr?format=png&name=small",
            "https://pbs.twimg.com/media/EM50Uv0WkAAtCqd?format=png&name=small",
            };

            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Onpu.EmbedColor)
                .WithImageUrl(arrRandom[new Random().Next(0, arrRandom.Length)])
                .Build());
        }
        //

        [Command("stats"), Alias("bio"), Summary("I will show you my biography info")]
        public async Task showStats()
        {
            await ReplyAsync($"Pururun purun famifami faa! Give {MentionUtils.MentionUser(Context.User.Id)} my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Onpu.EmbedName, Config.Onpu.EmbedAvatarUrl)
            .WithDescription("Onpu Segawa (瀬川おんぷ, Segawa Onpu) is one of the Main Characters and the fifth Ojamajo, initially starting off as an antagonistic Apprentice beneath Majoruka. She began attending Misora Elementary School and quickly befriended Doremi, Hazuki, and Aiko with the intention of revealing her true goals to them.\n" +
            "At the start of Sharp, Onpu officially joined the group as a tritagonist after revealing that she became a real friend of theirs after losing their Apprentice status.She joined them under Majorika when they were given the job of watching Hana.")
            .AddField("Full Name", "瀬川 おんぷ Segawa Onpu", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "B", true)
            .AddField("Birthday", "March 3rd, 1991", true)
            .AddField("Instrument", "Flute", true)
            .AddField("Favorite Food", "Waffles, Crepes, Fat-free Candies", true)
            .AddField("Debut", "[The Transfer student is a Witch Apprentice?!](https://ojamajowitchling.fandom.com/wiki/The_Transfer_student_is_a_Witch_Apprentice%3F!)", true)
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRSfjMF-ijylKYP4f7-Lvdf9Vx_HDrmCWc1DGkoSVWu-CPrHfJl")
            .WithFooter("Source: [Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Onpu_Segawa)")
            .Build());
        }

        [Command("thank you"), Alias("thank you,", "thanks", "arigatou")]
        public async Task thankYou([Remainder] string query = "")
        {
            await ReplyAsync($"Your welcome, {MentionUtils.MentionUser(Context.User.Id)}. I'm glad that you're happy with it :smile:");
        }

        [Command("turn"), Alias("transform"), Summary("Transform <username> into <wishes>")]
        public async Task spells(IUser username, [Remainder] string wishes)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync($"Pururun purun famifami faa! Turn {username.Mention} into {wishes}",
            embed: new EmbedBuilder()
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl("https://i.ytimg.com/vi/iOkN602s-JQ/hqdefault.jpg")
            .Build());
        }

        //todo: onpu segawa reaction as signatures

    }

    class OnpuMagicalStageModule : ModuleBase<SocketCommandContext>
    {

    }

    class OnpuRandomEventModule : ModuleBase<SocketCommandContext>
    {
        
    }

}
