using ColossalFramework.UI;
using System;
using UnityEngine;

namespace QCommonLib.UI
{
    internal class QToast : UIPanel
    {
        public static QToast Factory(string name, Vector2 position, Vector2 size, int arrowOffset = -1, bool autoHeight = false)
        {
            QToast instance = UIView.GetAView().AddUIComponent(typeof(QToast)) as QToast;
            instance.Initialise(name, position, size, arrowOffset, autoHeight);

            return instance;
        }

        private bool autoSetHeight = false;
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
        //internal readonly Dictionary<string, UIComponent> frame = new Dictionary<string, UIComponent>();

        public QToast()
        {
            autoLayout = false;
            autoSize = false;
            atlas = ToastFrame.GetAtlas();
            isVisible = false;
        }

        public void Initialise(string name, Vector2 position, Vector2 size, int arrowOffset = -1, bool autoHeight = false)
        {
            autoSetHeight = autoHeight;
            this.name = "QToast_" + name;

            absolutePosition = position;
            size.x = Mathf.Clamp(size.x, 100f, 500f);
            size.y = Mathf.Clamp(size.y, 80f, 400f);
            this.size = size;

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

        public void SetText(string titleText, string bodyText)
        {
            Title.text = titleText;
            Title.relativePosition = new Vector2((width / 2) - (Title.width / 2), Title.relativePosition.y);
            Body.text = bodyText;

            if (autoSetHeight)
            {
                frame.SetHeight();
            }
            //Body.area = frame.GetBodySize();
            //Body.PerformLayout();
        }
    }
}
