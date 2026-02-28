using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;
using RunAndGun.Utilities;
using Tacticowl;

namespace RunAndGun
{
    class Stance_RunAndGun_Cooldown : Stance_Cooldown
    {
        private static FieldInfo burstsLeft;
        

        private bool? hasOffHand;
        private int ticksBetweenBurst;
        public override bool StanceBusy => Pawn?.CurJob == null || (Pawn.CurJob.def != JobDefOf.Goto && CheckDWBusy());

        public Stance_RunAndGun_Cooldown()
        {
            if (!TacticowlMod.Settings.DualWieldEnabled || !verb.CasterIsPawn) return;

            hasOffHand = verb.CasterPawn?.equipment.HasOffHand();
        }
        public Stance_RunAndGun_Cooldown(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
        {
            ticksBetweenBurst = ticks;

            if (!TacticowlMod.Settings.DualWieldEnabled || !verb.CasterIsPawn) return;

            hasOffHand = verb.CasterPawn?.equipment.HasOffHand();
        }

        protected override void Expire()
        {
            burstsLeft ??= AccessTools.Field(typeof(Verb), "burstShotsLeft");

            if (Pawn.GetComps<CompRunAndGun>().Any(x => x.isEnabled) && burstsLeft.GetValue(verb) is int bursts &&
                bursts > 0)
                stanceTracker.SetStance(new Stance_RunAndGun(ticksBetweenBurst, focusTarg, verb));
            else
                base.Expire();
        }

        private bool CheckDWBusy()
        {
            if (!TacticowlMod.Settings.DualWieldEnabled)
                return true;

            if (!verb.CasterIsPawn)
                return true;

            hasOffHand ??= verb.CasterPawn.equipment.HasOffHand();
            if (!hasOffHand.Value)
                return true;

            var stance = Pawn.GetOffHandStance();

            return !(stance?.curStance is Stance_Mobile);

        }

    }

}
