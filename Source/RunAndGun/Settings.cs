using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RunAndGun.Utilities;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RunAndGun
{

    public class Settings : ModSettings
    {
        // === Stored settings ===
        public bool dialogCEShown = false;
        public bool enabledByDefault = true;
        public bool enableForAI = true;
        public int enableForFleeChance = 100;
        public int accuracyPenalty = 10;
        public int movementPenaltyHeavy = 40;
        public int movementPenaltyLight = 10;
        public string tabsHandler = "none";
        public float weightLimitFilter = 3.4f;
        public Dictionary<string, WeaponRecord> selectedWeapons = new Dictionary<string, WeaponRecord>();
        public Dictionary<string, WeaponRecord> forbiddenWeapons = new Dictionary<string, WeaponRecord>();

        public readonly bool DualWieldInstalled;

        private SettingsTab tab;
        private QuickSearchWidget search = new QuickSearchWidget();


        // === Cached state ===
        public List<ThingDef> allWeapons;
        private float maxWeightMelee, maxWeightRanged, maxWeightTotal;

        public Settings()
        {
            DualWieldInstalled = ModLister.AnyModActiveNoSuffix(["MemeGoddess.DualWield"]);
        }

        public void Initialize()
        {
            allWeapons = WeaponUtility.getAllWeapons();
            WeaponUtility.getHeaviestWeapons(allWeapons, out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);

            bool combatExtendedLoaded = AssemblyExists("CombatExtended");
            if (combatExtendedLoaded && !dialogCEShown)
            {
                Find.WindowStack.Add(new Dialog_CE("RG_Dialog_CE_Title".Translate(), "RG_Dialog_CE_Description".Translate()));
                dialogCEShown = true;
            }
            else if (!combatExtendedLoaded)
            {
                dialogCEShown = false;
            }

            if (selectedWeapons == null)
                DrawUtility.FilterWeapons(ref selectedWeapons, allWeapons, weightLimitFilter);
            if(forbiddenWeapons == null)
                DrawUtility.FilterWeapons(ref forbiddenWeapons, allWeapons);
        }

        public void DoWindowContents(Rect rect)
        {
            Initialize();

            var listing = new Listing_Standard();
            listing.Begin(rect);

            // === General Settings ===
            listing.CheckboxLabeled("RG_EnableRGForColonists_Title".Translate(), ref enabledByDefault, "RG_EnableRGForColonists_Description".Translate());
            listing.CheckboxLabeled("RG_EnableRGForAI_Title".Translate(), ref enableForAI, "RG_EnableRGForAI_Description".Translate());
            var box = listing.GetRect((22f + Text.LineHeight + listing.verticalSpacing + listing.verticalSpacing) * 2);
            if (enableForAI)
            {
                var AISettings = new Listing_Standard();
                box.SplitVerticallyWithMargin(out var aiListingBox, out box, 6f);
                AISettings.Begin(aiListingBox);
                AISettings.Label("RG_EnableRGForFleeChance_Title".Translate() + ": " + enableForFleeChance + "%");
                enableForFleeChance = (int)Widgets.HorizontalSlider(AISettings.GetRect(22f), enableForFleeChance, 0, 100, false, "");

                AISettings.Label("RG_AccuracyPenalty_Title".Translate() + ": " + accuracyPenalty + "%");
                accuracyPenalty = (int)Widgets.HorizontalSlider(AISettings.GetRect(22f), accuracyPenalty, 0, 100, false, "");
                AISettings.End();
            }

            // === Movement Penalties ===
            var movementSettings = new Listing_Standard();
            movementSettings.Begin(box);

            movementSettings.Label("RG_MovementPenaltyHeavy_Title".Translate() + ": " + movementPenaltyHeavy + "%");
            movementPenaltyHeavy = (int)Widgets.HorizontalSlider(movementSettings.GetRect(22f), movementPenaltyHeavy, 0, 100, false, "");

            movementSettings.Label("RG_MovementPenaltyLight_Title".Translate() + ": " + movementPenaltyLight + "%");
            movementPenaltyLight = (int)Widgets.HorizontalSlider(movementSettings.GetRect(22f), movementPenaltyLight, 0, 100, false, "");
            movementSettings.End();

            listing.GapLine();

            // === Tabs ===
            listing.Label("RG_Tabs_Title".Translate());
            
            //var tabs = new Listing_Standard();
            var tabRect = listing.GetRect(Text.LineHeight);

            var tabsList = new List<TabRecord>
            {
                new("RG_tab1".Translate(), () => tab = SettingsTab.Heavy, () => tab == SettingsTab.Heavy),
                new("RG_tab2".Translate(), () => tab = SettingsTab.Forbidden, () => tab == SettingsTab.Forbidden)

            };

            DrawTabs(tabRect, tabsList);

            // === Filters and Custom UI ===
            float remainingHeight;
            switch (tab)
            {
                case SettingsTab.Heavy:
                    listing.GapLine();
                    listing.Label("RG_WeightLimitFilter_Title".Translate() + $" ({weightLimitFilter:F1})");
                    listing.Gap(4f);
                    weightLimitFilter = Widgets.HorizontalSlider(listing.GetRect(22f), weightLimitFilter, 0f, maxWeightTotal, false, "", "0", maxWeightTotal.ToString("F1"));

                    search.OnGUI(listing.GetRect(30f));
                    DrawUtility.CustomDrawer_MatchingWeapons_active(listing.GetRect(253f), ref selectedWeapons,
                        allWeapons.Where(weapon => 
                            search.filter.Matches(weapon.label) ||
                            search.filter.Matches(weapon.defName)
                            ).ToList(), 
                        weightLimitFilter, "RG_ConsideredLight".Translate(), "RG_ConsideredHeavy".Translate());
                    break;
                case SettingsTab.Forbidden:
                    listing.GapLine();
                    search.OnGUI(listing.GetRect(30f));
                    remainingHeight = rect.height - listing.CurHeight;
                    DrawUtility.CustomDrawer_MatchingWeapons_active(listing.GetRect(remainingHeight), ref forbiddenWeapons,
                        allWeapons.Where(weapon =>
                            search.filter.Matches(weapon.label) ||
                            search.filter.Matches(weapon.defName)
                        ).ToList(), 
                        null, "RG_Allow".Translate(), "RG_Forbid".Translate());
                    break;
            }

            listing.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref dialogCEShown, nameof(dialogCEShown), false);
            Scribe_Values.Look(ref enableForAI, nameof(enableForAI), true);
            Scribe_Values.Look(ref enableForFleeChance, nameof(enableForFleeChance), 100);
            Scribe_Values.Look(ref accuracyPenalty, nameof(accuracyPenalty), 10);
            Scribe_Values.Look(ref movementPenaltyHeavy, nameof(movementPenaltyHeavy), 40);
            Scribe_Values.Look(ref movementPenaltyLight, nameof(movementPenaltyLight), 10);
            Scribe_Values.Look(ref tabsHandler, nameof(tabsHandler), "none");
            Scribe_Values.Look(ref weightLimitFilter, nameof(weightLimitFilter), 3.4f);

            Scribe_Collections.Look(ref selectedWeapons, nameof(selectedWeapons), LookMode.Value);
            Scribe_Collections.Look(ref forbiddenWeapons, nameof(forbiddenWeapons), LookMode.Value);

            if (selectedWeapons == null)
                selectedWeapons = new Dictionary<string, WeaponRecord>();

            if (forbiddenWeapons == null)
                forbiddenWeapons = new Dictionary<string, WeaponRecord>();
            
            base.ExposeData();
        }

        private bool AssemblyExists(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.FullName.StartsWith(assemblyName))
                    return true;
            return false;
        }

        //private void DoTabClick(SettingsTab selectedTab)
        //{
        //    search = new QuickSearchWidget();

        //    tab = selectedTab == tab 
        //        ? SettingsTab.None 
        //        : selectedTab;
        //}

        private static Color SelectedColor = new Color(0.5f, 1f, 0.5f, 1f);
        private void DrawTabs(Rect rect, List<TabRecord> tabs)
        {
            var buttons = tabs.Count;
            var rects = SplitRectangle(rect, buttons, 4f);

            var color = GUI.color;
            for (var index = 0; index < rects.Length; index++)
            {
                var button = rects[index];
                var tab = tabs[index];

                if (tab.Selected)
                    GUI.color = SelectedColor;
                if (Widgets.ButtonText(button, tab.label))
                    tab.clickedAction();
                GUI.color = color;
            }
        }

        private Rect[] SplitRectangle(Rect rect, int count, float margin)
        {
            var rects = new Rect[count];
            var totalMargin = margin * (count - 1);
            var usableWidth = rect.width - totalMargin;
            var rectWidth = usableWidth / count;

            for (var i = 0; i < count; i++)
            {
                var xPosition = rect.x + (i * (rectWidth + margin));
                rects[i] = new Rect(xPosition, rect.y, rectWidth, rect.height);
            }

            return rects;
        }
    }

    public enum SettingsTab
    {
        Heavy,
        Forbidden
    }

    public static class SettingsExtensions
    {
        private static Color SelectedColor = new Color(0.5f, 1f, 0.5f, 1f);
        public static float Button(this Listing_Standard listing, string label, bool active, Action action)
        {
            var original = GUI.color;
            GUI.color = active ? SelectedColor : original;
            if (listing.ButtonText(label))
                action.Invoke();
            GUI.color = original;
            return 30f + listing.verticalSpacing;
        }
    }
}
