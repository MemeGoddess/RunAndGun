using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RunAndGun.Utilities
{
    [StaticConstructorOnStartup]
    internal static class DualWieldCompat
    {
        internal static MethodInfo _getOffHandStance;
        internal static MethodInfo _getOffHand;
        static DualWieldCompat()
        {
            if (!RunAndGun.settings.DualWieldInstalled)
                return;

            _getOffHandStance = AccessTools.Method(AccessTools.TypeByName("DualWield.Ext_Pawn"), "GetStancesOffHand");
            _getOffHand = AccessTools.Method(AccessTools.TypeByName("DualWield.Ext_Pawn_EquipmentTracker"), "TryGetOffHandEquipment");

            if (_getOffHandStance == null)
                throw new Exception("Unable to find method in Dual Wield to get OffHand Stance");

            if (_getOffHand == null)
                throw new Exception("Unable to find method in Dual Wield to get OffHand Weapon");
        }

        internal static Pawn_StanceTracker GetOffHandStance(this Pawn pawn)
        {
            if (!RunAndGun.settings.DualWieldInstalled)
                return null;

            if (pawn == null)
                return null;

            return _getOffHandStance.Invoke(null, new[] { pawn }) as Pawn_StanceTracker;
        }

        internal static bool HasOffHand(this Pawn_EquipmentTracker equipment)
        {
            if (!RunAndGun.settings.DualWieldInstalled)
                return false;

            if (equipment == null)
                return false;

            return _getOffHand.Invoke(null, new[] { equipment, (object)null }) is true;
        }

        internal static bool GetOffHand(this Pawn_EquipmentTracker equipment, out ThingWithComps offhand)
        {
            offhand = null;

            if (!RunAndGun.settings.DualWieldInstalled)
                return false;

            if(equipment == null)
                return false;

            var param = new object[] { equipment, null };
            var found = _getOffHand.Invoke(null, param) is true;
            offhand = found ? param[1] as ThingWithComps : null;

            return found;
        }
    }
}
