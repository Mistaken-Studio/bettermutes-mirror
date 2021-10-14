// -----------------------------------------------------------------------
// <copyright file="MuteCommandHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using CommandSystem;
using Mistaken.API;
using Mistaken.API.Commands;

namespace Mistaken.BetterMutes
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class MuteCommandHandler : IBetterCommand, IPermissionLocked, IUsageProvider
    {
        public override string Command => "mute2";

        public override string[] Aliases => new string[] { "m2", "mute" };

        public override string Description => "Mute option that allows to mute with reason and for limited time";

        public string Permission => "mute";

        public string PluginName => PluginHandler.Instance.Name;

        public string[] Usage => new string[]
        {
            "%player%",
            "duration (-1 = infinity)",
            "reason",
        };

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length < 3)
                return new string[] { this.GetUsage() };
            var target = this.GetPlayers(args[0]).FirstOrDefault();
            if (target == null)
                return new string[] { "Player not found", this.GetUsage() };
            if (!MuteHandler.GetDuration(args[1], out int duration))
                return new string[] { "Wrong duration, Too bad" };
            string reason = string.Join(" ", args.Skip(2));
            success = true;
            if (MuteHandler.Mute(target, false, reason, duration))
                return new string[] { $"Muted ({target.Id}) {target.Nickname} for {duration} minutes with reason \"{reason}\"" };
            success = false;
            return new string[] { "User is already muted" };
        }

        public string GetUsage() =>
            "mute2 [playerId] [duration (-1 = perm)] [reason]";
    }
}
