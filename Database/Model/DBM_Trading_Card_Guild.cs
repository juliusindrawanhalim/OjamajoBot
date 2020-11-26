using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Trading_Card_Guild
    {
        public static readonly string tableName = "trading_card_guild";

        public static class Columns
        {
            public static readonly string id_guild = "id_guild";
            public static readonly string id_channel_spawn = "id_channel_spawn";
            public static readonly string spawn_interval = "spawn_interval";
            public static readonly string spawn_id = "spawn_id";
            public static readonly string spawn_parent = "spawn_parent";
            public static readonly string spawn_category = "spawn_category";
            public static readonly string spawn_token = "spawn_token";
            public static readonly string spawn_is_mystery = "spawn_is_mystery";
            public static readonly string spawn_is_badcard = "spawn_is_badcard";
            public static readonly string spawn_is_zone = "spawn_is_zone";
            public static readonly string spawn_badcard_question = "spawn_badcard_question";
            public static readonly string spawn_badcard_answer = "spawn_badcard_answer";
            public static readonly string spawn_badcard_type = "spawn_badcard_type";
            public static readonly string spawn_time = "spawn_time";
        }

    }
}
