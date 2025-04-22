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
        private readonly int level;
        public readonly Map map;
        public readonly WealthNode parent;
        public readonly Color chartColor;

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

        public WealthNode(WealthNode parent, Map map, int level)
        {
            this.parent = parent;
            this.map = map;
            this.level = level;
            chartColor = Color.HSVToRGB(Rand.Value, 0.3f + Rand.Value * 0.7f, 0.6f);
        }

        public bool IsLeafNode => Children.Count() == 0;

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

        public bool ThisOrAnyChildMatchesSearch()
        {
            if (Visible && Dialog_WealthBreakdown.Search.filter.Matches(Text))
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

        public abstract bool Visible { get; }

        public abstract float Value { get; }

        public virtual Def InfoDef { get; }

        public virtual Thing InfoThing { get; }

        public virtual void OnOpen() { }

        public virtual void OnClose() { }

        public virtual float DrawIcon(Rect rect) => 0f;

        public void Draw(float width, ref float y)
        {
            if (ThisOrAnyChildMatchesSearch())
            {
                Rect rect = new Rect(level * Indent, y, width - level * Indent, drawHeight);
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
                        Find.WindowStack.Add(InfoThing != null ? new Dialog_InfoCard(InfoThing) : new Dialog_InfoCard(InfoDef));
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
