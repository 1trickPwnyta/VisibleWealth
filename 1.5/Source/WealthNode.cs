using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VisibleWealth
{
    public abstract class WealthNode
    {
        public static readonly Vector2 Size = new Vector2(600f, 30f);
        public static readonly Vector2 IconSize = new Vector2(25f, 25f);
        public static readonly float VerticalSpacing = 2f;
        public static readonly Color Color = new Color(0.15f, 0.15f, 0.15f);
        public static readonly float Indent = 20f;
        public static SortBy SortBy = SortBy.Value;
        public static bool SortAscending = false;

        private bool open;
        private readonly int level;
        protected readonly Map Map;

        protected bool Open
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

        public WealthNode(Map map, int level)
        {
            Map = map;
            this.level = level;
        }

        public IEnumerable<WealthNode> LeafNodes
        {
            get 
            {
                List<WealthNode> leafNodes = new List<WealthNode>();
                if (Children.Count() == 0)
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

        public abstract IEnumerable<WealthNode> Children { get; }

        public virtual bool SortChildren => true;

        public abstract bool Visible { get; }

        public abstract float Value { get; }

        public virtual Def InfoDef { get; }

        public virtual Thing InfoThing { get; }

        public virtual void OnOpen() { }

        public virtual void OnClose() { }

        public virtual float DrawIcon(Rect rect) => 0f;

        public void Draw(ref float y)
        {
            if (ThisOrAnyChildMatchesSearch())
            {
                Rect rect = new Rect(level * Indent, y, Size.x - level * Indent, Size.y);
                Widgets.DrawRectFast(rect, Color);
                float x = 0f;

                if (Children.Count() > 0)
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
                Widgets.Label(labelRect, Text + " (" + ("$" + Value.ToString("F0")).Colorize(ColoredText.CurrencyColor) + ")");
                Verse.Text.Anchor = TextAnchor.UpperLeft;

                if (InfoDef != null || InfoThing != null)
                {
                    Rect infoRect = new Rect(rect.xMax - IconSize.x, rect.y + (rect.height - IconSize.y) / 2, IconSize.x, IconSize.y);
                    if (Widgets.ButtonImage(infoRect, TexButton.Info))
                    {
                        Find.WindowStack.Add(InfoThing != null ? new Dialog_InfoCard(InfoThing) : new Dialog_InfoCard(InfoDef));
                    }
                }

                y += Size.y + VerticalSpacing;

                if (Open || Dialog_WealthBreakdown.Search.filter.Active)
                {
                    foreach (WealthNode node in SortChildren ? SortBy.Sorted(Children, SortAscending) : Children)
                    {
                        node.Draw(ref y);
                    }
                }
            }
        }
    }
}
