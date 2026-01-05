using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;
using RunAndGun.Utilities;

namespace RunAndGun.Harmony
{

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_DraftController_GetGizmos_Patch
    {
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

            if (RunAndGun.settings.DualWieldInstalled && (__instance.equipment?.GetOffHand(out var offhand) ?? false) && offhand.def.IsRangedWeapon)
                weapons.Add(offhand.def);

            data._disabled = !weapons.Any() || weapons.Any(x =>
                RunAndGun.settings.forbiddenWeapons.TryGetValue(x.defName, out var forbidden) &&
                forbidden.isSelected);

            var uiElement = "enable_RG";
            string label = "RG_Action_Enable_Label".Translate();
            string description = data.isEnabled ? "RG_Action_Disable_Description".Translate() : "RG_Action_Enable_Description".Translate();

            if(!weapons.Any())
                yield break;

            yield return new Command_Toggle
            {
                defaultLabel = label,
                defaultDesc = description,
                icon = ContentFinder<Texture2D>.Get(("UI/Buttons/" + uiElement), true),
                isActive = () => data.isEnabled,
                toggleAction = () => { data.isEnabled = !data.isEnabled; } ,
                Disabled = data._disabled,
            };
        }
    }

}