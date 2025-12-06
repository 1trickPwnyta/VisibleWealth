using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VisibleWealth
{
    public abstract class WealthNode : IPieFlavor
    {
        public static readonly float drawHeight = 30f;
        public static readonly Vector2 IconSize = new Vector2(25f, 25f);
        public static readonly float VerticalSpacing = 2f;
        public static readonly Color BackgroundColor = new Color(0.15f, 0.15f, 0.15f);
        public static readonly float Indent = 20f;

        private bool open;
        public readonly int level;
        public readonly Map map;
        public readonly WealthNode parent;
        public readonly bool isMapRoot;
        public readonly HashSet<Def> usedDefsOnMap = new HashSet<Def>();
        public Color chartColor;
        public Color chartColorBackup;

        public static IEnumerable<WealthNode> MakeRootNodes(Map map)
        {
            WealthNode[] roots = new[]
            {
                new WealthNode_WealthCategory(null, map, 0, WealthCategory.Items, true),
                new WealthNode_WealthCategory(null, map, 0, WealthCategory.Buildings, true),
                new WealthNode_WealthCategory(null, map, 0, WealthCategory.Pawns, true),
                new WealthNode_WealthCategory(null, map, 0, WealthCategory.PocketMaps)
            };
            return roots;
        }

        public static IEnumerable<WealthNode> GetAllNodes(IEnumerable<WealthNode> nodes)
        {
            foreach (WealthNode node in nodes)
            {
                foreach (WealthNode subNode in node.ThisAndAllChildren())
                {
                    yield return subNode;
                }
            }
        }

        public WealthNode(WealthNode parent, Map map, int level, bool isMapRoot = false)
        {
            this.parent = parent;
            this.map = map;
            this.level = level;
            this.isMapRoot = isMapRoot;
        }

        public bool Open
        {
            get
            {
                return open;
            }
            set
            {
                open = value;
                if (open)
                {
                    OnOpen();
                }
                else
                {
                    OnClose();
                }
            }
        }

        public bool IsLeafNode => Children.Count() == 0;

        public WealthNode MapRoot => isMapRoot ? this : parent?.MapRoot;

        public IEnumerable<WealthNode> LeafNodes
        {
            get 
            {
                List<WealthNode> leafNodes = new List<WealthNode>();
                if (IsLeafNode)
                {
                    leafNodes.Add(this);
                }
                else
                {
                    foreach (WealthNode node in Children.Where(n => n.Visible))
                    {
                        leafNodes.AddRange(node.LeafNodes);
                    }
                }
                return leafNodes;
            }
        }

        public IEnumerable<WealthNode> ThisAndAllChildren()
        {
            yield return this;
            foreach (WealthNode child in Children)
            {
                foreach (WealthNode node in child.ThisAndAllChildren())
                {
                    yield return node;
                }
            }
        }

        public bool MatchesSearch() => Visible && Dialog_WealthBreakdown.Search.filter.Matches(Text);

        public bool ThisOrAnyChildMatchesSearch()
        {
            if (MatchesSearch())
            {
                return true;
            }
            foreach (WealthNode node in Children)
            {
                if (node.ThisOrAnyChildMatchesSearch())
                {
                    return true;
                }
            }
            return false;
        }

        public abstract string Text { get; }

        public TaggedString GetLabel() => Text + " " + ("$" + Value.ToString("F0")).Colorize(ColoredText.CurrencyColor) + " " + VisibleWealthSettings.PercentOf.Text(this).Colorize(ColoredText.SubtleGrayColor);

        public abstract IEnumerable<WealthNode> Children { get; }

        public virtual bool SortChildren => true;

        public virtual float ValueFactor => 1f;

        public abstract bool Visible { get; }

        public abstract float RawValue { get; }

        public float Value => RawValue * ValueFactor;

        protected virtual Def InfoDef { get; }

        protected virtual Thing InfoThing { get; }

        protected virtual ThingDef InfoStuff { get; }

        public void ShowInfoCard() => Find.WindowStack.Add(InfoThing != null ? new Dialog_InfoCard(InfoThing) : InfoStuff != null ? new Dialog_InfoCard(InfoDef as ThingDef, InfoStuff) : new Dialog_InfoCard(InfoDef));

        public virtual void OnOpen() { }

        public virtual void OnClose() { }

        public virtual float DrawIcon(Rect rect) => 0f;

        public void Draw(float width, ref float y, bool nested = true)
        {
            if (ThisOrAnyChildMatchesSearch())
            {
                float nestedIndent = nested ? level * Indent : 0f;
                Rect rect = new Rect(nestedIndent, y, width - nestedIndent, drawHeight);
                Widgets.DrawRectFast(rect, BackgroundColor);
                float x = 0f;

                if (!IsLeafNode)
                {
                    Rect arrowRect = new Rect(rect.x + x, rect.y + (rect.height - IconSize.y) / 2, IconSize.x, IconSize.y);
                    if (Widgets.ButtonImage(arrowRect, Open ? TexButton.Collapse : TexButton.Reveal))
                    {
                        (Open ? SoundDefOf.TabClose : SoundDefOf.TabOpen).PlayOneShot(null);
                        Open = !Open;
                    }
                    x += IconSize.x + 2f;
                }

                Rect iconRect = new Rect(rect.x + x, rect.y + (rect.height - IconSize.y) / 2, IconSize.x, IconSize.y);
                x += DrawIcon(iconRect);

                Rect labelRect = new Rect(rect.x + x, rect.y, rect.width - x, rect.height);
                Verse.Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, GetLabel());
                Verse.Text.Anchor = TextAnchor.UpperLeft;

                if (InfoDef != null || InfoThing != null)
                {
                    Rect infoRect = new Rect(rect.xMax - IconSize.x, rect.y + (rect.height - IconSize.y) / 2, IconSize.x, IconSize.y);
                    if (Widgets.ButtonImage(infoRect, TexButton.Info))
                    {
                        ShowInfoCard();
                    }
                }

                y += drawHeight + VerticalSpacing;

                if (Open || Dialog_WealthBreakdown.Search.filter.Active)
                {
                    foreach (WealthNode node in SortChildren ? VisibleWealthSettings.SortBy.Sorted(Children, VisibleWealthSettings.SortAscending) : Children)
                    {
                        node.Draw(width, ref y);
                    }
                }
            }
        }
    }
}
