# Mass Consume

A RimWorld 1.6 mod that adds one **Consume** command for the whole selection:
select any number of drafted pawns, press **Consume**, pick an item from the
list, and every selected pawn that carries that item in their inventory
consumes one on the spot. The button shows at a glance how many of the
selected pawns carry something consumable - `Consume (6/7)` means six of the
seven selected pawns have an edible or drinkable item in their pockets.

Requires [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077).
Works with all DLC and mods by design, and is safe to add or remove at any
time.

## How to use

1. Draft two or more pawns and select them all. The command bar shows
   **Consume (n/m)** - n of the m selected pawns carry something consumable.
2. Click the button. A list drops down with every carried item type, each
   entry labeled with its own carrier count, e.g. **Simple meal (6/7)**,
   **Beer (2/7)**, **Go-juice (1/7)**.
3. Pick an entry. Every selected pawn that carries that item immediately
   consumes one of it, exactly like pressing the consume button in their
   Inventory tab. Pawns without the item do nothing.
4. Hold **Shift** while picking to queue orders instead of replacing them:
   eat the meal, then drink the beer, then take the go-juice - the pawns work
   through the queue in order. This is standard RimWorld Shift-queueing: the
   button is a normal command, the menu entries issue normal ordered jobs.

The button also appears for a single selected pawn, where it is a convenient
"consume something" shortcut that covers food as well as drugs.

## What counts as consumable

Everything the game itself allows a pawn to consume from inventory: meals and
raw food the pawn's food restrictions accept, drugs their drug policy and
traits permit, and anything modded or DLC in between. The decision is
delegated to the same vanilla checks (`FoodUtility.WillIngestFromInventoryNow`)
that power the Inventory tab's consume button - nothing about specific items,
food kinds or drugs is hardcoded, so future DLC content and modded items just
work.

## Mod settings

- **Enabled** - master switch; hides the button while off.
- **Debug logging** - Off / Basic / Verbose:
  - Basic logs menu opens and every consume order.
  - Verbose additionally logs each selection scan with a line per pawn and
    per item, plus every skipped pawn (very spammy).
- **Log selection overview** - dumps the current selection to the log: every
  eligible pawn and each inventory item with its consumability verdict and
  the reason.

## Known limitations

- The command addresses drafted, player-controlled pawns on the map
  (colonists and player-controlled subhumans). Animals and colony mechs are
  not pocket-eaters and get no button.
- Counts are computed while drawing the button and re-checked when you pick
  an entry; if something changed in between (an item was consumed elsewhere),
  the affected pawns are simply skipped.
- Menu entries consume one item per pick (tiny items per the game's own
  ingest amount, e.g. several berries at once). Picking the same entry again
  orders the next carried item, so repeat picks queue one item each.
- The Ideology precept hints that vanilla's right-click menu shows for food
  in the world are not reproduced; consuming still causes the normal
  thoughts and mood effects.

## Technical notes

For modders and the curious - no def, DLC or mod lists are hardcoded:

- A Harmony postfix on `Pawn.GetGizmos` appends the command per pawn; the
  vanilla gizmo grid merges the identical per-pawn commands into one button,
  and a click on the merged button lands in `ProcessGroupInput` with every
  pawn's command instance, where one shared float menu is opened.
- Consumables are found with `FoodUtility.WillIngestFromInventoryNow` and
  ordered with `FoodUtility.IngestFromInventoryNow` - the exact helpers
  behind the vanilla Inventory tab consume button - so restrictions, stack
  amounts, hediffs and thoughts all stay vanilla.
- Ordered jobs go through `Pawn_JobTracker.TryTakeOrderedJob`, so vanilla
  Shift-queueing (the QueueOrder key binding) applies at menu-pick time.
- The selection scan is cached per frame, so the per-frame UI cost is one
  scan regardless of selection size.
- Debug logging: Basic covers menu opens and orders; Verbose additionally
  covers the per-frame scans and skips.

## Build from source

Requires the .NET SDK and a RimWorld 1.6 install. Build the Release
configuration for the dll you ship - a plain `dotnet build` defaults to
Debug:

```
cd Source/MassConsume
dotnet build -c Release -p:RimWorldDir="C:\Path\To\RimWorld"
```

The output lands in `Assemblies/MassConsume.dll`. Copy or symlink the whole
`MassConsume` folder into the game's `Mods` directory to try it.
