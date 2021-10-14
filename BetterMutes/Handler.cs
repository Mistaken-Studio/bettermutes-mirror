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
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => this.Player_Verified(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => this.Server_RestartingRound(), "RoundRestart");
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => this.Player_Verified(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => this.Server_RestartingRound(), "RoundRestart");
        }

        private IEnumerator<float> AutoMuteReload()
        {
            while (true)
            {
                // Normaly I whould use MuteHandler.Reload() but it console logs "Loading saved mutes..." and I don't what it
                global::MuteHandler._path = GameCore.ConfigSharing.Paths[1] + "mutes.txt";
                try
                {
                    using (StreamReader streamReader = new StreamReader(global::MuteHandler._path))
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
                }
                catch
                {
                    global::ServerConsole.AddLog("Can't load the mute file!", ConsoleColor.Gray);
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
    }
}
