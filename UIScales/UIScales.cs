using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wish;


namespace UIScales;

[BepInPlugin("p1xel8ted.sunhaven.uiscales", "UI Scale", "0.1.0")]
public class UiScales : BaseUnityPlugin
{
    public static Harmony Hi;
    public static ConfigEntry<bool> ModEnabled;
    public static ConfigEntry<bool> UiModEnabled;
    public static ConfigEntry<bool> ZoomModEnabled;
    public static ConfigEntry<float> MainMenuUiScale;
    public static ConfigEntry<float> InGameUiScale;
    public static ConfigEntry<float> ZoomLevel;
    public static ManualLogSource Log;

    public void Awake()
    {
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        ModEnabled = Config.Bind("General", "ModEnabled", true, "Enable/disable this mod.");
        UiModEnabled = Config.Bind("General", "UiModEnabled", true, "Enable/disable UI adjustment.");
        ZoomModEnabled = Config.Bind("General", "ZoomModEnabled", true, "Enable/disable zoom adjustment.");
        InGameUiScale = Config.Bind<float>("Scale", "GameUIScale", 3, "UI scale while in game.");
        MainMenuUiScale = Config.Bind<float>("Scale", "MenuUIScale", 2, "UI scale while at the main menu.");
        ZoomLevel = Config.Bind<float>("Scale", "ZoomLevel", 2, "Zoom level while in game.");
        Hi = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        Log = new ManualLogSource("Log");
        BepInEx.Logging.Logger.Sources.Add(Log);

    }

    public static void OnDestroy()
    {
        Hi?.UnpatchSelf();
    }


    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.SetZoom))]
    public static class SetZoomPatch
    {
        public static void Prefix(ref float zoomLevel)
        {
            if (!ModEnabled.Value) return;
            if (!ZoomModEnabled.Value) return;
            if (zoomLevel < 1) return;
            zoomLevel = ZoomLevel.Value;
        }

        public static void Postfix()
        {
        }
    }

    [HarmonyPatch(typeof(AdaptiveUIScale), "LateUpdate")]
    public static class SetUiScalePatch
    {
        public static bool Prefix()
        {
            return !ModEnabled.Value;
        }

        public static void Postfix(AdaptiveUIScale __instance)
        {
            if (!ModEnabled.Value) return;

            var scaler = __instance.GetComponent<CanvasScaler>();
            var newGameValue = InGameUiScale.Value; //get saved value
            var newMenuValue = MainMenuUiScale.Value;

            if (!UiModEnabled.Value) return;
            if (SceneManager.GetActiveScene().name.Contains("MainMenu"))
            {
                if (newMenuValue < 0) return;
                scaler.scaleFactor = newMenuValue;
            }
            else
            {
                if (newGameValue < 0) return;
                scaler.scaleFactor = newGameValue;
            }
        }
    }
}