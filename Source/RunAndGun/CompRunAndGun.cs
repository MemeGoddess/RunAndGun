using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using RunAndGun.Utilities;

namespace RunAndGun
{
    public class CompRunAndGun : ThingComp
    {
        private Pawn pawn
        {
            get
            {
                Pawn pawn = (Pawn) (parent as Pawn);
                if (pawn == null)
                    Log.Error("pawn is null");
                return pawn;
            }
        }

        internal bool _isEnabled = false;
        internal bool _disabled = false;
        public bool isEnabled
        {
            get => _isEnabled && IsValid();
            set => _isEnabled = value;
        }

        public void RefreshDisabledState()
        {
            var rangedWeapons = GetEquippedRangedWeapons().ToList();
            _disabled = !rangedWeapons.Any() || rangedWeapons.Any(IsForbiddenWeapon);
        }

        private bool IsValid()
        {
            if (_disabled)
                return false;

            if (pawn == null)
                return false;

            if (!pawn.IsColonist && !RunAndGun.settings.enableForAI)
                return false;

            return !GetEquippedRangedWeapons().Any(IsForbiddenWeapon);
        }

        private IEnumerable<ThingDef> GetEquippedRangedWeapons()
        {
            if (pawn?.equipment?.Primary?.def?.IsRangedWeapon ?? false)
                yield return pawn.equipment.Primary.def;

            if (RunAndGun.settings.DualWieldInstalled && (pawn?.equipment?.GetOffHand(out var offhand) ?? false) && offhand.def.IsRangedWeapon)
                yield return offhand.def;
        }

        private static bool IsForbiddenWeapon(ThingDef weapon)
        {
            return RunAndGun.settings.forbiddenWeapons.TryGetValue(weapon.defName, out var forbidden) && forbidden.isSelected;
        }

        //This can be misused to read isEnabled from other mods without using (expensive) reflection. 
        public override string GetDescriptionPart()
        {   
            return isEnabled.ToString();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _isEnabled, "isEnabled", false);
            Scribe_Values.Look(ref _disabled, "disabled", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                RefreshDisabledState();
        }

    }
}

