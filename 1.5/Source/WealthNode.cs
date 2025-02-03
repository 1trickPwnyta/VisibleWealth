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
        public static Vector2 Size = new Vector2(600f, 30f);
        public static Vector2 IconSize = new Vector2(25f, 25f);
        public static float VerticalSpacing = 2f;
        public static Color Color = new Color(0.15f, 0.15f, 0.15f);
        public static float Indent = 20f;

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

        public abstract bool Visible { get; }

        public abstract float Value { get; }

        public virtual Texture2D Icon { get; }

        public virtual Def InfoDef { get; }

        public virtual void OnOpen() { }

        public virtual void OnClose() { }

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

                Texture2D icon = Icon;
                if (icon != null)
                {
                    Rect iconRect = new Rect(rect.x + x, rect.y + (rect.height - IconSize.y) / 2, IconSize.x, IconSize.y);
                    GUI.DrawTexture(iconRect, icon);
                    x += IconSize.x + 2f;
                }

                Rect labelRect = new Rect(rect.x + x, rect.y, rect.width - x, rect.height);
                Verse.Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, Text + " (" + ("$" + Value.ToString("F0")).Colorize(ColoredText.CurrencyColor) + ")");
                Verse.Text.Anchor = TextAnchor.UpperLeft;

                if (InfoDef != null)
                {
                    Rect infoRect = new Rect(rect.xMax - IconSize.x, rect.y + (rect.height - IconSize.y) / 2, IconSize.x, IconSize.y);
                    if (Widgets.ButtonImage(infoRect, TexButton.Info))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(InfoDef));
                    }
                }

                y += Size.y + VerticalSpacing;

                if (Open || Dialog_WealthBreakdown.Search.filter.Active)
                {
                    foreach (WealthNode node in Children)
                    {
                        node.Draw(ref y);
                    }
                }
            }
        }
    }
}
