using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_PawnCategory : WealthNode
    {
        private static readonly HashSet<PawnCategory> openCategories = new HashSet<PawnCategory>();

        private readonly PawnCategory category;
        private readonly List<WealthNode> subNodes;

        public WealthNode_PawnCategory(WealthNode parent, Map map, int level, PawnCategory category) : base(parent, map, level)
        {
            this.category = category;
            subNodes = new List<WealthNode>();
            if (category == PawnCategory.Human)
            {
                subNodes.AddRange(map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => category.Matches(p) && !p.IsQuestLodger()).Select(p => new WealthNode_Pawn(this, map, level + 1, p)));
            }
            else
            {
                subNodes.AddRange(DefDatabase<PawnKindDef>.AllDefsListForReading.Select(d => d.race).Distinct().Select(r => new WealthNode_PawnRace(this, map, level + 1, category, r)));
                if (category == PawnCategory.Mutant)
                {
                    subNodes.Add(new WealthNode_PawnRaceGhoul(this, map, level + 1));
                }
            }
            Open = openCategories.Contains(category);
        }

        public override string Text => category.Label() + " x" + LeafNodes.Count();

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => subNodes.Any(n => n.Visible);

        public override float Value => subNodes.Sum(n => n.Value);

        public override void OnOpen()
        {
            openCategories.Add(category);
        }

        public override void OnClose()
        {
            openCategories.Remove(category);
        }
    }
}
