// -----------------------------------------------------------------------
// <copyright file="IUnMuteCommandHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using CommandSystem;
using Mistaken.API.Commands;

namespace Mistaken.BetterMutes
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class IUnMuteCommandHandler : IBetterCommand, IPermissionLocked, IUsageProvider
    {
        public override string Command => "iunmute2";

        public override string[] Aliases => new string[] { "iunm2", "iunmute" };

        public override string Description => "UnMute for IMute2";

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
            if (MuteHandler.RemoveMute(uId, true))
                return new string[] { $"Intercom UnMuted {uId}" };
            success = false;
            return new string[] { "User was not intercom muted" };
        }

        public string GetUsage() =>
            "iunmute2 [playerId/userId]";
    }
}
