using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace RunAndGun.Harmony
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Notify_PassedToWorld))]
    public static class Pawn_PostMake
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance)
        {
            if (!__instance.TryGetComp<CompRunAndGun>(out var comp))
                return;

            comp._isEnabled = __instance.IsColonist || RunAndGun.settings.enableForAI;
            comp.RefreshDisabledState();
        }
    }
}
