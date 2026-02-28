using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using Tacticowl;

namespace RunAndGun.Harmony
{


    [HarmonyPatchCategory(nameof(Tacticowl.PatchCategories.RunAndGun))]
    [HarmonyPatch(typeof(VerbProperties), "AdjustedAccuracy")]
    static class VerbProperties_AdjustedAccuracy
    {
        static void Postfix(VerbProperties __instance, ref Thing equipment, ref float __result)
        {

            if (equipment == null || equipment.holdingOwner == null || !(equipment.holdingOwner.Owner is Pawn_EquipmentTracker))
            {
                return;
            }
            if (equipment == null || equipment.holdingOwner == null || equipment.holdingOwner.Owner == null)
            {
                return;
            }
            Pawn_EquipmentTracker eqt = (Pawn_EquipmentTracker)equipment.holdingOwner.Owner;
            Pawn pawn = eqt.pawn;
            if(pawn == null || pawn.stances == null)
            {
                return;
            }
            if (pawn.stances.curStance is Stance_RunAndGun || pawn.stances.curStance is Stance_RunAndGun_Cooldown)
            {
                var value = TacticowlMod.Settings.RunAndGun.accuracyPenalty;
                float factor = ((float)(100 - value) / 100);
                __result *= factor;
            }
        }
    }
}
