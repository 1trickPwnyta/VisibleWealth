using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_Floor : WealthNode
    {
        public static Dictionary<Map, List<TerrainDef>> TerrainCache = new Dictionary<Map, List<TerrainDef>>();

        private static List<TerrainDef> GetTerrainCache(Map map)
        {
            if (!TerrainCache.ContainsKey(map))
            {
                TerrainCache[map] = map.AllCells.Where(c => !c.Fogged(map)).Select(c => c.GetTerrain(map)).ToList();
            }
            return TerrainCache[map];
        }

        private readonly TerrainDef def;
        private readonly int quantity;
        private readonly float value;

        public WealthNode_Floor(WealthNode parent, Map map, int level, TerrainDef def) : base(parent, map, level)
        {
            this.def = def;
            quantity = GetTerrainCache(map).Where(d => d == def).Count();
            value = quantity * ((float[])typeof(WealthWatcher).Field("cachedTerrainMarketValue").GetValue(map.wealthWatcher))[def.index];
        }

        public override string Text => def.LabelCap + " x" + quantity;

        public override IEnumerable<WealthNode> Children => new List<WealthNode>();

        public override bool Visible => quantity > 0 && value > 0f;

        public override float Value => value;

        public override float DrawIcon(Rect rect)
        {
            Widgets.DefIcon(rect, def);
            return IconSize.x + 2f;
        }

        public override Def InfoDef => def;
    }
}
