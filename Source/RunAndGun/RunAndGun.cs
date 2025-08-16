using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using HugsLib.Settings;

using RunAndGun.Utilities;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using System.Xml;

namespace RunAndGun
{
    public class RunAndGun : Mod
    {
        public string ModIdentifier
        {
            get { return "RunAndGun"; }
        }
        public static RunAndGun Instance { get; private set; }
        
        List<ThingDef> allWeapons;

        public static Settings settings;


        public RunAndGun(ModContentPack content) : base(content)
        {
            Instance = this;
            settings = GetSettings<Settings>();

            var harmony = new HarmonyLib.Harmony(ModIdentifier);
            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
            
        }

        public override string SettingsCategory()
        {
            return "Run and Gun";
        }

        internal void ResetForbidden()
        {
            if (allWeapons == null)
                allWeapons = WeaponUtility.getAllWeapons();

            settings.weaponForbidder = null;
            DrawUtility.FilterWeapons(ref settings.weaponForbidder, allWeapons, null);
        }
        private bool AssemblyExists(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith(assemblyName))
                    return true;
            }
            return false;
        }
    }
}
