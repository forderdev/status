using System;
using CommandSystem;
using RemoteAdmin;

namespace PlayerStatus.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class StatusCommand : ICommand
    {
        public string Command => "status";

        public string[] Aliases => new[] { "durum" };

        public string Description => "Karakterinin üstünde görünen durumu ayarlar. Kullanım: .status <metin>  /  .status sil";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Plugin.Instance?.Manager == null)
            {
                response = "PlayerStatus is not enabled.";
                return false;
            }

            if (!(sender is PlayerCommandSender playerSender) || playerSender.ReferenceHub == null)
            {
                response = "Bu komut oyun içi konsoldan kullanılır: .status <metin>";
                return false;
            }

            return Plugin.Instance.Manager.HandlePlayerCommand(playerSender.ReferenceHub, arguments, out response);
        }
    }
}
