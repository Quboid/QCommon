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
        private readonly RectOffset FramePadding = new RectOffset(15, 15, 30, 15);
        private static UITextureAtlas s_atlas = null;

        private readonly Dictionary<string, UIComponent> Frame = new Dictionary<string, UIComponent>();

        public QToast()
        {
            autoLayout = false;
            autoSize = false;
            atlas = GetAtlas();
            isVisible = false;

            Frame.Add("TopContainer", this.AddUIComponent<UIPanel>());
            Frame.Add("TopLeft", Frame["TopContainer"].AddUIComponent<UISprite>());
            Frame.Add("Top", Frame["TopContainer"].AddUIComponent<UITiledSprite>());
            Frame.Add("TopRight", Frame["TopContainer"].AddUIComponent<UISprite>());

            Frame.Add("MidContainer", this.AddUIComponent<UIPanel>());
            Frame.Add("MidLeft", Frame["MidContainer"].AddUIComponent<UITiledSprite>());
            Frame.Add("Mid", Frame["MidContainer"].AddUIComponent<UICanvasSprite>());
            Frame.Add("MidRight", Frame["MidContainer"].AddUIComponent<UITiledSprite>());

            Frame.Add("BottomContainer", this.AddUIComponent<UIPanel>());
            Frame.Add("BottomLeft", Frame["BottomContainer"].AddUIComponent<UISprite>());
            Frame.Add("Bottom", Frame["BottomContainer"].AddUIComponent<UITiledSprite>());
            Frame.Add("BottomRight", Frame["BottomContainer"].AddUIComponent<UISprite>());
        }

        public void Initialise(Vector2 position, Vector2 size)
        {
            absolutePosition = position;
            this.size = size;

            foreach (var pair in Frame) InitPanel(pair.Value);

            Frame["TopContainer"].relativePosition = Vector3.zero;
            Frame["TopContainer"].size = new Vector2(size.x, FramePadding.top);
            Frame["TopContainer"].name = "QToast_TopContainer";
            Frame["TopLeft"].relativePosition = Vector3.zero;
            Frame["TopLeft"].size = new Vector2(FramePadding.left, FramePadding.top);
            Frame["TopLeft"].name = "QToast_TopLeft";
            Frame["Top"].relativePosition = new Vector3(FramePadding.left, 0);
            Frame["Top"].size = new Vector2(size.x - FramePadding.left - FramePadding.right, FramePadding.top);
            Frame["Top"].name = "QToast_Top";
            Frame["TopRight"].relativePosition = new Vector3(size.x - FramePadding.right, 0);
            Frame["TopRight"].size = new Vector2(FramePadding.right, FramePadding.top);
            Frame["TopRight"].name = "QToast_TopRight";

            Frame["MidContainer"].relativePosition = new Vector3(0, FramePadding.top);
            Frame["MidContainer"].size = new Vector2(size.x, size.y - FramePadding.top - FramePadding.bottom);
            Frame["MidContainer"].name = "QToast_MidContainer";
            Frame["MidLeft"].relativePosition = Vector3.zero;
            Frame["MidLeft"].size = new Vector2(FramePadding.left, Frame["MidContainer"].size.y);
            Frame["MidLeft"].name = "QToast_MidLeft";
            Frame["Mid"].relativePosition = new Vector3(FramePadding.left, 0);
            Frame["Mid"].size = new Vector2(Frame["MidContainer"].size.x - FramePadding.left - FramePadding.right, Frame["MidContainer"].size.y);
            Frame["Mid"].name = "QToast_Mid";
            Frame["MidRight"].relativePosition = new Vector3(Frame["MidContainer"].size.x - FramePadding.right, 0);
            Frame["MidRight"].size = new Vector2(FramePadding.right, Frame["MidContainer"].size.y);
            Frame["MidRight"].name = "QToast_MidRight";

            Frame["BottomContainer"].relativePosition = new Vector3(0, size.y - FramePadding.bottom);
            Frame["BottomContainer"].size = new Vector2(size.x, FramePadding.bottom);
            Frame["BottomContainer"].name = "QToast_BottomContainer";
            Frame["BottomLeft"].relativePosition = Vector3.zero;
            Frame["BottomLeft"].size = new Vector2(FramePadding.left, FramePadding.bottom);
            Frame["BottomLeft"].name = "QToast_BottomLeft";
            Frame["Bottom"].relativePosition = new Vector3(FramePadding.left, 0);
            Frame["Bottom"].size = new Vector2(Frame["BottomContainer"].size.x - FramePadding.left - FramePadding.right, FramePadding.bottom);
            Frame["Bottom"].name = "QToast_Bottom";
            Frame["BottomRight"].relativePosition = new Vector3(Frame["BottomContainer"].size.x - FramePadding.right, 0);
            Frame["BottomRight"].size = new Vector2(FramePadding.right, FramePadding.bottom);
            Frame["BottomRight"].name = "QToast_BottomRight";

            ((UISprite)Frame["TopLeft"]).spriteName = "TopLeft";
            ((UITiledSprite)Frame["Top"]).spriteName = "Top";
            ((UITiledSprite)Frame["Top"]).fillDirection = UIFillDirection.Horizontal;
            ((UISprite)Frame["TopRight"]).spriteName = "TopRight";

            ((UITiledSprite)Frame["MidLeft"]).spriteName = "Left";
            ((UITiledSprite)Frame["MidLeft"]).fillDirection = UIFillDirection.Vertical;
            ((UICanvasSprite)Frame["Mid"]).Fill(0, 0, Frame["Mid"].size.x, Frame["Mid"].size.y, new Color32(255, 244, 150, 255));
            ((UITiledSprite)Frame["MidRight"]).spriteName = "Right";
            ((UITiledSprite)Frame["MidRight"]).fillDirection = UIFillDirection.Vertical;

            ((UISprite)Frame["BottomLeft"]).spriteName = "BottomLeft";
            ((UITiledSprite)Frame["Bottom"]).spriteName = "Bottom";
            ((UITiledSprite)Frame["Bottom"]).fillDirection = UIFillDirection.Horizontal;
            ((UISprite)Frame["BottomRight"]).spriteName = "BottomRight";
        }

        private static void InitPanel(UIComponent panel)
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

        private static UITextureAtlas GetAtlas()
        {
            if (s_atlas != null) return s_atlas;

            string[] spriteNames = new string[]
            {
                "Bottom",
                "BottomLeft",
                "BottomRight",
                "Left",
                "Right",
                "Top",
                "TopLeft",
                "TopRight"
            };

            s_atlas = QTextures.CreateTextureAtlas("QToast", spriteNames, Assembly.GetAssembly(typeof(QToast)).GetName().Name + ".UI.Toast.");
            return s_atlas;
        }
    }
}
