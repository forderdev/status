using System;
using System.Collections.Generic;
using CommandSystem;
using Utils;

namespace PlayerStatus.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class PlayerStatusCommand : ICommand, IUsageProvider
    {
        private const string UsageText = "Usage: playerstatus list | clear <players|all> | set <player> <text>";

        public string Command => "playerstatus";

        public string[] Aliases => new[] { "pstatus" };

        public string Description => "List, clear or set player statuses.";

        public string[] Usage => new[] { "list / clear / set", "%player%", "text" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StatusManager manager = Plugin.Instance?.Manager;

            if (manager == null)
            {
                response = "PlayerStatus is not enabled.";
                return false;
            }

            if (!sender.CheckPermission(PlayerPermissions.PlayersManagement, out response))
                return false;

            if (arguments.Count < 1)
            {
                response = UsageText;
                return false;
            }

            switch (arguments.At(0).ToLowerInvariant())
            {
                case "list":
                    response = manager.ListStatuses();
                    return true;

                case "clear":
                    return Clear(manager, arguments, sender, out response);

                case "set":
                    return Set(manager, arguments, sender, out response);

                default:
                    response = UsageText;
                    return false;
            }
        }

        private static bool Clear(StatusManager manager, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: playerstatus clear <players|all>";
                return false;
            }

            string target = arguments.At(1).ToLowerInvariant();

            if (target == "all" || target == "*")
            {
                response = $"Cleared {manager.ClearAll()} status(es).";
                return true;
            }

            List<ReferenceHub> hubs = RAUtils.ProcessPlayerIdOrNamesList(arguments, 1, out _);

            if (hubs == null || hubs.Count == 0)
            {
                response = "No matching players.";
                return false;
            }

            int cleared = 0;

            foreach (ReferenceHub hub in hubs)
            {
                if (manager.ClearStatus(hub, sender.LogName))
                    cleared++;
            }

            response = $"Cleared {cleared}/{hubs.Count} status(es).";
            return true;
        }

        private static bool Set(StatusManager manager, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 3)
            {
                response = "Usage: playerstatus set <player> <text>";
                return false;
            }

            List<ReferenceHub> hubs = RAUtils.ProcessPlayerIdOrNamesList(arguments, 1, out string[] rest);

            if (hubs == null || hubs.Count == 0)
            {
                response = "No matching players.";
                return false;
            }

            string text = TextSanitizer.Clean(rest == null ? string.Empty : string.Join(" ", rest), allowRichText: true);

            if (text.Length == 0)
            {
                response = "Status text is empty.";
                return false;
            }

            if (text.Length > 256)
                text = text.Substring(0, 256);

            int set = 0;

            foreach (ReferenceHub hub in hubs)
            {
                if (hub == null)
                    continue;

                manager.SetStatus(hub, text, sender.LogName);
                set++;
            }

            response = $"Status set on {set} player(s): {text}";
            return true;
        }
    }
}
