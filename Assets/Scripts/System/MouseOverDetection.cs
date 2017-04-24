using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Gossage.System
{
    /// <summary>
    /// Routine to detect mouse over events and report them
    /// https://gamedev.stackexchange.com/questions/108625/how-to-detect-mouse-over-for-ui-image-in-unity-5
    /// </summary>
    public class MouseOverDetection: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool IsOver { get { return isOver; } }
        public UnityAction OnMouseEnterAction, OnMouseExitAction;

        private bool isOver = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("Mouse enter");
            isOver = true;
            if (OnMouseEnterAction != null)
                OnMouseEnterAction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("Mouse exit");
            isOver = false;
            if (OnMouseExitAction != null)
                OnMouseExitAction();
        }
    }

}