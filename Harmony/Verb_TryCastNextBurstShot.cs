using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using RimWorld;
using Verse.Sound;
using System.Reflection.Emit;
using System.Reflection;

namespace RunAndGun.Harmony
{
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    public static class Verb_TryCastNextBurstShot
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            // There's the Dev version which people are probably still using.
            // Can't require an update on that side as Ogliss doesn't want to support it anymore.
            // Will remove this legacy patch at some point
            return ModsConfig.IsActive("roolo.dualwield") ? 
                Verb_TryCastNextBurstShot_Legacy.ReplaceMethod(instructions) : 
                ReplaceValue(instructions, il);
        }

        public static IEnumerable<CodeInstruction> ReplaceValue(IEnumerable<CodeInstruction> instructions,
            ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);

            var setStance = AccessTools.Method(typeof(Pawn_StanceTracker), nameof(Pawn_StanceTracker.SetStance));
            var modifyStance = AccessTools.Method(typeof(Verb_TryCastNextBurstShot), nameof(UpdateStance));

            var stanceTemp = il.DeclareLocal(typeof(Stance));

            for (var i = 0; i < code.Count; i++)
            {
                if (code[i].opcode != OpCodes.Callvirt || !(code[i].operand is MethodInfo mi) ||
                    mi != setStance) continue;

                // Stack should be [stanceTracker, stance], replace with [stanceTracker, updatedStance]
                code.Insert(i++, new CodeInstruction(OpCodes.Stloc, stanceTemp));
                code.Insert(i++, new CodeInstruction(OpCodes.Dup));
                code.Insert(i++, new CodeInstruction(OpCodes.Ldloc, stanceTemp));
                code.Insert(i++, new CodeInstruction(OpCodes.Call, modifyStance));
            }

            return code;
        }

        public static Stance_Cooldown UpdateStance(Pawn_StanceTracker stanceTracker, Stance_Cooldown stance)
        {
            if (stanceTracker.pawn.equipment?.Primary == null || stanceTracker.pawn.equipment.Primary.def.IsMeleeWeapon)
                return stance;

            if (stanceTracker.pawn.equipment.Primary == stance.verb.EquipmentSource ||
                stance.verb.EquipmentSource == null || stance.verb.EquipmentSource is Apparel)
            {
                if ((((stanceTracker.curStance is Stance_RunAndGun) ||
                      (stanceTracker.curStance is Stance_RunAndGun_Cooldown))) && stanceTracker.pawn.pather.Moving)
                {
                    return new Stance_RunAndGun_Cooldown(stance.ticksLeft, stance.focusTarg,
                        stance.verb);
                }

            }

            return stance;
        }
        
    }

    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    public class Verb_TryCastNextBurstShot_Legacy
    {
        public static IEnumerable<CodeInstruction> ReplaceMethod(IEnumerable<CodeInstruction> instructions)
        {
            var message = @"Ogliss/Roolo's Dual Wield and MemeGoddess' RunAndGun aren't fully compatible. Please wait for/go subscribe to MemeGoddess' version of Dual Wield. ";
            Log.ErrorOnce(message, message.GetHashCode());
            var instructionsList = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction instruction in instructionsList)
            {
                if (instruction.operand as MethodInfo == typeof(Pawn_StanceTracker).GetMethod(nameof(Pawn_StanceTracker.SetStance)))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(Verb_TryCastNextBurstShot_Legacy).GetMethod(nameof(SetStanceRunAndGun)));
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static void SetStanceRunAndGun(Pawn_StanceTracker stanceTracker, Stance_Cooldown stance)
        {
            if (stanceTracker.pawn.equipment?.Primary == null || stanceTracker.pawn.equipment.Primary.def.IsMeleeWeapon)
            {
                stanceTracker.SetStance(stance);
                return;
            }
            if (stanceTracker.pawn.equipment.Primary == stance.verb.EquipmentSource || stance.verb.EquipmentSource == null || stance.verb.EquipmentSource is Apparel)
            {
                if ((((stanceTracker.curStance is Stance_RunAndGun) || (stanceTracker.curStance is Stance_RunAndGun_Cooldown))) && stanceTracker.pawn.pather.Moving)
                {
                    stanceTracker.SetStance(new Stance_RunAndGun_Cooldown(stance.ticksLeft, stance.focusTarg, stance.verb));
                    return;
                }

            }
            stanceTracker.SetStance(stance);
        }
    }
}
