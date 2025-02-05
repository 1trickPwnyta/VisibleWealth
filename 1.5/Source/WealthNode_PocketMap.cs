using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_PocketMap : WealthNode
    {
        private static readonly HashSet<Map> openMaps = new HashSet<Map>();

        private readonly Map pocketMap;
        private readonly List<WealthNode> subNodes;

        public WealthNode_PocketMap(Map map, int level, Map pocketMap) : base(map, level)
        {
            this.pocketMap = pocketMap;
            subNodes = new List<WealthNode>()
            {
                new WealthNode_WealthCategory(pocketMap, level + 1, WealthCategory.Items),
                new WealthNode_WealthCategory(pocketMap, level + 1, WealthCategory.Buildings),
                new WealthNode_WealthCategory(pocketMap, level + 1, WealthCategory.Pawns)
            };
            Open = openMaps.Contains(pocketMap);
        }

        public override string Text => pocketMap.generatorDef.LabelCap;

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool SortChildren => false;

        public override bool Visible => true;

        public override float Value => subNodes.Sum(n => n.Value);

        public override void OnOpen()
        {
            openMaps.Add(pocketMap);
        }

        public override void OnClose()
        {
            openMaps.Remove(pocketMap);
        }
    }
}
