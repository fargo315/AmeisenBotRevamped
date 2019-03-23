# AmeisenBotRevamped
An almost complete rewrite of this -> https://github.com/Jnnshschl/AmeisenBot-3.3.5a

Things will be much more organized in this revamp :^)

⚠️ This Bot is not really useable at the moment!

## Credits

❤️ **Blackmagic** (Memory Editing) - https://github.com/acidburn974/Blackmagic

## Config

### Basic Config

Stuff should be pretty self explaining.

```JSON
{
  "WowExecutableFilePath":"G:\\WoW Stuff\\World of Warcraft WotLK - Bot\\Wow.exe", 
  "BotListFilePath":"C:\\Users\\Jannis\\source\\repos\\AmeisenBotRevamped\\AmeisenBotRevamped.Gui\\bin\\Debug\\bots.json"
}
```

### Experimental: BotFleet Config

The Bot will keep this cccounts running automatically if you set both, "WowExecutableFilePath" and "BotListFilePath" paths. It will recognize if a WoW instance has crashed and will automatically restart it and do the login for you.

```JSON
[
  {"Username":"Account1", "Password":"Password", "CharacterSlot":0, "CharacterName":"Jannis"},
  {"Username":"Account2", "Password":"Password", "CharacterSlot":0, "CharacterName":"Notabot"}
]
```

## Screenshots

![alt text](https://github.com/Jnnshschl/AmeisenBotRevamped/blob/master/images/Bot.png?raw=true "Bot")
