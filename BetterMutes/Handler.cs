// -----------------------------------------------------------------------
// <copyright file="Handler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using MEC;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;

namespace Mistaken.BetterMutes
{
    internal class Handler : Module
    {
        public Handler(PluginHandler plugin)
            : base(plugin)
        {
            this.RunCoroutine(this.AutoMuteReload(), "AutoMuteReload");
        }

        public override bool IsBasic => true;

        public override string Name => "BetterMutes";

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Player_Verified;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.PreAuthenticating += this.Player_PreAuthenticating;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Player_Verified;
            Exiled.Events.Handlers.Server.RestartingRound -= this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.PreAuthenticating -= this.Player_PreAuthenticating;
        }

        private IEnumerator<float> AutoMuteReload()
        {
            while (true)
            {
                // Normaly I whould use MuteHandler.Reload() but it generate console logs "Loading saved mutes..." and I don't what it
                global::MuteHandler._path = GameCore.ConfigSharing.Paths[1] + "mutes.txt";
                string path = global::MuteHandler._path;
                try
                {
                    using (StreamReader streamReader = new StreamReader(path))
                    {
                        string text;
                        while ((text = streamReader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                global::MuteHandler.Mutes.Add(text.Trim());
                            }
                        }
                    }

                    this.Log.Debug("Loaded mute file", PluginHandler.Instance.Config.VerbouseOutput);
                }
                catch
                {
                    this.Log.Warn($"Can't load the mute file from path: {path}");
                }

                yield return Timing.WaitForSeconds(60);
            }
        }

        private void Server_RestartingRound()
        {
            MuteHandler.UpdateMutes();
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            var mute = MuteHandler.GetMute(ev.Player.UserId);
            if (!mute.HasValue)
                return;
            var endString = mute.Value.EndTime == -1 ? PluginHandler.Instance.Translation.Never : new DateTime(mute.Value.EndTime).ToString("dd.MM.yyyy HH:mm:ss");
            var reasonString = mute.Value.Reason;
            var type = mute.Value.Intercom ? PluginHandler.Instance.Translation.Intercom : PluginHandler.Instance.Translation.Server;

            string message = string.Format(PluginHandler.Instance.Translation.MutedMessage, type, endString, reasonString);

            ev.Player.Broadcast("MUTE", 5, message, Broadcast.BroadcastFlags.AdminChat);
            ev.Player.SendConsoleMessage(message, "red");
        }

        private void Player_PreAuthenticating(Exiled.Events.EventArgs.PreAuthenticatingEventArgs ev)
        {
            if (!PluginHandler.Instance.Config.KickMuted)
                return;

            var mute = MuteHandler.GetMute(ev.UserId);

            if (!mute.HasValue)
                return;

            if (mute.Value.Intercom)
                return;

            var writer = new LiteNetLib.Utils.NetDataWriter();
            writer.Put((byte)10);
            string reason = mute.Value.Reason == "removeme" ? "No reason provided" : mute.Value.Reason;
            if (mute.Value.EndTime == -1)

                // writer.Put($"You are muted so you can't play on RolePlay servers.. You are muted for \"{reason}\", mute has no end date, ask Admin to unmute you.");
                writer.Put($"You are muted and this server is not allowing muted players to join.. You are muted for \"{reason}\", mute has no end date, ask Admin to unmute you.");
            else

                // writer.Put($"You are muted so you can't play on RolePlay servers.. You are muted for \"{reason}\" until {new DateTime(mute.Value.EndTime):dd.MM.yyyy HH:mm:ss} UTC");
                writer.Put($"You are muted and this server is not allowing muted players to join.. You are muted for \"{reason}\" until {new DateTime(mute.Value.EndTime):dd.MM.yyyy HH:mm:ss} UTC");
            ev.Request.Reject(writer);
            ev.Disallow();
        }
    }
}
