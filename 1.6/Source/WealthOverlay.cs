using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthOverlay : ICellBoolGiver
    {
        private static readonly Dictionary<Map, WealthOverlay> overlays = new Dictionary<Map, WealthOverlay>();
        private static readonly float radius = 8.9f;
        private static bool visible;
        private static float threshold;
        private static bool thresholdChange;

        public static bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                if (visible)
                {
                    if (VisibleWealthSettings.WealthOverlayPause)
                    {
                        Current.Game.tickManager.Pause();
                    }
                    SetAllDirty();
                }
            }
        }

        public static void RegisterMap(Map map)
        {
            if (!overlays.ContainsKey(map))
            {
                overlays[map] = new WealthOverlay(map);
            }
        }

        public static void SetAllDirty()
        {
            foreach (WealthOverlay overlay in overlays.Values)
            {
                overlay.SetDirty();
            }
        }

        public static WealthOverlay ForMap(Map map) => overlays.ContainsKey(map) ? overlays[map] : null;

        public static void DoGlobalControls(float leftX, float width, ref float curBaseY)
        {
            if (Visible && Find.CurrentMap != null)
            {
                Rect rect = new Rect(leftX, curBaseY - 24f, width, 24f);
                string label = "VisibleWealth_WealthOverlayThreshold".Translate();
                Rect labelRect = rect.LeftPartPixels(Text.CalcSize(label).x);
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, label);
                Rect sliderRect = rect.RightPartPixels(rect.width - labelRect.width - 5f);
                float newThreshold = Widgets.HorizontalSlider(sliderRect, threshold, 0f, ForMap(Find.CurrentMap).maxWealth - 1f, label: threshold.ToStringMoney(), roundTo: 1f);
                if (newThreshold != threshold)
                {
                    threshold = newThreshold;
                    thresholdChange = true;
                }
                if (Event.current.type == EventType.MouseUp)
                {
                    if (thresholdChange)
                    {
                        SetAllDirty();
                    }
                    thresholdChange = false;
                }
                curBaseY -= rect.height + 8f;
            }
        }

        private Map map;
        private CellBoolDrawer drawer;
        private bool dirty = true;
        private float[] wealthAt;
        private float maxWealth;

        public Color Color => Color.white;

        public WealthOverlay(Map map)
        {
            this.map = map;
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.9f);
            wealthAt = new float[map.Size.x * map.Size.z];
        }

        public void WealthOverlayOnGUI()
        {
            if (Visible && Event.current.type == EventType.Repaint && !Mouse.IsInputBlockedNow)
            {
                IntVec3 mouseCell = UI.MouseCell();
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(mouseCell, radius, true))
                {
                    if (cell.InBounds(map) && !cell.Fogged(map))
                    {
                        int index = map.cellIndices.CellToIndex(cell);
                        if (wealthAt[index] > 0f)
                        {
                            GenMapUI.DrawThingLabel((Vector3)GenMapUI.LabelDrawPosFor(cell), wealthAt[index].ToStringMoney(), Color.white);
                        }
                    }
                }
            }
        }

        public void WealthOverlayUpdate()
        {
            if (Visible && !Find.ScreenshotModeHandler.Active)
            {
                if (dirty)
                {
                    RecalculateWealth();
                }
                drawer.MarkForDraw();
                threshold = Mathf.Clamp(threshold, 0f, maxWealth - 1f);
                drawer.CellBoolDrawerUpdate();
            }
        }

        public void SetDirty()
        {
            drawer.SetDirty();
            dirty = true;
        }

        private IEnumerable<TerrainDef> TerrainAt(int index)
        {
            TerrainDef terrain = map.terrainGrid.TopTerrainAt(index);
            if (terrain != null)
            {
                yield return terrain;
            }
            terrain = map.terrainGrid.FoundationAt(index);
            if (terrain != null)
            {
                yield return terrain;
            }
        }

        private void RecalculateWealth()
        {
            for (int i = 0; i < wealthAt.Length; i++)
            {
                if (!map.fogGrid.IsFogged(i))
                {
                    List<Thing> things = map.thingGrid.ThingsListAtFast(i);
                    List<Thing> items = new List<Thing>();
                    List<Pawn> pawns = new List<Pawn>();
                    List<Building> buildings = new List<Building>();
                    foreach (Thing thing in things)
                    {
                        if (thing is Pawn pawn)
                        {
                            if (pawn.Faction != null && pawn.Faction.IsPlayer && !pawn.IsQuestLodger())
                            {
                                pawns.Add(pawn);
                            }
                        }
                        else if (thing is Building building)
                        {
                            if (building.Faction != null && building.Faction.IsPlayer)
                            {
                                buildings.Add(building);
                            }
                        }
                        else
                        {
                            items.Add(thing);
                        }
                    }
                    wealthAt[i] = items.Sum(t => t.MarketValue * t.stackCount)
                        + pawns.Sum(p => p.MarketValue)
                        + buildings.Sum(b => b.MarketValue)
                        + TerrainAt(i).Sum(t => ((float[])typeof(WealthWatcher).Field("cachedTerrainMarketValue").GetValue(map.wealthWatcher))[t.index]);
                }
                else
                {
                    wealthAt[i] = 0f;
                }
            }
            maxWealth = wealthAt.Max();
            dirty = false;
        }

        public bool GetCellBool(int index) => wealthAt[index] > threshold;

        public Color GetCellExtraColor(int index) => ColoredText.CurrencyColor.WithAlpha(Mathf.Lerp(0.15f, 1f, (wealthAt[index] - threshold) / (maxWealth - threshold)));
    }
}
