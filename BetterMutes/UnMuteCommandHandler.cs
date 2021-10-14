// -----------------------------------------------------------------------
// <copyright file="UnMuteCommandHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using CommandSystem;
using Mistaken.API.Commands;

namespace Mistaken.BetterMutes
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class UnMuteCommandHandler : IBetterCommand, IPermissionLocked, IUsageProvider
    {
        public override string Command => "unmute2";

        public override string[] Aliases => new string[] { "unm2", "unmute" };

        public override string Description => "UnMute for Mute2";

        public string Permission => "mute";

        public string PluginName => PluginHandler.Instance.Name;

        public string[] Usage => new string[]
        {
            "%player%",
        };

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return new string[] { this.GetUsage() };
            var target = this.GetPlayers(args[0]).FirstOrDefault();
            if (target == null && args[0].Split('@')[0].Length != 17)
                return new string[] { "Player not found", this.GetUsage() };
            var uId = target?.UserId ?? args[0];
            if (!uId.Contains("@"))
                uId += "@steam";
            success = true;
            if (MuteHandler.RemoveMute(uId, false))
                return new string[] { $"UnMuted {uId}" };
            success = false;
            return new string[] { "User was not muted" };
        }

        public string GetUsage() =>
            "unmute2 [playerId/userId]";
    }
}
