using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Model
{
    public class DBM_Guild
    {
        public static readonly string tableName = "guild";

        public static class Columns
        {
            public static readonly string id_guild = "id_guild";
            public static readonly string id_channel_birthday_announcement = "id_channel_birthday_announcement";
            public static readonly string id_channel_notification_chat_level_up = "id_channel_notification_chat_level_up";
            public static readonly string id_channel_notification_user_welcome = "id_channel_notification_user_welcome";
            public static readonly string id_channel_user_leaving_log = "id_channel_user_leaving_log";
            public static readonly string birthday_announcement_date_last = "birthday_announcement_date_last";
            public static readonly string user_leaving_notification = "user_leaving_notification";
            public static readonly string welcome_message = "welcome_message";
            public static readonly string welcome_image = "welcome_image";
            public static readonly string role_id_doremi = "role_id_doremi";
            public static readonly string role_id_hazuki = "role_id_hazuki";
            public static readonly string role_id_aiko = "role_id_aiko";
            public static readonly string role_id_onpu = "role_id_onpu";
            public static readonly string role_id_momoko = "role_id_momoko";
            public static readonly string role_detention = "role_detention";
        }

    }
}
