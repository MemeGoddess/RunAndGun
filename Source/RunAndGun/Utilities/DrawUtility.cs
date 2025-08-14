using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RunAndGun.Utilities
{
    public static class DrawUtility
    {
        private const float ContentPadding = 5f;
        private const float IconSize = 32f;
        private const float IconGap = 1f;
        private const float TextMargin = 20f;
        private const float BottomMargin = 2f;

        private static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);
        private static readonly Color SelectedOptionColor = new Color(0.5f, 1f, 0.5f, 1f);
        private static readonly Color constGrey = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static readonly Color background = new Color(0.5f, 0f, 0f, 0.1f);

        private static void DrawBackground(Rect rect, Color color)
        {
            Color save = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, TexUI.FastFillTex);
            GUI.color = save;
        }

        private static void DrawLabel(string labelText, Rect textRect, float offset)
        {
            float labelHeight = Text.CalcHeight(labelText, textRect.width) - 2f;
            var labelRect = new Rect(textRect.x, textRect.yMin - labelHeight + offset, textRect.width, labelHeight);
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, labelText);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static Color GetColor(ThingDef weapon) =>
            weapon.graphicData?.color ?? Color.white;

        private static bool DrawIconForWeapon(ThingDef weapon, KeyValuePair<string, WeaponRecord> item, Rect contentRect, Vector2 iconOffset, int buttonID)
        {
            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            if (!contentRect.Contains(iconRect)) return false;

            TooltipHandler.TipRegion(iconRect, weapon.label);
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);

            GUI.color = (Mouse.IsOver(iconRect) || item.Value.isException) ? iconMouseOverColor : iconBaseColor;
            GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("square"));

            Texture2D resolvedIcon = weapon.uiIcon ?? weapon.graphicData?.Graphic?.MatSingle.mainTexture as Texture2D ?? BaseContent.BadTex;
            GUI.color = GetColor(weapon);
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                Event.current.button = buttonID;
                return true;
            }
            return false;
        }

        // 🔹 Draws a slider with a background and label
        public static float CustomDrawer_Filter(Rect rect, float currentValue, bool isPercentage, float min, float max, Color bg)
        {
            DrawBackground(rect, bg);
            const int labelWidth = 50;

            Rect sliderRect = new Rect(rect.x, rect.y, rect.width - labelWidth, rect.height).ContractedBy(2f);
            Rect labelRect = new Rect(sliderRect.xMax + 5f, rect.y + 4f, labelWidth, rect.height);

            Widgets.Label(labelRect, isPercentage ? $"{Mathf.Round(currentValue * 100f):F0}%" : currentValue.ToString("F2"));
            return Widgets.HorizontalSlider(sliderRect, currentValue, min, max, true);
        }

        // 🔹 Simple tabs (returns new selected tab string)
        public static string CustomDrawer_Tabs(Rect rect, string currentTab, string[] tabs)
        {
            float buttonWidth = 140f;
            float offset = 0f;
            string newSelection = currentTab;

            foreach (string tab in tabs)
            {
                Rect buttonRect = new Rect(rect.x + offset, rect.y, buttonWidth, 24f);
                Color originalColor = GUI.color;
                if (tab == currentTab) GUI.color = SelectedOptionColor;

                if (Widgets.ButtonText(buttonRect, tab))
                    newSelection = (currentTab == tab) ? "none" : tab;

                GUI.color = originalColor;
                offset += buttonWidth;
            }
            return newSelection;
        }

        // 🔹 Filters weapons for selection (vanilla version)
        public static void FilterWeapons(ref DictWeaponRecordHandler setting, List<ThingDef> allWeapons, float? filter = null)
        {
            if (setting == null) setting = new DictWeaponRecordHandler();

            if (setting.InnerList == null)
            {
                Log.Error("Caught Inner List being null");
                setting.InnerList = new Dictionary<string, WeaponRecord>();
            }

            Dictionary<string, WeaponRecord> selection = new Dictionary<string, WeaponRecord>();

            foreach (ThingDef weapon in allWeapons)
            {
                bool shouldSelect = false;
                if (filter != null)
                {
                    float mass = weapon.GetStatValueAbstract(StatDefOf.Mass);
                    shouldSelect = mass >= filter.Value;
                }

                if (setting.InnerList.TryGetValue(weapon.defName, out WeaponRecord value) && value.isException)
                {
                    selection[weapon.defName] = value;
                }
                else
                {
                    bool defaultForbidden = weapon.GetModExtension<DefModExtension_SettingDefaults>() is DefModExtension_SettingDefaults modExt && modExt.weaponForbidden;
                    shouldSelect = filter == null ? defaultForbidden : shouldSelect;
                    selection[weapon.defName] = new WeaponRecord(shouldSelect, false, weapon.label);
                }
            }
            setting.InnerList = selection.OrderBy(d => d.Value.label).ToDictionary(d => d.Key, d => d.Value);
        }

        // 🔹 Draw weapon selection UI
        public static bool CustomDrawer_MatchingWeapons_active(Rect wholeRect, DictWeaponRecordHandler setting, List<ThingDef> allWeapons, float? filter = null, string yesText = "Light", string noText = "Heavy")
        {
            DrawBackground(wholeRect, background);

            Rect leftRect = new Rect(wholeRect.x, wholeRect.y + TextMargin, wholeRect.width / 2, wholeRect.height - TextMargin);
            Rect rightRect = new Rect(leftRect.xMax, wholeRect.y + TextMargin, wholeRect.width / 2, wholeRect.height - TextMargin);

            DrawLabel(yesText, leftRect, TextMargin);
            DrawLabel(noText, rightRect, TextMargin);

            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y + TextMargin);
            rightRect.position = new Vector2(rightRect.position.x, rightRect.position.y + TextMargin);

            FilterWeapons(ref setting, allWeapons, filter);
            Dictionary<string, WeaponRecord> selection = setting.InnerList;

            int iconsPerRow = (int)(leftRect.width / (IconGap + IconSize));
            int indexLeft = 0, indexRight = 0;
            bool changed = false;

            Dictionary<string, ThingDef> weaponDefs = allWeapons.ToDictionary(o => o.defName);

            foreach (var item in selection)
            {
                bool isSelected = item.Value.isSelected;
                Rect targetRect = isSelected ? rightRect : leftRect;
                int index = isSelected ? indexRight++ : indexLeft++;

                int col = index % iconsPerRow;
                int row = index / iconsPerRow;

                if (weaponDefs.TryGetValue(item.Key, out ThingDef weapon))
                {
                    Vector2 pos = new Vector2(IconSize * col + col * IconGap, IconSize * row + row * IconGap);
                    if (DrawIconForWeapon(weapon, item, targetRect, pos, index))
                    {
                        item.Value.isSelected = !item.Value.isSelected;
                        item.Value.isException = !item.Value.isException;
                        changed = true;
                    }
                }
            }

            if (changed) setting.InnerList = selection;
            return changed;
        }
    }
}
