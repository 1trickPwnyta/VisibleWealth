using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_BuildingStuff : WealthNode
    {
        private readonly WealthNode_Building parentBuilding;
        private readonly ThingDef stuff;
        private readonly int quantity;
        private readonly float value;

        public WealthNode_BuildingStuff(WealthNode parent, Map map, int level, ThingDef stuff) : base(parent, map, level)
        {
            this.stuff = stuff;
            parentBuilding = parent as WealthNode_Building;
            List<Thing> list = map.listerThings.ThingsOfDef(parentBuilding.def).Where(b => b.Faction == Faction.OfPlayer && b.Stuff == stuff).ToList();
            quantity = list.Count;
            value = list.Sum(t => t.GetStatValue(StatDefOf.MarketValueIgnoreHp));
        }

        public override string Text => GenLabel.ThingLabel(parentBuilding.def, stuff).CapitalizeFirst() + " x" + quantity;

        public override IEnumerable<WealthNode> Children => new List<WealthNode>();

        public override bool Visible => quantity > 0 && value > 0f;

        public override float RawValue => value;

        public override float ValueFactor => VisibleWealthSettings.RaidPointMode ? 0.5f : 1f;

        public override float DrawIcon(Rect rect)
        {
            Widgets.ThingIcon(rect, parentBuilding.def, stuff);
            return IconSize.x + 2f;
        }

        protected override Def InfoDef => parentBuilding.def;

        protected override ThingDef InfoStuff => stuff;
    }
}
