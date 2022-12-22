using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace QCommonLib.UI
{
    internal class QToast : UIPanel
    {
        internal readonly Dictionary<string, UIComponent> frame = new Dictionary<string, UIComponent>();

        public QToast()
        {
            autoLayout = false;
            autoSize = false;
            atlas = Frame.GetAtlas();
            isVisible = false;

            //Frame.Add("TopContainer", this.AddUIComponent<UIPanel>());
            //Frame.Add("TopLeft", Frame["TopContainer"].AddUIComponent<UISprite>());
            //Frame.Add("Top", Frame["TopContainer"].AddUIComponent<UISprite>());
            //Frame.Add("TopRight", Frame["TopContainer"].AddUIComponent<UISprite>());

            //Frame.Add("MidContainer", this.AddUIComponent<UIPanel>());
            //Frame.Add("MidLeft", Frame["MidContainer"].AddUIComponent<UISprite>());
            //Frame.Add("MidRight", Frame["MidContainer"].AddUIComponent<UISprite>());

            //Frame.Add("BottomContainer", this.AddUIComponent<UIPanel>());
            //Frame.Add("BottomLeft", Frame["BottomContainer"].AddUIComponent<UISprite>());
            //Frame.Add("Bottom", Frame["BottomContainer"].AddUIComponent<UISprite>());
            //Frame.Add("BottomRight", Frame["BottomContainer"].AddUIComponent<UISprite>());

        }

        public void Initialise(Vector2 position, Vector2 size, int arrowOffset = -1)
        {
            absolutePosition = position;
            size.x = Mathf.Clamp(size.x, 100f, 500f);
            size.y = Mathf.Clamp(size.y, 80f, 400f);
            this.size = size;

            RectOffset FramePadding = Frame.Padding;

            Dictionary<string, Frame>  frameElements = new Dictionary<string, Frame>()
            {
                { "TopContainer",       new FrameContainer(Vector3.zero, new Vector2(size.x, FramePadding.top)) },
                { "MidContainer",       new FrameContainer(new Vector3(0, FramePadding.top), new Vector2(size.x, size.y - FramePadding.top - FramePadding.bottom)) },
                { "BottomContainer",    new FrameContainer(new Vector3(0, size.y - FramePadding.bottom), new Vector2(size.x, FramePadding.bottom)) },

                { "TopLeft",            new FrameSprite("TopContainer",     "TopLeft",     UISpriteFlip.None,              UIHorizontalAlignment.Left, UIVerticalAlignment.Top) },
                { "Top",                new FrameSprite("TopContainer",     "Top",         UISpriteFlip.None,              UIHorizontalAlignment.Center, UIVerticalAlignment.Top) },
                { "TopRight",           new FrameSprite("TopContainer",     "TopLeft",     UISpriteFlip.FlipHorizontal,    UIHorizontalAlignment.Right, UIVerticalAlignment.Top) },

                { "MidLeft",            new FrameSprite("MidContainer",     "Left",        UISpriteFlip.None,              UIHorizontalAlignment.Left, UIVerticalAlignment.Middle) },
                { "MidRight",           new FrameSprite("MidContainer",     "Left",        UISpriteFlip.FlipHorizontal,    UIHorizontalAlignment.Right, UIVerticalAlignment.Middle) },

                { "BottomLeft",         new FrameSprite("BottomContainer",  "BottomLeft",   UISpriteFlip.None,             UIHorizontalAlignment.Left, UIVerticalAlignment.Bottom) },
                { "Bottom",             new FrameSprite("BottomContainer",  "Bottom",       UISpriteFlip.None,             UIHorizontalAlignment.Center, UIVerticalAlignment.Bottom) },
                { "BottomRight",        new FrameSprite("BottomContainer",  "BottomLeft",   UISpriteFlip.FlipHorizontal,   UIHorizontalAlignment.Right, UIVerticalAlignment.Bottom) },
            };

            frame.Clear();
            foreach (var pair in frameElements)
            {
                frame.Add(pair.Key, pair.Value.Make(this, pair.Key));
            }

            Frame.MakeMid(this);
            Frame.MakeArrow(this, arrowOffset);

            UIButton closeBtn = this.AddUIComponent<UIButton>();
            closeBtn.atlas = Frame.GetAtlas();
            closeBtn.size = new Vector2(24, 24);
            closeBtn.relativePosition = new Vector2(size.x - 32, 8);
            closeBtn.normalFgSprite = "Close";
            closeBtn.hoveredFgSprite = "CloseHover";
            closeBtn.eventClicked += (c, p) => { this.Hide(); };
        }
    }
}
