using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_PawnRace : WealthNode
    {
        private static readonly HashSet<ThingDef> openDefs = new HashSet<ThingDef>();

        private readonly ThingDef def;
        private readonly List<WealthNode> subNodes = new List<WealthNode>();

        public WealthNode_PawnRace(WealthNode parent, Map map, int level, PawnCategory category, ThingDef def) : base(parent, map, level)
        {
            this.def = def;
            subNodes.AddRange(map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.def == def && category.Matches(p) && !p.IsGhoul && !p.IsQuestLodger()).Select(p => new WealthNode_Pawn(this, map, level + 1, p)));
            Open = openDefs.Contains(def);
        }

        public override string Text => def.LabelCap + " x" + LeafNodes.Count();

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => subNodes.Any(n => n.Visible);

        public override float RawValue => subNodes.Sum(n => n.Value);

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
