using HarmonyLib;
using Verse;

#if DEBUG
namespace RunAndGun
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
    public static class CanRunAndGunDebug
    {
        public static void Postfix(Pawn __instance, ref string __result)
        {
            var comp = __instance.GetComp<CompRunAndGun>();
            if (comp == null)
                return;
            __result += $"\nRunAndGun: {comp._isEnabled}, Disabled: {comp._disabled}";
        }

    }
}
#endif
