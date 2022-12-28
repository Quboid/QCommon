using ColossalFramework.UI;
using QCommonLib;
using UnityEngine;

namespace QCommonLib.UI
{
    public abstract class QPopupWindow : QPopup
    {
        protected override bool GrabFocus => true;
        protected virtual bool IncludeBottomButtonGap => false;
        public UIButton closeBtn, okBtn;
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

            closeBtn = AddUIComponent<UIButton>();
            closeBtn.size = new Vector2(30f, 30f);
            closeBtn.text = "X";
            closeBtn.textScale = 0.9f;
            closeBtn.textColor = new Color32(118, 123, 123, 255);
            closeBtn.focusedTextColor = new Color32(118, 123, 123, 255);
            closeBtn.hoveredTextColor = new Color32(140, 142, 142, 255);
            closeBtn.pressedTextColor = new Color32(99, 102, 102, 102);
            closeBtn.textPadding = new RectOffset(8, 8, 8, 8);
            closeBtn.canFocus = false;
            closeBtn.playAudioEvents = true;
            closeBtn.relativePosition = new Vector3(width - closeBtn.width, 0);

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
            blurb.size = new Vector2(width - 10, height - 34 - (IncludeBottomButtonGap ? 42 : 0));
            blurb.relativePosition = new Vector2(5, 28);
            blurb.padding = new RectOffset(6, 6, 8, 8);
            blurb.atlas = atlas;
            blurb.SendToBack();

            closeBtn.eventClicked += (c, p) =>
            {
                Close();
            };

            BringToFront();
        }

        public void SetSize(Vector2 newSize)
        {
            size = newSize;
            closeBtn.relativePosition = new Vector3(width - closeBtn.width, 0);
            blurb.size = new Vector2(width - 10, height - 34 - (IncludeBottomButtonGap ? 42 : 0));
        }

        public override void SetText(string titleText, string bodyText)
        {
            title.text = titleText;
            blurb.text = bodyText;
        }

        internal void OKButton(string text = "OK")
        {
            okBtn = CreateButton(this);
            okBtn.text = text;
            okBtn.playAudioEvents = true;
            okBtn.textHorizontalAlignment = UIHorizontalAlignment.Center;
            okBtn.relativePosition = new Vector3(width / 2 - okBtn.width / 2, height - 40);
            okBtn.size = new Vector2(80, 30);

            okBtn.eventClicked += (c, p) =>
            {
                Close();
            };
        }
    }
}
