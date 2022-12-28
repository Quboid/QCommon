using ColossalFramework;
using ColossalFramework.UI;
using Epic.OnlineServices.Presence;
using System;
using UnityEngine;

namespace QCommonLib.UI
{
    public abstract class QPopup : UIPanel
    {
        // Consider these properties as abstract; children must override
        protected virtual bool GrabFocus => throw new NotImplementedException();
        protected virtual bool ShouldShow => throw new NotImplementedException();

        public static QPopup Open(Type type)
        {
            GameObject gameObject = new GameObject(type.Name, type);
            QPopup popupPanel = gameObject.GetComponent(type) as QPopup;

            if (!popupPanel.ShouldShow)
            {
                Destroy(gameObject);
                return null;
            }

            QPopup instance = UIView.GetAView().AttachUIComponent(popupPanel.gameObject) as QPopup;

            if (instance.GrabFocus) UIView.PushModal(instance);
            return instance;
        }

        public virtual void Close()
        {
            if (!ShouldShow)
            {
                Destroy(gameObject);
                return;
            }

            if (GrabFocus)
            {
                UIView.PopModal();

                UIComponent modalEffect = GetUIView().panelsLibraryModalEffect;
                if (modalEffect != null && modalEffect.isVisible)
                {
                    ValueAnimator.Animate("ModalEffect", delegate (float val)
                    {
                        modalEffect.opacity = val;
                    }, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), delegate
                    {
                        modalEffect.Hide();
                    });
                }
            }

            isVisible = false;
            Destroy(this.gameObject);
        }

        public abstract void SetText(string titleText, string bodyText);

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                p.Use();
                Close();
            }

            base.OnKeyDown(p);
        }

        protected override void OnPositionChanged()
        {
            Vector2 resolution = GetUIView().GetScreenResolution();

            if (absolutePosition.x == -1000)
            {
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);
                MakePixelPerfect();
            }

            absolutePosition = new Vector2(
                (int)Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int)Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            base.OnPositionChanged();
        }

        public void RefreshPosition()
        {
            float x = (GetUIView().GetScreenResolution().x / 2) - (width / 2);
            float y = (GetUIView().GetScreenResolution().y / 2) - (height / 2) - 50;

            absolutePosition = new Vector3(x, y);
        }

        public static UIButton CreateButton(UIComponent parent)
        {
            UIButton button = parent.AddUIComponent<UIButton>();

            button.atlas = QTextures.GetAtlas("Ingame");
            button.size = new Vector2(90f, 30f);
            button.textScale = 0.9f;
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.canFocus = false;

            return button;
        }
    }


    public class ExampleWindow : QPopupWindow
    {
        protected override bool ShouldShow => true;

        public override void Start()
        {
            base.Start();

            blurb.text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris pharetra turpis eget dui tincidunt, vitae.";

            UIButton button = CreateButton(this);
            button.autoSize = false;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.size = new Vector2(250, 30);
            button.text = "Button";
            button.tooltip = "Button tooltip";
            button.relativePosition = new Vector3(width / 2 - button.width / 2, height - 40);
            button.eventClicked += (c, p) =>
            { };
        }
    }

    public class ExampleToast : QToast
    {
        protected override bool ShouldShow => true;

        public override void Start()
        {
            size = new Vector2(400, 300);
            absolutePosition = new Vector2(500, 200);
            name = "ExampleToast";
            autoPanelVAlign = PanelVAlignment.Bottom;
            arrowOffset = 25;

            base.Start();

            SetText("Hello", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam sem ligula, semper a enim quis, volutpat accumsan magna. Sed tristique tortor nec sem lacinia, quis venenatis est porta.\n\n" +
                "Suspendisse laoreet blandit vehicula. Nunc tincidunt quam purus, in aliquet ligula faucibus ac. Phasellus vulputate, turpis aliquet viverra lacinia, ex mi consectetur.");
        }
    }
}
