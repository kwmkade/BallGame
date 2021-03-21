using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kwmkade.CrossPlatformInput
{
    public class JoystickPanelTouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [NotNull] public GameObject _joystick;
        [NotNull] public GameObject _frame;

        void Start()
        {
            _joystick.SetActive(false);
            _frame.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _joystick.transform.position = eventData.position;
            _joystick.SetActive(true);

            _frame.transform.position = eventData.position;
            _frame.SetActive(true);

            _joystick.GetComponent<JoystickHandler>()?.OnPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _joystick.GetComponent<JoystickHandler>()?.OnPointerUp(eventData);

            _joystick.SetActive(false);
            _frame.SetActive(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _joystick.GetComponent<JoystickHandler>()?.OnDrag(eventData);
        }
    }
}