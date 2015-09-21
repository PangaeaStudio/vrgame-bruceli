using UnityEngine;

public class HydraHandsController : MonoBehaviour
{
    HydraHand[] m_hands;

    Vector3	m_baseOffset;
    const float m_sensitivity = 0.001f;
    // Sixense units are in mm
    bool m_bInitialized;

    public bool mirror = false;
    // Use this for initialization
    void Start()
    {
        m_hands = GetComponentsInChildren<HydraHand>();
    }


    // Update is called once per frame
    void Update()
    {
        bool bResetHandPosition = false;

        foreach (HydraHand hand in m_hands)
        {
            if (IsControllerActive(hand.m_controller) && hand.m_controller.GetButtonDown(SixenseButtons.START))
            {
                bResetHandPosition = true;
            }

            if (m_bInitialized)
            {
                UpdateHand(hand);
            }
        }
				
        if (bResetHandPosition)
        {
            m_bInitialized = true;

            m_baseOffset = Vector3.zero;

            // Get the base offset assuming forward facing down the z axis of the base
            foreach (HydraHand hand in m_hands)
            {
                m_baseOffset += hand.m_controller.Position;
            }

            m_baseOffset /= 2;
        }
    }


    /** Updates hand position and rotation */
    void UpdateHand(HydraHand hand)
    {
        bool bControllerActive = IsControllerActive(hand.m_controller);

        if (bControllerActive)
        {
            hand.transform.localPosition = (hand.m_controller.Position - m_baseOffset) * m_sensitivity * (mirror?-1:1);
            hand.transform.localRotation = hand.m_controller.Rotation * hand.InitialRotation;
        }
        else
        {
            // use the inital position and orientation because the controller is not active
            hand.transform.localPosition = hand.InitialPosition;
            hand.transform.localRotation = hand.InitialRotation;
        }
    }


    void OnGUI()
    {
        if (!m_bInitialized)
        {
            GUI.Box(new Rect(Screen.width / 2 - 50, Screen.height - 40, 100, 30), "Press Start");
        }
    }


    /** returns true if a controller is enabled and not docked */
    bool IsControllerActive(SixenseInput.Controller controller)
    {
        return (controller != null && controller.Enabled && !controller.Docked);
    }
}
