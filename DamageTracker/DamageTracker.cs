using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using BepInEx;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Cards;
using UnboundLib.Utils.UI;
using HarmonyLib;
using DamageTracker.MonoBehaviors;

namespace DamageTracker
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]

    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]

    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class DamageTracker : BaseUnityPlugin
    {
        private const string ModId = "com.pudassassin.rounds.DamageTracker";
        private const string ModName = "Damage Tracker";
        private const string Version = "1.2.1"; //build #27 / Release 1-2-0

        private const string CompatibilityModName = "DamageTracker";

        // !! // config method sector
        private static Color easyChangeColor = new Color(0.521f, 1f, 0.521f, 1f);
        private static Color hardChangeColor = new Color(1f, 0.521f, 0.521f, 1f);

        internal static string ConfigKey(string name)
        {
            return $"{DamageTracker.CompatibilityModName}_{name.ToLower()}";
        }
        internal static bool GetBool(string name, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(ConfigKey(name), defaultValue ? 1 : 0) == 1;
        }
        internal static void SetBool(string name, bool value)
        {
            PlayerPrefs.SetInt(ConfigKey(name), value ? 1 : 0);
        }
        internal static int GetInt(string name, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(ConfigKey(name), defaultValue);
        }
        internal static void SetInt(string name, int value)
        {
            PlayerPrefs.SetInt(ConfigKey(name), value);
        }
        internal static float GetFloat(string name, float defaultValue = 0)
        {
            return PlayerPrefs.GetFloat(ConfigKey(name), defaultValue);
        }
        internal static void SetFloat(string name, float value)
        {
            PlayerPrefs.SetFloat(ConfigKey(name), value);
        }

        // !! // read and write to Unity game preference, persist between mod profiles
        public static int NumberFormattingStyle
        {
            // enum from 0 - 5
            get
            {
                return GetInt("NumberFormattingStyle", 1);
            }
            set
            {
                SetInt("NumberFormattingStyle", value);
            }
        }
        public static bool UsePlayerColor
        {
            get
            {
                return GetBool("UsePlayerColor", true);
            }
            set
            {
                SetBool("UsePlayerColor", value);
            }
        }

        public static float MaxNumberToShowDecimal
        {
            // range: 0.0 - 1000.0 float
            get
            {
                return GetFloat("MaxNumberToShowDecimal", 25.0f);
            }
            set
            {
                SetFloat("MaxNumberToShowDecimal", value);
            }
        }
        public static float ScaleDecimalTextSize
        {
            // range: 0.1 - 1.0 float
            get
            {
                return GetFloat("ScaleDecimalTextSize", 0.75f);
            }
            set
            {
                SetFloat("ScaleDecimalTextSize", value);
            }
        }
        public static bool HideAllDecimal
        {
            get
            {
                return GetBool("HideAllDecimal", false);
            }
            set
            {
                SetBool("HideAllDecimal", value);
            }
        }

        public static float NumberFontSize
        {
            // range: 4 - 120 int
            get
            {
                return GetFloat("NumberFontSize", 24f);
            }
            set
            {
                SetFloat("NumberFontSize", value);
            }
        }
        public static float ScaleNumberSizeByValue
        {
            // log factor range: 0.0 - 2.0f
            get
            {
                return GetFloat("ScaleNumberSizeByValue", 0.25f);
            }
            set
            {
                SetFloat("ScaleNumberSizeByValue", value);
            }
        }

        public static float NumberLifetime
        {
            // range: 0.5 - 5.0 float
            get
            {
                return GetFloat("NumberLifetime", 2.25f);
            }
            set
            {
                SetFloat("NumberLifetime", value);
            }
        }
        public static float NumberOpaqueTime
        {
            // range: 0.0 - 5.0 float
            get
            {
                return GetFloat("NumberOpaqueTime", 1.45f);
            }
            set
            {
                SetFloat("NumberOpaqueTime", value);
            }
        }
        public static float NumberSumTimeWindow
        {
            // range: 0.0 - 5.0 float, up to Number Lifetime
            get
            {
                return GetFloat("NumberSumTimeWindow", 0.995f);
            }
            set
            {
                SetFloat("NumberSumTimeWindow", value);
            }
        }
        public static float NumberNewSumDelay
        {
            // range: 0.01 - 5.0 float, up to Number Lifetime
            get
            {
                return GetFloat("NumberNewSumDelay", 0.35f);
            }
            set
            {
                SetFloat("NumberNewSumDelay", value);
            }
        }

        // !! // other variables

        public const float RealtimeToRefresh = 0.05f;
        public static float RealtimeLastRefreshed;

        public static bool damageDemoMode = false;

        internal static Dictionary<string, List<Toggle>> TogglesToSync = new Dictionary<string, List<Toggle>>();
        internal static Dictionary<string, List<Slider>> SlidersToSync = new Dictionary<string, List<Slider>>();

        // !! // Mod Config Menu
        // presets -- more to come
        private static void ClassicPlusPreset()
        {
            NumberFormattingStyle = (int)PlayerDamageTracker.NumberFormatting.spacing;
            UsePlayerColor = false;

            MaxNumberToShowDecimal = 10.0f;
            ScaleDecimalTextSize = 0.75f;
            HideAllDecimal = false;

            NumberFontSize = 24f;
            ScaleNumberSizeByValue = 0.10f;

            NumberLifetime = 1.65f;
            NumberOpaqueTime = 1.15f;
            NumberSumTimeWindow = 1.0f;
            NumberNewSumDelay = 0.1f;

            PlayerDamageTracker.UpdateConfigs();
        }
        private static void DefaultPreset()
        {
            NumberFormattingStyle = (int)PlayerDamageTracker.NumberFormatting.spacing;
            UsePlayerColor = true;

            MaxNumberToShowDecimal = 25.0f;
            ScaleDecimalTextSize = 0.75f;
            HideAllDecimal = false;

            NumberFontSize = 24f;
            ScaleNumberSizeByValue = 0.25f;

            NumberLifetime = 2.25f;
            NumberOpaqueTime = 1.45f;
            NumberSumTimeWindow = 0.995f;
            NumberNewSumDelay = 0.35f;

            PlayerDamageTracker.UpdateConfigs();
        }
        private static void BeegNumberPreset()
        {
            NumberFormattingStyle = (int)PlayerDamageTracker.NumberFormatting.shortenNum;
            UsePlayerColor = false;

            MaxNumberToShowDecimal = 5.0f;
            ScaleDecimalTextSize = 0.85f;
            HideAllDecimal = false;

            NumberFontSize = 30f;
            ScaleNumberSizeByValue = 0.35f;

            NumberLifetime = 2.35f;
            NumberOpaqueTime = 1.65f;
            NumberSumTimeWindow = 1.0f;
            NumberNewSumDelay = 0.5f;

            PlayerDamageTracker.UpdateConfigs();
        }

        // menu managing
        private static void InitializeOptionsDictionaries()
        {
            // if (!TogglesToSync.Keys.Contains("DisableCardParticleAnimations")) { TogglesToSync["DisableCardParticleAnimations"] = new List<Toggle>() { }; }
            // if (!SlidersToSync.Keys.Contains("NumberOfGeneralParticles")){ SlidersToSync["NumberOfGeneralParticles"] = new List<Slider>(){};}

            if (!SlidersToSync.Keys.Contains("NumberFormattingStyle"))
            { SlidersToSync["NumberFormattingStyle"] = new List<Slider>(); }

            if (!TogglesToSync.Keys.Contains("UsePlayerColor"))
            { TogglesToSync["UsePlayerColor"] = new List<Toggle>(); }


            if (!SlidersToSync.Keys.Contains("MaxNumberToShowDecimal"))
            { SlidersToSync["MaxNumberToShowDecimal"] = new List<Slider>(); }

            if (!SlidersToSync.Keys.Contains("ScaleDecimalTextSize"))
            { SlidersToSync["ScaleDecimalTextSize"] = new List<Slider>(); }

            if (!TogglesToSync.Keys.Contains("HideAllDecimal"))
            { TogglesToSync["HideAllDecimal"] = new List<Toggle>(); }


            if (!SlidersToSync.Keys.Contains("NumberFontSize"))
            { SlidersToSync["NumberFontSize"] = new List<Slider>(); }

            if (!SlidersToSync.Keys.Contains("ScaleNumberSizeByValue"))
            { SlidersToSync["ScaleNumberSizeByValue"] = new List<Slider>(); }


            if (!SlidersToSync.Keys.Contains("NumberLifetime"))
            { SlidersToSync["NumberLifetime"] = new List<Slider>(); }

            if (!SlidersToSync.Keys.Contains("NumberOpaqueTime"))
            { SlidersToSync["NumberOpaqueTime"] = new List<Slider>(); }

            if (!SlidersToSync.Keys.Contains("NumberSumTimeWindow"))
            { SlidersToSync["NumberSumTimeWindow"] = new List<Slider>(); }

            if (!SlidersToSync.Keys.Contains("NumberNewSumDelay"))
            { SlidersToSync["NumberNewSumDelay"] = new List<Slider>(); }
        }
        private static void SyncOptionsMenus(int recurse = 3)
        {
            // foreach (Toggle toggle in TogglesToSync["DisableCardParticleAnimations"]) { toggle.isOn = DisableCardParticleAnimations; }
            // foreach (Slider slider in SlidersToSync["NumberOfGeneralParticles"]) { slider.value = NumberOfGeneralParticles; }

            foreach (Slider slider in SlidersToSync["NumberFormattingStyle"]) { slider.value = NumberFormattingStyle; }
            foreach (Toggle toggle in TogglesToSync["UsePlayerColor"]) { toggle.isOn = UsePlayerColor; }

            foreach (Slider slider in SlidersToSync["MaxNumberToShowDecimal"]) { slider.value = MaxNumberToShowDecimal; }
            foreach (Slider slider in SlidersToSync["ScaleDecimalTextSize"]) { slider.value = ScaleDecimalTextSize; }
            foreach (Toggle toggle in TogglesToSync["HideAllDecimal"]) { toggle.isOn = HideAllDecimal; }

            foreach (Slider slider in SlidersToSync["NumberFontSize"]) { slider.value = NumberFontSize; }
            foreach (Slider slider in SlidersToSync["ScaleNumberSizeByValue"]) { slider.value = ScaleNumberSizeByValue; }

            foreach (Slider slider in SlidersToSync["NumberLifetime"]) { slider.value = NumberLifetime; }
            foreach (Slider slider in SlidersToSync["NumberOpaqueTime"]) { slider.value = NumberOpaqueTime; }
            foreach (Slider slider in SlidersToSync["NumberSumTimeWindow"]) { slider.value = NumberSumTimeWindow; }
            foreach (Slider slider in SlidersToSync["NumberNewSumDelay"]) { slider.value = NumberNewSumDelay; }

            if (recurse > 0) { SyncOptionsMenus(recurse - 1); }
        }

        // preview
        private static string demoNumberOneString       = "1.234";
        private static string demoNumberTenString       = "55.555";
        private static string demoNumberHundredString   = "123.456";
        private static string demoNumberThousandString  = "12345.678";
        private static string demoNumberMillionString   = "1234567.899";
        private static string numberFormatStyleName = "#ERROR#";

        private static List<GameObject> demoNumberOneObjects        = new List<GameObject>();
        private static List<GameObject> demoNumberTenObjects        = new List<GameObject>();
        private static List<GameObject> demoNumberHundredObjects    = new List<GameObject>();
        private static List<GameObject> demoNumberThousandObjects   = new List<GameObject>();
        private static List<GameObject> demoNumberMillionObjects    = new List<GameObject>();
        private static List<GameObject> numberFormatStyleObjects = new List<GameObject>();

        private static bool RefreshCheck()
        {
            if (Time.time > RealtimeLastRefreshed + RealtimeToRefresh)
            {
                RealtimeLastRefreshed = Time.time;
                return true;
            }

            return false;
        }
        private static void UpdateAndRefreshPreviews()
        {
            if (!RefreshCheck()) return;
            SyncOptionsMenus();

            int numOneSize              = Mathf.CeilToInt( PlayerDamageTracker.ScaleFontSizeByValue(1.234f) );
            int numTenSize              = Mathf.CeilToInt( PlayerDamageTracker.ScaleFontSizeByValue(55.555f) );
            int numHundredSize          = Mathf.CeilToInt( PlayerDamageTracker.ScaleFontSizeByValue(123.456f) );
            int numThousandSize         = Mathf.CeilToInt( PlayerDamageTracker.ScaleFontSizeByValue(12345.678f) );
            int numMillionSize          = Mathf.CeilToInt( PlayerDamageTracker.ScaleFontSizeByValue(1234567.899f) );

            demoNumberOneString         = PlayerDamageTracker.FormatNumber(1.234f, (PlayerDamageTracker.NumberFormatting)NumberFormattingStyle);
            demoNumberTenString         = PlayerDamageTracker.FormatNumber(55.555f, (PlayerDamageTracker.NumberFormatting)NumberFormattingStyle);
            demoNumberHundredString     = PlayerDamageTracker.FormatNumber(123.456f, (PlayerDamageTracker.NumberFormatting)NumberFormattingStyle);
            demoNumberThousandString    = PlayerDamageTracker.FormatNumber(12345.678f, (PlayerDamageTracker.NumberFormatting)NumberFormattingStyle);
            demoNumberMillionString     = PlayerDamageTracker.FormatNumber(1234567.899f, (PlayerDamageTracker.NumberFormatting)NumberFormattingStyle);

            demoNumberOneString         = PlayerDamageTracker.ResizeDecimals(demoNumberOneString, numOneSize);
            demoNumberTenString         = PlayerDamageTracker.ResizeDecimals(demoNumberTenString, numTenSize);
            demoNumberHundredString     = PlayerDamageTracker.ResizeDecimals(demoNumberHundredString, numHundredSize);
            demoNumberThousandString    = PlayerDamageTracker.ResizeDecimals(demoNumberThousandString, numThousandSize);
            demoNumberMillionString     = PlayerDamageTracker.ResizeDecimals(demoNumberMillionString, numMillionSize);

            if (UsePlayerColor)
            {
                demoNumberOneString         = "+ " + demoNumberOneString;
                demoNumberTenString         = "- " + demoNumberTenString;
                demoNumberHundredString     = "+ " + demoNumberHundredString;
                demoNumberThousandString    = "- " + demoNumberThousandString + "*";
                demoNumberMillionString     = "+ " + demoNumberMillionString;
            }

            switch ((PlayerDamageTracker.NumberFormatting)NumberFormattingStyle)
            {
                case PlayerDamageTracker.NumberFormatting.none:
                    numberFormatStyleName = "Plain";
                    break;

                case PlayerDamageTracker.NumberFormatting.spacing:
                    numberFormatStyleName = "Spacing";
                    break;

                case PlayerDamageTracker.NumberFormatting.writtenNum:
                    numberFormatStyleName = "Commas";
                    break;

                case PlayerDamageTracker.NumberFormatting.shortenNum:
                    numberFormatStyleName = "Shorten";
                    break;

                case PlayerDamageTracker.NumberFormatting.metric:
                    numberFormatStyleName = "Metric";
                    break;

                case PlayerDamageTracker.NumberFormatting.sciencific:
                    numberFormatStyleName = "Science!";
                    break;

                default:
                    numberFormatStyleName = "##ERROR##";
                    break;
            }

            if (demoNumberOneObjects.Count > 0)
            {
                foreach (var demoNumberObject in demoNumberOneObjects)
                {
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().text = demoNumberOneString;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMin = numOneSize;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMax = numOneSize;

                    if (UsePlayerColor)
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(.25f, 0.5625f, 1.0f);
                    }
                    else
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = PlayerDamageTracker.colorHeal;
                    }
                }
            }

            if (demoNumberTenObjects.Count > 0)
            {
                foreach (var demoNumberObject in demoNumberTenObjects)
                {
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().text = demoNumberTenString;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMin = numTenSize;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMax = numTenSize;

                    if (UsePlayerColor)
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(.25f, 0.5625f, 1.0f);
                    }
                    else
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = PlayerDamageTracker.colorDamage;
                    }
                }
            }

            if (demoNumberHundredObjects.Count > 0)
            {
                foreach (var demoNumberObject in demoNumberHundredObjects)
                {
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().text = demoNumberHundredString;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMin = numHundredSize;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMax = numHundredSize;

                    if (UsePlayerColor)
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(1.0f, .625f, .25f);
                    }
                    else
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = PlayerDamageTracker.colorHeal;
                    }
                }
            }

            if (demoNumberThousandObjects.Count > 0)
            {
                foreach (var demoNumberObject in demoNumberThousandObjects)
                {
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().text = demoNumberThousandString;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMin = numThousandSize;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMax = numThousandSize;

                    if (UsePlayerColor)
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(1.0f, .625f, .25f);
                    }
                    else
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = PlayerDamageTracker.colorNegHeal;
                    }
                }
            }

            if (demoNumberMillionObjects.Count > 0)
            {
                foreach (var demoNumberObject in demoNumberMillionObjects)
                {
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().text = demoNumberMillionString;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMin = numMillionSize;
                    demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMax = numMillionSize;

                    if (UsePlayerColor)
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(.25f, 0.5625f, 1.0f);
                    }
                    else
                    {
                        demoNumberObject.GetComponentInChildren<TextMeshProUGUI>().color = PlayerDamageTracker.colorHeal;
                    }
                }
            }

            if (numberFormatStyleObjects.Count > 0)
            {
                foreach (var numberFormatStyleObject in numberFormatStyleObjects)
                {
                    numberFormatStyleObject.GetComponentInChildren<TextMeshProUGUI>().text = numberFormatStyleName;
                }
            }
        }

        // !! // Menu directiroy
        // main Mods option
        private static void NewGUI(GameObject menu)
        {
            InitializeOptionsDictionaries();

            MenuHandler.CreateText("<b>" + ModName + " Options</b>", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);

            GameObject presetMenu = MenuHandler.CreateMenu("Presets", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            PresetsMenu(presetMenu);

            GameObject damageTrackerSetting = MenuHandler.CreateMenu("Setting", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            DamageTrackerSetting(damageTrackerSetting);

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }

        // Main/Presets
        private static void PresetsMenu(GameObject menu)
        {
            MenuHandler.CreateButton("Damage Indicator+", menu, ClassicPlusPreset, 60, color: easyChangeColor);
            MenuHandler.CreateText("<i>a familiar classic</i>", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 20);

            MenuHandler.CreateButton("Default", menu, DefaultPreset, 60, color: easyChangeColor);
            MenuHandler.CreateText("<i>recommended settings</i>", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 20);

            MenuHandler.CreateButton("BEEG NUMBERS", menu, BeegNumberPreset, 60, color: easyChangeColor);
            MenuHandler.CreateText("<i>bigger number in a shorter format</i>", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);


            // 'go back' events
            menu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(delegate ()
            {
                damageDemoMode = false;
                SyncOptionsMenus();
            });
            menu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                damageDemoMode = false;
                SyncOptionsMenus();
            });
        }
        
        // Main/Setting
        private static void DamageTrackerSetting(GameObject menu)
        {
            UpdateAndRefreshPreviews();
            // MenuHandler.CreateText("Damage Tracker Setting", menu, out TextMeshProUGUI _, 60);
            // MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);

            MenuHandler.CreateText("Number Formatting", menu, out _, 60);

            // Demo numbers
            demoNumberOneObjects     .Add( MenuHandler.CreateText(demoNumberOneString     , menu, out _, 24) );
            demoNumberTenObjects     .Add( MenuHandler.CreateText(demoNumberTenString     , menu, out _, 24) );
            demoNumberHundredObjects .Add( MenuHandler.CreateText(demoNumberHundredString , menu, out _, 24) );
            demoNumberThousandObjects.Add( MenuHandler.CreateText(demoNumberThousandString, menu, out _, 24) );
            demoNumberMillionObjects .Add( MenuHandler.CreateText(demoNumberMillionString , menu, out _, 24) );


            // Formatting
            void NumberFormatChanged(float val)
            {
                NumberFormattingStyle = (int)val;
                PlayerDamageTracker.UpdateConfigs();
                UpdateAndRefreshPreviews();
            }
            numberFormatStyleObjects.Add( MenuHandler.CreateSlider(numberFormatStyleName, menu, 30, 0, 5, NumberFormattingStyle, NumberFormatChanged, out Slider slider0, true, color: Color.white) );
            Destroy(numberFormatStyleObjects[numberFormatStyleObjects.Count - 1].GetComponentInChildren<TMP_InputField>().gameObject);
            SlidersToSync["NumberFormattingStyle"].Add(slider0);

            void UsePlayerColorToggled(bool val)
            { 
                UsePlayerColor = val;
                PlayerDamageTracker.UpdateConfigs();
                UpdateAndRefreshPreviews();
            }
            MenuHandler.CreateToggle(UsePlayerColor, "Use Player's Color", menu, UsePlayerColorToggled, 30, color: easyChangeColor);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 20);

            UpdateAndRefreshPreviews();

            // Decimal portions
            void MaxNumberToShowDecimalChanged(float val)
            {
                MaxNumberToShowDecimal = val;
                PlayerDamageTracker.UpdateConfigs();
                UpdateAndRefreshPreviews();
            }
            MenuHandler.CreateSlider("Max Number to Show Decimals", menu, 30, 0, 1000, MaxNumberToShowDecimal, MaxNumberToShowDecimalChanged, out Slider slider1, true, color: Color.white);
            SlidersToSync["MaxNumberToShowDecimal"].Add(slider1);

            void ScaleDecimalTextSizeChanged(float val)
            {
                ScaleDecimalTextSize = val;
                PlayerDamageTracker.UpdateConfigs();
                UpdateAndRefreshPreviews();
            }
            MenuHandler.CreateSlider("Decimal Text Scale", menu, 30, 0.1f, 1.0f, ScaleDecimalTextSize, ScaleDecimalTextSizeChanged, out Slider slider2, false, color: Color.white);
            SlidersToSync["ScaleDecimalTextSize"].Add(slider2);

            void HideAllDecimalToggled(bool val)
            {
                HideAllDecimal = val;
                PlayerDamageTracker.UpdateConfigs();
                UpdateAndRefreshPreviews();
            }
            MenuHandler.CreateToggle(HideAllDecimal, "Hide ALL Decimals", menu, HideAllDecimalToggled, 30, color: easyChangeColor);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 20);


            // Number font size
            void NumberFontSizeChanged(float val)
            {
                NumberFontSize = val;
                PlayerDamageTracker.UpdateConfigs();
                UpdateAndRefreshPreviews();
            }
            MenuHandler.CreateSlider("Number Base Size", menu, 30, 4, 96, NumberFontSize, NumberFontSizeChanged, out Slider slider7, false, color: Color.white);
            SlidersToSync["NumberFontSize"].Add(slider7);

            void ScaleNumberSizeByValueChanged(float val)
            {
                ScaleNumberSizeByValue = val;
                PlayerDamageTracker.UpdateConfigs();
                UpdateAndRefreshPreviews();
            }
            MenuHandler.CreateSlider("Value-to-Size Scaling", menu, 30, 0.0f, 1.0f, ScaleNumberSizeByValue, ScaleNumberSizeByValueChanged, out Slider slider8, false, color: Color.white);
            SlidersToSync["ScaleNumberSizeByValue"].Add(slider8);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 20);


            // Number lifetime and consolidation
            void NumberLifetimeChanged(float val)
            {
                NumberLifetime = val;
                PlayerDamageTracker.UpdateConfigs();
            }
            MenuHandler.CreateSlider("Number Display Time", menu, 30, 0.5f, 5.0f, NumberLifetime, NumberLifetimeChanged, out Slider slider3, false, color: Color.white);
            SlidersToSync["NumberLifetime"].Add(slider3);

            void NumberOpaqueTimeChanged(float val)
            {
                NumberOpaqueTime = val;
                PlayerDamageTracker.UpdateConfigs();
            }
            MenuHandler.CreateSlider("Number Opaque Time", menu, 30, 0.0f, 5.0f, NumberOpaqueTime, NumberOpaqueTimeChanged, out Slider slider4, false, color: Color.white);
            SlidersToSync["NumberOpaqueTime"].Add(slider4);

            void NumberSumTimeWindowChanged(float val)
            {
                NumberSumTimeWindow = Mathf.Min(val, NumberLifetime);
                PlayerDamageTracker.UpdateConfigs();
            }
            MenuHandler.CreateSlider("Number Sum Time Window", menu, 30, 0.0f, 5.0f, NumberSumTimeWindow, NumberSumTimeWindowChanged, out Slider slider5, false, color: hardChangeColor);
            SlidersToSync["NumberSumTimeWindow"].Add(slider5);

            void NumberNewSumDelayChanged(float val)
            {
                NumberNewSumDelay = Mathf.Min(val, NumberLifetime);
                PlayerDamageTracker.UpdateConfigs();
            }
            MenuHandler.CreateSlider("New Number Sum Delay", menu, 30, 0.0f, 5.0f, NumberNewSumDelay, NumberNewSumDelayChanged, out Slider slider6, false, color: hardChangeColor);
            SlidersToSync["NumberNewSumDelay"].Add(slider6);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 20);


            // 'go back' events
            menu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(delegate ()
            {
                damageDemoMode = false;
                SyncOptionsMenus();
            });
            menu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                damageDemoMode = false;
                SyncOptionsMenus();
            });

        }

        // !! // primary + main methods

        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

        }
        void Start()
        {
            Unbound.RegisterClientSideMod(ModId);

            Unbound.RegisterCredits
            (
                ModName,
                new string[] { "Pudassassin, Creator of GearUp Cards", "[Root], UX feedback", "Digital-7 font by Sizenko Alexander" },
                new string[] { "github" },
                new string[] { "https://github.com/Pudassassin/CardMagnifier" }
            );
            
            // add GUI to modoptions menu
            Unbound.RegisterMenu
            (
                ModName,
                delegate ()
                { 
                    
                },
                NewGUI,
                showInPauseMenu: true
            );

            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnGameStart);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);

            // Action<Player> oldJoinAction = PlayerManager.instance.PlayerJoinedAction;
            // Action<Player> trackNewPlayer = (Player player) =>
            // {
            //     player.gameObject.GetOrAddComponent<PlayerDamageTracker>();
            // };
            // Delegate.Combine(oldJoinAction, trackNewPlayer);
            // Traverse.Create(PlayerManager.instance.PlayerJoinedAction).SetValue(oldJoinAction);
        }

        public void Update()
        {

        }


        // !! // Gamehook Methods
        IEnumerator OnGameStart(IGameModeHandler gm)
        {
            PlayerDamageTracker.UpdateConfigs();

            foreach (Player player in PlayerManager.instance.players)
            {
                PlayerDamageTracker tracker = player.gameObject.GetComponent<PlayerDamageTracker>();
                if (tracker != null)
                {
                    Destroy(tracker);
                }

                player.gameObject.AddComponent<PlayerDamageTracker>();
            }

            yield break;
        }

        IEnumerator OnPointStart(IGameModeHandler gm)
        {
            PlayerDamageTracker.stopTracking = false;

            yield break;
        }
        IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            PlayerDamageTracker.stopTracking = true;

            yield break;
        }

        // !! // assets and prefabs
        public static readonly AssetBundle MainAsset = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("dmgtracker_asset", typeof(DamageTracker).Assembly);

    }

}
