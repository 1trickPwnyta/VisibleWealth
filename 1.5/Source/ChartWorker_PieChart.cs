using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VisibleWealth
{
    public class ChartWorker_PieChart : ChartWorker
    {
        private class PieChart
        {
            public Texture2D tex;
            public float radius;
            public Pie<WealthNode> pie;
            public List<Tuple<WealthNode, float>> lowestOpenNodes;
            public List<WealthNode> leftLabels;
            public List<WealthNode> rightLabels;
        }

        private static readonly float radiusFactor = 0.4f;
        private static readonly float border = 1f;
        private static readonly float labelMargin = 50f;
        private static readonly Color transparent = new Color(0f, 0f, 0f, 0f);

        private static float iteratorFraction;
        private static WealthNode mouseOverNode;
        private static bool mouseOverLabel;

        private Dictionary<long, PieChart> cache = new Dictionary<long, PieChart>();

        public override void Draw(Rect outRect, Rect viewRect, ref float y, IEnumerable<WealthNode> rootNodes)
        {
            long state = GetTotalState(rootNodes);
            PieChart chart;
            if (!cache.ContainsKey(state))
            {
                int size = (int)GetSize(outRect);
                float radius = size * radiusFactor;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
                Pie<WealthNode> pie = new Pie<WealthNode>(Flatten(rootNodes));
                tex.SetPixelData(GetPixelData(size, radius, pie), 0);
                tex.filterMode = FilterMode.Point;
                tex.Apply(updateMipmaps: false);
                chart = new PieChart()
                {
                    tex = tex,
                    radius = radius,
                    pie = pie,
                    lowestOpenNodes = LowestOpenNodes(rootNodes)
                };
                ListsForLabels(pie, out chart.leftLabels, out chart.rightLabels);
                cache[state] = chart;
            }
            else
            {
                chart = cache[state];
            }

            Widgets.DrawTextureFitted(viewRect, chart.tex, 1f);
            DrawLabels(chart, outRect, viewRect);
            DrawButtons(chart, outRect, viewRect);

            y = outRect.height;
        }

        private void DrawButtons(PieChart chart, Rect outRect, Rect viewRect)
        {
            foreach (Tuple<WealthNode, float> node in chart.lowestOpenNodes)
            {
                Vector2 pos = GetVector(node.Item2, chart.radius + 15f);
                Rect rect = Rect.zero.ExpandedBy(15f);
                rect.position += GraphToUICoord(pos, outRect, viewRect);
                if (Mouse.IsOver(rect))
                {
                    GUI.color = GenUI.MouseoverColor;
                }
                Widgets.DrawTextureRotated(rect, TexButton.Reveal, 90f + node.Item2 * 360f);
                GUI.color = Color.white;
                TooltipHandler.TipRegion(rect, new TipSignal("VisibleWealth_ClickToCollapse".Translate(node.Item1.Text)));
                if (Widgets.ButtonInvisible(rect))
                {
                    node.Item1.Open = false;
                    SoundDefOf.TabClose.PlayOneShot(null);
                }
            }
        }

        private void DrawLabels(PieChart chart, Rect outRect, Rect viewRect)
        {
            Text.Font = GameFont.Tiny;
            float totalValue = chart.pie.TotalValue;
            mouseOverLabel = false;
            foreach (Tuple<float, WealthNode> slice in chart.pie.Slices)
            {
                WealthNode node = slice.Item2;
                bool isLeft = chart.leftLabels.Contains(node);
                bool isRight = chart.rightLabels.Contains(node);
                if (isLeft || isRight)
                {
                    float fraction = (slice.Item1 - slice.Item2.Value / 2f) / totalValue;
                    Rect textRect;
                    Vector2 startpoint;
                    TaggedString label = node.GetLabel();
                    float y;
                    float width = viewRect.width / 2f - chart.radius - labelMargin;
                    float height = Text.CalcHeight(label, width);
                    if (isLeft)
                    {
                        y = (chart.leftLabels.IndexOf(node) + 1f) / (chart.leftLabels.Count + 1f) * outRect.height;
                        textRect = new Rect(0f, y - height / 2f, width, height);
                        Text.Anchor = TextAnchor.MiddleRight;
                        startpoint = new Vector2(textRect.xMax + labelMargin / 4f, y);
                    }
                    else
                    {
                        y = (chart.rightLabels.IndexOf(node) + 1f) / (chart.rightLabels.Count + 1f) * outRect.height;
                        textRect = new Rect(viewRect.width / 2f + chart.radius + labelMargin, y - height / 2f, width, height);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        startpoint = new Vector2(textRect.x - labelMargin / 4f, y);
                    }
                    Vector2 endpoint;
                    float graphY = UIToGraphCoord(new Vector2(0, y), outRect, viewRect).y;
                    IEnumerable<float> edgePoints = EdgePoints(slice, graphY, chart);
                    if (edgePoints.Any())
                    {
                        endpoint = new Vector2(edgePoints.Average(), graphY);
                    }
                    else
                    {
                        endpoint = GetVector(fraction, chart.radius * 0.8f);
                    }
                    Widgets.Label(textRect, label);
                    Widgets.DrawHighlightIfMouseover(textRect);
                    if (!node.IsLeafNode)
                    {
                        TooltipHandler.TipRegionByKey(textRect, "VisibleWealth_ClickToExpand");
                    }
                    if (Mouse.IsOver(textRect))
                    {
                        if (mouseOverNode != node)
                        {
                            mouseOverNode = node;
                            SoundDefOf.Mouseover_Standard.PlayOneShot(null);
                        }
                        mouseOverLabel = true;
                    }
                    if (Widgets.ButtonInvisible(textRect, false))
                    {
                        if (!node.Open && !node.IsLeafNode)
                        {
                            node.Open = true;
                            SoundDefOf.TabOpen.PlayOneShot(null);
                        }
                    }
                    Vector2 uiEndpoint = GraphToUICoord(endpoint, outRect, viewRect);
                    Widgets.DrawLine(startpoint, uiEndpoint, Color.white, 1f);
                    Rect endRect = new Rect(uiEndpoint.x - 1f, uiEndpoint.y - 1f, 3f, 3f);
                    Widgets.DrawRectFast(endRect, Color.white);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            Text.Font = GameFont.Small;
        }

        public override void OnMouseOver(Vector2 pos, Rect outRect, Rect viewRect, IEnumerable<WealthNode> rootNodes)
        {
            long state = GetTotalState(rootNodes);
            if (cache.ContainsKey(state))
            {
                PieChart chart = cache[state];
                pos = UIToGraphCoord(pos, outRect, viewRect);
                if (pos.magnitude < chart.radius)
                {
                    float fraction = GetFraction(pos);
                    WealthNode node = chart.pie.GetSlice(fraction);
                    TipSignal tip = new TipSignal(node.GetLabel() + (!node.IsLeafNode ? "\n\n" + "VisibleWealth_ClickToExpand".Translate() : TaggedString.Empty));
                    TooltipHandler.TipRegion(new Rect(Event.current.mousePosition, Vector2.one), tip);
                    if (mouseOverNode != node)
                    {
                        mouseOverNode = node;
                        SoundDefOf.Mouseover_Standard.PlayOneShot(null);
                    }
                }
                else if (!mouseOverLabel)
                {
                    mouseOverNode = null;
                }
            }
        }

        public override void OnClick(Vector2 pos, Rect outRect, Rect viewRect, IEnumerable<WealthNode> rootNodes)
        {
            long state = GetTotalState(rootNodes);
            if (cache.ContainsKey(state))
            {
                PieChart chart = cache[state];
                pos = UIToGraphCoord(pos, outRect, viewRect);
                if (pos.magnitude < chart.radius)
                {
                    float fraction = GetFraction(pos);
                    WealthNode node = chart.pie.GetSlice(fraction);
                    if (!node.Open && !node.IsLeafNode)
                    {
                        node.Open = true;
                        SoundDefOf.TabOpen.PlayOneShot(null);
                    }
                }
            }
        }

        public override void Cleanup()
        {
            cache.Clear();
            Resources.UnloadUnusedAssets();
        }

        private byte[] GetPixelData(int size, float radius, Pie<WealthNode> pie)
        {
            byte[] bytes = new byte[size * size * 4];
            for (int i = 0, y = size - 1; y >= 0; y--)
            {
                for (int x = 0; x < size; x++, i += 4)
                {
                    Color c = GetColor(x, y, pie, size, radius);
                    bytes[i] = (byte)(c.r * 255);
                    bytes[i + 1] = (byte)(c.g * 255);
                    bytes[i + 2] = (byte)(c.b * 255);
                    bytes[i + 3] = (byte)(c.a * 255);
                }
            }
            return bytes;
        }

        private Color GetColor(int x, int y, Pie<WealthNode> pie, int size, float radius)
        {
            Vector2 pos = DataToGraphCoord(new Vector2(x, y), size);
            if (pos.magnitude < radius)
            {
                if ((pos.x == 0f && pos.y == 0f) || pos.magnitude > radius - border)
                {
                    return Color.white;
                }
                else
                {
                    float fraction = GetFraction(pos);
                    WealthNode node = pie.GetSlice(fraction);
                    return mouseOverNode == node ? node.chartColor.ClampToValueRange(new FloatRange(0.8f)) : node.chartColor;
                }
            }
            else
            {
                return transparent;
            }
        }

        private static float GetSize(Rect rect) => Mathf.Min(rect.width, rect.height);

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

        private static List<Tuple<WealthNode, float>> LowestOpenNodes(IEnumerable<WealthNode> nodes)
        {
            iteratorFraction = 0f;
            return LowestOpenNodes_Recursive(nodes).ToList();
        }

        private static IEnumerable<Tuple<WealthNode, float>> LowestOpenNodes_Recursive(IEnumerable<WealthNode> nodes)
        {
            float totalValue = nodes.ElementAt(0).map.wealthWatcher.WealthTotal;
            foreach (WealthNode node in nodes)
            {
                float nodeFraction = node.Value / totalValue;
                bool openChildren = false;
                if (node.Open)
                {
                    foreach (WealthNode child in node.Children)
                    {
                        if (child.Open)
                        {
                            openChildren = true;
                            break;
                        }
                    }
                    if (openChildren)
                    {
                        foreach (Tuple<WealthNode, float> child in LowestOpenNodes_Recursive(node.Children))
                        {
                            yield return child;
                        }
                    }
                    else
                    {
                        yield return new Tuple<WealthNode, float>(node, iteratorFraction + nodeFraction / 2f);
                    }
                }
                if (!openChildren)
                {
                    iteratorFraction += nodeFraction;
                }
            }
        }

        private static void ListsForLabels(Pie<WealthNode> pie, out List<WealthNode> left, out List<WealthNode> right)
        {
            float totalValue = pie.TotalValue;
            List<Tuple<float, WealthNode>> leftSlices = new List<Tuple<float, WealthNode>>();
            List<Tuple<float, WealthNode>> rightSlices = new List<Tuple<float, WealthNode>>();
            foreach (Tuple<float, WealthNode> slice in pie.Slices)
            {
                if (slice.Item2.Value > 0f)
                {
                    float fraction = (slice.Item1 - slice.Item2.Value / 2f) / totalValue;
                    if (fraction < 0.5f)
                    {
                        rightSlices.Add(slice);
                    }
                    else
                    {
                        leftSlices.Add(slice);
                    }
                }
            }

            if (leftSlices.Count > 6)
            {
                leftSlices.SortByDescending(s => s.Item2.Value);
                leftSlices.RemoveRange(6, leftSlices.Count - 6);
            }
            leftSlices.SortByDescending(s => s.Item1);
            left = leftSlices.Select(s => s.Item2).ToList();

            if (rightSlices.Count > 6)
            {
                rightSlices.SortByDescending(s => s.Item2.Value);
                rightSlices.RemoveRange(6, rightSlices.Count - 6);
            }
            rightSlices.SortBy(s => s.Item1);
            right = rightSlices.Select(s => s.Item2).ToList();
        }

        private static IEnumerable<float> EdgePoints(Tuple<float, WealthNode> slice, float y, PieChart chart)
        {
            float arc = chart.radius * chart.radius - y * y;
            if (arc >= 0f)
            {
                float sqrt = Mathf.Sqrt(arc);
                if (chart.pie.GetSlice(GetFraction(new Vector2(sqrt, y))) == slice.Item2)
                {
                    yield return sqrt;
                }
                if (chart.pie.GetSlice(GetFraction(new Vector2(-sqrt, y))) == slice.Item2)
                {
                    yield return -sqrt;
                }
            }
            if (y == 0f)
            {
                yield return 0f;
            }
            else
            {
                Vector2 lowerEdge = GetVector((slice.Item1 - slice.Item2.Value) / chart.pie.TotalValue, chart.radius);
                Vector2 upperEdge = GetVector(slice.Item1 / chart.pie.TotalValue, chart.radius);
                if (lowerEdge.y != 0f)
                {
                    float lowerX = y * lowerEdge.x / lowerEdge.y;
                    if (lowerEdge.y * y > 0f && new Vector2(lowerX, y).magnitude < chart.radius)
                    {
                        yield return lowerX;
                    }
                }
                if (upperEdge.y != 0f)
                {
                    float upperX = y * upperEdge.x / upperEdge.y;
                    if (upperEdge.y * y > 0f && new Vector2(upperX, y).magnitude < chart.radius)
                    {
                        yield return upperX;
                    }
                }
            }
        }

        private static Vector2 GraphToUICoord(Vector2 graphCoord, Rect outRect, Rect viewRect) => new Vector2(graphCoord.x + viewRect.width / 2f, outRect.height / 2f - graphCoord.y);

        private static Vector2 UIToGraphCoord(Vector2 uiCoord, Rect outRect, Rect viewRect) => new Vector2(uiCoord.x - viewRect.width / 2f, outRect.height / 2f - uiCoord.y);

        private static Vector2 DataToGraphCoord(Vector2 dataCoord, int size) => new Vector2(dataCoord.x - size / 2f, size / 2f - dataCoord.y);

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

        private static Vector2 GetVector(float fraction, float magnitude)
        {
            float angle = (fraction * -360f + 90f) / 57.2958f;
            float x, y;
            x = Mathf.Cos(angle) * magnitude;
            y = Mathf.Sin(angle) * magnitude;
            return new Vector2(x, y);
        }

        private static long GetTotalState(IEnumerable<WealthNode> nodes) => nodes.Sum(n => GetState(n));

        private static long GetState(WealthNode node)
        {
            int code = node.GetHashCode();
            long state = code * 2950 * (node.Open ? 1 : -1) + code * 790372 * (node == mouseOverNode ? 1 : -1);
            foreach (WealthNode child in node.Children)
            {
                state += GetState(child);
            }
            return state;
        }
    }
}
