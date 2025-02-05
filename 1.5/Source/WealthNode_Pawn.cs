﻿using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_Pawn : WealthNode
    {
        private readonly Pawn pawn;
        private readonly float value;

        public WealthNode_Pawn(Map map, int level, Pawn pawn) : base(map, level)
        {
            this.pawn = pawn;
            value = pawn.MarketValue * (pawn.IsSlave ? 0.75f : 1f);
        }

        public override string Text => pawn.LabelCap;

        public override IEnumerable<WealthNode> Children => new List<WealthNode>();

        public override bool Visible => true;

        public override float Value => value;

        public override float DrawIcon(Rect rect)
        {
            Widgets.ThingIcon(rect, pawn);
            return IconSize.x + 2f;
        }

        public override Thing InfoThing => pawn;
    }
}
