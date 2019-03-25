using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.StateMachine;
using AmeisenBotRevamped.Autologin;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.EventAdapters;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using Magic;
using System;
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

        public AmeisenBotStateMachine StateMachine { get; private set; }

        public BlackMagic BlackMagic { get; private set; }
        public Process Process { get; private set; }
        public bool Attached { get; private set; }

        public AmeisenBot(BlackMagic blackMagic, IWowDataAdapter wowDataAdapter, IAutologinProvider autologinProvider, Process process)
        {
            Attached = false;
            AutologinProvider = autologinProvider;
            Process = process;

            WowDataAdapter = wowDataAdapter;
            WowDataAdapter.OnGamestateChanged = COnGamestateChanged;
            BlackMagic = blackMagic;
        }

        public void Attach(IWowActionExecutor wowActionExecutor, IPathfindingClient wowPathfindingClient, IWowEventAdapter wowEventAdapter)
        {
            Attached = true;
            WowPathfindingClient = wowPathfindingClient;
            WowDataAdapter?.StartObjectUpdates();

            WowActionExecutor = wowActionExecutor;
            WowActionExecutor.IsWorldLoaded = true;

            WowEventAdapter = wowEventAdapter;
            WowEventAdapter?.Start();
            WowEventAdapter?.Subscribe(WowEvents.PARTY_INVITE_REQUEST, OnPartyInvitation);

            StateMachine = new AmeisenBotStateMachine(WowDataAdapter, wowActionExecutor, wowPathfindingClient);
            StateMachine?.Start();
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
