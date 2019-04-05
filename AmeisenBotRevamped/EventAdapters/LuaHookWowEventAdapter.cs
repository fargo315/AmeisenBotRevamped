using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.EventAdapters.Structs;
using AmeisenBotRevamped.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace AmeisenBotRevamped.EventAdapters
{
    public class LuaHookWowEventAdapter : IWowEventAdapter
    {
        public delegate void OnEventFired(long timestamp, List<string> args);

        public Dictionary<string, OnEventFired> EventDictionary { get; private set; }

        private Timer EventReaderTimer { get; set; }
        private IWowActionExecutor WowActionExecutor { get; set; }

        private bool IsSetUp { get; set; }

        public LuaHookWowEventAdapter(IWowActionExecutor actionExecutor)
        {
            EventDictionary = new Dictionary<string, OnEventFired>();
            WowActionExecutor = actionExecutor;

            EventReaderTimer = new Timer(1000);
            EventReaderTimer.Elapsed += CEventReaderTimer;

            IsSetUp = false;
        }

        public void Start()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X")}]\tStarting EventHook...");
            if (!IsSetUp)
            {
                IsSetUp = true;
                SetupEventHook();
            }
            EventReaderTimer.Start();
        }
        public void Stop()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X")}]\tStopping EventHook...");
            WowActionExecutor.LuaDoString($"abFrame:UnregisterAllEvents(); abFrame:SetScript(\"OnEvent\", nil);");
            EventReaderTimer.Stop();
        }

        public bool Enabled => EventReaderTimer.Enabled;

        private void CEventReaderTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!IsSetUp)
                {
                    IsSetUp = true;
                    SetupEventHook();
                }

                // Unminified lua code can be found im my github repo "WowLuaStuff"
                WowActionExecutor.LuaDoString("abEventJson='['for a,b in pairs(abEventTable)do abEventJson=abEventJson..'{'for c,d in pairs(b)do if type(d)==\"table\"then abEventJson=abEventJson..'\"args\": ['for e,f in pairs(d)do abEventJson=abEventJson..'\"'..f..'\"'if e<=table.getn(d)then abEventJson=abEventJson..','end end;abEventJson=abEventJson..']}'if a<table.getn(abEventTable)then abEventJson=abEventJson..','end else if type(d)==\"string\"then abEventJson=abEventJson..'\"event\": \"'..d..'\",'else abEventJson=abEventJson..'\"time\": \"'..d..'\",'end end end end;abEventJson=abEventJson..']'abEventTable={}");
                string eventJson = WowActionExecutor.GetLocalizedText("abEventJson");

                List<RawEvent> rawEvents = new List<RawEvent>();
                try
                {
                    List<RawEvent> finalEvents = new List<RawEvent>();
                    rawEvents = JsonConvert.DeserializeObject<List<RawEvent>>(eventJson);

                    foreach (RawEvent rawEvent in rawEvents)
                    {
                        if (!finalEvents.Contains(rawEvent))
                        {
                            finalEvents.Add(rawEvent);
                        }
                    }

                    if (finalEvents.Count > 0)
                    {
                        foreach (RawEvent rawEvent in finalEvents)
                        {
                            if (EventDictionary.ContainsKey(rawEvent.@event))
                            {
                                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X")}]\t{EventDictionary[rawEvent.@event].Method.Name}({rawEvent.time}, {JsonConvert.SerializeObject(rawEvent.args)})");
                                EventDictionary[rawEvent.@event].Invoke(rawEvent.time, rawEvent.args);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tCrash at StateMachine: \n{ex.ToString()}");
                }
            }
            catch (Exception ex)
            {
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tCrash at StateMachine: \n{ex.ToString()}");
            }
        }

        ~LuaHookWowEventAdapter()
        {
            Stop();
        }

        private void SetupEventHook()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X")}]\tPreparing EventHook...");
            StringBuilder luaStuff = new StringBuilder();
            luaStuff.Append("abFrame = CreateFrame(\"FRAME\", \"AbotEventFrame\") ");
            luaStuff.Append("abEventTable = {} ");
            luaStuff.Append("function abEventHandler(self, event, ...) ");
            luaStuff.Append("table.insert(abEventTable, {time(), event, {...}}) end ");
            luaStuff.Append("if abFrame:GetScript(\"OnEvent\") == nil then ");
            luaStuff.Append("abFrame:SetScript(\"OnEvent\", abEventHandler) end");
            WowActionExecutor.LuaDoString(luaStuff.ToString());
        }

        public void Subscribe(string eventName, OnEventFired onEventFired)
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X")}]\tSubscribed to \"{eventName}\"");
            WowActionExecutor.LuaDoString($"abFrame:RegisterEvent(\"{eventName}\");");
            EventDictionary.Add(eventName, onEventFired);
        }

        public void Unsubscribe(string eventName)
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X")}]\tUnsubscribed from \"{eventName}\"");
            WowActionExecutor.LuaDoString($"abFrame:UnregisterEvent(\"{eventName}\");");
            EventDictionary.Remove(eventName);
        }
    }
}
