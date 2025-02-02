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
        private ThingDef def;
        private int quantity;
        private float value;

        public WealthNode_Item(Map map, int level, ThingDef def) : base(map, level)
        {
            this.def = def;
            List<Thing> list = new List<Thing>();
            ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForDef(def), list, false, new Predicate<IThingHolder>(WealthWatcher.WealthItemsFilter));
            list.RemoveAll(t => !t.SpawnedOrAnyParentSpawned || t.PositionHeld.Fogged(map) || !ThingRequestGroup.HaulableEver.Includes(t.def));
            quantity = list.Sum(t => t.stackCount);
            value = list.Sum(t => t.MarketValue * t.stackCount);
        }

        public override string Text => def.LabelCap + " x" + quantity;

        public override IEnumerable<WealthNode> Children => new List<WealthNode>();

        public override bool Visible => quantity > 0 && value > 0f;

        public override float Value => value;

        public override Texture2D Icon => def.uiIcon;
    }
}
