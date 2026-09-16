using System;
using System.Globalization;
using System.Threading;
using CommandSystem;
using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Plugins;
using RemoteAdmin;
using Logger = LabApi.Features.Console.Logger;

namespace PlayerStatus
{
    public sealed class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name => "PlayerStatus";

        public override string Description => "Shows a roleplay status above the player's head (.status <text>).";

        public override string Author => "forderdev";

        public override Version Version => new Version(1, 0, 0);

        public override Version RequiredApiVersion => new Version(1, 1, 0);

        internal StatusManager Manager { get; private set; }

        public override void LoadConfigs()
        {
            CultureInfo previousCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                base.LoadConfigs();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previousCulture;
            }
        }

        public override void Enable()
        {
            Instance = this;

            Manager = new StatusManager();
            CustomHandlersManager.RegisterEventsHandler(Manager);
            Manager.Enable();

            EnsureRegistered<Commands.StatusCommand>(QueryProcessor.DotCommandHandler, "status");
            EnsureRegistered<Commands.PlayerStatusCommand>(CommandProcessor.RemoteAdminCommandHandler, "playerstatus");
            EnsureRegistered<Commands.PlayerStatusCommand>(GameCore.Console.ConsoleCommandHandler, "playerstatus");
        }

        public override void Disable()
        {
            if (Manager != null)
            {
                Manager.Dispose();
                CustomHandlersManager.UnregisterEventsHandler(Manager);
                Manager = null;
            }

            Instance = null;
        }

        private static void EnsureRegistered<T>(CommandHandler handler, string name) where T : ICommand, new()
        {
            if (handler == null)
                return;

            if (handler.TryGetCommand(name, out ICommand existing))
            {
                if (!(existing is T))
                    Logger.Warn($"[PlayerStatus] Command '{name}' is already taken by {existing.GetType().FullName}.");

                return;
            }

            try
            {
                handler.RegisterCommand(new T());
            }
            catch (Exception exception)
            {
                Logger.Error($"[PlayerStatus] Could not register '{name}': {exception.Message}");
            }
        }
    }
}
