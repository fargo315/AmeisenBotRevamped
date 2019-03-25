using static AmeisenBotRevamped.EventAdapters.LuaHookWowEventAdapter;

namespace AmeisenBotRevamped.EventAdapters
{
    public interface IWowEventAdapter
    {
        void Start();
        void Stop();
        bool Enabled { get; }
        void Subscribe(string eventName, OnEventFired onEventFired);
        void Unsubscribe(string eventName);
    }
}
