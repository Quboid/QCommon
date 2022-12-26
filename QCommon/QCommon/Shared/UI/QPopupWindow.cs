using ColossalFramework.UI;
using QCommonLib;
using UnityEngine;

namespace QCommonLib.UI
{
    public abstract class QPopupWindow : QPopup
    {
        public UIButton closeTop, closeBottom;
        public UILabel blurb, title;
        public Vector2 defaultSize = new Vector2(400f, 300f);

        public override void Start()
        {
            name = "QPopupWindow";
            atlas = QTextures.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            size = defaultSize;
            canFocus = true;
            absolutePosition = new Vector3(-1000f, -1000f);
            autoLayout = false;

            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.target = parent;
            dragHandle.relativePosition = Vector3.zero;

            closeTop = AddUIComponent<UIButton>();
            closeTop.size = new Vector2(30f, 30f);
            closeTop.text = "X";
            closeTop.textScale = 0.9f;
            closeTop.textColor = new Color32(118, 123, 123, 255);
            closeTop.focusedTextColor = new Color32(118, 123, 123, 255);
            closeTop.hoveredTextColor = new Color32(140, 142, 142, 255);
            closeTop.pressedTextColor = new Color32(99, 102, 102, 102);
            closeTop.textPadding = new RectOffset(8, 8, 8, 8);
            closeTop.canFocus = false;
            closeTop.playAudioEvents = true;
            closeTop.relativePosition = new Vector3(width - closeTop.width, 0);

            title = AddUIComponent<UILabel>();
            title.textScale = 0.9f;
            title.text = "";
            title.name = "QPopupWindow_Title";
            title.relativePosition = new Vector2(8, 8);
            title.SendToBack();

            blurb = AddUIComponent<UILabel>();
            blurb.autoSize = false;
            blurb.name = "QPopupWindow_Blurb";
            blurb.text = "";
            blurb.wordWrap = true;
            blurb.backgroundSprite = "UnlockingPanel";
            blurb.color = new Color32(206, 206, 206, 255);
            blurb.size = new Vector2(width - 10, 214);
            blurb.relativePosition = new Vector2(5, 28);
            blurb.padding = new RectOffset(6, 6, 8, 8);
            blurb.atlas = atlas;
            blurb.SendToBack();

            closeTop.eventClicked += (c, p) =>
            {
                Close();
            };

            BringToFront();
        }

        public override void SetText(string titleText, string bodyText)
        {
            title.text = titleText;
            blurb.text = bodyText;
        }

        internal void CloseButton()
        {
            closeBottom = CreateButton(this);
            closeBottom.text = "Close";
            closeBottom.playAudioEvents = true;
            closeBottom.relativePosition = new Vector3(width / 2 - closeBottom.width / 2, height - 40);

            closeBottom.eventClicked += (c, p) =>
            {
                Close();
            };
        }
    }
}
