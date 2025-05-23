﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_PawnRaceGhoul : WealthNode
    {
        private static bool open = false;

        private readonly List<WealthNode> subNodes = new List<WealthNode>();

        public WealthNode_PawnRaceGhoul(WealthNode parent, Map map, int level) : base(parent, map, level)
        {
            subNodes.AddRange(map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.IsGhoul && !p.IsQuestLodger()).Select(p => new WealthNode_Pawn(this, map, level + 1, p)));
            Open = open;
        }

        public override string Text => "VisibleWealth_Ghouls".Translate() + " x" + LeafNodes.Count();

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => subNodes.Any(n => n.Visible);

        public override float RawValue => subNodes.Sum(n => n.Value);

        public override void OnOpen()
        {
            open = true;
        }

        public override void OnClose()
        {
            open = false;
        }
    }
}
