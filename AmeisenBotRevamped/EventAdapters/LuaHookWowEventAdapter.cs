using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.EventAdapters.Structs;
using Newtonsoft.Json;
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

        public LuaHookWowEventAdapter(IWowActionExecutor actionExecutor)
        {
            EventDictionary = new Dictionary<string, OnEventFired>();
            WowActionExecutor = actionExecutor;

            EventReaderTimer = new Timer(1000);
            EventReaderTimer.Elapsed += CEventReaderTimer;

            SetupEventHook();
        }

        public void Start() => EventReaderTimer.Start();
        public void Stop() => EventReaderTimer.Stop();
        public bool Enabled => EventReaderTimer.Enabled;

        private void CEventReaderTimer(object sender, ElapsedEventArgs e)
        {
            // Unminified lua code can be found im my github repo "WowLuaStuff"
            WowActionExecutor.LuaDoString("abEventJson='['for a,b in pairs(abEventTable)do abEventJson=abEventJson..'{'for c,d in pairs(b)do if type(d)==\"table\"then abEventJson=abEventJson..'\"args\": ['for e,f in pairs(d)do abEventJson=abEventJson..'\"'..f..'\"'if e<=table.getn(d)then abEventJson=abEventJson..','end end;abEventJson=abEventJson..']}'if a<table.getn(abEventTable)then abEventJson=abEventJson..','end else if type(d)==\"string\"then abEventJson=abEventJson..'\"event\": \"'..d..'\",'else abEventJson=abEventJson..'\"time\": \"'..d..'\",';end end end end;abEventJson=abEventJson..']';abEventTable={};");
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
                            EventDictionary[rawEvent.@event].Invoke(rawEvent.time, rawEvent.args);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        ~LuaHookWowEventAdapter()
        {
            WowActionExecutor.LuaDoString($"abFrame:UnregisterAllEvents(); abFrame:SetScript(\"OnEvent\", nil);");
            EventReaderTimer.Stop();
        }

        private void SetupEventHook()
        {
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
            WowActionExecutor.LuaDoString($"abFrame:RegisterEvent(\"{eventName}\");");
            EventDictionary.Add(eventName, onEventFired);
        }

        public void Unsubscribe(string eventName)
        {
            WowActionExecutor.LuaDoString($"abFrame:UnregisterEvent(\"{eventName}\");");
            EventDictionary.Remove(eventName);
        }
    }
}
