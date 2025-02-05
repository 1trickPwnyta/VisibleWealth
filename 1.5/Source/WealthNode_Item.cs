using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_Item : WealthNode
    {
        private readonly ThingDef def;
        private readonly int quantity;
        private readonly float value;

        public WealthNode_Item(Map map, int level, ThingDef def) : base(map, level)
        {
            this.def = def;
            List<Thing> things = new List<Thing>();
            ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForDef(def), things, false, new Predicate<IThingHolder>(WealthWatcher.WealthItemsFilter));
            List<Thing> minifiedThings = new List<Thing>();
            ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.MinifiedThing), minifiedThings, false, new Predicate<IThingHolder>(WealthWatcher.WealthItemsFilter));
            minifiedThings.RemoveAll(t => t.GetInnerIfMinified().def != def);
            things.AddRange(minifiedThings);
            things.RemoveAll(t => !t.SpawnedOrAnyParentSpawned || t.PositionHeld.Fogged(map) || !ThingRequestGroup.HaulableEver.Includes(t.def));
            quantity = things.Sum(t => t.stackCount);
            value = things.Sum(t => t.MarketValue * t.stackCount);
        }

        public override string Text => def.LabelCap + " x" + quantity;

        public override IEnumerable<WealthNode> Children => new List<WealthNode>();

        public override bool Visible => quantity > 0 && value > 0f;

        public override float Value => value;

        public override float DrawIcon(Rect rect)
        {
            Widgets.ThingIcon(rect, def);
            return IconSize.x + 2f;
        }

        public override Def InfoDef => def;
    }
}
