﻿using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using ChatCore.Utilities;
using SongRequestManagerV2.Interfaces;
using SongRequestManagerV2.Statics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongRequestManagerV2.Utils
{
    public static class Utility
    {
        public static void EmptyDirectory(string directory, bool delete = true)
        {
            if (Directory.Exists(directory)) {
                var directoryInfo = new DirectoryInfo(directory);
                foreach (FileInfo file in directoryInfo.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);

                if (delete) Directory.Delete(directory);
            }
        }

        public static TimeSpan GetFileAgeDifference(string filename)
        {
            DateTime lastModified = File.GetLastWriteTime(filename);
            return DateTime.Now - lastModified;
        }

        public static bool HasRights(ISRMCommand botcmd, IChatUser user, CmdFlags flags)
        {
            if (flags.HasFlag(CmdFlags.Local)) return true;
            if (botcmd.Flags.HasFlag(CmdFlags.Disabled)) return false;
            if (botcmd.Flags.HasFlag(CmdFlags.Everyone)) return true; // Not sure if this is the best approach actually, not worth thinking about right now
            if (user.IsModerator & RequestBotConfig.Instance.ModFullRights) return true;
            if (user.IsBroadcaster & botcmd.Flags.HasFlag(CmdFlags.Broadcaster)) return true;
            if (user.IsModerator & botcmd.Flags.HasFlag(CmdFlags.Mod)) return true;
            if (user is TwitchUser twitchUser && twitchUser.IsSubscriber & botcmd.Flags.HasFlag(CmdFlags.Sub)) return true;
            if (user is TwitchUser twitchUser1 && twitchUser1.IsVip & botcmd.Flags.HasFlag(CmdFlags.VIP)) return true;
            return false;
        }

        public static string GetStarRating(JSONObject song, bool mode = true)
        {
            if (!mode) return "";

            string stars = "******";
            float rating = song["rating"].AsFloat;
            if (rating < 0 || rating > 100) rating = 0;
            string starrating = stars.Substring(0, (int)(rating / 17)); // 17 is used to produce a 5 star rating from 80ish to 100.
            return starrating;
        }

        public static string GetRating(JSONObject song, bool mode = true)
        {
            if (!mode) return "";

            string rating = song["rating"].AsInt.ToString();
            if (rating == "0") return "";
            return rating + '%';
        }
    }
}
