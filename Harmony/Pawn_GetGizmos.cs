using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;
using RunAndGun.Utilities;
using Tacticowl;

namespace RunAndGun.Harmony
{

    [HarmonyPatchCategory(nameof(Tacticowl.PatchCategories.RunAndGun))]
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_DraftController_GetGizmos_Patch
    {
        private static string RG_Action_Enable_Label;
        private static string RG_Action_Disable_Description;
        private static string RG_Action_Enable_Description;
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            var results = __result?.ToList() ?? new List<Gizmo>();
            if (!results.Any())
                yield break;

            foreach (var result in results)
                yield return result;

            if (!(__instance is { Drafted: true }) || !__instance.Faction.Equals(Faction.OfPlayer))
                yield break;

            var data = __instance.TryGetComp<CompRunAndGun>();
            if(data == null)
                yield break;

            var weapons = new List<ThingDef>();

            if (__instance.equipment?.Primary != null && __instance.equipment.Primary.def.IsRangedWeapon) 
                weapons.Add(__instance.equipment.Primary.def);

            if (TacticowlMod.Settings.DualWieldEnabled && (__instance.equipment?.GetOffHand(out var offhand) ?? false) && offhand.def.IsRangedWeapon)
                weapons.Add(offhand.def);

            data._disabled = !weapons.Any() || weapons.Any(x =>
                TacticowlMod.Settings.RunAndGun.forbiddenWeapons.TryGetValue(x.defName, out var forbidden) &&
                forbidden.isSelected);

            if(!weapons.Any())
                yield break;

            RG_Action_Enable_Label ??= "RG_Action_Enable_Label".Translate();
            RG_Action_Disable_Description ??= "RG_Action_Disable_Description".Translate();
            RG_Action_Enable_Description ??= "RG_Action_Enable_Description".Translate();

            yield return new Command_Toggle
            {
                defaultLabel = RG_Action_Enable_Label,
                defaultDesc = data.isEnabled ? RG_Action_Disable_Description : RG_Action_Enable_Description,
                icon = ContentFinder<Texture2D>.Get(("UI/Buttons/enable_RG"), true),
                isActive = () => data.isEnabled,
                toggleAction = () => { data.isEnabled = !data.isEnabled; } ,
                Disabled = data._disabled,
            };
        }
    }

}