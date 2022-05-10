using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Wish;

namespace UIScale
{
    [BepInPlugin("p1xel8ted.sunhaven.uiscale", "UI Scale", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static Harmony _hi;
        private static ConfigEntry<bool> _modEnabled;
        private static ConfigEntry<float> _uiScale;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            _modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            _uiScale = Config.Bind<float>("Scale", "UIScale", 3, "UI scale");
            _hi = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        private static void OnDestroy()
        {
            //destroy self in preparation for reload when the (default script engine F6) key is pressed
            _hi?.UnpatchSelf();
        }


        [HarmonyPatch(typeof(AdaptiveUIScale), "LateUpdate")]
        private static class SetUiScalePatch
        {
            private static bool Prefix()
            {
                return !_modEnabled.Value;
            }

            private static void Postfix(AdaptiveUIScale __instance)
            {
                var newValue = _uiScale.Value; //get saved value
                var curValue = __instance.GetComponent<CanvasScaler>().scaleFactor; //get current value
                const int scale = 3;
                if (!_modEnabled.Value)
                {
                    //restore to default values if mod disabled
                    if (Screen.width < 1000)
                    {
                        __instance.GetComponent<CanvasScaler>().scaleFactor = (float)(scale - 2);
                    }
                    if (Screen.width < 1700)
                    {
                        __instance.GetComponent<CanvasScaler>().scaleFactor = (float)(scale - 1);
                    }
                    if (Screen.width < 3000)
                    {
                        __instance.GetComponent<CanvasScaler>().scaleFactor = (float)scale;
                    }
                    __instance.GetComponent<CanvasScaler>().scaleFactor = (float)(scale + 1);
                }
                else
                {
                    //don't bother changing if values practically the same
                    if (Math.Abs(newValue - curValue) < 0.01) return;
                    //don't use negative numbers
                    if (newValue < 0) return;
                    //set new scale
                    __instance.GetComponent<CanvasScaler>().scaleFactor = newValue;
                }
            }
        }
    }
}
