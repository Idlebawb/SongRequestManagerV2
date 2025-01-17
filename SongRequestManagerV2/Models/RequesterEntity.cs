﻿

using CatCore.Models.Shared;
using SongRequestManagerV2.SimpleJSON;

namespace SongRequestManagerV2.Models
{
    public class RequesterEntity : IChatUser
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string DisplayName { get; set; }

        public string Color { get; set; }

        public bool IsBroadcaster { get; set; }

        public bool IsModerator { get; set; }

        public IChatBadge[] Badges { get; set; }

        private JSONObject ToJson()
        {
            return null;
        }

        public RequesterEntity()
        {
            this.Id = "unknow";
            this.UserName = "unknow";
            this.DisplayName = "unknown";
            this.Color = "";
            this.Badges = new IChatBadge[0];
        }
    }
}
