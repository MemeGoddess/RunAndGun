using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RunAndGun
{
    public class DictWeaponRecordHandler : SettingHandleConvertible, IExposable
    {
        public Dictionary<String, WeaponRecord> inner = new Dictionary<String, WeaponRecord>();
        public Dictionary<String, WeaponRecord> InnerList { get { return inner; } set { inner = value; } }

        public override void FromString(string settingValue)
        {
            inner = new Dictionary<String, WeaponRecord>();
            if (!settingValue.Equals(string.Empty))
            {
                foreach (string str in settingValue.Split('|'))
                {
                    string[] split = str.Split(',');
                    if (split.Count() < 4) //ensures that it works for users that still have old WeaponRecords saved. 
                    {
                        inner.Add(str.Split(',')[0], new WeaponRecord(Convert.ToBoolean(str.Split(',')[1]), Convert.ToBoolean(str.Split(',')[2]), ""));
                    }
                    else
                    {
                        inner.Add(str.Split(',')[0], new WeaponRecord(Convert.ToBoolean(str.Split(',')[1]), Convert.ToBoolean(str.Split(',')[2]), str.Split(',')[3]));
                    }
                }
            }
        }

        public override string ToString()
        {
            List<String> strings = new List<string>();
            foreach (KeyValuePair<string, WeaponRecord> item in inner)
            {
                strings.Add(item.Key + "," + item.Value.ToString());
            }

            return inner != null ? String.Join("|", strings.ToArray()) : "";
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                string serialized = ToString();
                Scribe_Values.Look(ref serialized, "innerSerialized", "");
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string serialized = "";
                Scribe_Values.Look(ref serialized, "innerSerialized", "");
                FromString(serialized);
            }
            else if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (inner == null) inner = new Dictionary<string, WeaponRecord>();
            }
        }
    }
}
