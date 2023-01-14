using ColossalFramework;
using ColossalFramework.Plugins;
using static ColossalFramework.Plugins.PluginManager;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using System.Reflection;
using UnityEngine.Networking;

namespace QCommonLib
{
    public class QIncompatible
    {

        private QLogger Log;
        private Dictionary<ulong, string> m_incompatibleMods;
        private List<string> m_globalIncompatibleMods;

        public QIncompatible(Dictionary<ulong, string> incompatible, QLogger Log)
        {
            this.Log = Log;
            m_incompatibleMods = incompatible;
            m_globalIncompatibleMods = GetGlobalIncompatibleMods();

            Dictionary<PluginInfo, IncompatibleMod> found = Scan();

            if (found.Count > 0)
            {
                IncompatibleModsPanel panel = UIView.GetAView().AddUIComponent(typeof(IncompatibleModsPanel)) as IncompatibleModsPanel;
                panel.Initialize(found, Log);
                UIView.PushModal(panel);
                UIView.SetFocus(panel);
            }
        }

        internal Dictionary<PluginInfo, IncompatibleMod> Scan()
        {
            Dictionary<PluginInfo, IncompatibleMod> found = new Dictionary<PluginInfo, IncompatibleMod>();
            string logMsg = "";

            foreach (PluginInfo plugin in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (plugin == null) continue;
                if (plugin.isCameraScript) continue;
                if (plugin.userModInstance == null) continue;

                IncompatibleMod mod = new IncompatibleMod()
                {
                    steamId = plugin.publishedFileID.AsUInt64,
                    name = GetPluginName(plugin)
                };

                // Global incompatible mods fail even if not enabled
                if (m_globalIncompatibleMods.Where(m => mod.name.Contains(m)).Count() > 0)
                {
                    found.Add(plugin, mod);
                    logMsg += $"\n  \"{mod}\"";
                }

                if (!plugin.isEnabled) continue;
                if (m_incompatibleMods.ContainsKey(mod.steamId))
                {
                    found.Add(plugin, mod);
                    logMsg += $"\n  \"{mod}\"";
                }

            }

            Log.Info($"Found {found.Count} incompatible mods", "[Q01]");
            if (found.Count > 0) Log.Debug($"Incompatible mods:{logMsg}", "[Q02]");

            return found;
        }

        private string GetPluginName(PluginInfo plugin)
        {
            // String.IsNullOrEmpty(plugin.name) ? "(name unknown)" : plugin.name;
            return ((IUserMod)plugin.userModInstance).Name;
        }

        /// <summary>
        /// A list of mods that are known to be incompatible with the game
        /// </summary>
        /// <returns>List of partial Display Name</returns>
        private static List<string> GetGlobalIncompatibleMods()
        {
            return new List<string>
            {
                "Harmony (redesigned)"
            };
        }
    }

    public class IncompatibleMod
    {
        private const ulong LOCAL_MOD = ulong.MaxValue;

        public string name;
        public ulong steamId;

        public override string ToString()
        {
            return String.Format("{0,-10}:{1}", (steamId == LOCAL_MOD ? "Local" : steamId.ToString()), name);
        }
    }

    /// <summary>
    /// IncompatibleModsPanel, based on code from TMPE
    /// Thank you Krzychu124
    /// </summary>
    public class IncompatibleModsPanel : UIPanel
    {
        private const ulong LOCAL_MOD = ulong.MaxValue;

        private QLogger Log;
        private UILabel title;
        private UIButton closeBtn;
        private UISprite warningIcon;
        private UIPanel mainPanel;
        //private UICheckBox runModsCheckerOnStartup;
        private UIComponent blurEffect;
        private bool modListChanged;

        public Dictionary<PluginInfo, IncompatibleMod> IncompatibleMods { get; set; }

        /// <summary>
        /// Initialises the dialog, populates it with list of incompatible mods, and adds it to the modal stack.
        /// If the modal stack was previously empty, a blur effect is added over the screen background.
        /// </summary>
        public void Initialize(Dictionary<PluginInfo, IncompatibleMod> incompatibleMods, QLogger log)
        {
            IncompatibleMods = incompatibleMods;
            Log = log;

            Log.Debug("IncompatibleModsPanel initialize", "[Q03]");
            mainPanel?.OnDestroy();

            modListChanged = false;
            isVisible = true;

            mainPanel = AddUIComponent<UIPanel>();
            mainPanel.backgroundSprite = "UnlockingPanel2";
            mainPanel.color = new Color32(75, 75, 135, 255);
            width = 600;
            height = 350;
            mainPanel.width = 600;
            mainPanel.height = 350;

            Vector2 resolution = UIView.GetAView().GetScreenResolution();
            relativePosition = new Vector3((resolution.x / 2) - 300, resolution.y / 3);
            mainPanel.relativePosition = Vector3.zero;

            warningIcon = mainPanel.AddUIComponent<UISprite>();
            warningIcon.size = new Vector2(40f, 40f);
            warningIcon.spriteName = "IconWarning";
            warningIcon.relativePosition = new Vector3(15, 15);
            warningIcon.zOrder = 0;

            title = mainPanel.AddUIComponent<UILabel>();
            title.autoSize = true;
            title.padding = new RectOffset(10, 10, 15, 15);
            title.relativePosition = new Vector2(60, 12);

            title.text = "QCommon Incompatible Mods Check";

            closeBtn = mainPanel.AddUIComponent<UIButton>();
            closeBtn.eventClick += CloseButtonClick;
            closeBtn.relativePosition = new Vector3(width - closeBtn.width - 45, 15f);
            closeBtn.normalBgSprite = "buttonclose";
            closeBtn.hoveredBgSprite = "buttonclosehover";
            closeBtn.pressedBgSprite = "buttonclosepressed";

            UIPanel panel = mainPanel.AddUIComponent<UIPanel>();
            panel.relativePosition = new Vector2(20, 70);
            panel.size = new Vector2(565, 270);

            //UIHelper helper = new UIHelper(mainPanel);
            //string checkboxLabel = Translation.ModConflicts.Get("Checkbox:Scan for known incompatible mods on startup");
            //runModsCheckerOnStartup = helper.AddCheckbox(
            //                              checkboxLabel,
            //                              GlobalConfig.Instance.Main.ScanForKnownIncompatibleModsAtStartup,
            //                              RunModsCheckerOnStartup_eventCheckChanged) as UICheckBox;
            //runModsCheckerOnStartup.relativePosition = new Vector3(20, height - 30f);

            UIScrollablePanel scrollablePanel = panel.AddUIComponent<UIScrollablePanel>();
            scrollablePanel.backgroundSprite = string.Empty;
            scrollablePanel.size = new Vector2(550, 290);
            scrollablePanel.relativePosition = new Vector3(0, 0);
            scrollablePanel.clipChildren = true;
            scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            scrollablePanel.autoLayout = true;

            // Populate list of incompatible mods
            if (IncompatibleMods.Count != 0)
            {
                IncompatibleMods.ForEach(
                    pair => { CreateEntry(ref scrollablePanel, pair.Value, pair.Key); });
            }

            scrollablePanel.FitTo(panel);
            scrollablePanel.scrollWheelDirection = UIOrientation.Vertical;
            scrollablePanel.builtinKeyNavigation = true;

            UIScrollbar verticalScroll = panel.AddUIComponent<UIScrollbar>();
            verticalScroll.stepSize = 1;
            verticalScroll.relativePosition = new Vector2(panel.width - 15, 0);
            verticalScroll.orientation = UIOrientation.Vertical;
            verticalScroll.size = new Vector2(20, 270);
            verticalScroll.incrementAmount = 25;
            verticalScroll.scrollEasingType = EasingType.BackEaseOut;

            scrollablePanel.verticalScrollbar = verticalScroll;

            UISlicedSprite track = verticalScroll.AddUIComponent<UISlicedSprite>();
            track.spriteName = "ScrollbarTrack";
            track.relativePosition = Vector3.zero;
            track.size = new Vector2(16, 270);

            verticalScroll.trackObject = track;

            UISlicedSprite thumb = track.AddUIComponent<UISlicedSprite>();
            thumb.spriteName = "ScrollbarThumb";
            thumb.autoSize = true;
            thumb.relativePosition = Vector3.zero;
            verticalScroll.thumbObject = thumb;

            // Add blur effect if applicable
            blurEffect = GameObject.Find("ModalEffect").GetComponent<UIComponent>();
            AttachUIComponent(blurEffect.gameObject);
            blurEffect.size = new Vector2(resolution.x, resolution.y);
            blurEffect.absolutePosition = new Vector3(0, 0);
            blurEffect.SendToBack();
            blurEffect.eventPositionChanged += OnBlurEffectPositionChange;
            blurEffect.eventZOrderChanged += OnBlurEffectZOrderChange;
            blurEffect.opacity = 0;
            blurEffect.isVisible = true;
            ValueAnimator.Animate(
                "ModalEffect",
                val => blurEffect.opacity = val,
                new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));

            // Make sure modal dialog is in front of all other UI
            BringToFront();
        }

        private void OnBlurEffectPositionChange(UIComponent component, Vector2 position)
        {
            blurEffect.absolutePosition = Vector3.zero;
        }

        private void OnBlurEffectZOrderChange(UIComponent component, int value)
        {
            blurEffect.zOrder = 0;
            mainPanel.zOrder = 1000;
        }

        /// <summary>
        /// Allows the user to press "Esc" to close the dialog.
        /// </summary>
        ///
        /// <param name="p">Details about the key press.</param>
        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return))
            {
                TryPopModal();
                p.Use();
                Hide();
                Unfocus();
            }

            base.OnKeyDown(p);
        }

        /// <summary>
        /// Hnadles click of the "Run incompatible check on startup" checkbox and updates game options accordingly.
        /// </summary>
        ///
        /// <param name="value">The new value of the checkbox; <c>true</c> if checked, otherwise <c>false</c>.</param>
        //private void RunModsCheckerOnStartup_eventCheckChanged(bool value)
        //{
        //    Log.Debug("Incompatible mods checker run on game launch changed to " + value);
        //    GeneralTab.SetScanForKnownIncompatibleMods(value);
        //}

        /// <summary>
        /// Handles click of the "close dialog" button; pops the dialog off the modal stack.
        /// </summary>
        ///
        /// <param name="component">Handle to the close button UI component (not used).</param>
        /// <param name="eventparam">Details about the click event.</param>
        private void CloseButtonClick(UIComponent component, UIMouseEventParameter eventparam)
        {
            CloseDialog();
            eventparam.Use();
        }

        /// <summary>
        /// Pops the popup dialog off the modal stack.
        /// </summary>
        private void CloseDialog()
        {
            closeBtn.eventClick -= CloseButtonClick;
            TryPopModal();
            Hide();
            Unfocus();
            if (modListChanged)
            {
                ShowInfoAboutRestart();
            }
        }

        private void ShowInfoAboutRestart()
        {
            ExceptionPanel exceptionPanel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            exceptionPanel.SetMessage(
                title: "Game restart required",
                message: "List of mods changed (deleted or unsubscribed).\n" +
                         "Please restart the game to complete operation",
                error: false);
        }

        /// <summary>
        /// Creates a panel representing the mod and adds it to the <paramref name="parent"/> UI component.
        /// </summary>
        ///
        /// <param name="parent">The parent UI component that the panel will be added to.</param>
        /// <param name="mod">The name of the mod, which is displayed to user.</param>
        /// <param name="plugin">The <see cref="PluginInfo"/> instance of the incompatible mod.</param>
        private void CreateEntry(ref UIScrollablePanel parent, IncompatibleMod mod, PluginInfo plugin)
        {
            string caption = plugin.publishedFileID.AsUInt64 == LOCAL_MOD ? "Delete mod" : "Unsubscribe mod";

            UIPanel panel = parent.AddUIComponent<UIPanel>();
            panel.size = new Vector2(560, 50);
            panel.backgroundSprite = "ContentManagerItemBackground";

            UILabel label = panel.AddUIComponent<UILabel>();
            label.text = mod.name;
            label.textAlignment = UIHorizontalAlignment.Left;
            label.relativePosition = new Vector2(10, 15);

            CreateButton(panel, caption, (int)panel.width - 170, 10,
                (component, param) => UnsubscribeClick(component, param, plugin));
        }

        /// <summary>
        /// Handles click of "Unsubscribe" or "Delete" button; removes the associated mod and updates UI.
        ///
        /// Once all incompatible mods are removed, the dialog will be closed automatically.
        /// </summary>
        ///
        /// <param name="component">A handle to the UI button that was clicked.</param>
        /// <param name="eventparam">Details of the click event.</param>
        /// <param name="mod">The <see cref="PluginInfo"/> instance of the mod to remove.</param>
        private void UnsubscribeClick(UIComponent component, UIMouseEventParameter eventparam, PluginInfo mod)
        {
            eventparam.Use();
            bool success;

            // disable button to prevent accidental clicks
            component.isEnabled = false;
            Log.Info($"Removing incompatible mod '{mod.name}' from {mod.modPath}");

            success = mod.publishedFileID.AsUInt64 == LOCAL_MOD ? DeleteLocalMod(mod) : PlatformService.workshop.Unsubscribe(mod.publishedFileID);

            if (success)
            {
                modListChanged = true;
                IncompatibleMods.Remove(mod);
                component.parent.Disable();
                component.isVisible = false;

                // automatically close the dialog if no more mods to remove
                if (IncompatibleMods.Count == 0)
                {
                    CloseDialog();
                }
            }
            else
            {
                Log.Warning($"Failed to remove mod '{mod.name}'");
                component.isEnabled = true;
            }
        }

        /// <summary>
        /// Deletes a locally installed TM:PE mod.
        /// </summary>
        /// <param name="mod">The <see cref="PluginInfo"/> associated with the mod that needs deleting.</param>
        /// <returns>Returns <c>true</c> if successfully deleted, otherwise <c>false</c>.</returns>
        private bool DeleteLocalMod(PluginInfo mod)
        {
            try
            {
                string modPath = mod.modPath;
                Log.Debug($"Deleting local mod from {modPath}");
                if (modPath.Contains($"Files{Path.DirectorySeparatorChar}Mods"))
                {
                    // mods located in /Files/Mods are not monitored,
                    // game will not unload them automatically after removing mod directory
                    MethodInfo removeAtPath = typeof(PluginManager).GetMethod(
                        name: "RemovePluginAtPath",
                        bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic);
                    removeAtPath.Invoke(PluginManager.instance, new object[] { modPath });
                }

                DirectoryUtils.DeleteDirectory(modPath);
                Log.Debug($"Successfully deleted mod from {modPath}");
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Creates an `Unsubscribe` or `Delete` button (as applicable to mod location) and attaches
        ///     it to the <paramref name="parent"/> UI component.
        /// </summary>
        /// <param name="parent">The parent UI component which the button will be attached to.</param>
        /// <param name="text">The translated text to display on the button.</param>
        /// <param name="x">The x position of the top-left corner of the button, relative to
        ///     <paramref name="parent"/>.</param>
        /// <param name="y">The y position of the top-left corner of the button, relative to
        ///     <paramref name="parent"/>.</param>
        /// <param name="eventClick">The event handler for when the button is clicked.</param>
        private void CreateButton(UIComponent parent, string text, int x, int y, MouseEventHandler eventClick)
        {
            var button = parent.AddUIComponent<UIButton>();
            button.textScale = 0.8f;
            button.width = 150f;
            button.height = 30;
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.playAudioEvents = true;
            button.text = text;
            button.relativePosition = new Vector3(x, y);
            button.eventClick += eventClick;
        }

        /// <summary>
        /// Pops the dialog from the modal stack. If no more modal dialogs are present, the
        /// background blur effect is also removed.
        /// </summary>
        private void TryPopModal()
        {
            if (UIView.HasModalInput())
            {
                UIView.PopModal();
                UIComponent component = UIView.GetModalComponent();
                if (component != null)
                {
                    UIView.SetFocus(component);
                }
            }

            if (blurEffect != null && UIView.ModalInputCount() == 0)
            {
                blurEffect.eventPositionChanged -= OnBlurEffectPositionChange;
                blurEffect.eventZOrderChanged -= OnBlurEffectZOrderChange;
                ValueAnimator.Animate(
                    "ModalEffect",
                    val => blurEffect.opacity = val,
                    new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut),
                    () => blurEffect.Hide());
            }
        }
    }
}
