using UnityEngine;

namespace Pangaea.Input
{
	public interface IPanInputDevice
	{
		HandType GetHandType();
		
		bool GetBtnDown(PanButton btn);

		bool GetBtnUp(PanButton btn);

		bool GetBtn(PanButton btn);

		float GetTriger();

		float GetHorizental();

		float GetVertical();

		Vector3 GetVelocity();

		void OnGrab(GameObject grabedObj);

		void OnDrop(GameObject grabedObj);

	}

	public enum PanButton
	{
		BTN0,
		BTN1,
		BTN2,
		BTN3,
		BTN4,
		BTN5,
		LEN
	}

	public enum HandType
	{
		Left,
		Right,
		Unkwon
	}
}


