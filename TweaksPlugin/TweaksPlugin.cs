using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace IO_Tweaks
{
    [BepInPlugin(GUID, DisplayName, Version)]
    [BepInDependency(ConfigurationManager.ConfigurationManager.GUID, ConfigurationManager.ConfigurationManager.Version)]
    [BepInDependency("gravydevsupreme.xunity.autotranslator", BepInDependency.DependencyFlags.SoftDependency)]
    public class TweaksPlugin : BaseUnityPlugin
    {
        public const string Version = "1.0";
        public const string GUID = "IO_Tweaks";
        public const string DisplayName = "Translation fixes and other tweaks";

        internal static new ManualLogSource Logger;

        private static ConfigEntry<bool> _swapCameraKey;
        private static ConfigEntry<bool> _showDebugMode;
        private static ConfigEntry<bool> _showConfigManButton;

        private static ConfigurationManager.ConfigurationManager _configMan;
        private static GameObject _debugButton;
        private Harmony _hi;

        private static bool _isJp = true;

        private void Awake()
        {
            Logger = base.Logger;

            try
            {
                var lang = Traverse.CreateWithType("XUnity.AutoTranslator.Plugin.Core.AutoTranslatorSettings").Property<string>("DestinationLanguage").Value;
                _isJp = string.IsNullOrEmpty(lang) || lang.StartsWith("ja");
                Logger.LogInfo($"Running in {(_isJp ? "Japanese" : "non-Japanese")} mode");
            }
            catch (Exception e)
            {
                Logger.LogWarning("AutoTranslator is not installed or is outdated, some features will not work properly. Error: " + e.Message);
            }

            if (!_isJp)
                NativeMethods.SetWindowTitle("INSULT ORDER ~Cheeky nyan girl's menu of debauchery~");

            _configMan = (ConfigurationManager.ConfigurationManager)Chainloader.PluginInfos[ConfigurationManager.ConfigurationManager.GUID].Instance;

            _swapCameraKey = Config.Bind("General", "Fix camera rotate hotkey", !_isJp, "Swap camera rotate right button from the tilde key to the quote key.\nThe game is made for a Japanese keyboard layout where these two keys are swapped.");
            _swapCameraKey.SettingChanged += (sender, args) =>
            {
                _hi?.UnpatchSelf();
                _hi = Harmony.CreateAndPatchAll(typeof(Hooks));
            };

            _showDebugMode = Config.Bind("General", "Show debug mode button in the Title screen", false, "Show a 'Open debug mode' button on the title screen (in bottom left corner). Normally you have to 100% the game to unlock it.\nThe debug screen allows you to skip to any part of the story and change any parameter of the current save file.");
            _showDebugMode.SettingChanged += (sender, args) =>
            {
                if (_debugButton != null)
                    _debugButton.SetActive(_showDebugMode.Value);
            };

            _showConfigManButton = Config.Bind("General", "Show Plugin settings button in the Settings screen", true, "Changes take effect after a game restart.");

            _hi = Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")] private static extern bool SetWindowText(IntPtr hwnd, string lpString);
            [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();

            public static void SetWindowTitle(string name)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
                var activeWindow = GetActiveWindow();
                if (activeWindow != IntPtr.Zero)
                    SetWindowText(activeWindow, name);
                else 
                    Logger.LogWarning("Failed to change window title text: GetActiveWindow returned a null pointer");
            }
        }

        private static class Hooks
        {
            /// <summary>
            /// Show the debug button on the title screen
            /// </summary>
            [HarmonyPostfix]
            [HarmonyWrapSafe]
            [HarmonyPatch(typeof(TitleScript), nameof(TitleScript.Awake))]
            private static void TitleFadeHook(TitleScript __instance)
            {
                _debugButton = GameObject.Instantiate(__instance.version.gameObject, __instance.version.transform.parent);

                _debugButton.name = "DebugButton";

                var boxcollider = _debugButton.AddComponent<BoxCollider>();

                var label = _debugButton.GetComponent<UILabel>();
                label.text = "Open debug mode";
                label.fontSize = 10;
                label.alignment = NGUIText.Alignment.Left;
                label.pivot = UIWidget.Pivot.Left;
                label.UpdateNGUIText();
                label.ResizeCollider();

                boxcollider.isTrigger = true;
                boxcollider.tag = "PushBt";

                var btn = _debugButton.AddComponent<UIButton>();
                btn.mWidget = label;
                btn.isEnabled = true;
                btn.onClick.Add(new EventDelegate(() => OpenDebugMenu(__instance)));

                _debugButton.AddComponent<TweenScale>().enabled = false;
                var btnscale = _debugButton.AddComponent<UIButtonScale>();
                btnscale.hover = new Vector3(1.1f, 1.1f, 1.1f);
                btnscale.pressed = new Vector3(0.5f, 0.5f, 0.5f);
                btnscale.tweenTarget = _debugButton.transform;

                _debugButton.transform.localPosition = new Vector3(-622, -345, 0);
                _debugButton.SetActive(_showDebugMode.Value);

                void OpenDebugMenu(TitleScript titleScript)
                {
                    titleScript.DebugPanel = true;
                    titleScript.Bell.Play();
                    titleScript.MainTitle.duration = 0.7f;
                    titleScript.MainTitle.PlayReverse();
                    EventDelegate.Set(titleScript.MainTitle.onFinished, titleScript.TitleToChapter);
                    titleScript.Invoke("BgmPlay", 2f);
                }
            }

            /// <summary>
            /// Show the Plugin settings button in the Settings screen
            /// </summary>
            [HarmonyPostfix]
            [HarmonyWrapSafe]
            [HarmonyPatch(typeof(ConfigSetting), nameof(ConfigSetting.Start))]
            private static void ConfigSettingHook(ConfigSetting __instance)
            {
                if (!_showConfigManButton.Value) return;

                var bt3 = __instance.transform.Find("ConfBt03");
                var bt4 = __instance.transform.Find("ConfBt04");

                var newbt = GameObject.Instantiate(bt4.gameObject, bt4.parent);
                newbt.name = "ConfBtPluginSettings";

                newbt.transform.localPosition = new Vector3(bt4.localPosition.x, bt4.localPosition.y - Mathf.Abs(bt3.localPosition.y - bt4.localPosition.y), bt4.localPosition.z);

                var label = newbt.GetComponentInChildren<UILabel>();
                label.text = "Plugin settings";

                var button = newbt.GetComponent<UIButton>();
                button.onClick.Clear();

                button.onClick.Add(new EventDelegate(() => _configMan.DisplayingWindow = true));
            }

            /// <summary>
            /// Fix the game using wrong key for camera rotate right on non-Japanese keyboards
            /// </summary>
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(Aim2), nameof(Aim2.Update))]
            private static IEnumerable<CodeInstruction> FixBackQuoteKey(IEnumerable<CodeInstruction> instructions)
            {
                if (!_swapCameraKey.Value)
                    return instructions;

                return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)KeyCode.BackQuote))
                                                    .ThrowIfInvalid("No matches")
                                                    .Repeat(matcher => matcher.Operand = (sbyte)KeyCode.Quote)
                                                    .Instructions();
            }

            /// <summary>
            /// Look for local manual file and open it instead of the online manual
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ConfigSetting), nameof(ConfigSetting.OnlineManual))]
            private static bool OnlineManualHook(ConfigSetting __instance)
            {
                try
                {
                    var localUrl = Path.GetFullPath(Paths.GameRootPath + "/../Manual/" + (_isJp ? "manual_jp.html" : "manual_en.html"));
                    Logger.LogDebug("Trying to open " + localUrl);
                    if (File.Exists(localUrl))
                    {
                        Process.Start(new ProcessStartInfo(localUrl) { UseShellExecute = true });
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }

                return true;
            }

            /// <summary>
            /// Fix character names in backlog not being translated correctly, resulting in the entire backlog text being auto-translated
            /// </summary>
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(ADV_Loader), nameof(ADV_Loader.LineLoad))]
            private static IEnumerable<CodeInstruction> BackLogTranslationFix(IEnumerable<CodeInstruction> instructions)
            {
                if (_isJp) return instructions;

                return new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ADV_Loader), nameof(ADV_Loader.LogList))),
                                                                  new CodeMatch(OpCodes.Ldarg_0),
                                                                  new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ADV_Loader), nameof(ADV_Loader.Name))))
                                                    .ThrowIfInvalid("No matches")
                                                    .Set(OpCodes.Call, AccessTools.Method(typeof(Hooks), nameof(Hooks.GetTranslatedName)))
                                                    .Instructions();
            }
            private static string GetTranslatedName(ADV_Loader instance)
            {
                var name = instance.Name;
                return TranslationHelper.TryTranslate(name, out var nameTl) ? nameTl : name;
            }

            /// <summary>
            /// Fix backlog entries not appearing when it's first opened
            /// </summary>
            [HarmonyPostfix]
            [HarmonyWrapSafe]
            [HarmonyPatch(typeof(ADV_Loader), nameof(ADV_Loader.LogDis))]
            private static void BackLogDisplayFix(ADV_Loader __instance)
            {
                IEnumerator DelayedLogDisp()
                {
                    yield return null;
                    __instance.LogDisp();
                }
                __instance.StartCoroutine(DelayedLogDisp());
            }
        }
    }
}
