using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace OrganicIdeology
{
    public class OrganicIdeology : Mod
    {
        public static OrganicSettings Settings;

        public OrganicIdeology(ModContentPack content) : base(content)
        {
            Settings = GetSettings<OrganicSettings>();
        }

        public override string SettingsCategory()
        {
            return "Organic Ideology";
        }

        private Vector2 scroll = Vector2.zero;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var bigotedTitleRec = new Rect(inRect.position, new Vector2(inRect.width, 24));

            Widgets.Label(bigotedTitleRec, "Bigoted Precepts");

            #region Bigoted Precepts Buttons

            var bigotedButtonSection = new Rect(inRect.position, new Vector2(200, 200));
            bigotedButtonSection.position += new Vector2(0, bigotedTitleRec.size.y);
            Widgets.DrawMenuSection(bigotedButtonSection);

            var buttonLayout = new Listing_Standard();
            buttonLayout.Begin(bigotedButtonSection);

            var buttonStart = new Rect(bigotedButtonSection);
            buttonStart.position = new Vector2(4, 4);
            buttonStart.size = new Vector2(200 - 8, 30);

            if (Widgets.ButtonText(buttonStart, "Add Precept"))
            {
                FloatMenuUtility.MakeMenu(DefDatabase<PreceptDef>.AllDefsListForReading, def => def.defName, def =>
                {
                    if (OrganicSettings.BigotDefs.Contains(def.defName))
                        return null;

                    return () => { OrganicSettings.BigotDefs.Add(def.defName); };
                });
            }

            buttonStart.position += new Vector2(0, 32);

            if (Widgets.ButtonText(buttonStart, "Add All"))
            {
                OrganicSettings.BigotDefs =
                    DefDatabase<PreceptDef>.AllDefsListForReading.Select(def => def.defName).ToList();
            }

            buttonStart.position += new Vector2(0, 32);

            if (Widgets.ButtonText(buttonStart, "Clear All"))
                OrganicSettings.BigotDefs.Clear();

            buttonStart.position += new Vector2(0, 32);

            if (Widgets.ButtonText(buttonStart, "Reset to Default"))
                OrganicSettings.ToDefault(true);

            buttonLayout.End();

            #endregion

            #region Bigoted Precepts List

            var bigotedListSection = new Rect(bigotedButtonSection);
            bigotedListSection.position += new Vector2(204, 0);
            bigotedListSection.size = new Vector2(inRect.width - 204, 200);
            Widgets.DrawMenuSection(bigotedListSection);

            var bigotedScrollSection = new Rect(bigotedListSection);
            bigotedScrollSection.position += new Vector2(4, 4);
            bigotedScrollSection.size -= new Vector2(8, 8);

            var bigotedInnerScroll = new Rect(bigotedScrollSection);
            bigotedInnerScroll.size -=
                new Vector2(OrganicSettings.BigotDefs.Count * 21f > 200 ? GenUI.ScrollBarWidth : 0, 0);
            bigotedInnerScroll.size = new Vector2(bigotedInnerScroll.size.x, OrganicSettings.BigotDefs.Count * 21f);

            Widgets.BeginScrollView(bigotedScrollSection, ref scroll, bigotedInnerScroll);
            var standardHelper = new Listing_Standard();
            standardHelper.Begin(bigotedInnerScroll);

            var tempList = OrganicSettings.BigotDefs.ToList();
            foreach (var defName in tempList)
            {
                standardHelper.SelectableDef(defName, false, () => { OrganicSettings.BigotDefs.Remove(defName); });
            }

            standardHelper.End();
            Widgets.EndScrollView();

            #endregion

            #region Various Settings Section

            var newSectionLabel = new Rect(bigotedButtonSection.position.x,
                bigotedButtonSection.y + bigotedButtonSection.height + 4, 250f, 22f);
            Text.Font = GameFont.Small;
            Widgets.Label(newSectionLabel, "Various Settings");

            var newSection = new Rect();
            newSection.position = newSectionLabel.position + Vector2.down * -newSectionLabel.size.y;
            newSection.size = new Vector2(inRect.xMax, 300f);

            Widgets.DrawMenuSection(newSection);

            var innerSection = new Rect(newSection);
            innerSection.position += new Vector2(4, 4);
            innerSection.size -= new Vector2(8, 8);

            var innerSectionStandard = new Listing_Standard();
            innerSectionStandard.Begin(innerSection);
            innerSectionStandard.Label("Precept Adopt Amount", -1f,
                "How many precepts will the ideology change incident generate.");
            innerSectionStandard.IntRange(ref OrganicSettings.PreceptRange, 1, 8);

            innerSectionStandard.CheckboxLabeled("Only allow changing of initial ideology",
                ref OrganicSettings.PlayerOnly,
                "Do you want limit the ideology changes to the initial-starting ideology? (In case it switches)");

            innerSectionStandard.End();

            #endregion
        }
    }
}