using ColossalFramework.UI;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace QCommonLib.UI
{
    internal class ToastFrame
    {
        internal static readonly RectOffset Padding = new RectOffset(15, 15, 30, 15);
        private static UITextureAtlas s_atlas = null;
        private static Texture2D s_midTexture = null;

        internal int ArrowOffset { get; private set; }
        internal QToast Toast { get; private set; }
        internal Dictionary<string, UIComponent> Frame { get; private set; } = new Dictionary<string, UIComponent>();

        internal ToastFrame(QToast toast, int arrowOffset)
        {
            Toast = toast;
            ArrowOffset = arrowOffset;

            Dictionary<string, FrameElement> elements = new Dictionary<string, FrameElement>()
            {
                { "TopContainer",       new FrameContainer(Vector3.zero, new Vector2(toast.size.x, Padding.top)) },
                { "MidContainer",       new FrameContainer(new Vector3(0, Padding.top), new Vector2(toast.size.x, toast.size.y - Padding.top - Padding.bottom)) },
                { "BottomContainer",    new FrameContainer(new Vector3(0, toast.size.y - Padding.bottom), new Vector2(toast.size.x, Padding.bottom)) },

                { "TopLeft",            new FrameSprite("TopContainer",     "TopLeft",      UISpriteFlip.None,              UIHorizontalAlignment.Left, UIVerticalAlignment.Top) },
                { "Top",                new FrameSprite("TopContainer",     "Top",          UISpriteFlip.None,              UIHorizontalAlignment.Center, UIVerticalAlignment.Top) },
                { "TopRight",           new FrameSprite("TopContainer",     "TopLeft",      UISpriteFlip.FlipHorizontal,    UIHorizontalAlignment.Right, UIVerticalAlignment.Top) },

                { "MidLeft",            new FrameSprite("MidContainer",     "Left",         UISpriteFlip.None,              UIHorizontalAlignment.Left, UIVerticalAlignment.Middle) },
                { "MidRight",           new FrameSprite("MidContainer",     "Left",         UISpriteFlip.FlipHorizontal,    UIHorizontalAlignment.Right, UIVerticalAlignment.Middle) },

                { "BottomLeft",         new FrameSprite("BottomContainer",  "BottomLeft",   UISpriteFlip.None,              UIHorizontalAlignment.Left, UIVerticalAlignment.Bottom) },
                { "Bottom",             new FrameSprite("BottomContainer",  "Bottom",       UISpriteFlip.None,              UIHorizontalAlignment.Center, UIVerticalAlignment.Bottom) },
                { "BottomRight",        new FrameSprite("BottomContainer",  "BottomLeft",   UISpriteFlip.FlipHorizontal,    UIHorizontalAlignment.Right, UIVerticalAlignment.Bottom) },
            };

            foreach (var pair in Frame)
            {
                pair.Value.parent.RemoveUIComponent(pair.Value);
            }
            Frame.Clear();
            foreach (var pair in elements)
            {
                Frame.Add(pair.Key, pair.Value.Make(this, pair.Key));
            }

            Frame.Add("Mid", MakeMid());
            Frame.Add("Arrow", MakeArrow());

            toast.CloseBtn = toast.AddUIComponent<UIButton>();
            toast.CloseBtn.name = toast.name + "_CloseBtn";
            toast.CloseBtn.atlas = GetAtlas();
            toast.CloseBtn.size = new Vector2(24, 24);
            toast.CloseBtn.relativePosition = new Vector2(toast.size.x - 32, 8);
            toast.CloseBtn.normalFgSprite = "Close";
            toast.CloseBtn.hoveredFgSprite = "CloseHover";
            toast.CloseBtn.eventClicked += (c, p) => { toast.Hide(); };

            toast.Title = toast.AddUIComponent<UILabel>();
            toast.Title.autoSize = false;
            toast.Title.name = toast.name + "_Title";
            toast.Title.relativePosition = new Vector2(0, 10);
            toast.Title.textColor = new Color32(0, 0, 0, 255);
            toast.Title.textScale = 1.2f;
            toast.Title.textAlignment = UIHorizontalAlignment.Center;
            toast.Title.text = "";

            toast.Body = toast.AddUIComponent<UILabel>();
            toast.Body.autoSize = false;
            toast.Body.name = toast.name + "_Body";
            toast.Body.relativePosition = new Vector2(Padding.left, Padding.top + 8);
            toast.Body.textColor = new Color32(10, 10, 12, 225);
            toast.Body.width = Toast.size.x - Padding.left - Padding.right;
            toast.Body.autoHeight = true;
            toast.Body.wordWrap = true;
            toast.Body.text = "";
        }

        //internal Vector4 GetBodySize()
        //{
        //    return new Vector4(Padding.left, Padding.top + 8, Toast.size.x - Padding.left - Padding.right, Toast.size.y - Padding.top - Padding.bottom - 8 - (ArrowOffset == -1 ? 0 : 45));
        //}

        internal void SetHeight()
        {
            UICanvasSprite mid = (UICanvasSprite)Frame["Mid"];
            float oldMidHeight = Frame["MidContainer"].height;
            float newMidHeight = Toast.Body.height + 8;
            float delta = oldMidHeight - newMidHeight;

            Frame["MidLeft"].height -= delta;
            Frame["Mid"].height -= delta;
            Frame["MidRight"].height -= delta;
            Frame["MidContainer"].height -= delta;
            Frame["BottomContainer"].relativePosition -= new Vector3(0, delta, 0);
            Frame["Arrow"].relativePosition -= new Vector3(0, delta, 0);
            Toast.height -= delta;

            mid.ResizeCanvas(0, 0, mid.size.x, mid.size.y);
            mid.texture = GetMidTexture();

            if (Toast.autoPanelVAlign == PanelVAlignment.Bottom)
            {
                Toast.relativePosition = new Vector3(Toast.relativePosition.x, Toast.relativePosition.y + delta, Toast.relativePosition.z);
            }
        }

        internal UICanvasSprite MakeMid()
        {
            UICanvasSprite mid = Frame["MidContainer"].AddUIComponent<UICanvasSprite>();
            InitPanel(mid);

            mid.relativePosition = new Vector3(Padding.left, 0);
            mid.size = new Vector2(Frame["MidContainer"].size.x - Padding.left - Padding.right, Frame["MidContainer"].size.y);
            mid.name = Toast.name + "_Mid";
            mid.texture = GetMidTexture();
            mid.ResizeCanvas(0, 0, mid.size.x, mid.size.y);

            return mid;
        }

        internal UISprite MakeArrow()
        {
            UISprite arrow = Toast.AddUIComponent<UISprite>();
            arrow.name = Toast.name + "_Arrow";
            InitPanel(arrow);

            if (ArrowOffset == -1)
            {
                arrow.isVisible = false;
            }
            else
            {
                arrow.isVisible = true;
                Toast.size += new Vector2(0, 45);
                arrow.relativePosition = new Vector2(Padding.left + ArrowOffset, Frame["BottomContainer"].relativePosition.y);
                arrow.spriteName = "DownArrow";
            }

            return arrow;
        }

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

        internal static Texture2D GetMidTexture()
        {
            if (s_midTexture == null)
            {

                Color32[] textureColours = new Color32[16];
                for (int i = 0; i < textureColours.Length; i++) textureColours[i] = new Color32(255, 243, 178, 255);
                Texture2D texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
                texture.SetPixels32(0, 0, 4, 4, textureColours);
                texture.name = "QToast_MidTexture";
                texture.Apply();
                s_midTexture = texture;
            }

            Texture2D copy = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            copy.LoadRawTextureData(s_midTexture.GetRawTextureData());
            copy.Apply();

            return copy;
        }
    }

    internal abstract class FrameElement
    {
        internal abstract UIComponent Make(ToastFrame frame, string name);
        protected static RectOffset Padding = ToastFrame.Padding;

    }

    internal class FrameContainer : FrameElement
    {
        internal Vector3 relativePosition;
        internal Vector2 size;

        internal FrameContainer(Vector3 relativePosition, Vector2 size)
        {
            this.relativePosition = relativePosition;
            this.size = size;
        }

        internal override UIComponent Make(ToastFrame frame, string name)
        {
            UIPanel container = frame.Toast.AddUIComponent<UIPanel>();
            ToastFrame.InitPanel(container);
            container.name = frame.Toast.name + "_" + name;
            container.relativePosition = relativePosition;
            container.size = size;

            return container;
        }
    }

    internal class FrameSprite : FrameElement
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

        internal override UIComponent Make(ToastFrame frame, string name)
        {
            UIComponent parent = frame.Frame[parentName];
            UISprite sprite = parent.AddUIComponent<UISprite>();
            ToastFrame.InitPanel(sprite);
            sprite.name = frame.Toast.name + "_" + name;

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

    internal enum PanelVAlignment
    {
        None,
        Top,
        Bottom
    }
}
