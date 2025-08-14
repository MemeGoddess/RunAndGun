using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool enableForAI = true;
        public int enableForFleeChance = 100;
        public int accuracyPenalty = 10;
        public int movementPenaltyHeavy = 40;
        public int movementPenaltyLight = 10;
        public string tabsHandler = "none";
        public float weightLimitFilter = 3.4f;
        public DictWeaponRecordHandler weaponSelecter = new DictWeaponRecordHandler();
        public DictWeaponRecordHandler weaponForbidder = new DictWeaponRecordHandler();

        // === Cached state ===
        public List<ThingDef> allWeapons;
        private string[] tabNames = new[] { "Weapons", "Forbidden" };
        private float maxWeightMelee, maxWeightRanged, maxWeightTotal;

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

            DrawUtility.FilterWeapons(ref weaponSelecter, allWeapons, weightLimitFilter);
            DrawUtility.FilterWeapons(ref weaponForbidder, allWeapons);
        }

        public void DoWindowContents(Rect rect)
        {
            Initialize();

            var listing = new Listing_Standard();
            listing.Begin(rect);

            // === General Settings ===
            listing.CheckboxLabeled("RG_EnableRGForAI_Title".Translate(), ref enableForAI, "RG_EnableRGForAI_Description".Translate());
            if (enableForAI)
            {
                listing.Label("RG_EnableRGForFleeChance_Title".Translate() + ": " + enableForFleeChance + "%");
                enableForFleeChance = (int)Widgets.HorizontalSlider(listing.GetRect(22f), enableForFleeChance, 0, 100, false, "");

                listing.Label("RG_AccuracyPenalty_Title".Translate() + ": " + accuracyPenalty + "%");
                accuracyPenalty = (int)Widgets.HorizontalSlider(listing.GetRect(22f), accuracyPenalty, 0, 100, false, "");
            }

            // === Movement Penalties ===
            listing.Label("RG_MovementPenaltyHeavy_Title".Translate() + ": " + movementPenaltyHeavy + "%");
            movementPenaltyHeavy = (int)Widgets.HorizontalSlider(listing.GetRect(22f), movementPenaltyHeavy, 0, 100, false, "");

            listing.Label("RG_MovementPenaltyLight_Title".Translate() + ": " + movementPenaltyLight + "%");
            movementPenaltyLight = (int)Widgets.HorizontalSlider(listing.GetRect(22f), movementPenaltyLight, 0, 100, false, "");

            listing.GapLine();

            // === Tabs ===
            listing.Label("RG_Tabs_Title".Translate());
            if (Widgets.ButtonText(listing.GetRect(24f), tabsHandler))
            {
                List<FloatMenuOption> menu = new List<FloatMenuOption>();
                foreach (var name in tabNames)
                {
                    string local = name;
                    menu.Add(new FloatMenuOption(local, () => tabsHandler = local));
                }
                Find.WindowStack.Add(new FloatMenu(menu));
            }

            listing.GapLine();

            // === Filters and Custom UI ===
            if (tabsHandler == tabNames[0])
            {
                listing.Label("RG_WeightLimitFilter_Title".Translate() + $" ({weightLimitFilter:F1})");
                weightLimitFilter = Widgets.HorizontalSlider(listing.GetRect(22f), weightLimitFilter, 0f, maxWeightTotal, false, "", "0", maxWeightTotal.ToString("F1"));

                //DrawUtility.CustomDrawer_Filter(listing.GetRect(120f), weightLimitFilter, false, 0, maxWeightTotal, Color.yellow);
                DrawUtility.CustomDrawer_MatchingWeapons_active(listing.GetRect(200f), weaponSelecter, allWeapons, weightLimitFilter, "RG_ConsideredLight".Translate(), "RG_ConsideredHeavy".Translate());
            }
            else if (tabsHandler == tabNames[1])
            {
                DrawUtility.CustomDrawer_MatchingWeapons_active(listing.GetRect(200f), weaponForbidder, allWeapons, null, "RG_Allow".Translate(), "RG_Forbid".Translate());
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

            Scribe_Deep.Look(ref weaponSelecter, nameof(weaponSelecter), new object[0]);
            Scribe_Deep.Look(ref weaponForbidder, nameof(weaponForbidder), new object[0]);

            base.ExposeData();
        }

        private bool AssemblyExists(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.FullName.StartsWith(assemblyName))
                    return true;
            return false;
        }
    }

}
