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
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    // [BepInDependency("pykess.rounds.plugins.pickncards", BepInDependency.DependencyFlags.HardDependency)]

    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]

    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class DamageTracker : BaseUnityPlugin
    {
        private const string ModId = "com.pudassassin.rounds.DamageTracker";
        private const string ModName = "Damage Tracker";
        private const string Version = "0.0.1"; //build #2 / Release 0-1-0

        private const string CompatibilityModName = "DamageTracker";

        // public static GameObject timerUI = null;
        // public static Vector3 timerUIPos = new Vector3(-200.0f, -400.0f, 0.0f);

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
        // public static float ZoomScale
        // {
        //     get
        //     {
        //         return GetFloat("ZoomScale", 1.35f);
        //     }
        //     set
        //     {
        //         SetFloat("ZoomScale", value);
        //     }
        // }
        // public static bool ZoomToAbsoluteSize
        // { 
        //     get
        //     {
        //         return GetBool("ZoomToAbsoluteSize", true);
        //     }
        //     set
        //     {
        //         SetBool("ZoomToAbsoluteSize", value);
        //     }
        // }


        // !! // other variables

        public const float RealtimeToRefresh = 0.05f;
        public static float RealtimeLastRefreshed;

        public static bool damageDemoMode = false;

        // public static int PickTimerSearchCount = 5;

        internal static Dictionary<string, List<Toggle>> TogglesToSync = new Dictionary<string, List<Toggle>>();
        internal static Dictionary<string, List<Slider>> SlidersToSync = new Dictionary<string, List<Slider>>();

        // presets
        private static void VanillaPlusPreset()
        {

        }


        private static void InitializeOptionsDictionaries()
        {
            // if (!TogglesToSync.Keys.Contains("DisableCardParticleAnimations")) { TogglesToSync["DisableCardParticleAnimations"] = new List<Toggle>() { }; }
            // if (!SlidersToSync.Keys.Contains("NumberOfGeneralParticles")){ SlidersToSync["NumberOfGeneralParticles"] = new List<Slider>(){};}

        }
        private static void SyncOptionsMenus(int recurse = 3)
        {
            // foreach (Toggle toggle in TogglesToSync["DisableCardParticleAnimations"]) { toggle.isOn = DisableCardParticleAnimations; }
            // foreach (Slider slider in SlidersToSync["NumberOfGeneralParticles"]) { slider.value = NumberOfGeneralParticles; }

            if (recurse > 0) { SyncOptionsMenus(recurse - 1); }
        }

        // preview
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

            // SyncOptionsMenus();
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

        // Main\Presets
        private static void PresetsMenu(GameObject menu)
        {
            // MenuHandler.CreateButton("Vanilla+", menu, VanillaPlusPreset, 60, color: easyChangeColor);
            // MenuHandler.CreateText("* motion-sickness warning", menu, out TextMeshProUGUI _, 30);
            // MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 20);

            // MenuHandler.CreateButton("Reduce Card Motions", menu, ReduceMotionOverride, 60, color: easyChangeColor);
            // MenuHandler.CreateText("(disable motion-sickness inducing motions in the current setting)", menu, out TextMeshProUGUI _, 20);
            // MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);

            void ToggleDemo()
            {
                damageDemoMode = !damageDemoMode;
            }
            MenuHandler.CreateButton("Toggle Demo", menu, ToggleDemo, 30);
            MenuHandler.CreateText("RMB-drag to move demo card around, NUMPAD+/- to scale \'starting size\', NUMPAD* to reset to default scale.", menu, out TextMeshProUGUI _, 20);
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
        private static void DamageTrackerSetting(GameObject menu)
        {
            MenuHandler.CreateText("Damage Tracker Setting", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);


            // void ZoomScaleChanged(float val)
            // {
            //     ZoomScale = (float)val;
            //     UpdateAndRefreshCardZoom();
            // }
            // MenuHandler.CreateSlider("Card Zoom Scale", menu, 30, 0.5f, 5.0f, ZoomScale, ZoomScaleChanged, out Slider slider4, false, color: easyChangeColor);
            // SlidersToSync["ZoomScale"].Add(slider4);
            // 
            // void ZoomToAbsoluteSizeChanged(bool val)
            // {
            //     ZoomToAbsoluteSize = (bool)val;
            //     UpdateAndRefreshCardZoom();
            // }
            // Toggle toggle1 = MenuHandler.CreateToggle(ZoomToAbsoluteSize, "Zoom Card to Fixed Size", menu, ZoomToAbsoluteSizeChanged, 20, color: easyChangeColor).GetComponent<Toggle>();
            // TogglesToSync["ZoomToAbsoluteSize"].Add(toggle1);



            void ToggleDemo()
            {
                damageDemoMode = !damageDemoMode;
            }
            MenuHandler.CreateButton("Toggle Demo", menu, ToggleDemo, 30);
            MenuHandler.CreateText("RMB-drag to move demo card around, NUMPAD+/- to scale \'starting size\', NUMPAD* to reset to default scale.", menu, out TextMeshProUGUI _, 20);
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

        // !! // primary + main methods

        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            // CustomCard.BuildCard<MyCardName>();
            Unbound.RegisterClientSideMod(ModId);

            // Unbound.RegisterCredits
            // (
            //     ModName,
            //     new string[] { "Pudassassin, Creator of GearUp Cards", "Willuwontu (coding guide)", "[Root] (UX testing and suggestion)" },
            //     new string[] { "github" },
            //     new string[] { "https://github.com/Pudassassin/CardMagnifier" }
            // );
            // 
            // // add GUI to modoptions menu
            // Unbound.RegisterMenu
            // (
            //     ModName,
            //     delegate ()
            //     { 
            //         
            //     },
            //     NewGUI,
            //     showInPauseMenu: true
            // );

            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnGameStart);

        }

        public void Update()
        {
            
        }

        IEnumerator OnGameStart(IGameModeHandler gm)
        {
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

        // public IEnumerator OnPlayerPickStart(IGameModeHandler gm)
        // {
        //     // UnityEngine.Debug.Log("[DamageTracker] Player Picking Started");
        // 
        //     yield break;
        // }

        // public IEnumerator OnPointStart(IGameModeHandler gm)
        // {
        //     // UnityEngine.Debug.Log("[CardMagnifier] Point Start");
        // 
        //     yield break;
        // }

        // public IEnumerator OnPlayerPickEnd(IGameModeHandler gm)
        // {
        //     UnityEngine.Debug.Log("[CardMagnifier] Player Picking Ended");
        //     CardEnlarger.isCardPickPhase = false;
        // 
        //     yield break;
        // }

        // !! // assets and prefabs
        public static readonly AssetBundle MainAsset = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("dmgtracker_asset", typeof(DamageTracker).Assembly);

    }

}
