namespace VisibleWealth
{
    public static class Debug
    {
        public static void Log(object message)
        {
#if DEBUG
            Verse.Log.Message($"[{VisibleWealthMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
