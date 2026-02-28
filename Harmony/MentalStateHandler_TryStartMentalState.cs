using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using RimWorld;
using Tacticowl;
using Verse.AI;

namespace RunAndGun.Harmony
{
    [HarmonyPatchCategory(nameof(Tacticowl.PatchCategories.RunAndGun))]
    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
    public class MentalStateHandler_TryStartMentalState
    {
        static void Postfix(MentalStateHandler __instance, MentalStateDef stateDef, ref Pawn ___pawn)
        {
            if (stateDef != MentalStateDefOf.PanicFlee)
            {
                return;
            }
            CompRunAndGun comp = ___pawn.TryGetComp<CompRunAndGun>();
            if (comp != null && TacticowlMod.Settings.RunAndGun.enableForAI)
            {
                comp.isEnabled = shouldRunAndGun();
            }
        }
        static bool shouldRunAndGun()
        {
            var chance = TacticowlMod.Settings.RunAndGun.enableForFleeChance;

            if (chance < 1)
                return false;

            if (chance > 99)
                return true;

            var r = UnityEngine.Random.Range(1f, 100f);
            return r <= chance;

        }
    }
}
