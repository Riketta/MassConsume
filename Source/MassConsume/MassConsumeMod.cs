using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace MassConsume
{
    /// <summary>Debug verbosity levels. Stored as int in settings so enum
    /// reordering can never corrupt saves.</summary>
    public enum DebugLogLevel
    {
        Off = 0,
        Basic = 1,
        Verbose = 2
    }

    public class MassConsumeSettings : ModSettings
    {
        public bool enabled = true;

        public int debugLevel = (int)DebugLogLevel.Off;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref debugLevel, "debugLevel", (int)DebugLogLevel.Off);
        }
    }

    public class MassConsumeMod : Mod
    {
        public const string PackageId = "Riketta.MassConsume";

        public static MassConsumeSettings Settings;

        /// <summary>Master switch, read by the patch on each call. Null-safe:
        /// without settings the patch stays active rather than silently
        /// disabling the mod.</summary>
        public static bool Active => Settings?.enabled ?? true;

        public MassConsumeMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MassConsumeSettings>();
            // Patch each class separately: a game update that renames one target must
            // degrade to "that vanilla behavior stays", never break the other patch.
            Harmony harmony = new Harmony(PackageId);
            PatchSafe(harmony, typeof(Patch_Pawn_GetGizmos));
            DebugLog.Message("loaded (enabled=" + (Settings.enabled ? "true" : "false")
                + ", debugLevel=" + (DebugLogLevel)Settings.debugLevel + ").");
        }

        private static void PatchSafe(Harmony harmony, Type patchClass)
        {
            try
            {
                harmony.CreateClassProcessor(patchClass).Patch();
                DebugLog.Message("applied " + patchClass.Name + ".");
            }
            catch (Exception e)
            {
                Log.Error("[MassConsume] Patch " + patchClass.Name + " could not be applied (game update?). " + e.Message);
            }
        }

        public override string SettingsCategory()
        {
            return "MassConsume.SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.CheckboxLabeled("MassConsume.Enabled".Translate(), ref Settings.enabled, "MassConsume.Enabled.Tip".Translate());
            list.Gap(6f);

            Rect debugRect = list.GetRect(30f);
            string levelName = ((DebugLogLevel)Settings.debugLevel).ToString();
            if (Widgets.ButtonText(debugRect, "MassConsume.DebugLevel".Translate(levelName)))
            {
                Settings.debugLevel = (Settings.debugLevel + 1) % 3;
            }
            TooltipHandler.TipRegion(debugRect, "MassConsume.DebugLevel.Tip".Translate());
            list.Gap(6f);

            Rect overviewRect = list.GetRect(30f);
            if (Widgets.ButtonText(overviewRect, "MassConsume.LogOverview".Translate()))
            {
                MassConsumeUtility.LogSelectionOverview();
            }
            TooltipHandler.TipRegion(overviewRect, "MassConsume.LogOverview.Tip".Translate());
            list.Gap(12f);

            GUI.color = ColoredText.SubtleGrayColor;
            list.Label("MassConsume.BehaviorNote".Translate());
            GUI.color = Color.white;
            list.End();
        }
    }
}
