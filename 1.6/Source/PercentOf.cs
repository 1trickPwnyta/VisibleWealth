using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public enum PercentOf
    {
        None, 
        Category, 
        Total
    }

    [StaticConstructorOnStartup]
    public static class PercentOfUtility
    {
        private static readonly Texture2D PercentOfTotalIcon = ContentFinder<Texture2D>.Get("UI/Options/WealthBreakdown_PercentOfTotal");
        private static readonly Texture2D PercentOfCategoryIcon = ContentFinder<Texture2D>.Get("UI/Options/WealthBreakdown_PercentOfCategory");
        private static readonly Texture2D PercentOfNoneIcon = ContentFinder<Texture2D>.Get("UI/Options/WealthBreakdown_PercentOfNone");

        public static string GetLabel(this PercentOf percentOf)
        {
            switch (percentOf)
            {
                case PercentOf.Total: return "VisibleWealth_PercentOfTotal".Translate(); ;
                case PercentOf.Category: return "VisibleWealth_PercentOfCategory".Translate();
                case PercentOf.None: return "VisibleWealth_PercentOfNone".Translate();
                default: throw new NotImplementedException("Invalid percent of.");
            }
        }

        public static Texture2D GetIcon(this PercentOf percentOf)
        {
            switch (percentOf)
            {
                case PercentOf.Total: return PercentOfTotalIcon;
                case PercentOf.Category: return PercentOfCategoryIcon;
                case PercentOf.None: return PercentOfNoneIcon;
                default: throw new NotImplementedException("Invalid percent of.");
            }
        }

        public static string Text(this PercentOf percentOf, WealthNode node)
        {
            switch (percentOf)
            {
                case PercentOf.Total: return "(" + (node.Value / Dialog_WealthBreakdown.Current.TotalWealth * 100).ToString("F1") + "%)";
                case PercentOf.Category: return "(" + (node.Value / (node.parent?.Value ?? Dialog_WealthBreakdown.Current.TotalWealth) * 100).ToString("F0") + "%)";
                case PercentOf.None: return "";
                default: throw new NotImplementedException("Invalid percent of.");
            }
        }
    }
}
