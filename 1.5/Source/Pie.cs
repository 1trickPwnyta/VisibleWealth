using System;
using System.Collections.Generic;

namespace VisibleWealth
{
    public interface IPieFlavor
    {
        float Value { get; }
    }

    public class Pie<T> where T : IPieFlavor
    {
        private List<Tuple<float, T>> slices = new List<Tuple<float, T>>();
        private float size = 0f;

        public Pie(IEnumerable<T> flavors)
        {
            foreach (T flavor in flavors)
            {
                size += flavor.Value;
                slices.Add(new Tuple<float, T>(size, flavor));
            }
        }

        public T GetSlice(float fraction)
        {
            int i = 0, lower = 0, upper = slices.Count - 1;
            while (lower != upper && lower != upper - 1)
            {
                if (slices[i].Item1 / size < fraction)
                {
                    lower = i;
                    i += (slices.Count - i) / 2;
                }
                else
                {
                    upper = i;
                    i -= (upper - lower) / 2;
                }
            }
            return slices[upper].Item2;
        }
    }
}
