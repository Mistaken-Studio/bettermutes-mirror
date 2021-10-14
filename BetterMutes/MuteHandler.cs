// -----------------------------------------------------------------------
// <copyright file="MuteHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Mistaken.API;
using Newtonsoft.Json;

namespace Mistaken.BetterMutes
{
    internal static class MuteHandler
    {
        public static readonly List<MuteData> Mutes = new List<MuteData>();

        public static bool GetDuration(string input, out int duration)
        {
            if (input.EndsWith("mo"))
            {
                if (int.TryParse(input.Replace("mo", string.Empty), out duration))
                    duration *= 60 * 24 * 30;
                else
                    return false;
            }
            else if (input.EndsWith("y"))
            {
                if (int.TryParse(input.Replace("y", string.Empty), out duration))
                    duration *= 60 * 24 * 365;
                else
                    return false;
            }
            else if (input.EndsWith("w"))
            {
                if (int.TryParse(input.Replace("w", string.Empty), out duration))
                    duration *= 60 * 24 * 7;
                else
                    return false;
            }
            else if (input.EndsWith("d"))
            {
                if (int.TryParse(input.Replace("d", string.Empty), out duration))
                    duration *= 60 * 24;
                else
                    return false;
            }
            else if (input.EndsWith("h"))
            {
                if (int.TryParse(input.Replace("h", string.Empty), out duration))
                    duration *= 60;
                else
                    return false;
            }
            else if (input.EndsWith("m"))
            {
                if (int.TryParse(input.Replace("m", string.Empty), out duration))
                    duration *= 1;
                else
                    return false;
            }
            else
            {
                if (int.TryParse(input, out duration))
                    duration *= 1;
                else
                    return false;
            }

            return true;
        }

        public static bool Mute(Player player, bool intercomMute, string reason = "removeme", float duration = -1)
        {
            global::MuteHandler.IssuePersistentMute((intercomMute ? "ICOM-" : string.Empty) + player.UserId);
            var mute = GetMute(player.UserId);
            if (mute.HasValue)
            {
                if (mute.Value.Intercom && !intercomMute)
                {
                    RemoveMute(player.UserId, true);
                }
                else
                    return false;
            }

            if (intercomMute)
                player.IsIntercomMuted = true;
            else
                player.ReferenceHub.dissonanceUserSetup.AdministrativelyMuted = true;

            bool disconnect = reason.Contains("-dc");
            if (disconnect)
                reason = reason.Replace("-dc", string.Empty);
            File.AppendAllLines(
                Path,
#pragma warning disable SA1118 // Parameter should not span multiple lines
                new string[]
                {
                    JsonConvert.SerializeObject(
                        new MuteData
                        {
                            UserId = player.UserId,
                            Reason = reason,
                            EndTime = duration == -1 ? -1 : DateTime.UtcNow.AddMinutes(duration).Ticks,
                            Intercom = intercomMute,
                        }),
                });
#pragma warning restore SA1118 // Parameter should not span multiple lines
            if (!intercomMute && disconnect)
                player.Disconnect(PluginHandler.Instance.Translation.DisconnectMessage);
            return true;
        }

        public static MuteData? GetMute(string userId)
        {
            foreach (var line in File.ReadAllLines(Path))
            {
                var mute = JsonConvert.DeserializeObject<MuteData>(line);
                if (mute.UserId != userId)
                    continue;

                return mute;
            }

            return null;
        }

        public static bool RemoveMute(string userId, bool intercom)
        {
            bool success = false;
            var lines = File.ReadAllLines(Path);
            List<string> toWrite = NorthwoodLib.Pools.ListPool<string>.Shared.Rent(File.ReadAllLines(Path));
            foreach (var line in lines)
            {
                var mute = JsonConvert.DeserializeObject<MuteData>(line);
                if (mute.UserId != userId)
                    continue;
                if (mute.Intercom != intercom)
                    continue;
                success = true;
                toWrite.Remove(line);
                if (intercom)
                    global::MuteHandler.RevokePersistentMute($"ICOM-{mute.UserId}");
                else
                    global::MuteHandler.RevokePersistentMute(mute.UserId);

                if (RealPlayers.List.Any(p => p.UserId == mute.UserId))
                {
                    var player = RealPlayers.List.First(p => p.UserId == mute.UserId);
                    if (mute.Intercom)
                        player.IsIntercomMuted = false;
                    else
                        player.ReferenceHub.dissonanceUserSetup.AdministrativelyMuted = false;
                }

                break;
            }

            File.WriteAllLines(Path, toWrite.ToArray());
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(toWrite);
            return success;
        }

        public static void UpdateMutes()
        {
            foreach (var line in File.ReadAllLines(Path))
            {
                var mute = JsonConvert.DeserializeObject<MuteData>(line);
                if (mute.EndTime == -1)
                    continue;

                Log.Debug($"Mute end check, ends: {new DateTime(mute.EndTime)}, now: {DateTime.UtcNow}", PluginHandler.Instance.Config.VerbouseOutput);
                if ((new DateTime(mute.EndTime) - DateTime.UtcNow).TotalSeconds < 0)
                {
                    RemoveMute(mute.UserId, mute.Intercom);
                    Log.Info($"Ended mute for {mute.UserId}");
                }
            }
        }

        public struct MuteData
        {
            public string UserId;
            public string Reason;
            public long EndTime;
            public bool Intercom;
        }

        private static string Path => System.IO.Path.Combine(Paths.Configs, "BetterMutes.txt");
    }
}
