using ColossalFramework.UI;
using System;
using UnityEngine;

namespace QCommonLib.UI
{
    public class QToast : QPopup
    {
        //public static QToast Factory(string name, Vector2 position, Vector2 size, int arrowOffset = -1, PanelVAlignment panelVAlign = PanelVAlignment.None)
        //{
        //    QToast instance = UIView.GetAView().AddUIComponent(typeof(QToast)) as QToast;
        //    instance.Initialise(name, position, size, arrowOffset, panelVAlign);

        //    return instance;
        //}

        protected override bool GrabFocus => false;
        internal PanelVAlignment autoPanelVAlign = PanelVAlignment.None;
        internal int arrowOffset;
        private bool initialised = false;

        private UILabel title = null;
        internal UILabel Title
        {
            get { return title; }
            set { title = value; }
        }

        private UILabel body = null;
        internal UILabel Body
        {
            get { return body; }
            set { body = value; }
        }

        private UIButton closeBtn = null;
        internal UIButton CloseBtn
        {
            get { return closeBtn; }
            set { closeBtn = value; }
        }

        internal ToastFrame frame;

        public override void Start()
        {
            name = "QToast_" + name;
            atlas = ToastFrame.GetAtlas();
            size = new Vector2(Mathf.Clamp(size.x, 100f, 500f), Mathf.Clamp(size.y, 80f, 400f));
            canFocus = true;
            autoLayout = false;
            autoSize = false;

            frame = new ToastFrame(this, arrowOffset);
            initialised = true;
        }

        public new void Show()
        {
            if (!initialised && isVisible)
            {
                throw new Exception("Attempting to show QToast \"" + name + "\" before initialisation.");
            }
            base.Show();
        }

        public new void Hide()
        {
            base.Hide();
        }

        public override void SetText(string titleText, string bodyText)
        {
            Title.text = titleText;
            Title.relativePosition = new Vector2((width / 2) - (Title.width / 2), Title.relativePosition.y);
            Body.text = bodyText;

            if (autoPanelVAlign != PanelVAlignment.None)
            {
                frame.SetHeight();
            }
        }
    }
}
