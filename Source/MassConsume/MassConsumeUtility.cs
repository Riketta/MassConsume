using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MassConsume
{
    /// <summary>Scans the current selection for consumables carried in pawn
    /// inventories and builds the shared Consume command. Every edibility
    /// decision is delegated to the vanilla helpers behind the Inventory
    /// tab's consume button (FoodUtility.WillIngestFromInventoryNow /
    /// IngestFromInventoryNow), so meals, raw food, drugs and modded or DLC
    /// consumables all behave like their own buttons - nothing about specific
    /// defs is hardcoded.</summary>
    internal static class MassConsumeUtility
    {
        // Per-frame cache: every selected pawn's gizmo postfix reads the same
        // aggregate, so the selection is scanned once per frame, not per pawn.
        private static int scanFrame = -1;
        private static bool scannedThisFrame;
        private static int totalPawns;
        private static int pawnsWithItems;
        private static readonly List<Pawn> tmpEligible = new List<Pawn>();
        private static readonly List<Pawn> tmpGroupPawns = new List<Pawn>();
        private static readonly HashSet<ThingDef> tmpDefFlags = new HashSet<ThingDef>();
        private static readonly HashSet<ThingDef> tmpKnownDefs = new HashSet<ThingDef>();

        public static int TotalPawns => totalPawns;

        public static int PawnsWithItems => pawnsWithItems;

        /// <summary>Drafted, player-controlled pawns with an inventory - the
        /// audience vanilla's draft commands address, restricted to pawns that
        /// actually eat from pockets (animals and colony mechs are left out).</summary>
        public static bool PawnEligible(Pawn pawn)
        {
            if (pawn == null || pawn.Destroyed || !pawn.Spawned)
            {
                return false;
            }
            if (pawn.drafter == null || !pawn.Drafted)
            {
                return false;
            }
            if (!pawn.IsColonistPlayerControlled && !pawn.IsColonySubhumanPlayerControlled)
            {
                return false;
            }
            return pawn.inventory != null;
        }

        /// <summary>Returns the shared Consume command for this pawn, or null
        /// when the pawn or the current selection can't consume anything.</summary>
        public static Gizmo CreateConsumeCommand(Pawn pawn)
        {
            if (!PawnEligible(pawn) || Find.Selector == null || !Find.Selector.IsSelected(pawn))
            {
                return null;
            }
            ScanSelection();
            if (pawnsWithItems <= 0)
            {
                return null;
            }
            return new Command_MassConsume(pawn);
        }

        private static void ScanSelection()
        {
            if (scannedThisFrame && scanFrame == Time.frameCount)
            {
                return;
            }
            scanFrame = Time.frameCount;
            scannedThisFrame = true;

            tmpEligible.Clear();
            tmpKnownDefs.Clear();
            totalPawns = 0;
            pawnsWithItems = 0;

            List<Pawn> selected = Find.Selector.SelectedPawns;
            for (int i = 0; i < selected.Count; i++)
            {
                if (PawnEligible(selected[i]))
                {
                    tmpEligible.Add(selected[i]);
                }
            }
            totalPawns = tmpEligible.Count;

            for (int i = 0; i < tmpEligible.Count; i++)
            {
                Pawn pawn = tmpEligible[i];
                ThingOwner<Thing> inventory = pawn.inventory.innerContainer;
                if (inventory == null)
                {
                    continue;
                }
                tmpDefFlags.Clear();
                bool carriesAnything = false;
                for (int j = 0; j < inventory.Count; j++)
                {
                    Thing item = inventory[j];
                    if (!IsConsumableFor(pawn, item) || !tmpDefFlags.Add(item.def))
                    {
                        continue;
                    }
                    tmpKnownDefs.Add(item.def);
                    carriesAnything = true;
                    if (DebugLog.VerboseEnabled)
                    {
                        DebugLog.Verbose("scan: " + pawn.LabelShortCap + " can consume " + item.LabelCapNoCount
                            + " (" + item.def.defName + ").");
                    }
                }
                if (carriesAnything)
                {
                    pawnsWithItems++;
                }
            }

            if (DebugLog.VerboseEnabled)
            {
                DebugLog.Verbose("scan: " + pawnsWithItems + "/" + totalPawns + " selected pawn(s) carry consumables, "
                    + tmpKnownDefs.Count + " distinct item type(s).");
            }
        }

        /// <summary>The vanilla check behind the Inventory tab's consume
        /// button: food the pawn will eat, drugs it can take, ingestible now.</summary>
        private static bool IsConsumableFor(Pawn pawn, Thing item)
        {
            if (item == null || item.Destroyed || item.def?.ingestible == null)
            {
                return false;
            }
            return FoodUtility.WillIngestFromInventoryNow(pawn, item);
        }

        /// <summary>Opens the shared consume menu for one clicked command
        /// group: one entry per carried item type, each issuing a consume job
        /// to every selected pawn that carries the item. Pawns without the
        /// item do nothing - that is what the counts are for.</summary>
        public static void OpenConsumeMenu(List<Gizmo> group)
        {
            List<Pawn> pawns = CollectGroupPawns(group);
            if (pawns.Count == 0)
            {
                return;
            }

            // Recompute at click time: inventories may have changed since the
            // button was drawn (the counts in its label are display-only).
            Dictionary<ThingDef, List<Pawn>> pawnsPerDef = new Dictionary<ThingDef, List<Pawn>>();
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                ThingOwner<Thing> inventory = pawn.inventory.innerContainer;
                if (inventory == null)
                {
                    continue;
                }
                tmpDefFlags.Clear();
                for (int j = 0; j < inventory.Count; j++)
                {
                    Thing item = inventory[j];
                    if (!IsConsumableFor(pawn, item) || !tmpDefFlags.Add(item.def))
                    {
                        continue;
                    }
                    if (!pawnsPerDef.TryGetValue(item.def, out List<Pawn> carriers))
                    {
                        carriers = new List<Pawn>();
                        pawnsPerDef.Add(item.def, carriers);
                    }
                    carriers.Add(pawn);
                }
            }
            if (pawnsPerDef.Count == 0)
            {
                DebugLog.Message("consume menu skipped: nothing consumable is carried anymore.");
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (KeyValuePair<ThingDef, List<Pawn>> entry in pawnsPerDef)
            {
                ThingDef def = entry.Key;
                List<Pawn> carriers = entry.Value;
                string label = def.LabelCap;
                if (pawns.Count > 1)
                {
                    label += " (" + carriers.Count + "/" + pawns.Count + ")";
                }
                options.Add(new FloatMenuOption(label, delegate
                {
                    OrderConsume(carriers, def);
                }, def));
            }
            options.Sort(CompareOptionsByLabel);

            DebugLog.Message("consume menu opened: " + pawns.Count + " pawn(s), " + options.Count + " item type(s).");
            Find.WindowStack.Add(new FloatMenu(options) { givesColonistOrders = true });
        }

        private static List<Pawn> CollectGroupPawns(List<Gizmo> group)
        {
            tmpGroupPawns.Clear();
            if (group == null)
            {
                return tmpGroupPawns;
            }
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i] is Command_MassConsume command
                    && PawnEligible(command.Pawn)
                    && !tmpGroupPawns.Contains(command.Pawn))
                {
                    tmpGroupPawns.Add(command.Pawn);
                }
            }
            return tmpGroupPawns;
        }

        private static int CompareOptionsByLabel(FloatMenuOption a, FloatMenuOption b)
        {
            return string.Compare(a.Label, b.Label, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void OrderConsume(List<Pawn> carriers, ThingDef def)
        {
            // Re-resolve the actual item at pick time. Shift-queueing comes
            // for free: IngestFromInventoryNow issues an ordered job, and the
            // game queues ordered jobs while the QueueOrder key (Shift) is
            // held, so several picks line up (meal, then beer, then go-juice).
            for (int i = 0; i < carriers.Count; i++)
            {
                Pawn pawn = carriers[i];
                if (pawn.Dead || pawn.Destroyed)
                {
                    DebugLog.Verbose("order skipped: " + pawn.LabelShortCap + " is gone.");
                    continue;
                }
                Thing item = FindConsumableInInventory(pawn, def);
                if (item == null)
                {
                    DebugLog.Verbose("order skipped: " + pawn.LabelShortCap + " has no " + def.label
                        + " left to order (carried item already consumed or ordered).");
                    continue;
                }
                DebugLog.Message("ordered: " + pawn.LabelShortCap + " consumes " + item.LabelCapNoCount + ".");
                FoodUtility.IngestFromInventoryNow(pawn, item);
            }
        }

        private static Thing FindConsumableInInventory(Pawn pawn, ThingDef def)
        {
            ThingOwner<Thing> inventory = pawn.inventory?.innerContainer;
            if (inventory == null)
            {
                return null;
            }
            for (int i = 0; i < inventory.Count; i++)
            {
                Thing item = inventory[i];
                if (item != null && item.def == def && IsConsumableFor(pawn, item) && !IsTargetedByOrderedJobs(pawn, item))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>True when the pawn's current or queued orders already
        /// consume this exact item, so picking the same menu entry again
        /// targets the next one instead of queueing a second job onto a thing
        /// another job will destroy.</summary>
        private static bool IsTargetedByOrderedJobs(Pawn pawn, Thing item)
        {
            if (pawn.jobs == null)
            {
                return false;
            }
            if (pawn.jobs.curJob != null && pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing == item)
            {
                return true;
            }
            JobQueue queue = pawn.jobs.jobQueue;
            for (int i = 0; i < queue.Count; i++)
            {
                Job job = queue[i].job;
                if (job != null && job.GetTarget(TargetIndex.A).Thing == item)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Dumps the current selection with per-item consumability
        /// verdicts to the log (mod settings button; always logs).</summary>
        public static void LogSelectionOverview()
        {
            if (Current.ProgramState != ProgramState.Playing || Find.Selector == null)
            {
                Log.Message("[MassConsume] selection overview: not in game.");
                return;
            }
            List<Pawn> selected = Find.Selector.SelectedPawns;
            Log.Message("[MassConsume] selection overview: " + selected.Count + " object(s) selected.");
            for (int i = 0; i < selected.Count; i++)
            {
                Pawn pawn = selected[i];
                if (!PawnEligible(pawn))
                {
                    Log.Message("[MassConsume] - " + (pawn?.LabelShortCap ?? "null")
                        + ": not eligible (needs a drafted, player-controlled pawn with an inventory).");
                    continue;
                }
                Log.Message("[MassConsume] - " + pawn.LabelShortCap + ": eligible, drafted.");
                ThingOwner<Thing> inventory = pawn.inventory.innerContainer;
                if (inventory == null || inventory.Count == 0)
                {
                    Log.Message("[MassConsume]   inventory: empty.");
                    continue;
                }
                for (int j = 0; j < inventory.Count; j++)
                {
                    Thing item = inventory[j];
                    Log.Message("[MassConsume]   " + (item?.LabelCapNoCount ?? "null") + " x" + (item?.stackCount ?? 0)
                        + ": " + DescribeItem(pawn, item) + ".");
                }
            }
        }

        /// <summary>Mirrors the branches of WillIngestFromInventoryNow so the
        /// overview explains WHY an item is (not) consumable.</summary>
        private static string DescribeItem(Pawn pawn, Thing item)
        {
            if (item == null || item.def == null)
            {
                return "null item";
            }
            if (item.def.ingestible == null)
            {
                return "not ingestible";
            }
            if (!item.IngestibleNow)
            {
                return "not ingestible right now (e.g. burning)";
            }
            if (item.def.IsNutritionGivingIngestible)
            {
                return pawn.WillEat(item) ? "food - will eat" : "food - will NOT eat (food restrictions)";
            }
            if (item.def.GetCompProperties<CompProperties_Drug>() != null)
            {
                return pawn.CanTakeDrug(item.def) ? "drug - can take" : "drug - will NOT take (drug policy or traits)";
            }
            return "ingestible but neither nutrition-giving nor a drug";
        }
    }
}
