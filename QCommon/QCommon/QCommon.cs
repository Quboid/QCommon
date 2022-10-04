using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Globalization;
using ColossalFramework.Globalization;

namespace QCommonLib
{
    public class QCommon
    {
        /// <summary>
        /// Get the assembly of another game mod
        /// </summary>
        /// <param name="modName">The IUserMod class name (lowercase)</param>
        /// <param name="assName">The assembly name (lowercase)</param>
        /// <param name="assNameExcept">An assembly name exception to skip even if matches previous parameter</param>
        /// <param name="onlyEnabled">Limit result to enabled mods?</param>
        /// <returns>Game mod's assembly</returns>
        public static Assembly GetAssembly(string modName, string assName, string assNameExcept = "", bool onlyEnabled = true)
        {
            foreach (PluginManager.PluginInfo pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                try
                {
                    if (pluginInfo.userModInstance?.GetType().Name.ToLower() == modName && (!onlyEnabled || pluginInfo.isEnabled))
                    {
                        if (assNameExcept.Length > 0)
                        {
                            if (pluginInfo.GetAssemblies().Any(mod => mod.GetName().Name.ToLower() == assNameExcept))
                            {
                                break;
                            }
                        }
                        foreach (Assembly assembly in pluginInfo.GetAssemblies())
                        {
                            if (assembly.GetName().Name.ToLower() == assName)
                            {
                                return assembly;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                } // If the plugin fails to process, move on to next plugin
            }

            return null;
        }

        /// <summary>
        /// Dump a list of all mods (assemblies and names) to the output log
        /// </summary>
        public static void DumpMods()
        {
            string msg = $"Mods:";
            foreach (PluginManager.PluginInfo pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                msg += $"\n{pluginInfo.name} ({pluginInfo.isEnabled}, {pluginInfo.userModInstance.GetType().Name}):\n  ";
                foreach (Assembly assembly in pluginInfo.GetAssemblies())
                {
                    msg += $"{assembly.GetName().Name.ToLower()}, ";
                }
            }
            UnityEngine.Debug.Log(msg);
        }

        /// <summary>
        /// Get the CultureInfo, correcting for invalid codes and adapting Chinese code
        /// </summary>
        /// <returns>CultureInfo to use for localisation</returns>
        public static CultureInfo GetCultureInfo()
        {
            string lang = SingletonLite<LocaleManager>.instance.language == "zh" ? "zh-cn" : SingletonLite<LocaleManager>.instance.language;
            if (!CultureInfo.GetCultures(CultureTypes.AllCultures).Any(c => c.Name.ToLower() == lang))
            {
                lang = DefaultSettings.localeID;
            }
            return new CultureInfo(lang);
        }
    }
}
