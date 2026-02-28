using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Tacticowl;
using UnityEngine;

namespace RunAndGun
{
    public class CompRunAndGun : ThingComp
    {
        private Pawn pawn
        {
            get
            {
                Pawn pawn = (Pawn) (parent as Pawn);
                if (pawn == null)
                    Log.Error("pawn is null");
                return pawn;
            }
        }

        internal bool _isEnabled = false;
        internal bool _disabled = false;
        public bool isEnabled
        {
            get => !_disabled && _isEnabled;
            set => _isEnabled = value;
        }

        //This can be misused to read isEnabled from other mods without using (expensive) reflection. 
        public override string GetDescriptionPart()
        {   
            return isEnabled.ToString();
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Pawn pawn = (Pawn)(parent as Pawn);
            bool enableRGForAI = TacticowlMod.Settings.RunAndGun.enableForAI;
            if (!pawn.IsColonist && enableRGForAI)
            {
                isEnabled = true;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _isEnabled, "isEnabled", false);
            Scribe_Values.Look(ref _disabled, "disabled", false);
        }

    }
}

