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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using TrashMemCore;

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

        public TrashMem TrashMem { get; private set; }
        public Process Process { get; private set; }
        public bool Attached { get; private set; }

        public AmeisenBot(TrashMem trashMem, IWowDataAdapter wowDataAdapter, IAutologinProvider autologinProvider, Process process, IItemComparator itemComparator = null)
        {
            Attached = false;
            AutologinProvider = autologinProvider;
            Process = process;

            WowDataAdapter = wowDataAdapter;
            WowDataAdapter.OnGamestateChanged = COnGamestateChanged;
            TrashMem = trashMem;

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
            WowEventAdapter?.Subscribe(WowEvents.LOOT_OPENED, OnLootWindowOpened);
            WowEventAdapter?.Subscribe(WowEvents.RESURRECT_REQUEST, OnResurrectRequest);
            WowEventAdapter?.Subscribe(WowEvents.CONFIRM_SUMMON, OnSummonRequest);
            WowEventAdapter?.Subscribe(WowEvents.LOOT_BIND_CONFIRM, OnConfirmBindOnPickup);
            WowEventAdapter?.Subscribe(WowEvents.CONFIRM_LOOT_ROLL, OnConfirmBindOnPickup);
            WowEventAdapter?.Subscribe(WowEvents.READY_CHECK, OnReadyCheck);
            WowEventAdapter?.Subscribe(WowEvents.DELETE_ITEM_CONFIRM, OnConfirmDeleteItem);
            WowEventAdapter?.Subscribe(WowEvents.ITEM_PUSH, OnNewItemReceived);
            
            WowEventAdapter?.Subscribe(WowEvents.COMBAT_LOG_EVENT_UNFILTERED, OnCombatLogEvent);

            StateMachine = new AmeisenBotStateMachine(WowDataAdapter, wowActionExecutor, wowPathfindingClient, movementProvider, spellStrategy);
            StateMachine?.Start();
            AmeisenBotLogger.Instance.Log($"[{Process?.Id.ToString("X")}]\tStarted StateMachine...");

            CharacterManager?.UpdateFullCharacter();
            AmeisenBotLogger.Instance.Log($"[{Process?.Id.ToString("X")}]\tUpdated Character...");

            AmeisenBotLogger.Instance.Log($"[{Process?.Id.ToString("X")}]\tAmeisenBot attached...");
        }

        private void OnCombatLogEvent(long timestamp, List<string> args)
        {
            // TODO: parse the log or whatever...
        }

        private void OnConfirmDeleteItem(long timestamp, List<string> args)
        {
            // type delete and confirm this shit
        }

        private void OnNewItemReceived(long timestamp, List<string> args)
        {
            AmeisenBotLogger.Instance.Log($"Received new Item {JsonConvert.SerializeObject(args)}");
        }

        private void OnReadyCheck(long timestamp, List<string> args)
        {
            WowActionExecutor.CofirmReadyCheck(true);
        }

        private void OnConfirmBindOnPickup(long timestamp, List<string> args)
        {
            WowActionExecutor.CofirmBop();
        }

        private void OnSummonRequest(long timestamp, List<string> args)
        {
            WowActionExecutor.AcceptSummon();
        }

        private void OnResurrectRequest(long timestamp, List<string> args)
        {
            WowActionExecutor.AcceptResurrect();
        }

        private void OnLootWindowOpened(long timestamp, List<string> args)
        {
            WowActionExecutor.LootEveryThing();
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
