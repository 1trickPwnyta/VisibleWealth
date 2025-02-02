using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public abstract class WealthNode
    {
        public static Vector2 Size = new Vector2(600f, 30f);
        public static Vector2 IconSize = new Vector2(25f, 25f);
        public static float VerticalSpacing = 2f;
        public static Color Color = new Color(0.15f, 0.15f, 0.15f);
        public static float Indent = 20f;

        private int level;
        protected Map Map;

        public WealthNode(Map map, int level)
        {
            Map = map;
            this.level = level;
        }

        public abstract string Text { get; }

        public abstract IEnumerable<WealthNode> Children { get; }

        public abstract bool Visible { get; }

        public abstract float Value { get; }

        public abstract Texture2D Icon { get; }

        public void Draw(ref float y)
        {
            if (Visible)
            {
                Rect rect = new Rect(level * Indent, y, Size.x - level * Indent, Size.y);
                Widgets.DrawRectFast(rect, Color);
                Texture2D icon = Icon;
                float x = 0f;
                if (icon != null)
                {
                    Rect iconRect = new Rect(rect.x + x, rect.y + (rect.height - IconSize.y) / 2, IconSize.x, IconSize.y);
                    GUI.DrawTexture(iconRect, icon);
                    x += IconSize.x + 2f;
                }
                Rect labelRect = new Rect(rect.x + x, rect.y, rect.width - x, rect.height);
                Widgets.Label(labelRect, Text + " (" + ("$" + Value.ToString("F0")).Colorize(ColoredText.CurrencyColor) + ")");
                y += Size.y + VerticalSpacing;

                foreach (WealthNode node in Children)
                {
                    node.Draw(ref y);
                }
            }
        }
    }
}
