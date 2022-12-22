using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace QCommonLib.UI
{
    internal abstract class Frame
    {
        internal static readonly RectOffset Padding = new RectOffset(15, 15, 30, 15);
        private static UITextureAtlas s_atlas = null;

        internal abstract UIComponent Make(QToast toast, string name);

        internal static void InitPanel(UIComponent panel)
        {
            panel.isVisible = true;
            panel.autoSize = false;
            if (panel is UISprite s)
            {
                s.atlas = GetAtlas();
            }
            if (panel is UITiledSprite t)
            {
                t.atlas = GetAtlas();
            }
        }

        internal static UITextureAtlas GetAtlas()
        {
            if (s_atlas != null) return s_atlas;

            string[] spriteNames = new string[]
            {
                "Bottom",
                "BottomLeft",
                "Close",
                "CloseHover",
                "DownArrow",
                "Left",
                "Top",
                "TopLeft"
            };

            s_atlas = QTextures.CreateTextureAtlas("QToast", spriteNames, Assembly.GetAssembly(typeof(QToast)).GetName().Name + ".UI.Toast.");
            return s_atlas;
        }

        internal static void MakeMid(QToast toast)
        {
            toast.frame.Add("Mid", toast.frame["MidContainer"].AddUIComponent<UICanvasSprite>());
            UICanvasSprite mid = (UICanvasSprite)toast.frame["Mid"];
            InitPanel(mid);

            mid.relativePosition = new Vector3(Padding.left, 0);
            mid.size = new Vector2(toast.frame["MidContainer"].size.x - Padding.left - Padding.right, toast.frame["MidContainer"].size.y);
            mid.name = "QToast_Mid";
            Color32[] textureColours = new Color32[16];
            for (int i = 0; i < textureColours.Length; i++) textureColours[i] = new Color32(255, 243, 142, 255);
            Texture2D texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            texture.SetPixels32(0, 0, 4, 4, textureColours);
            texture.name = "MidTexture";
            texture.Apply();
            mid.texture = texture;
            mid.ResizeCanvas(0, 0, mid.size.x, mid.size.y);
        }

        internal static void MakeArrow(QToast toast, int arrowOffset)
        {
            toast.frame.Add("Arrow", toast.AddUIComponent<UISprite>());
            UISprite arrow = (UISprite)toast.frame["Arrow"];
            InitPanel(arrow);

            if (arrowOffset == -1)
            {
                arrow.isVisible = false;
            }
            else
            {
                arrow.isVisible = true;
                toast.size += new Vector2(0, 45);
                arrow.relativePosition = new Vector2(Padding.left + arrowOffset, toast.frame["BottomContainer"].relativePosition.y);
                arrow.spriteName = "DownArrow";
            }
        }
    }

    internal class FrameContainer : Frame
    {
        internal Vector3 relativePosition;
        internal Vector2 size;

        internal FrameContainer(Vector3 relativePosition, Vector2 size)
        {
            this.relativePosition = relativePosition;
            this.size = size;
        }

        internal override UIComponent Make(QToast toast, string name)
        {
            UIPanel container = toast.AddUIComponent<UIPanel>();
            InitPanel(container);
            container.name = "QToast_" + name;
            container.relativePosition = relativePosition;
            container.size = size;

            return container;
        }
    }

    internal class FrameSprite : Frame
    {
        internal string parentName;
        internal string spriteName;
        internal UISpriteFlip flip;
        internal UIHorizontalAlignment hAlign;
        internal UIVerticalAlignment vAlign;

        internal FrameSprite(string parentName, string spriteName, UISpriteFlip flip, UIHorizontalAlignment hAlign, UIVerticalAlignment vAlign)
        {
            this.parentName = parentName;
            this.spriteName = spriteName;
            this.flip = flip;
            this.hAlign = hAlign;
            this.vAlign = vAlign;
        }

        internal override UIComponent Make(QToast toast, string name)
        {
            UIComponent parent = toast.frame[parentName];
            UISprite sprite = parent.AddUIComponent<UISprite>();
            InitPanel(sprite);
            sprite.name = "QToast_" + name;

            int posX = 0, posY = 0, sizeX = 0, sizeY = 0;

            switch (hAlign)
            {
                case UIHorizontalAlignment.Left:
                    posX = 0;
                    sizeX = Padding.left;
                    break;

                case UIHorizontalAlignment.Center:
                    posX = Padding.left;
                    sizeX = (int)parent.size.x - Padding.left - Padding.right;
                    break;

                case UIHorizontalAlignment.Right:
                    posX = (int)parent.size.x - Padding.right;
                    sizeX = Padding.right;
                    break;
            }

            switch (vAlign)
            {
                case UIVerticalAlignment.Top:
                    sizeY = Padding.top;
                    break;

                case UIVerticalAlignment.Middle:
                    sizeY = (int)parent.size.y;
                    break;

                case UIVerticalAlignment.Bottom:
                    sizeY = Padding.bottom;
                    break;
            }

            sprite.relativePosition = new Vector3(posX, posY, 0);
            sprite.size = new Vector2(sizeX, sizeY);
            sprite.spriteName = spriteName;
            sprite.flip = flip;

            return sprite;
        }
    }
}
