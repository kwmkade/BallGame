using UnityEngine;

namespace Kwmkade.UI.Dialog
{
    public abstract class IDialog : MonoBehaviour
    {
        /// <summary>
        /// 複製したボタンを押下した際に呼ばれる
        /// </summary>
        /// <param name="idx">押したボタンのインデックス</param>
        public abstract void OnClickButton(int idx);
    }
}
