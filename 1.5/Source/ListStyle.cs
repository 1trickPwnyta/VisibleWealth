using System;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public enum ListStyle
    {
        Flat,
        Nested
    }

    [StaticConstructorOnStartup]
    public static class ListStyleUtility
    {
        private static readonly Texture2D flatListIcon = ContentFinder<Texture2D>.Get("UI/Options/WealthBreakdown_FlatList");
        private static readonly Texture2D nestedListIcon = ContentFinder<Texture2D>.Get("UI/Options/WealthBreakdown_List");

        public static string GetLabel(this ListStyle listStyle)
        {
            switch (listStyle)
            {
                case ListStyle.Flat: return "VisibleWealth_Flat".Translate();
                case ListStyle.Nested: return "VisibleWealth_Nested".Translate();
                default: throw new NotImplementedException("Invalid list style.");
            }
        }

        public static Texture2D GetIcon(this ListStyle listStyle)
        {
            switch (listStyle)
            {
                case ListStyle.Flat: return flatListIcon;
                case ListStyle.Nested: return nestedListIcon;
                default: throw new NotImplementedException("Invalid list style.");
            }
        }
    }
}
