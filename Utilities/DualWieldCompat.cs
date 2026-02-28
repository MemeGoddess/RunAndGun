using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DualWield;
using Tacticowl;
using Verse;

namespace RunAndGun.Utilities
{
    internal static class DualWieldCompat
    {
        internal static Pawn_StanceTracker GetOffHandStance(this Pawn pawn)
        {
            return !TacticowlMod.Settings.DualWieldEnabled ? null : pawn?.GetStancesOffHand();
        }

        internal static bool HasOffHand(this Pawn_EquipmentTracker equipment)
        {
            return TacticowlMod.Settings.DualWieldEnabled && equipment?.TryGetOffHandEquipment(out _) is true;
        }

        internal static bool GetOffHand(this Pawn_EquipmentTracker equipment, out ThingWithComps offhand)
        {
            offhand = null;

            if (!TacticowlMod.Settings.DualWieldEnabled)
                return false;

            if(equipment == null)
                return false;

            var found = equipment?.TryGetOffHandEquipment(out offhand) is true;
            return found;
        }
    }
}
