using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public enum SortBy
    {
        Name,
        Value
    }

    [StaticConstructorOnStartup]
    public static class SortByUtility
    {
        private static readonly Texture2D sortByNameIcon = ContentFinder<Texture2D>.Get("UI/Sort/WealthBreakdown_SortByName");
        private static readonly Texture2D sortByValueIcon = ContentFinder<Texture2D>.Get("UI/Sort/WealthBreakdown_SortByValue");

        public static readonly Texture2D SortDirectionIcon = ContentFinder<Texture2D>.Get("UI/Sort/WealthBreakdown_SortDirection");
        public static readonly string SortDirectionAscending = "VisibleWealth_SortDirectionAscending".Translate();
        public static readonly string SortDirectionDescending = "VisibleWealth_SortDirectionDescending".Translate();

        public static string GetLabel(this SortBy sortBy)
        {
            switch (sortBy)
            {
                case SortBy.Name: return "VisibleWealth_SortByName".Translate();
                case SortBy.Value: return "VisibleWealth_SortByValue".Translate();
                default: throw new NotImplementedException("Invalid sort by.");
            }
        }

        public static Texture2D GetIcon(this SortBy sortBy)
        {
            switch (sortBy)
            {
                case SortBy.Name: return sortByNameIcon;
                case SortBy.Value: return sortByValueIcon;
                default: throw new NotImplementedException("Invalid sort by.");
            }
        }

        public static IEnumerable<WealthNode> Sorted(this SortBy sortBy, IEnumerable<WealthNode> nodes, bool ascending)
        {
            switch (sortBy)
            {
                case SortBy.Name: return ascending ? nodes.OrderBy(n => n.Text) : nodes.OrderByDescending(n => n.Text);
                case SortBy.Value: return ascending ? nodes.OrderBy(n => n.Value) : nodes.OrderByDescending(n => n.Value);
                default: throw new NotImplementedException("Invalid sort by.");
            }
        }
    }
}
