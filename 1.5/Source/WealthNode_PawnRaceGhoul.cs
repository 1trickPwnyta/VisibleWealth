using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_PawnRaceGhoul : WealthNode
    {
        private static bool open = false;

        private readonly List<WealthNode> subNodes;

        public WealthNode_PawnRaceGhoul(Map map, int level) : base(map, level)
        {
            subNodes = new List<WealthNode>();
            subNodes.AddRange(map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.IsGhoul && !p.IsQuestLodger()).Select(p => new WealthNode_Pawn(map, level + 1, p)));
            Open = open;
        }

        public override string Text => "VisibleWealth_Ghouls".Translate() + " x" + LeafNodes.Count();

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => subNodes.Any(n => n.Visible);

        public override float Value => subNodes.Sum(n => n.Value);

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
