// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Interfaces;

namespace Mistaken.BetterMutes
{
    /// <inheritdoc/>
    internal class Translation : ITranslation
    {
        public string DisconnectMessage { get; set; } = "You have been muted by server administrator.";

        public string Never { get; set; } = "NEVER";

        public string Server { get; set; } = "Server";

        public string Intercom { get; set; } = "Intercom";

        public string MutedMessage { get; set; } = "You are {0} muted until {1} UTC\nReason: {2}";
    }
}
