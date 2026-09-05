using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace MassConsume
{
    /// <summary>Appends the shared Consume command to drafted, player-
    /// controlled pawns. The vanilla gizmo grid merges the identical per-pawn
    /// commands into a single button when multiple pawns are selected.</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    internal static class Patch_Pawn_GetGizmos
    {
        private static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                yield return gizmo;
            }
            if (MassConsumeMod.Active)
            {
                Gizmo command = MassConsumeUtility.CreateConsumeCommand(__instance);
                if (command != null)
                {
                    yield return command;
                }
            }
        }
    }
}
