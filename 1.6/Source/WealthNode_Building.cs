using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_Building : WealthNode
    {
        private static readonly HashSet<ThingDef> openDefs = new HashSet<ThingDef>();

        public readonly ThingDef def;
        private readonly List<WealthNode> subNodes = new List<WealthNode>();
        private readonly int quantity;
        private readonly float value;

        public WealthNode_Building(WealthNode parent, Map map, int level, ThingDef def) : base(parent, map, level)
        {
            this.def = def;
            if (def.MadeFromStuff)
            {
                subNodes.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.stuffProps?.categories?.Intersect(def.stuffCategories).Any() == true).Select(s => new WealthNode_BuildingStuff(this, map, level + 1, s)));
                Open = openDefs.Contains(def);
            }
            else
            {
                List<Thing> list = map.listerThings.ThingsOfDef(def).Where(b => b.Faction == Faction.OfPlayer).ToList();
                quantity = list.Count;
                value = list.Sum(t => t.GetStatValue(StatDefOf.MarketValueIgnoreHp));
            }
        }

        public override string Text => def.LabelCap + (IsLeafNode ? " x" + quantity : string.Empty);

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => IsLeafNode ? quantity > 0 && value > 0f : subNodes.Any(n => n.Visible);

        public override float RawValue => IsLeafNode ? value : subNodes.Sum(n => n.Value);

        public override float ValueFactor => VisibleWealthSettings.RaidPointMode && IsLeafNode ? 0.5f : 1f;

        public override float DrawIcon(Rect rect)
        {
            Widgets.ThingIcon(rect, def);
            return IconSize.x + 2f;
        }

        protected override Def InfoDef => IsLeafNode ? def : null;

        public override void OnOpen()
        {
            openDefs.Add(def);
        }

        public override void OnClose()
        {
            openDefs.Remove(def);
        }
    }
}
