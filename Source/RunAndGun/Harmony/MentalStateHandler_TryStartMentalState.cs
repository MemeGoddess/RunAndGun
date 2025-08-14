using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using RimWorld;
using Verse.AI;
using HugsLib.Settings;
using HugsLib;

namespace RunAndGun.Harmony
{
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
            if (comp != null && RunAndGun.settings.enableForAI)
            {
                comp.isEnabled = shouldRunAndGun();
            }
        }
        static bool shouldRunAndGun()
        {
            var chance = RunAndGun.settings.enableForFleeChance;

            if (chance < 1)
                return false;

            if (chance > 99)
                return true;

            var r = UnityEngine.Random.Range(1f, 100f);
            return r <= chance;

        }
    }
}
