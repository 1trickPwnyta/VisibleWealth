using System;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class ChartDef : Def
    {
        private Texture2D iconTex;
        private ChartWorker worker;

        public string icon;
        public Type workerClass;

        public Texture2D Icon
        {
            get
            {
                if (iconTex == null)
                {
                    if (icon == null)
                    {
                        return null;
                    }
                    else
                    {
                        iconTex = ContentFinder<Texture2D>.Get(icon);
                    }
                }
                return iconTex;
            }
        }

        public ChartWorker Worker
        {
            get
            {
                if (worker == null)
                {
                    worker = (ChartWorker)Activator.CreateInstance(workerClass);
                }
                return worker;
            }
        }
    }
}
