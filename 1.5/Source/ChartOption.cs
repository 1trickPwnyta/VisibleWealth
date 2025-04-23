using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VisibleWealth
{
    public abstract class ChartOption
    {
        public abstract void DoOption(Rect rect);
    }

    public class ChartOption<T> : ChartOption
    {
        private Func<T> optionGetter;
        private Action<T> optionSetter;
        private Func<IEnumerable<T>> allOptionsGetter;
        private Func<T, string> labelGetter;
        private Func<T, Texture2D> iconGetter;

        public ChartOption(Func<T> optionGetter, Action<T> optionSetter, Func<IEnumerable<T>> allOptionsGetter, Func<T, string> labelGetter, Func<T, Texture2D> iconGetter)
        {
            this.optionGetter = optionGetter;
            this.optionSetter = optionSetter;
            this.allOptionsGetter = allOptionsGetter;
            this.labelGetter = labelGetter;
            this.iconGetter = iconGetter;
        }

        public override void DoOption(Rect rect)
        {
            T option = optionGetter();
            if (Widgets.ButtonImage(rect, iconGetter(option), true, labelGetter(option)))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (T choice in allOptionsGetter())
                {
                    options.Add(new FloatMenuOption(labelGetter(choice), () => optionSetter(choice), iconGetter(choice), Color.white));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }

    public class ChartOption_Def<T> : ChartOption<T> where T : Def
    {
        public ChartOption_Def(Func<T> optionGetter, Action<T> optionSetter, Func<T, Texture2D> iconGetter) : base(optionGetter, optionSetter, () => DefDatabase<T>.AllDefsListForReading, def => def.LabelCap, iconGetter)
        {

        }
    }

    public class ChartOption_Enum<T> : ChartOption<T> where T : Enum
    {
        public ChartOption_Enum(Func<T> optionGetter, Action<T> optionSetter, Func<T, string> labelGetter, Func<T, Texture2D> iconGetter) : base(optionGetter, optionSetter, () => (IEnumerable<T>)typeof(T).GetEnumValues(), labelGetter, iconGetter)
        {

        }
    }

    public class ChartOption_Toggle : ChartOption
    {
        private Func<bool> optionGetter;
        private Action<bool> optionSetter;
        private Func<bool, TaggedString> tooltipGetter;
        private Texture2D icon;

        public ChartOption_Toggle(Func<bool> optionGetter, Action<bool> optionSetter, Func<bool, TaggedString> tooltipGetter, Texture2D icon)
        {
            this.optionGetter = optionGetter;
            this.optionSetter = optionSetter;
            this.tooltipGetter = tooltipGetter;
            this.icon = icon;
        }

        public override void DoOption(Rect rect)
        {
            bool option = optionGetter();
            if (Widgets.ButtonImage(rect, icon, true, tooltipGetter(option)))
            {
                option = !option;
                optionSetter(option);
                (option ? SoundDefOf.Tick_High : SoundDefOf.Tick_Low).PlayOneShot(null);
            }
            Rect checkRect = new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height / 2);
            GUI.DrawTexture(checkRect, option ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
        }
    }
}
