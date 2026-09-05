using Verse;

namespace MassConsume
{
    /// <summary>Optional layered logging, off by default (see mod settings).
    /// Basic: menu opens, consume orders and skips.
    /// Verbose: additionally every selection scan with a line per pawn/item.
    /// Errors are always logged regardless of level.</summary>
    internal static class DebugLog
    {
        private const string Prefix = "[MassConsume] ";

        private static bool AtLeast(DebugLogLevel level)
        {
            return (MassConsumeMod.Settings?.debugLevel ?? (int)DebugLogLevel.Off) >= (int)level;
        }

        /// <summary>Hot-path guard: check BEFORE building log strings so
        /// disabled logging costs nothing but a property read.</summary>
        public static bool VerboseEnabled => AtLeast(DebugLogLevel.Verbose);

        /// <summary>Basic-level message (player-visible actions and results).</summary>
        public static void Message(string message)
        {
            if (AtLeast(DebugLogLevel.Basic))
            {
                Verse.Log.Message(Prefix + message);
            }
        }

        /// <summary>Verbose-level message (per-frame scans, per-item decisions).</summary>
        public static void Verbose(string message)
        {
            if (AtLeast(DebugLogLevel.Verbose))
            {
                Verse.Log.Message(Prefix + message);
            }
        }

        /// <summary>Unconditional warning (degraded behavior, not a crash).</summary>
        public static void Warning(string message)
        {
            Verse.Log.Warning(Prefix + message);
        }
    }
}
