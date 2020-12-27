using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_User_Trading_Card_Data
    {
        public static readonly string tableName = "user_trading_card_data";


        public static class Columns
        {
            public static readonly string id_user = "id_user";
            public static readonly string rank = "rank";
            public static readonly string exp = "exp";
            public static readonly string catch_attempt = "catch_attempt";
            public static readonly string catch_token = "catch_token";
            public static readonly string card_zone = "card_zone";
            public static readonly string fragment_point = "fragment_point";
            public static readonly string last_second_chance_time = "last_second_chance_time";

            public static readonly string boost_doremi_normal = "boost_doremi_normal";
            public static readonly string boost_doremi_platinum = "boost_doremi_platinum";
            public static readonly string boost_doremi_metal = "boost_doremi_metal";
            public static readonly string boost_doremi_ojamajos = "boost_doremi_ojamajos";

            public static readonly string boost_hazuki_normal = "boost_hazuki_normal";
            public static readonly string boost_hazuki_platinum = "boost_hazuki_platinum";
            public static readonly string boost_hazuki_metal = "boost_hazuki_metal";
            public static readonly string boost_hazuki_ojamajos = "boost_hazuki_ojamajos";

            public static readonly string boost_aiko_normal = "boost_aiko_normal";
            public static readonly string boost_aiko_platinum = "boost_aiko_platinum";
            public static readonly string boost_aiko_metal = "boost_aiko_metal";
            public static readonly string boost_aiko_ojamajos = "boost_aiko_ojamajos";

            public static readonly string boost_onpu_normal = "boost_onpu_normal";
            public static readonly string boost_onpu_platinum = "boost_onpu_platinum";
            public static readonly string boost_onpu_metal = "boost_onpu_metal";
            public static readonly string boost_onpu_ojamajos = "boost_onpu_ojamajos";

            public static readonly string boost_momoko_normal = "boost_momoko_normal";
            public static readonly string boost_momoko_platinum = "boost_momoko_platinum";
            public static readonly string boost_momoko_metal = "boost_momoko_metal";
            public static readonly string boost_momoko_ojamajos = "boost_momoko_ojamajos";

            public static readonly string boost_other_special = "boost_other_special";
            
            public static readonly string created_at = "created_at";
        }

    }
}
