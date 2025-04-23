using System;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public enum PieStyle
    {
        Flat,
        Nested
    }

    [StaticConstructorOnStartup]
    public static class PieStyleUtility
    {
        private static readonly Texture2D flatPieIcon = ContentFinder<Texture2D>.Get("UI/Options/WealthBreakdown_FlatPie");
        private static readonly Texture2D nestedPieIcon = ContentFinder<Texture2D>.Get("UI/Options/WealthBreakdown_NestedPie");

        public static string GetLabel(this PieStyle pieStyle)
        {
            switch (pieStyle)
            {
                case PieStyle.Flat: return "VisibleWealth_FlatPie".Translate();
                case PieStyle.Nested: return "VisibleWealth_NestedPie".Translate();
                default: throw new NotImplementedException("Invalid pie style.");
            }
        }

        public static Texture2D GetIcon(this PieStyle pieStyle)
        {
            switch (pieStyle)
            {
                case PieStyle.Flat: return flatPieIcon;
                case PieStyle.Nested: return nestedPieIcon;
                default: throw new NotImplementedException("Invalid pie style.");
            }
        }
    }
}
