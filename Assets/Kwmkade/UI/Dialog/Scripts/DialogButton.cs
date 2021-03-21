using System;
using UnityEngine;

namespace Kwmkade.UI.Dialog
{

    public class DialogButton : MonoBehaviour
    {

        public IDialog parent;
        public int index;

        public void OnClick()
        {
            parent.OnClickButton(index);
        }

        /// <summary>
        /// ボタンのラベルとコールバック用
        /// </summary>
        public class ActionButton
        {
            public string text;
            public Action action;

            public ActionButton(string text, Action action = null)
            {
                this.text = text;
                this.action = action;
            }
        }
    }

}
