using Verse;

namespace KeepYourGear
{
    /// <summary>Optional per-event logging, off by default (see mod settings; dev mode only).</summary>
    internal static class DebugLog
    {
        private const string Prefix = "[KeepYourGear] ";

        public static void Log(string message)
        {
            if (GlobalState.Debug)
            {
                Verse.Log.Message(Prefix + message);
            }
        }
    }
}
