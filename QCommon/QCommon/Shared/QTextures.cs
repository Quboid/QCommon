﻿using ColossalFramework.Threading;
using ColossalFramework.UI;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace QCommonLib
{
    public static class QTextures
    {
        public static UITextureAtlas CreateEmptyTextureAtlas(string atlasName, int maxSize = 1024)
        {
            UITextureAtlas textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            textureAtlas.material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            textureAtlas.material.mainTexture = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
            textureAtlas.name = atlasName;
            textureAtlas.padding = 2;
            return textureAtlas;
        }

        public static Texture2D GetTextureFromAtlas(UITextureAtlas atlas, string name)
        {
            foreach (UITextureAtlas.SpriteInfo sprite in atlas.sprites)
            {
                if (sprite == null) continue;
                if (sprite.texture == null) continue;
                if (sprite.texture.name == null) continue;
                if (sprite.texture.name == name) return sprite.texture;
            }
            throw new System.Exception($"Sprite {name} not found.");
        }

        public static UITextureAtlas CreateTextureAtlas(string atlasName, string[] spriteNames, string assemblyPath)
        {
            int maxSize = 1024;
            Texture2D texture2D = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
            Texture2D[] textures = new Texture2D[spriteNames.Length];
            Rect[] regions;

            for (int i = 0; i < spriteNames.Length; i++)
                textures[i] = LoadTextureFromAssembly(Assembly.GetCallingAssembly(), assemblyPath + spriteNames[i] + ".png");

            regions = texture2D.PackTextures(textures, 2, maxSize);

            UITextureAtlas textureAtlas = CreateEmptyTextureAtlas(atlasName);
            textureAtlas.material.mainTexture = texture2D;

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

        public static Texture2D LoadTextureFromAssembly(Assembly assembly, string path)
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