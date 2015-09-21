using UnityEngine;
using Pangaea.Input;

public class HydraHand : MonoBehaviour, IPanInputDevice
{
	public HandType GetHandType()
	{
		switch(m_hand)
		{
			case SixenseHands.LEFT:
				return HandType.Left;
			case SixenseHands.RIGHT:
				return HandType.Right;
			case SixenseHands.UNKNOWN:
				return HandType.Unkwon;
		}
		return HandType.Unkwon;
	}
		
	public bool GetBtnDown(PanButton btn)
	{
		if(m_controller == null) return false;
		return m_controller.GetButtonDown(hydraKeyMap[(int)btn]);
	}

	public bool GetBtnUp(PanButton btn)
	{
		if(m_controller == null) return false;
		return m_controller.GetButtonUp(hydraKeyMap[(int)btn]);
	}

	public bool GetBtn(PanButton btn)
	{
		if(m_controller == null) return false;
		return m_controller.GetButton(hydraKeyMap[(int)btn]);
	}

	public float GetTriger()
	{
		if(m_controller == null) return 0;
		return m_controller.Trigger;
	}

	public float GetHorizental()
	{
		if(m_controller == null) return 0;
		return m_controller.JoystickX;
	}

	public float GetVertical()
	{
		if(m_controller == null) return 0;
		return m_controller.JoystickY;
	}

	public Vector3 GetVelocity()
	{
		if(m_controller == null) return Vector3.zero;
		return Vector3.zero;
	}

	public void OnGrab(GameObject grabedObj)
	{
		
	}

	public void OnDrop(GameObject grabedObj)
	{
		
	}

	public SixenseHands	m_hand;
	public SixenseInput.Controller m_controller = null;

	Vector3		m_initialPosition;
	Quaternion 	m_initialRotation;

	private SixenseButtons[] hydraKeyMap = new SixenseButtons[]
	{
		SixenseButtons.ONE,
		SixenseButtons.TWO,
		SixenseButtons.THREE,
		SixenseButtons.FOUR,
		SixenseButtons.BUMPER,
		SixenseButtons.START
	};


	protected void Start() 
	{
		m_initialRotation = transform.localRotation;
		m_initialPosition = transform.localPosition;
	}


	protected void Update()
	{
		if ( m_controller == null )
		{
			m_controller = SixenseInput.GetController( m_hand );
		}
	}

	public Quaternion InitialRotation
	{
		get { return m_initialRotation; }
	}
	
	public Vector3 InitialPosition
	{
		get { return m_initialPosition; }
	}

	void OnDrawGizmos()
	{
		if (m_controller != null)
		{
			Gizmos.color = Color.red;
			Vector3 handPosition = m_controller.Position * 0.001f;
			Gizmos.DrawLine(Vector3.zero, handPosition);
		}

	}
}
