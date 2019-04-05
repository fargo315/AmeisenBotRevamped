using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.CombatEngine.MovementProvider;
using AmeisenBotRevamped.AI.CombatEngine.SpellStrategies;
using AmeisenBotRevamped.AI.StateMachine;
using AmeisenBotRevamped.Autologin;
using AmeisenBotRevamped.CharacterManager;
using AmeisenBotRevamped.CharacterManager.ItemComparator;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.EventAdapters;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using Magic;
using System.Collections.Generic;
using System.Diagnostics;

namespace AmeisenBotRevamped
{
    public class AmeisenBot
    {
        public string CharacterName => WowDataAdapter.BasicInfoDataSet.CharacterName;
        public string RealmName => WowDataAdapter.BasicInfoDataSet.RealmName;

        public IWowDataAdapter WowDataAdapter { get; private set; }
        public IWowActionExecutor WowActionExecutor { get; private set; }
        public IPathfindingClient WowPathfindingClient { get; private set; }
        public IWowEventAdapter WowEventAdapter { get; private set; }
        public IAutologinProvider AutologinProvider { get; private set; }

        public WowObjectManager ObjectManager => WowDataAdapter.ObjectManager;
        public WowCharacterManager CharacterManager { get; private set; }

        public AmeisenBotStateMachine StateMachine { get; private set; }

        public BlackMagic BlackMagic { get; private set; }
        public Process Process { get; private set; }
        public bool Attached { get; private set; }

        public AmeisenBot(BlackMagic blackMagic, IWowDataAdapter wowDataAdapter, IAutologinProvider autologinProvider, Process process, IItemComparator itemComparator = null)
        {
            Attached = false;
            AutologinProvider = autologinProvider;
            Process = process;

            WowDataAdapter = wowDataAdapter;
            WowDataAdapter.OnGamestateChanged = COnGamestateChanged;
            BlackMagic = blackMagic;

            if (itemComparator == null)
                itemComparator = new BasicItemLevelComparator(wowDataAdapter);

            CharacterManager = new WowCharacterManager(WowDataAdapter, WowActionExecutor, itemComparator);

            AmeisenBotLogger.Instance.Log($"[{process?.Id.ToString("X")}]\tAmeisenBot initialised [{wowDataAdapter?.AccountName}, {CharacterName}, {RealmName}, {wowDataAdapter?.WowBuild}]");
        }

        public void Attach(IWowActionExecutor wowActionExecutor, IPathfindingClient wowPathfindingClient, IWowEventAdapter wowEventAdapter, IMovementProvider movementProvider, ISpellStrategy spellStrategy)
        {
            Attached = true;
            WowPathfindingClient = wowPathfindingClient;
            WowDataAdapter?.StartObjectUpdates();
            AmeisenBotLogger.Instance.Log($"[{Process?.Id.ToString("X")}]\tStarted ObjectUpdates...");

            WowActionExecutor = wowActionExecutor;
            WowActionExecutor.IsWorldLoaded = true;
            AmeisenBotLogger.Instance.Log($"[{Process?.Id.ToString("X")}]\tStarted ActionExecutor...");

            WowEventAdapter = wowEventAdapter;
            WowEventAdapter?.Start();
            AmeisenBotLogger.Instance.Log($"[{Process?.Id.ToString("X")}]\tStarted EventAdapter...");

            WowEventAdapter?.Subscribe(WowEvents.PARTY_INVITE_REQUEST, OnPartyInvitation);

            StateMachine = new AmeisenBotStateMachine(WowDataAdapter, wowActionExecutor, wowPathfindingClient, movementProvider, spellStrategy);
            StateMachine?.Start();
            AmeisenBotLogger.Instance.Log($"[{Process?.Id.ToString("X")}]\tStarted StateMachine...");

            AmeisenBotLogger.Instance.Log($"[{Process?.Id.ToString("X")}]\tAmeisenBot attached...");
        }

        private void OnPartyInvitation(long timestamp, List<string> args)
        {
            WowActionExecutor.AcceptPartyInvite();
        }

        public void Detach()
        {
            StateMachine?.Stop();
            WowEventAdapter?.Stop();
            WowActionExecutor?.Stop();
            WowPathfindingClient?.Disconnect();
            WowDataAdapter?.StopObjectUpdates();
        }

        private void COnGamestateChanged(bool IsWorldLoaded, WowGameState gameState)
        {
            CheckForStuffToStart(IsWorldLoaded);

            if (gameState == WowGameState.Crashed)
            {
                Detach();
                return;
            }

            if (WowActionExecutor != null)
            {
                WowActionExecutor.IsWorldLoaded = IsWorldLoaded;
            }
        }

        private void CheckForStuffToStart(bool isWorldLoaded)
        {
            if (!isWorldLoaded)
            {
                if (WowDataAdapter != null && WowDataAdapter.ObjectUpdatesEnabled) { WowDataAdapter.StopObjectUpdates(); }
                if (StateMachine != null && StateMachine.Enabled) { StateMachine.Stop(); }
                if (WowEventAdapter != null && WowEventAdapter.Enabled) { WowEventAdapter.Stop(); }
            }
            else
            {
                if (WowDataAdapter != null && !WowDataAdapter.ObjectUpdatesEnabled) { WowDataAdapter.StartObjectUpdates(); }
                if (StateMachine != null && !StateMachine.Enabled) { StateMachine.Start(); }
                if (WowEventAdapter != null && !WowEventAdapter.Enabled) { WowEventAdapter.Start(); }
            }
        }

        public void ClearCaches()
        {
            WowDataAdapter.ClearCaches();
        }
    }
}
