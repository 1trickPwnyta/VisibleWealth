using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_Building : WealthNode
    {
        private readonly ThingDef def;
        private readonly int quantity;
        private readonly float value;

        public WealthNode_Building(WealthNode parent, Map map, int level, ThingDef def) : base(parent, map, level)
        {
            this.def = def;
            List<Thing> list = map.listerThings.ThingsOfDef(def).Where(b => b.Faction == Faction.OfPlayer).ToList();
            quantity = list.Count;
            value = list.Sum(t => t.GetStatValue(StatDefOf.MarketValueIgnoreHp));
            if (VisibleWealthSettings.RaidPointMode)
            {
                value *= 0.5f;
            }
        }

        public override string Text => def.LabelCap + " x" + quantity;

        public override IEnumerable<WealthNode> Children => new List<WealthNode>();

        public override bool Visible => quantity > 0 && value > 0f;

        public override float RawValue => value;

        public override float ValueFactor => VisibleWealthSettings.RaidPointMode ? 0.5f : 1f;

        public override float DrawIcon(Rect rect)
        {
            Widgets.ThingIcon(rect, def);
            return IconSize.x + 2f;
        }

        protected override Def InfoDef => def;
    }
}
