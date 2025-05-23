﻿using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_ResourceCategory : WealthNode
    {
        private static readonly HashSet<ThingCategoryDef> openCategories = new HashSet<ThingCategoryDef>();

        private readonly ThingCategoryDef def;
        private readonly List<WealthNode> subNodes = new List<WealthNode>();

        public WealthNode_ResourceCategory(WealthNode parent, Map map, int level, ThingCategoryDef def) : base(parent, map, level)
        {
            this.def = def;
            subNodes.AddRange(def.childCategories.Select(d => new WealthNode_ResourceCategory(this, map, level + 1, d)));
            subNodes.AddRange(def.childThingDefs.Select(d => new WealthNode_Item(this, map, level + 1, d)));
            Open = openCategories.Contains(def);
        }

        public override string Text => def.LabelCap;

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => subNodes.Any(n => n.Visible);

        public override float RawValue => subNodes.Sum(n => n.Value);

        public override void OnOpen()
        {
            openCategories.Add(def);
        }

        public override void OnClose()
        {
            openCategories.Remove(def);
        }
    }
}
