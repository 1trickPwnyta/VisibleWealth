using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class ChartWorker_PieChart : ChartWorker
    {
        private static readonly Color transparent = new Color(0f, 0f, 0f, 0f);

        private Texture2D tex;
        private long state;

        public override void Draw(Rect outRect, Rect viewRect, ref float y, IEnumerable<WealthNode> rootNodes)
        {
            long newState = rootNodes.Sum(n => n.GetState());
            if (newState != state)
            {
                tex = null;
            }
            if (tex == null)
            {
                state = newState;
                int diameter = (int)Mathf.Min(outRect.width, outRect.height);
                tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, true);
                tex.SetPixelData(GetPixelData(diameter, new Pie<WealthNode>(Flatten(rootNodes))), 0);
                tex.filterMode = FilterMode.Point;
                tex.Apply(updateMipmaps: false);
            }
            Widgets.DrawTextureFitted(viewRect, tex, 1f);
            y = outRect.height;
        }

        public override void OnClick(Vector2 pos)
        {
            // TODO
        }

        public override void Cleanup()
        {
            tex = null;
            Resources.UnloadUnusedAssets();
        }

        private byte[] GetPixelData(int diameter, Pie<WealthNode> pie)
        {
            byte[] bytes = new byte[diameter * diameter * 4];
            for (int i = 0, y = diameter - 1; y >= 0; y--)
            {
                for (int x = 0; x < diameter; x++, i += 4)
                {
                    Color c = GetColor(x, y, pie, diameter / 2f);
                    bytes[i] = (byte)(c.r * 255);
                    bytes[i + 1] = (byte)(c.g * 255);
                    bytes[i + 2] = (byte)(c.b * 255);
                    bytes[i + 3] = (byte)(c.a * 255);
                }
            }
            return bytes;
        }

        private Color GetColor(int x, int y, Pie<WealthNode> pie, float radius)
        {
            Vector2 pos = new Vector2(x - radius, radius - y);
            if (pos.magnitude < radius)
            {
                if (pos.x == 0f && pos.y == 0f)
                {
                    return Color.black;
                }
                else
                {
                    float fraction = GetFraction(pos);
                    return pie.GetSlice(fraction).chartColor;
                }
            }
            else
            {
                return transparent;
            }
        }

        private static IEnumerable<WealthNode> Flatten(IEnumerable<WealthNode> nodes)
        {
            foreach (WealthNode node in nodes)
            {
                if (node.Open)
                {
                    foreach (WealthNode child in Flatten(node.Children))
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return node;
                }
            }
        }

        private static float GetFraction(Vector2 pos)
        {
            float f;
            if (pos.x == 0f)
            {
                f = pos.y > 0f ? 0f : 180f;
            }
            else if (pos.y == 0f)
            {
                f = pos.x > 0f ? 90f : 270f;
            }
            else
            {
                float acos = Mathf.Acos(Mathf.Abs(pos.x) / pos.magnitude) * 57.2958f;
                if (pos.x > 0f)
                {
                    if (pos.y > 0f)
                    {
                        f = 90f - acos;
                    }
                    else
                    {
                        f = 90f + acos;
                    }
                }
                else
                {
                    if (pos.y > 0f)
                    {
                        f = 270f + acos;
                    }
                    else
                    {
                        f = 270f - acos;
                    }
                }
            }
            return f / 360f;
        }
    }
}
