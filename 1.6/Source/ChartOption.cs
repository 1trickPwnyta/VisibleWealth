using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VisibleWealth
{
    [StaticConstructorOnStartup]
    public abstract class ChartOption
    {
        private static readonly Texture2D RaidPointModeIcon = ContentFinder<Texture2D>.Get("UI/Options/WealthBreakdown_RaidPointMode");

        public abstract void DoOption(Rect rect);

        public static readonly ChartOption CollapseAll = new ChartOption_Button(() =>
        {
            foreach (WealthNode node in VisibleWealthSettings.ChartType.Worker.GetCollapsableRootNodes(Dialog_WealthBreakdown.Current.rootNodes))
            {
                node.Open = false;
            }
        }, () => VisibleWealthSettings.ChartType.Worker.GetCollapsableRootNodes(Dialog_WealthBreakdown.Current.rootNodes).Any(n => n.Open), "VisibleWealth_CollapseAll".Translate(), TexButton.Reveal);

        public static readonly ChartOption PercentOf = new ChartOption_Enum<PercentOf>(() => VisibleWealthSettings.PercentOf, option => VisibleWealthSettings.PercentOf = option, null, option => option.GetLabel(), option => option.GetIcon());

        public static readonly ChartOption RaidPointMode = new ChartOption_Toggle(() => VisibleWealthSettings.RaidPointMode, enabled => VisibleWealthSettings.RaidPointMode = enabled, "VisibleWealth_RaidPointMode".Translate(), RaidPointModeIcon);
    }

    public class ChartOption<T> : ChartOption
    {
        private readonly Func<T> optionGetter;
        private readonly Action<T> optionSetter;
        private readonly string optionName;
        private readonly Func<IEnumerable<T>> allOptionsGetter;
        private readonly Func<T, string> labelGetter;
        private readonly Func<T, Texture2D> iconGetter;

        public ChartOption(Func<T> optionGetter, Action<T> optionSetter, string optionName, Func<IEnumerable<T>> allOptionsGetter, Func<T, string> labelGetter, Func<T, Texture2D> iconGetter)
        {
            this.optionGetter = optionGetter;
            this.optionSetter = optionSetter;
            this.optionName = optionName;
            this.allOptionsGetter = allOptionsGetter;
            this.labelGetter = labelGetter;
            this.iconGetter = iconGetter;
        }

        public override void DoOption(Rect rect)
        {
            T option = optionGetter();
            if (Widgets.ButtonImage(rect, iconGetter(option), true, (optionName != null ? optionName + ": " : "") + labelGetter(option)))
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
        public ChartOption_Def(Func<T> optionGetter, Action<T> optionSetter, string optionName, Func<T, Texture2D> iconGetter) : base(optionGetter, optionSetter, optionName, () => DefDatabase<T>.AllDefsListForReading, def => def.LabelCap, iconGetter)
        {

        }
    }

    public class ChartOption_Enum<T> : ChartOption<T> where T : Enum
    {
        public ChartOption_Enum(Func<T> optionGetter, Action<T> optionSetter, string optionName, Func<T, string> labelGetter, Func<T, Texture2D> iconGetter) : base(optionGetter, optionSetter, optionName, () => (IEnumerable<T>)typeof(T).GetEnumValues(), labelGetter, iconGetter)
        {

        }
    }

    public class ChartOption_Button : ChartOption
    {
        private readonly Action action;
        private readonly Func<bool> enabledGetter;
        private readonly TaggedString tooltip;
        private readonly Texture2D icon;

        public ChartOption_Button(Action action, Func<bool> enabledGetter, TaggedString tooltip, Texture2D icon)
        {
            this.action = action;
            this.enabledGetter = enabledGetter;
            this.tooltip = tooltip;
            this.icon = icon;
        }

        public override void DoOption(Rect rect)
        {
            bool enabled = enabledGetter();
            if (Widgets.ButtonImage(rect, icon, enabled ? Color.white : Widgets.InactiveColor, enabled ? GenUI.MouseoverColor : Widgets.InactiveColor, enabled, tooltip))
            {
                if (enabled)
                {
                    action();
                    SoundDefOf.Click.PlayOneShot(null);
                }
                else
                {
                    SoundDefOf.ClickReject.PlayOneShot(null);
                }
            }
        }
    }

    public class ChartOption_Toggle : ChartOption
    {
        private readonly Func<bool> optionGetter;
        private readonly Action<bool> optionSetter;
        private readonly string optionName;
        private readonly Texture2D icon;

        public ChartOption_Toggle(Func<bool> optionGetter, Action<bool> optionSetter, string optionName, Texture2D icon)
        {
            this.optionGetter = optionGetter;
            this.optionSetter = optionSetter;
            this.optionName = optionName;
            this.icon = icon;
        }

        public override void DoOption(Rect rect)
        {
            bool option = optionGetter();
            if (Widgets.ButtonImage(rect, icon, true, optionName + ": " + (option ? "VisibleWealth_Enabled".Translate() : "VisibleWealth_Disabled".Translate())))
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
