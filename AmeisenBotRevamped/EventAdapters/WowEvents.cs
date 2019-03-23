using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.EventAdapters
{
    public static class WowEvents
    {
        /// <summary>
        /// CombatLog events
        /// </summary>
        public const string COMBAT_LOG_EVENT_UNFILTERED = "COMBAT_LOG_EVENT_UNFILTERED";

        /// <summary>
        /// Gets fired when you need to confirm to roll on BoP items
        /// </summary>
        public const string CONFIRM_LOOT_ROLL = "CONFIRM_LOOT_ROLL";

        /// <summary>
        /// Gets fired when you're summoned
        /// </summary>
        public const string CONFIRM_SUMMON = "CONFIRM_SUMMON";

        /// <summary>
        /// Gets fired when you're going to destroy an item
        /// </summary>
        public const string DELETE_ITEM_CONFIRM = "DELETE_ITEM_CONFIRM";

        /// <summary>
        /// Gets fired when you're invited to a guild
        /// </summary>
        public const string GUILD_INVITE_REQUEST = "GUILD_INVITE_REQUEST";

        /// <summary>
        /// Gets fired when you receive a new item
        /// </summary>
        public const string ITEM_PUSH = "ITEM_PUSH";

        /// <summary>
        /// Gets fired when the LFG is ready
        /// </summary>
        public const string LFG_PROPOSAL_SHOW = "LFG_PROPOSAL_SHOW";

        /// <summary>
        /// Gets fired when you pickup a BoP item
        /// </summary>
        public const string LOOT_BIND_CONFIRM = "LOOT_BIND_CONFIRM";

        /// <summary>
        /// Gets fired when you loot something
        /// </summary>
        public const string LOOT_OPENED = "LOOT_OPENED";

        /// <summary>
        /// Gets fired when you speak to a merchant
        /// </summary>
        public const string MERCHANT_SHOW = "MERCHANT_SHOW";

        /// <summary>
        /// Gets fired when you get a paryt invite
        /// </summary>
        public const string PARTY_INVITE_REQUEST = "PARTY_INVITE_REQUEST";

        /// <summary>
        /// new, removed, moved player from group/raid
        /// </summary>
        public const string PARTY_MEMBERS_CHANGED = "PARTY_MEMBERS_CHANGED";

        /// <summary>
        /// Gets fired when you gain control
        /// </summary>
        public const string PLAYER_CONTROL_GAINED = "PLAYER_CONTROL_GAINED";

        /// <summary>
        /// Gets fired when you lose control
        /// </summary>
        public const string PLAYER_CONTROL_LOST = "PLAYER_CONTROL_LOST";

        /// <summary>
        /// Gets fired when you die
        /// </summary>
        public const string PLAYER_DEAD = "PLAYER_DEAD";

        /// <summary>
        /// Gets fired when you're in loadingscreens
        /// </summary>
        public const string PLAYER_ENTERING_WORLD = "PLAYER_ENTERING_WORLD";

        /// <summary>
        /// Gets fired when you levelup
        /// </summary>
        public const string PLAYER_LEVEL_UP = "PLAYER_LEVEL_UP";

        /// <summary>
        /// Gets fired when you enter combat or get aggro
        /// </summary>
        public const string PLAYER_REGEN_DISABLED = "PLAYER_REGEN_DISABLED";

        /// <summary>
        /// Gets fired when you lose combat or aggro
        /// </summary>
        public const string PLAYER_REGEN_ENABLED = "PLAYER_REGEN_ENABLED";

        /// <summary>
        /// Gets fired when readycheck fires
        /// </summary>
        public const string READY_CHECK = "READY_CHECK";

        /// <summary>
        /// Gets fired when you get a resurrect
        /// </summary>
        public const string RESURRECT_REQUEST = "RESURRECT_REQUEST";

        /// <summary>
        /// Gets fired when you're able roll on an item
        /// </summary>
        public const string START_LOOT_ROLL = "START_LOOT_ROLL";

        /// <summary>
        /// Gets fired when the taxi map opens
        /// </summary>
        public const string TAXIMAP_OPENED = "TAXIMAP_OPENED";

        /// <summary>
        /// Gets fired when somebody accepts the trade
        /// </summary>
        public const string TRADE_ACCEPT_UPDATE = "TRADE_ACCEPT_UPDATE";

        /// <summary>
        /// Gets fired when the tradewindow closes
        /// </summary>
        public const string TRADE_CLOSED = "TRADE_CLOSED";

        /// <summary>
        /// Gets fired when the money changes in a trade
        /// </summary>
        public const string TRADE_MONEY_CHANGED = "TRADE_MONEY_CHANGED";

        /// <summary>
        /// Gets fired when the items in a trade change
        /// </summary>
        public const string TRADE_PLAYER_ITEM_CHANGED = "TRADE_PLAYER_ITEM_CHANGED";

        /// <summary>
        /// Gets fired when the tradewindow shows
        /// </summary>
        public const string TRADE_SHOW = "TRADE_SHOW";

        /// <summary>
        /// Gets fired when the trainer window closes
        /// </summary>
        public const string TRAINER_CLOSED = "TRAINER_CLOSED";

        /// <summary>
        /// Gets fired when the trainer window shows
        /// </summary>
        public const string TRAINER_SHOW = "TRAINER_SHOW";

        /// <summary>
        /// Gets fired when an error appears (red text)
        /// </summary>
        public const string UI_ERROR_MESSAGE = "UI_ERROR_MESSAGE";

        /// <summary>
        /// Gets fired when an info appears (yellow text)
        /// </summary>
        public const string UI_INFO_MESSAGE = "UI_INFO_MESSAGE";

        /// <summary>
        /// Gets fired when world events trigger (BG when somebody takes the flag)
        /// </summary>
        public const string UPDATE_WORLD_STATES = "UPDATE_WORLD_STATES";
    }
}
