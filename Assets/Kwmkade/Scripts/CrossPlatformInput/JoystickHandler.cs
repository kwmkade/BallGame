using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

namespace Kwmkade.CrossPlatformInput
{
    public class JoystickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public int MovementRange = 100;
		public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
		public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

		Vector3 m_StartPos;
		CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

		public Vector3 InputVector { get; set; }


		void Start()
		{
			m_StartPos = transform.position;
        }

		void UpdateVirtualAxes(Vector3 value)
		{
			var delta = m_StartPos - value;
			delta.y = -delta.y;
			delta /= MovementRange;

			m_HorizontalVirtualAxis.Update(-delta.x);
			m_VerticalVirtualAxis.Update(delta.y);
		}

		void CreateVirtualAxes()
		{
			m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
			CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);

			m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
			CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
		}

		public void OnDrag(PointerEventData data)
		{
			int deltaX = (int)(data.position.x - m_StartPos.x);
			int deltaY = (int)(data.position.y - m_StartPos.y);

			Vector3 newPos = new Vector3(deltaX, deltaY, 0);
			float rate = (float)(Math.Max(1.0, Math.Sqrt(deltaX * deltaX + deltaY * deltaY) / MovementRange));
			newPos /= rate;

			transform.position = new Vector3(m_StartPos.x + newPos.x, m_StartPos.y + newPos.y, m_StartPos.z + newPos.z);
			UpdateVirtualAxes(transform.position);

			InputVector = newPos;
		}

		public void OnPointerUp(PointerEventData data)
		{
			transform.position = m_StartPos;
			UpdateVirtualAxes(m_StartPos);

			InputVector = Vector3.zero;
		}


		public void OnPointerDown(PointerEventData data)
		{
			m_StartPos = data.position;

			InputVector = Vector3.zero;
		}

		void OnEnable()
		{
			CreateVirtualAxes();
		}

		void OnDisable()
		{
			m_HorizontalVirtualAxis.Remove();
			m_VerticalVirtualAxis.Remove();
		}
    }
}

