using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public static class ColorUtility
    {
        public static float GetHue(this Color color)
        {
            Color.RGBToHSV(color, out float hue, out _, out _);
            return hue;
        }

        public static float GetSufficientlyDifferentHue(IEnumerable<float> hues, float minHueDiff)
        {
            List<FloatRange> forbiddenRanges = new List<FloatRange>();
            foreach (float hue in hues)
            {
                if (new FloatRange(0f, 1f).Includes(hue))
                {
                    FloatRange forbiddenRange = new FloatRange(hue - minHueDiff, hue + minHueDiff);
                    if (forbiddenRange.min < 0f)
                    {
                        forbiddenRanges.Add(new FloatRange(forbiddenRange.min + 1f, 1f));
                        forbiddenRange.min = 0f;
                    }
                    if (forbiddenRange.max > 1f)
                    {
                        forbiddenRanges.Add(new FloatRange(0f, forbiddenRange.max - 1f));
                        forbiddenRange.max = 1f;
                    }
                    forbiddenRanges.Add(forbiddenRange);
                }
            }

            float range = 1f - forbiddenRanges.Sum(r => r.max - r.min);
            float randomHue = Rand.Value * range;
            foreach (FloatRange forbiddenRange in forbiddenRanges.OrderBy(r => r.min))
            {
                if (forbiddenRange.Includes(randomHue))
                {
                    randomHue += forbiddenRange.max - forbiddenRange.min;
                }
            }

            while (randomHue > 1f)
            {
                randomHue -= 1f;
            }
            return randomHue;
        }
    }
}
