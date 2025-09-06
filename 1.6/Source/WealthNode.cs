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

        public static IEnumerable<WealthNode> MakeRootNodes(Map map)
        {
            WealthNode[] roots = new[]
            {
                new WealthNode_WealthCategory(null, map, 0, WealthCategory.Items),
                new WealthNode_WealthCategory(null, map, 0, WealthCategory.Buildings),
                new WealthNode_WealthCategory(null, map, 0, WealthCategory.Pawns),
                new WealthNode_WealthCategory(null, map, 0, WealthCategory.PocketMaps)
            };
            ColorRoots(roots);
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

        public static void ColorRoots(IEnumerable<WealthNode> rootNodes)
        {
            List<WealthNode> nodes = rootNodes.Where(n => n.Visible).ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                WealthNode node = nodes[i];
                float hue, backupHue;
                if (i == 0)
                {
                    hue = ColorUtility.GetSufficientlyDifferentHue(new float[0], 0f);
                    backupHue = ColorUtility.GetSufficientlyDifferentHue(new[] { hue }, 0.05f);
                }
                else if (i < nodes.Count - 1)
                {
                    hue = ColorUtility.GetSufficientlyDifferentHue(new[]
                    {
                        nodes[i - 1].ChartColor.GetHue(),
                        nodes[i - 1].ChartColorBackup.GetHue()
                    }, 0.05f);
                    backupHue = ColorUtility.GetSufficientlyDifferentHue(new[]
                    {
                        nodes[i - 1].ChartColor.GetHue(),
                        nodes[i - 1].ChartColorBackup.GetHue(),
                        hue
                    }, 0.05f);
                }
                else
                {
                    hue = ColorUtility.GetSufficientlyDifferentHue(new[]
                    {
                        nodes[i - 1].ChartColor.GetHue(),
                        nodes[i - 1].ChartColorBackup.GetHue(),
                        nodes[0].ChartColor.GetHue(),
                        nodes[0].ChartColorBackup.GetHue()
                    }, 0.05f);
                    backupHue = ColorUtility.GetSufficientlyDifferentHue(new[]
                    {
                        nodes[i - 1].ChartColor.GetHue(),
                        nodes[i - 1].ChartColorBackup.GetHue(),
                        nodes[0].ChartColor.GetHue(),
                        nodes[0].ChartColorBackup.GetHue(),
                        hue
                    }, 0.05f);
                }
                node.SetChartColor(Color.HSVToRGB(hue, 0.3f + Rand.Value * 0.7f, 0.6f), Color.HSVToRGB(backupHue, 0.3f + Rand.Value * 0.7f, 0.6f));
            }
        }

        public WealthNode(WealthNode parent, Map map, int level)
        {
            this.parent = parent;
            this.map = map;
            this.level = level;
        }

        private void SetChartColor(Color color, Color colorBackup)
        {
            ChartColor = color;
            ChartColorBackup = colorBackup;

            List<WealthNode> children = Children.Where(c => c.Visible).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                WealthNode child = children[i];
                if (children.Count == 1)
                {
                    child.SetChartColor(ChartColorBackup, ChartColor);
                }
                else if (children.Count == 2)
                {
                    child.SetChartColor(i == 0 ? ChartColor : ChartColorBackup, i == 0 ? ChartColorBackup : ChartColor);
                }
                else if (i == 0 || i == children.Count - 1)
                {
                    child.SetChartColor(ChartColor, ChartColorBackup);
                }
                else
                {
                    float hue, backupHue;
                    if (i < children.Count - 2)
                    {
                        hue = ColorUtility.GetSufficientlyDifferentHue(new[]
                        {
                            children[i - 1].ChartColor.GetHue(),
                            children[i - 1].ChartColorBackup.GetHue()
                        }, 0.05f);
                        backupHue = ColorUtility.GetSufficientlyDifferentHue(new[]
                        {
                            children[i - 1].ChartColor.GetHue(),
                            children[i - 1].ChartColorBackup.GetHue(),
                            hue
                        }, 0.05f);
                    }
                    else
                    {
                        hue = ColorUtility.GetSufficientlyDifferentHue(new[]
                        {
                            children[i - 1].ChartColor.GetHue(),
                            children[i - 1].ChartColorBackup.GetHue(),
                            ChartColor.GetHue(),
                            ChartColorBackup.GetHue()
                        }, 0.05f);
                        backupHue = ColorUtility.GetSufficientlyDifferentHue(new[]
                        {
                            children[i - 1].ChartColor.GetHue(),
                            children[i - 1].ChartColorBackup.GetHue(),
                            ChartColor.GetHue(),
                            ChartColorBackup.GetHue(),
                            hue
                        }, 0.05f);
                    }
                    child.SetChartColor(Color.HSVToRGB(hue, 0.3f + Rand.Value * 0.7f, 0.6f), Color.HSVToRGB(backupHue, 0.3f + Rand.Value * 0.7f, 0.6f));
                }
            }
        }

        public Color ChartColor { get; private set; }

        public Color ChartColorBackup { get; private set; }

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
