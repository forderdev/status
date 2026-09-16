using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.Spectating;
using PlayerRoles.Visibility;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace PlayerStatus
{
    public enum DisplayMode
    {
        Billboard,
        Front,
    }

    internal sealed class StatusManager : CustomEventsHandler, IDisposable
    {
        private const byte ClientSmoothing = 60;
        private const float YawThreshold = 2f;

        private sealed class StatusEntry
        {
            public ReferenceHub Owner;
            public TextToy Toy;
            public string Text;
            public readonly Dictionary<int, ViewerState> Viewers = new Dictionary<int, ViewerState>();
        }

        private struct ViewerState
        {
            public bool Visible;
            public float LocalYaw;
        }

        private readonly Dictionary<uint, StatusEntry> _entries = new Dictionary<uint, StatusEntry>();
        private readonly Dictionary<uint, float> _nextChange = new Dictionary<uint, float>();
        private readonly List<uint> _staleOwners = new List<uint>();
        private readonly List<int> _staleViewers = new List<int>();
        private readonly HashSet<int> _seenViewers = new HashSet<int>();

        private bool _enabled;
        private float _tickAccumulator;
        private bool _layoutChecked;
        private bool _layoutValid;
        private int _lineOfSightMask;

        private static Config Settings => Plugin.Instance.Config;

        private static DisplayMode ConfiguredMode =>
            Enum.TryParse(Settings.Mode, true, out DisplayMode mode) ? mode : DisplayMode.Billboard;

        private DisplayMode EffectiveMode =>
            ConfiguredMode == DisplayMode.Billboard && _layoutChecked && !_layoutValid
                ? DisplayMode.Front
                : ConfiguredMode;

        public void Enable()
        {
            if (_enabled)
                return;

            if (!Enum.TryParse(Settings.Mode, true, out DisplayMode _))
                Logger.Warn($"[PlayerStatus] Unknown mode '{Settings.Mode}', using Billboard.");

            _lineOfSightMask = LayerMask.GetMask("Default", "Door");
            StaticUnityMethods.OnUpdate += OnUpdate;
            _enabled = true;
        }

        public void Dispose()
        {
            if (_enabled)
            {
                StaticUnityMethods.OnUpdate -= OnUpdate;
                _enabled = false;
            }

            ClearAll();
        }

        public bool HandlePlayerCommand(ReferenceHub hub, ArraySegment<string> arguments, out string response)
        {
            Config config = Settings;
            MessagesConfig messages = config.Messages;
            uint id = hub.netId;

            if (arguments.Count == 0)
            {
                response = _entries.TryGetValue(id, out StatusEntry current)
                    ? messages.Current.Replace("{status}", current.Text) + "\n" + messages.Usage
                    : messages.Usage;
                return true;
            }

            if (arguments.Count == 1 && IsClearKeyword(arguments.At(0)))
            {
                if (!ClearStatus(hub, hub.nicknameSync.MyNick))
                {
                    response = messages.NothingToClear;
                    return false;
                }

                response = messages.Cleared;
                return true;
            }

            if (!(hub.roleManager.CurrentRole is IFpcRole))
            {
                response = messages.NotAlive;
                return false;
            }

            float now = Time.unscaledTime;

            if (_nextChange.TryGetValue(id, out float next) && now < next)
            {
                response = messages.Cooldown.Replace("{seconds}", Mathf.CeilToInt(next - now).ToString(CultureInfo.InvariantCulture));
                return false;
            }

            string text = TextSanitizer.Clean(string.Join(" ", arguments), config.AllowRichText);

            if (text.Length == 0)
            {
                response = messages.Empty;
                return false;
            }

            if (text.Length > config.MaxLength)
            {
                response = messages.TooLong.Replace("{max}", config.MaxLength.ToString(CultureInfo.InvariantCulture));
                return false;
            }

            SetStatus(hub, text, hub.nicknameSync.MyNick);
            _nextChange[id] = now + Mathf.Max(0f, config.Cooldown);

            response = messages.Set.Replace("{status}", text);
            return true;
        }

        private static bool IsClearKeyword(string argument)
        {
            CultureInfo turkish = CultureInfo.GetCultureInfo("tr-TR");

            foreach (string keyword in Settings.ClearKeywords)
            {
                if (string.IsNullOrEmpty(keyword))
                    continue;

                if (string.Equals(keyword, argument, StringComparison.OrdinalIgnoreCase) ||
                    string.Compare(keyword, argument, turkish, CompareOptions.IgnoreCase) == 0)
                    return true;
            }

            return false;
        }

        public string ListStatuses()
        {
            if (_entries.Count == 0)
                return "Nobody has a status.";

            StringBuilder builder = new StringBuilder();
            builder.Append(_entries.Count).Append(" status(es):");

            foreach (StatusEntry entry in _entries.Values)
            {
                if (entry.Owner == null)
                    continue;

                builder.Append("\n- ").Append(Describe(entry.Owner)).Append(": ").Append(entry.Text);
            }

            return builder.ToString();
        }

        public void SetStatus(ReferenceHub hub, string text, string actor)
        {
            if (!_entries.TryGetValue(hub.netId, out StatusEntry entry) || entry.Toy.IsDestroyed)
            {
                if (entry != null)
                    Remove(hub.netId);

                entry = new StatusEntry { Owner = hub, Toy = CreateToy(hub) };
                _entries[hub.netId] = entry;
            }

            entry.Text = text;
            entry.Toy.TextFormat = TextLayout.Compose(text, Settings);

            if (Settings.LogChanges)
                Logger.Info($"[PlayerStatus] {Describe(hub)} status set by {actor}: {text}");
        }

        public bool ClearStatus(ReferenceHub hub, string actor)
        {
            if (hub == null || !_entries.ContainsKey(hub.netId))
                return false;

            Remove(hub.netId);

            if (Settings.LogChanges)
                Logger.Info($"[PlayerStatus] {Describe(hub)} status removed by {actor}.");

            return true;
        }

        public int ClearAll()
        {
            _staleOwners.Clear();
            _staleOwners.AddRange(_entries.Keys);

            foreach (uint id in _staleOwners)
                Remove(id);

            int count = _staleOwners.Count;
            _staleOwners.Clear();
            _nextChange.Clear();
            return count;
        }

        private TextToy CreateToy(ReferenceHub hub)
        {
            EnsureLayoutChecked();

            bool front = EffectiveMode == DisplayMode.Front;

            TextToy toy = TextToy.Create(
                LocalAnchor(hub),
                front ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity,
                front ? Vector3.one * Settings.Scale : Vector3.zero,
                hub.transform,
                networkSpawn: false);

            toy.IsStatic = false;
            toy.Base.NetworkMovementSmoothing = ClientSmoothing;
            toy.DisplaySize = new Vector2(Settings.DisplayWidth, Settings.DisplayHeight);
            toy.Spawn();
            return toy;
        }

        private void Remove(uint ownerId)
        {
            if (!_entries.TryGetValue(ownerId, out StatusEntry entry))
                return;

            _entries.Remove(ownerId);

            if (entry.Toy != null && !entry.Toy.IsDestroyed)
                entry.Toy.Destroy();
        }

        private void EnsureLayoutChecked()
        {
            if (_layoutChecked)
                return;

            string problem = TextToyWire.Validate();
            _layoutValid = problem == null;
            _layoutChecked = true;

            if (!_layoutValid && ConfiguredMode == DisplayMode.Billboard)
                Logger.Warn($"[PlayerStatus] TextToy format changed in this game version ({problem}). Using Front mode.");
        }

        private void OnUpdate()
        {
            if (!NetworkServer.active || _entries.Count == 0)
                return;

            _tickAccumulator += Time.unscaledDeltaTime;

            if (_tickAccumulator < 1f / Mathf.Clamp(Settings.UpdateRate, 1, 60))
                return;

            _tickAccumulator = 0f;

            try
            {
                Tick();
            }
            catch (Exception exception)
            {
                Logger.Error($"[PlayerStatus] Update failed: {exception}");
            }
        }

        private void Tick()
        {
            DisplayMode mode = EffectiveMode;
            _staleOwners.Clear();

            foreach (KeyValuePair<uint, StatusEntry> pair in _entries)
            {
                StatusEntry entry = pair.Value;

                if (entry.Owner == null || entry.Toy == null || entry.Toy.IsDestroyed)
                {
                    _staleOwners.Add(pair.Key);
                    continue;
                }

                bool ownerShown = entry.Owner.roleManager.CurrentRole is IFpcRole;

                if (ownerShown)
                {
                    Vector3 anchor = LocalAnchor(entry.Owner);

                    if ((entry.Toy.Position - anchor).sqrMagnitude > 0.0004f)
                        entry.Toy.Position = anchor;
                }

                if (mode == DisplayMode.Billboard)
                {
                    UpdateBillboard(entry, ownerShown);
                }
                else
                {
                    Vector3 scale = ownerShown ? Vector3.one * Settings.Scale : Vector3.zero;

                    if (entry.Toy.Scale != scale)
                        entry.Toy.Scale = scale;
                }
            }

            foreach (uint id in _staleOwners)
                Remove(id);

            _staleOwners.Clear();
        }

        private void UpdateBillboard(StatusEntry entry, bool ownerShown)
        {
            ReferenceHub owner = entry.Owner;
            NetworkIdentity identity = entry.Toy.Base.netIdentity;
            Vector3 textPosition = entry.Toy.Transform.position;
            Vector3 ownerHead = owner.PlayerCameraReference.position;
            Quaternion toLocal = Quaternion.Inverse(owner.transform.rotation);
            Vector3 visibleScale = Vector3.one * Settings.Scale;

            _seenViewers.Clear();

            foreach (Player player in Player.ReadyList)
            {
                if (player.IsDummy)
                    continue;

                NetworkConnectionToClient connection = player.ConnectionToClient;

                if (connection == null || !identity.observers.ContainsKey(connection.connectionId))
                    continue;

                int viewerId = connection.connectionId;
                _seenViewers.Add(viewerId);

                bool known = entry.Viewers.TryGetValue(viewerId, out ViewerState last);
                Vector3 eye = default;
                bool visible = ownerShown && IsVisibleTo(owner, ownerHead, textPosition, player.ReferenceHub, out eye);
                float localYaw = last.LocalYaw;

                if (visible)
                {
                    Vector3 flat = textPosition - eye;
                    flat.y = 0f;

                    if (flat.sqrMagnitude > 0.0001f)
                        localYaw = (toLocal * Quaternion.LookRotation(flat)).eulerAngles.y;
                }
                else if (!known)
                {
                    entry.Viewers[viewerId] = new ViewerState { Visible = false, LocalYaw = localYaw };
                    continue;
                }

                bool send = !known || last.Visible != visible ||
                            (visible && Mathf.Abs(Mathf.DeltaAngle(last.LocalYaw, localYaw)) > YawThreshold);

                if (!send)
                    continue;

                TextToyWire.SendPose(connection, entry.Toy.Base, Quaternion.Euler(0f, localYaw, 0f),
                    visible ? visibleScale : Vector3.zero);

                entry.Viewers[viewerId] = new ViewerState { Visible = visible, LocalYaw = localYaw };
            }

            _staleViewers.Clear();

            foreach (int viewerId in entry.Viewers.Keys)
            {
                if (!_seenViewers.Contains(viewerId))
                    _staleViewers.Add(viewerId);
            }

            foreach (int viewerId in _staleViewers)
                entry.Viewers.Remove(viewerId);
        }

        private bool IsVisibleTo(ReferenceHub owner, Vector3 ownerHead, Vector3 textPosition, ReferenceHub viewer, out Vector3 eye)
        {
            Config config = Settings;

            if (!TryGetViewPoint(viewer, config, out eye, out ReferenceHub pointOfView))
                return false;

            if (pointOfView == owner)
                return config.ShowToSelf;

            float maxDistance = Mathf.Max(0f, config.MaxViewDistance);

            if ((textPosition - eye).sqrMagnitude > maxDistance * maxDistance)
                return false;

            if (config.RespectInvisibility &&
                pointOfView.roleManager.CurrentRole is ICustomVisibilityRole visibilityRole &&
                !visibilityRole.VisibilityController.ValidateVisibility(owner))
                return false;

            return !config.RequireLineOfSight ||
                   !Physics.Linecast(eye, ownerHead, _lineOfSightMask, QueryTriggerInteraction.Ignore);
        }

        private static bool TryGetViewPoint(ReferenceHub viewer, Config config, out Vector3 eye, out ReferenceHub pointOfView)
        {
            PlayerRoleBase role = viewer.roleManager.CurrentRole;

            if (role is IFpcRole)
            {
                eye = viewer.PlayerCameraReference.position;
                pointOfView = viewer;
                return true;
            }

            if (role is SpectatorRole spectator && config.ShowToSpectators &&
                spectator.SyncedSpectatedNetId != 0 &&
                ReferenceHub.TryGetHubNetID(spectator.SyncedSpectatedNetId, out ReferenceHub target) &&
                target.roleManager.CurrentRole is IFpcRole)
            {
                eye = target.PlayerCameraReference.position;
                pointOfView = target;
                return true;
            }

            if (role is Scp079Role scp079 && scp079.CurrentCamera != null)
            {
                eye = scp079.CurrentCamera.CameraPosition;
                pointOfView = viewer;
                return true;
            }

            eye = default;
            pointOfView = null;
            return false;
        }

        private static Vector3 LocalAnchor(ReferenceHub hub)
        {
            Vector3 world = hub.PlayerCameraReference.position + Vector3.up * Settings.HeightAboveHead;
            return hub.transform.InverseTransformPoint(world);
        }

        private static string Describe(ReferenceHub hub) =>
            hub == null ? "(unknown)" : $"{hub.nicknameSync.MyNick} ({hub.PlayerId})";

        public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
        {
            ReferenceHub hub = ev.Player?.ReferenceHub;

            if (Settings.ClearOnRoleChange && hub != null)
                Remove(hub.netId);
        }

        public override void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            ReferenceHub hub = ev.Player?.ReferenceHub;

            if (hub == null)
                return;

            Remove(hub.netId);
            _nextChange.Remove(hub.netId);
        }

        public override void OnServerWaitingForPlayers()
        {
            ClearAll();
            EnsureLayoutChecked();
        }

        public override void OnServerRoundRestarted() => ClearAll();
    }
}
