using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections;
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

    public static class QTextures
    { 
        public static UITextureAtlas CreateTextureAtlas(Assembly assembly, string atlasName, string[] spriteNames, string assemblyPath)
        {
            int maxSize = 1024;
            Texture2D texture2D = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
            Texture2D[] textures = new Texture2D[spriteNames.Length];
            Rect[] regions;

            for (int i = 0; i < spriteNames.Length; i++)
                textures[i] = LoadTextureFromAssembly(assembly, assemblyPath + spriteNames[i] + ".png");

            regions = texture2D.PackTextures(textures, 2, maxSize);

            UITextureAtlas textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            Material material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            material.mainTexture = texture2D;
            textureAtlas.material = material;
            textureAtlas.name = atlasName;

            for (int i = 0; i < spriteNames.Length; i++)
            {
                UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo
                {
                    name = spriteNames[i],
                    texture = textures[i],
                    region = regions[i],
                };

                textureAtlas.AddSprite(item);
            }

            return textureAtlas;
        }

        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false)
        {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++)
            {
                Texture2D texture2D = atlas.sprites[i].texture;

                if (locked)
                {
                    // Locked textures workaround
                    RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    RenderTexture active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++)
            {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo
                {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        public static UITextureAtlas GetAtlas(string name)
        {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++)
            {
                if (atlases[i].name == name)
                    return atlases[i];
            }

            return UIView.GetAView().defaultAtlas;
        }

        private static Texture2D LoadTextureFromAssembly(Assembly assembly, string path)
        {
            Stream manifestResourceStream = assembly.GetManifestResourceStream(path);

            byte[] array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);

            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D.LoadImage(array);

            return texture2D;
        }
    }
}
