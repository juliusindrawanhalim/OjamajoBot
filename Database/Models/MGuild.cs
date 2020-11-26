using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot.Database.Models
{
    public class MGuild : Models
    {
        public MGuild() : base("guild") { }

        public ulong id_guild;
        public ulong id_channel_birthday_announcement;
        public ulong user_leaving_notification;
        public ulong role_id_doremi;
        public ulong role_id_hazuki;
        public ulong role_id_aiko;
        public ulong role_id_onpu;
        public ulong role_id_momoko;

        public string[] getDatabaseField()
        {
            return new string[] {
                "id_guild","id_channel_birthday_announcement","user_leaving_notification",
                "role_id_doremi","role_id_hazuki","role_id_aiko","role_id_onpu","role_id_momoko"
            };
        }
    }

    public class MGuildRoleList:Models
    {
        public MGuildRoleList() : base("guild_role_list") { }
        public ulong id_guild;
        public ulong role_list;

        public string[] getDatabaseField()
        {
            return new string[]
            {
                "id_guild","role_list"
            };
        }

    }

    public class MGuildRoleReact : Models
    {
        public MGuildRoleReact() : base("guild_role_react") { }
        public ulong id_guild;
        public ulong id_message;
        public ulong link;
        public ulong emoji;
        public ulong id_role;

        public string[] getDatabaseField()
        {
            return new string[]
            {
                "id_guild","id_message","link","emoji","id_role"
            };
        }
    }

}
