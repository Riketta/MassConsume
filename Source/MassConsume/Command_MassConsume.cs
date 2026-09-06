using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MassConsume
{
    /// <summary>The shared Consume button. One instance per selected pawn is
    /// appended by the Pawn.GetGizmos postfix; the vanilla gizmo grid merges
    /// instances with identical label and icon into one button and, on click,
    /// calls ProcessInput on every group member and ProcessGroupInput once -
    /// which is where the shared item menu opens for all pawns.</summary>
    internal class Command_MassConsume : Command
    {
        private static readonly Texture2D Icon = LoadIcon();

        public readonly Pawn Pawn;

        public Command_MassConsume(Pawn pawn)
        {
            Pawn = pawn;
            // Composed once per scan; identical labels across the selected
            // pawns' instances are also what makes the vanilla gizmo grid
            // merge them into one button.
            defaultLabel = MassConsumeUtility.SharedLabel;
            defaultDesc = MassConsumeUtility.SharedDesc;
            icon = Icon;
        }

        /// <summary>The vanilla gizmo grid routes a click on the merged button
        /// here once, with every pawn's command instance in the group.</summary>
        public override void ProcessGroupInput(Event ev, List<Gizmo> group)
        {
            MassConsumeUtility.OpenConsumeMenu(group);
        }

        private static Texture2D LoadIcon()
        {
            Texture2D texture = ContentFinder<Texture2D>.Get("UI/Commands/MassConsume", reportFailure: false);
            if (texture == null)
            {
                DebugLog.Warning("icon texture 'UI/Commands/MassConsume' is missing - falling back to the vanilla ingest icon.");
                texture = TexButton.Ingest;
            }
            return texture;
        }
    }
}
