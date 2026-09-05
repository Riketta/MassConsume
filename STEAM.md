# Steam Workshop description

Paste the text below into the Workshop item's description field when publishing
(the BBCode renders on Steam, but not in-game - `About/About.xml` carries its own
plain-text description).

```
[h3]Mass Consume[/h3]
One Consume button for your whole selection: select drafted pawns, pick an item from the list, and every selected pawn that carries it eats or drinks one right from their pockets.

[h3]What it does[/h3]
[list][*]Drafted pawns get a Consume command showing how many of them carry something consumable - Consume (6/7) means six of the seven selected pawns have an edible or drinkable item in their inventory.
[*]Clicking it drops a list of everything carried, with a per-item carrier count: Simple meal (6/7), Beer (2/7), Go-juice (1/7)...
[*]Picking an entry makes every carrier consume one item immediately, like the consume button in the Inventory tab. Pawns without the item do nothing.
[*]Shift-click list entries to queue: meal first, then beer, then go-juice - pawns work through the queue in order (picking the same entry again orders the next carried item).[/list]

[h3]Things to keep in mind[/h3]
[list][*]Anything consumable works: meals, raw food, drugs, alcohol and modded or DLC items - the game's own edibility checks decide, so food restrictions, drug policies and traits are respected.
[*]Works for a single selected pawn too - a handy "consume something" button covering food as well as drugs.
[*]Animals and colony mechs don't get the button (they don't eat from pockets).[/list]

[h3]Compatibility[/h3]
Requires RimWorld 1.6 and [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077]Harmony[/url]; all DLCs are optional.
Works with all DLC and mods by design - nothing is hardcoded to specific items, and consuming uses the vanilla inventory-tab pipeline. Safe to add or remove at any time.

Optional debug logging (Off / Basic / Verbose) and a selection overview are available in the mod settings.

Source code and details: [url]https://github.com/Riketta/MassConsume[/url]
```
