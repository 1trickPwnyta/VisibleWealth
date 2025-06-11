using System;
using Verse;

namespace VisibleWealth
{
    public enum WealthCategory
    {
        Items, 
        Buildings, 
        Pawns,
        PocketMaps
    }

    public static class WealthCategoryUtility
    {
        public static string Label(this WealthCategory category)
        {
            switch (category)
            {
                case WealthCategory.Items: return "VisibleWealth_WealthItems".Translate();
                case WealthCategory.Buildings: return "VisibleWealth_WealthBuildings".Translate();
                case WealthCategory.Pawns: return "VisibleWealth_WealthPawns".Translate();
                case WealthCategory.PocketMaps: return "VisibleWealth_WealthPocketMaps".Translate();
                default: throw new NotImplementedException("Invalid wealth category.");
            }
        }
    }
}
