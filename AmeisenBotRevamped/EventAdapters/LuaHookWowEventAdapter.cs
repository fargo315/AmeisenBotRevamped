using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.EventAdapters.Structs;
using AmeisenBotRevamped.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace AmeisenBotRevamped.EventAdapters
{
    public class LuaHookWowEventAdapter : IWowEventAdapter
    {
        public delegate void OnEventFired(long timestamp, List<string> args);

        public Dictionary<string, OnEventFired> EventDictionary { get; }

        private Thread EventReaderThread { get; }
        private IWowActionExecutor WowActionExecutor { get; }

        private bool IsSetUp { get; set; }

        public LuaHookWowEventAdapter(IWowActionExecutor actionExecutor)
        {
            EventDictionary = new Dictionary<string, OnEventFired>();
            WowActionExecutor = actionExecutor;

            Enabled = true;
            EventReaderThread = new Thread(new ThreadStart(CEventReader));

            IsSetUp = false;
        }

        public void Start()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tStarting EventHook...");
            if (!IsSetUp)
            {
                SetupEventHook();
                IsSetUp = true;
            }

            EventReaderThread.Start();
        }

        public void Stop()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tStopping EventHook...");
            WowActionExecutor.LuaDoString($"abFrame:UnregisterAllEvents();");
            WowActionExecutor.LuaDoString($"abFrame:SetScript(\"OnEvent\", nil);");

            Enabled = false;

            if (EventReaderThread?.IsAlive == true)
            {
                EventReaderThread.Join();
            }
        }

        public bool Enabled { get; set; }

        private void CEventReader()
        {
            while (Enabled)
            {
                try
                {
                    if (!IsSetUp)
                    {
                        SetupEventHook();
                        continue;
                    }

                    // Unminified lua code can be found im my github repo "WowLuaStuff"
                    WowActionExecutor.LuaDoString("abEventJson='['for a,b in pairs(abEventTable)do abEventJson=abEventJson..'{'for c,d in pairs(b)do if type(d)==\"table\"then abEventJson=abEventJson..'\"args\": ['for e,f in pairs(d)do abEventJson=abEventJson..'\"'..f..'\"'if e<=table.getn(d)then abEventJson=abEventJson..','end end;abEventJson=abEventJson..']}'if a<table.getn(abEventTable)then abEventJson=abEventJson..','end else if type(d)==\"string\"then abEventJson=abEventJson..'\"event\": \"'..d..'\",'else abEventJson=abEventJson..'\"time\": \"'..d..'\",'end end end end;abEventJson=abEventJson..']'abEventTable={}");
                    string eventJson = WowActionExecutor.GetLocalizedText("abEventJson");

                    HandlEvents(eventJson);
                }
                catch (Exception ex)
                {
                    AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tCrash at StateMachine: \n{ex}");
                }

                Thread.Sleep(1000);
            }
        }

        private void HandlEvents(string eventJson)
        {
            if (eventJson?.Length == 0)
                return;

            try
            {
                List<RawEvent> rawEvents = JsonConvert.DeserializeObject<List<RawEvent>>(eventJson);
                List<RawEvent> finalEvents = ParseRawEvents(rawEvents);

                if (finalEvents.Count > 0)
                {
                    FireEventHandlers(finalEvents);
                }
            }
            catch (Exception ex)
            {
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tCrash at StateMachine: \n{ex}");
            }
        }

        private void FireEventHandlers(List<RawEvent> finalEvents)
        {
            foreach (RawEvent rawEvent in finalEvents)
            {
                if (EventDictionary.ContainsKey(rawEvent.@event))
                {
                    AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\t{EventDictionary[rawEvent.@event].Method.Name}({rawEvent.time}, {JsonConvert.SerializeObject(rawEvent.args)})");
                    EventDictionary[rawEvent.@event].Invoke(rawEvent.time, rawEvent.args);
                }
            }
        }

        private static List<RawEvent> ParseRawEvents(List<RawEvent> rawEvents)
        {
            List<RawEvent> finalEvents = new List<RawEvent>();
            foreach (RawEvent rawEvent in rawEvents)
            {
                if (!finalEvents.Contains(rawEvent))
                {
                    finalEvents.Add(rawEvent);
                }
            }
            return finalEvents;
        }

        ~LuaHookWowEventAdapter()
        {
            Stop();
        }

        private void SetupEventHook()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tPreparing EventHook...");
            StringBuilder luaStuff = new StringBuilder();
            luaStuff.Append("abFrame = CreateFrame(\"FRAME\", \"AbotEventFrame\") ");
            luaStuff.Append("abEventTable = {} ");
            luaStuff.Append("function abEventHandler(self, event, ...) ");
            luaStuff.Append("table.insert(abEventTable, {time(), event, {...}}) end ");
            luaStuff.Append("if abFrame:GetScript(\"OnEvent\") == nil then ");
            luaStuff.Append("abFrame:SetScript(\"OnEvent\", abEventHandler) end");
            WowActionExecutor.LuaDoString(luaStuff.ToString());
            IsSetUp = true;
        }

        public void Subscribe(string eventName, OnEventFired onEventFired)
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tSubscribed to \"{eventName}\"");
            WowActionExecutor.LuaDoString($"abFrame:RegisterEvent(\"{eventName}\");");
            EventDictionary.Add(eventName, onEventFired);
        }

        public void Unsubscribe(string eventName)
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tUnsubscribed from \"{eventName}\"");
            WowActionExecutor.LuaDoString($"abFrame:UnregisterEvent(\"{eventName}\");");
            EventDictionary.Remove(eventName);
        }
    }
}
