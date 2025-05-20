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
            public Dictionary<int, HashSet<float>> siblingEdgeFractions;
            public List<Tuple<WealthNode, float>> lowestOpenNodes;
            public List<LabelData> leftLabels;
            public List<LabelData> rightLabels;
        }

        private class LabelData
        {
            public WealthNode node;
            public Rect textRect;
            public Vector2 lineStart;
            public Vector2 lineEnd;
        }

        private static readonly float radiusFactor = 0.4f;
        private static readonly float border = 1f;
        private static readonly float nestLevelThickness = 8f;
        private static readonly float labelMargin = 50f;
        private static readonly Color transparent = new Color(0f, 0f, 0f, 0f);
        private static readonly Color gray = new Color(0.25f, 0.25f, 0.25f);
        private static readonly IEnumerable<ChartOption> options = new ChartOption[]
        {
            new ChartOption_Enum<PieStyle>(() => VisibleWealthSettings.PieStyle, option => VisibleWealthSettings.PieStyle = option, "VisibleWealth_PieStyle".Translate(), option => option.GetLabel(), option => option.GetIcon()),
            ChartOption.RerollColors, 
            ChartOption.CollapseAll, 
            ChartOption.PercentOf,
            ChartOption.RaidPointMode
        };

        private static float iteratorFraction;
        private static WealthNode mouseOverNode;
        private static bool mouseOverLabel;

        private Dictionary<long, PieChart> cache = new Dictionary<long, PieChart>();

        public override IEnumerable<ChartOption> Options => options;

        public override void Draw(Rect outRect, Rect viewRect, ref float y, IEnumerable<WealthNode> rootNodes)
        {
            long state = GetState(rootNodes);
            PieChart chart;
            if (!cache.ContainsKey(state))
            {
                int size = (int)GetSize(outRect);
                float radius = size * radiusFactor;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
                Pie<WealthNode> pie = new Pie<WealthNode>(Flatten(rootNodes));
                chart = new PieChart()
                {
                    tex = tex,
                    radius = radius,
                    pie = pie,
                    siblingEdgeFractions = CalculateSiblingEdgeFractions(pie), 
                    lowestOpenNodes = LowestOpenNodes(rootNodes)
                };
                tex.SetPixelData(GetPixelData(size, radius, chart), 0);
                tex.filterMode = FilterMode.Point;
                tex.Apply(updateMipmaps: false);
                GetLabelData(chart, outRect, viewRect, out chart.leftLabels, out chart.rightLabels);
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
            if (!Dialog_WealthBreakdown.Search.filter.Active && VisibleWealthSettings.PieStyle == PieStyle.Flat)
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
        }

        private void DrawLabels(PieChart chart, Rect outRect, Rect viewRect)
        {
            Text.Font = GameFont.Tiny;
            mouseOverLabel = false;

            Action<LabelData> drawAction = data =>
            {
                Widgets.Label(data.textRect, data.node.GetLabel());
                TooltipHandler.TipRegionByKey(data.textRect, data.node.IsLeafNode ? "VisibleWealth_ClickForInfo" : "VisibleWealth_ClickToExpand");
                if (!Dialog_WealthBreakdown.Search.filter.Active)
                {
                    if (Mouse.IsOver(data.textRect))
                    {
                        if (mouseOverNode != data.node)
                        {
                            mouseOverNode = data.node;
                            SoundDefOf.Mouseover_Standard.PlayOneShot(null);
                        }
                        mouseOverLabel = true;
                    }
                    if (mouseOverNode == data.node)
                    {
                        Widgets.DrawHighlight(data.textRect);
                    }
                    if (Widgets.ButtonInvisible(data.textRect, false))
                    {
                        if (data.node.IsLeafNode)
                        {
                            data.node.ShowInfoCard();
                        }
                        else if (!data.node.Open)
                        {
                            data.node.Open = true;
                            SoundDefOf.TabOpen.PlayOneShot(null);
                        }
                    }
                }
                Widgets.DrawLine(data.lineStart, data.lineEnd, Color.white, 1f);
                Widgets.DrawRectFast(new Rect(data.lineEnd.x - 1f, data.lineEnd.y - 1f, 3f, 3f), Color.white);
            };

            Text.Anchor = TextAnchor.MiddleRight;
            foreach (LabelData data in chart.leftLabels)
            {
                drawAction(data);
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            foreach (LabelData data in chart.rightLabels)
            {
                drawAction(data);
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public override void OnMouseOver(Vector2 pos, Rect outRect, Rect viewRect, IEnumerable<WealthNode> rootNodes)
        {
            if (!Dialog_WealthBreakdown.Search.filter.Active)
            {
                long state = GetState(rootNodes);
                if (cache.ContainsKey(state))
                {
                    PieChart chart = cache[state];
                    pos = UIToGraphCoord(pos, outRect, viewRect);
                    if (pos.magnitude < chart.radius)
                    {
                        float fraction = GetFraction(pos);
                        WealthNode node = chart.pie.GetSlice(fraction);
                        bool originalNode = true;
                        if (VisibleWealthSettings.PieStyle == PieStyle.Nested)
                        {
                            WealthNode nestedParentNode = GetNestedNodeParent(node, chart.radius, pos.magnitude);
                            if (nestedParentNode != node)
                            {
                                node = nestedParentNode;
                                originalNode = false;
                            }
                        }
                        TaggedString tipText;
                        if (originalNode)
                        {
                            tipText = node.GetLabel();
                            if (!node.IsLeafNode)
                            {
                                tipText += "\n\n" + "VisibleWealth_ClickToExpand".Translate();
                            }
                            else
                            {
                                tipText += "\n\n" + "VisibleWealth_ClickForInfo".Translate();
                            }
                        }
                        else
                        {
                            tipText = "VisibleWealth_ClickToCollapse".Translate(node.Text);
                        }
                        TipSignal tip = new TipSignal(tipText);
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
            else
            {
                mouseOverNode = null;
            }
        }

        public override void OnMouseNotOver(IEnumerable<WealthNode> rootNodes)
        {
            mouseOverNode = null;
        }

        public override void OnClick(Vector2 pos, Rect outRect, Rect viewRect, IEnumerable<WealthNode> rootNodes)
        {
            if (!Dialog_WealthBreakdown.Search.filter.Active)
            {
                long state = GetState(rootNodes);
                if (cache.ContainsKey(state))
                {
                    PieChart chart = cache[state];
                    pos = UIToGraphCoord(pos, outRect, viewRect);
                    if (pos.magnitude < chart.radius)
                    {
                        float fraction = GetFraction(pos);
                        WealthNode node = chart.pie.GetSlice(fraction);
                        WealthNode nestedNodeParent = node;
                        if (VisibleWealthSettings.PieStyle == PieStyle.Nested)
                        {
                            nestedNodeParent = GetNestedNodeParent(node, chart.radius, pos.magnitude);
                        }
                        if (nestedNodeParent == node)
                        {
                            if (node.IsLeafNode)
                            {
                                node.ShowInfoCard();
                            }
                            else if (!node.Open)
                            {
                                node.Open = true;
                                SoundDefOf.TabOpen.PlayOneShot(null);
                            }
                        }
                        else
                        {
                            nestedNodeParent.Open = false;
                            SoundDefOf.TabClose.PlayOneShot(null);
                        }
                    }
                }
            }
        }

        public override void Cleanup()
        {
            cache.Clear();
            Resources.UnloadUnusedAssets();
        }

        private byte[] GetPixelData(int size, float radius, PieChart chart)
        {
            byte[] bytes = new byte[size * size * 4];
            for (int i = 0, y = size - 1; y >= 0; y--)
            {
                for (int x = 0; x < size; x++, i += 4)
                {
                    Color c = GetColor(x, y, chart, size, radius);
                    bytes[i] = (byte)(c.r * 255);
                    bytes[i + 1] = (byte)(c.g * 255);
                    bytes[i + 2] = (byte)(c.b * 255);
                    bytes[i + 3] = (byte)(c.a * 255);
                }
            }
            return bytes;
        }

        private Color GetColor(int x, int y, PieChart chart, int size, float radius)
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
                    WealthNode node = chart.pie.GetSlice(fraction);
                    if (Dialog_WealthBreakdown.Search.filter.Active)
                    {
                        return node.MatchesSearch() ? node.ChartColor : gray;
                    }
                    else
                    {
                        if (VisibleWealthSettings.PieStyle == PieStyle.Nested && pos.magnitude > radius - node.level * nestLevelThickness)
                        {
                            float rawLevel = GetNestLevel(radius, pos.magnitude);
                            int level = (int)rawLevel;
                            if (rawLevel - level > 0.9f || (chart.siblingEdgeFractions.ContainsKey(level) && chart.siblingEdgeFractions[level].Any(f => Mathf.Abs(fraction - f) < 0.0007f)))
                            {
                                return Color.white;
                            }
                            else
                            {
                                node = GetNestedNodeParent(node, radius, pos.magnitude);
                                return mouseOverNode == node ? GetHighlightColor(gray) : gray;
                            }
                        }
                        else
                        {
                            return mouseOverNode == node ? GetHighlightColor(node.ChartColor) : node.ChartColor;
                        }
                    }
                }
            }
            else
            {
                return transparent;
            }
        }

        private static float GetSize(Rect rect) => Mathf.Min(rect.width, rect.height);

        private static float GetNestLevel(float radius, float magnitude) => (radius - magnitude) / nestLevelThickness;

        private static Color GetHighlightColor(Color color)
        {
            Color.RGBToHSV(color, out _, out _, out float v);
            return color.ClampToValueRange(new FloatRange(Mathf.Min(v + 0.2f, 1f)));
        }

        private static WealthNode GetNestedNodeParent(WealthNode node, float radius, float magnitude)
        {
            int levelDiff = node.level - (int)GetNestLevel(radius, magnitude);
            for (int i = 0; i < levelDiff; i++)
            {
                node = node.parent;
            }
            return node;
        }

        private static IEnumerable<WealthNode> Flatten(IEnumerable<WealthNode> nodes)
        {
            foreach (WealthNode node in nodes)
            {
                if ((node.Open && !Dialog_WealthBreakdown.Search.filter.Active) || (Dialog_WealthBreakdown.Search.filter.Active && !node.MatchesSearch() && node.ThisOrAnyChildMatchesSearch()))
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
            float totalValue = Dialog_WealthBreakdown.Current.TotalWealth;
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

        private static void GetLabelData(PieChart chart, Rect outRect, Rect viewRect, out List<LabelData> left, out List<LabelData> right)
        {
            Text.Font = GameFont.Tiny;
            float totalValue = chart.pie.TotalValue;
            float labelWidth = viewRect.width / 2f - chart.radius - labelMargin;

            List<Tuple<float, WealthNode>> leftSlices = new List<Tuple<float, WealthNode>>();
            List<Tuple<float, WealthNode>> rightSlices = new List<Tuple<float, WealthNode>>();
            foreach (Tuple<float, WealthNode> slice in chart.pie.Slices)
            {
                if (slice.Item2.Value > 0f && (!Dialog_WealthBreakdown.Search.filter.Active || slice.Item2.MatchesSearch()))
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

            Func<Tuple<float, WealthNode>, float, float, Vector2> getLineEndAction = (slice, y, fraction) =>
            {
                float graphY = UIToGraphCoord(new Vector2(0, y), outRect, viewRect).y;
                IEnumerable<float> edgePoints = EdgePoints(slice, graphY, chart);
                if (edgePoints.Any(x => x != 0f))
                {
                    return GraphToUICoord(new Vector2(edgePoints.Average(), graphY), outRect, viewRect);
                }
                else
                {
                    return GraphToUICoord(GetVector(fraction, chart.radius * 0.8f), outRect, viewRect);
                }
            };

            if (leftSlices.Count > 6)
            {
                leftSlices.SortByDescending(s => s.Item2.Value);
                leftSlices.RemoveRange(6, leftSlices.Count - 6);
            }
            leftSlices.SortByDescending(s => s.Item1);

            left = new List<LabelData>();
            for (int i = 0; i < leftSlices.Count; i++)
            {
                Tuple<float, WealthNode> slice = leftSlices[i];
                float y = (i + 1f) / (leftSlices.Count + 1f) * outRect.height;
                float labelHeight = Text.CalcHeight(slice.Item2.GetLabel(), labelWidth);
                Rect textRect = new Rect(0f, y - labelHeight / 2f, labelWidth, labelHeight);
                float fraction = (slice.Item1 - slice.Item2.Value / 2f) / totalValue;
                left.Add(new LabelData()
                {
                    node = slice.Item2,
                    textRect = textRect,
                    lineStart = new Vector2(textRect.xMax + labelMargin / 4f, y),
                    lineEnd = getLineEndAction(slice, y, fraction)
                });
            }

            if (rightSlices.Count > 6)
            {
                rightSlices.SortByDescending(s => s.Item2.Value);
                rightSlices.RemoveRange(6, rightSlices.Count - 6);
            }
            rightSlices.SortBy(s => s.Item1);

            right = new List<LabelData>();
            for (int i = 0; i < rightSlices.Count; i++)
            {
                Tuple<float, WealthNode> slice = rightSlices[i];
                float y = (i + 1f) / (rightSlices.Count + 1f) * outRect.height;
                float labelHeight = Text.CalcHeight(slice.Item2.GetLabel(), labelWidth);
                Rect textRect = new Rect(viewRect.width / 2f + chart.radius + labelMargin, y - labelHeight / 2f, labelWidth, labelHeight);
                float fraction = (slice.Item1 - slice.Item2.Value / 2f) / totalValue;
                right.Add(new LabelData()
                {
                    node = slice.Item2,
                    textRect = textRect,
                    lineStart = new Vector2(textRect.x - labelMargin / 4f, y),
                    lineEnd = getLineEndAction(slice, y, fraction)
                });
            }

            Text.Font = GameFont.Small;
        }

        private static IEnumerable<float> EdgePoints(Tuple<float, WealthNode> slice, float y, PieChart chart)
        {
            float radius = chart.radius;
            if (VisibleWealthSettings.PieStyle == PieStyle.Nested)
            {
                radius -= slice.Item2.level * nestLevelThickness;
            }
            float arc = radius * radius - y * y;
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
                Vector2 lowerEdge = GetVector((slice.Item1 - slice.Item2.Value) / chart.pie.TotalValue, radius);
                Vector2 upperEdge = GetVector(slice.Item1 / chart.pie.TotalValue, radius);
                if (lowerEdge.y != 0f)
                {
                    float lowerX = y * lowerEdge.x / lowerEdge.y;
                    if (lowerEdge.y * y > 0f && new Vector2(lowerX, y).magnitude < radius)
                    {
                        yield return lowerX;
                    }
                }
                if (upperEdge.y != 0f)
                {
                    float upperX = y * upperEdge.x / upperEdge.y;
                    if (upperEdge.y * y > 0f && new Vector2(upperX, y).magnitude < radius)
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

        private static Dictionary<int, HashSet<float>> CalculateSiblingEdgeFractions(Pie<WealthNode> pie)
        {
            Dictionary<int, HashSet<float>> fractions = new Dictionary<int, HashSet<float>>();
            HashSet<WealthNode> visitedNodes = new HashSet<WealthNode>();
            foreach (Tuple<float, WealthNode> slice in pie.Slices)
            {
                CalculateSiblingEdgeFractionsRecursive(pie, slice.Item2, fractions, visitedNodes);
            }
            foreach (int level in fractions.Keys)
            {
                Debug.Log(level + ": " + string.Join(", ", fractions[level]));
            }
            return fractions;
        }

        private static void CalculateSiblingEdgeFractionsRecursive(Pie<WealthNode> pie, WealthNode node, Dictionary<int, HashSet<float>> fractions, HashSet<WealthNode> visitedNodes)
        {
            if (!visitedNodes.Contains(node))
            {
                if (!pie.Contains(node))
                {
                    if (!fractions.ContainsKey(node.level))
                    {
                        fractions[node.level] = new HashSet<float>() { 0f };
                    }
                    float effectiveFraction = GetEffectiveFraction(node, pie);
                    if (effectiveFraction < 1f)
                    {
                        fractions[node.level].Add(effectiveFraction);
                    }
                    fractions[node.level].Add(effectiveFraction - node.Value / pie.TotalValue);
                }
                if (node.parent != null)
                {
                    CalculateSiblingEdgeFractionsRecursive(pie, node.parent, fractions, visitedNodes);
                }
            }
            visitedNodes.Add(node);
        }

        private static float GetEffectiveFraction(WealthNode node, Pie<WealthNode> pie)
        {
            float? fraction = pie.GetFraction(node);
            if (fraction.HasValue)
            {
                return fraction.Value;
            }
            else if (!node.IsLeafNode)
            {
                return GetEffectiveFraction(node.Children.Last(), pie);
            }
            else
            {
                throw new ArgumentException("No descendant of node " + node.Text + " found in pie.");
            }
        }

        private static long GetState(IEnumerable<WealthNode> nodes) => nodes.Sum(n => GetNodeState(n)) + Dialog_WealthBreakdown.Search.filter.Text.GetHashCode() + (int)VisibleWealthSettings.PieStyle * 3678 + (VisibleWealthSettings.RaidPointMode ? -2172368 : 123);

        private static long GetNodeState(WealthNode node)
        {
            int code = node.GetHashCode();
            long state = code * 2950 * (node.Open ? 1 : -1) + code * 790372 * (node == mouseOverNode ? 1 : -1) + node.ChartColor.GetHashCode();
            foreach (WealthNode child in node.Children)
            {
                state += GetNodeState(child);
            }
            return state;
        }
    }
}
