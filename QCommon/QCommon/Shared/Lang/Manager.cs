using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

//
// LocalizeManager and ResxReader by MacSergey 
// https://github.com/MacSergey/ModsCommon/blob/master/LocalizeShared/LocalizeManager/Manager.cs
// https://github.com/MacSergey/ModsCommon/tree/master/LocalizeShared/VSIXLocalizeGenerator
//

namespace QCommonLib.Lang
{
    public class LocalizeManager
    {
        private string Name { get; }
        private string SubDirectory { get; }
        private string AssemblyPath { get; } = string.Empty;
        private Dictionary<string, LocalizeSet> Languages { get; } = new Dictionary<string, LocalizeSet>();

        public LocalizeManager(string name, Assembly assembly, string subDirectory = "")
        {
            Name = name;
            SubDirectory = subDirectory;

            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                try
                {
                    foreach (Assembly pluginAssembly in plugin.GetAssemblies())
                    {
                        if (pluginAssembly == assembly)
                        {
                            AssemblyPath = plugin.modPath;
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        public string GetString(string key, CultureInfo culture)
        {
            if (culture != null)
            {
                if (!Languages.ContainsKey(culture.Name))
                    Load(culture);

                while (culture != null)
                {
                    if (Languages.TryGetValue(culture.Name, out var language) && language.TryGetString(key, out var str))
                        return str;
                    else if (string.IsNullOrEmpty(culture.Name))
                        break;
                    else
                        culture = culture.Parent;
                }
            }

            return key;
        }

        private void Load(CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(culture.Name) && culture.Parent != null)
                Load(culture.Parent);

            if (!Languages.ContainsKey(culture.Name))
            {
                var file = GetLocaleFolder();
                if (string.IsNullOrEmpty(culture.Name))
                    file = Path.Combine(file, $"{Name}.resx");
                else
                    file = Path.Combine(file, $"{Name}.{culture.Name}.resx");

                var set = new LocalizeSet(file, culture);
                Languages[culture.Name] = set;
            }
        }

        public IEnumerable<string> GetSupportLocales()
        {
            if(!string.IsNullOrEmpty(AssemblyPath))
            {
                var localeFolder = GetLocaleFolder();
                if(Directory.Exists(localeFolder))
                {
                    foreach(var file in Directory.GetFiles(localeFolder, $"{Name}.*.resx"))
                    {
                        var locale = Path.GetFileNameWithoutExtension(file).Split('.').Last();
                        yield return locale;
                    }
                }
            }
        }

        private string GetLocaleFolder()
        {
            string localeFolder = Path.Combine(AssemblyPath, "Lang");
            if (SubDirectory != null && SubDirectory.Length > 0) localeFolder = Path.Combine(localeFolder, SubDirectory);
            return localeFolder;
        }
    }

    public class LocalizeSet
    {
        private CultureInfo Culture { get; }
        private Dictionary<string, string> Locales { get; } = new Dictionary<string, string>();

        public bool TryGetString(string key, out string str) => Locales.TryGetValue(key, out str);

        public LocalizeSet(string file, CultureInfo culture)
        {
            Culture = culture;

            try
            {
                var reader = new ResxReader(file);
                foreach (var item in reader)
                    Locales[item.Name] = item.Value;
            }
            catch { }
        }
    }
}
