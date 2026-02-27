using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RunAndGun
{
    public class WeaponRecord : IExposable
    {
        public bool isSelected = false;
        public bool isException = false;
        public String label = "";

        public void ExposeData()
        {
            Scribe_Values.Look(ref isSelected, nameof(isSelected), false);
            Scribe_Values.Look(ref isException, nameof(isException), false);
            Scribe_Values.Look(ref label, nameof(label), "");
        }
    }
}
