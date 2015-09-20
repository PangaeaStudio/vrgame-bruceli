using UnityEngine;
using Pangaea.Input;
using Leap;

public class LeapMotionHand : MonoBehaviour, IPanInputDevice
{

	
    public float grabTriggerDistance = 0.4f;

    private GameObject _grabedObj;

    // Filtering the rotation of grabbed object.
    public float rotationFiltering = 0.4f;

    private bool[] preBtn = new bool[(int)PanButton.LEN];
    private bool[] curBtn = new bool[(int)PanButton.LEN];

    private HandModel handmodel;
    private Vector3 palmPostion;
    private Vector3 velocity;

    void Start()
    {
    	handmodel = GetComponent<HandModel>();
        _grabedObj = null;
        palmPostion = handmodel.GetPalmPosition();
        velocity = Vector3.zero;
    }

    void OnDestory()
    {
        OnDrop(_grabedObj);
    }

    public HandType GetHandType()
    {
        return handmodel.GetLeapHand().IsLeft?HandType.Left:HandType.Right;
    }

    public bool GetBtnDown(PanButton btn)
    {
        int index = (int)btn;
        if(index > 3) return false;
        if (preBtn[index])
            return false;
        if (IsOtherFingersBend(index + 1))
            return false;
        return curBtn[index];
    }

    public bool GetBtnUp(PanButton btn)
    {
        int index = (int)btn;
        if(index > 3) return false;
        if (!preBtn[index])
            return false;
        if (IsOtherFingersBend(index + 1))
            return false;
        return !curBtn[index];
    }

    public bool GetBtn(PanButton btn)
    {
        int index = (int)btn;
        if(index > 3) return false;
        if (IsOtherFingersBend(index + 1))
            return false;
        return curBtn[index];
    }

    private bool IsOtherFingersBend(int fingerIndex, float targetAngle = -30)
    {
        for (int i = 1; i < HandModel.NUM_FINGERS; i++)
        {
            if (i != fingerIndex)
            {
                float angle = handmodel.fingers[i]
					.GetFingerJointStretchMecanim((int)Bone.BoneType.TYPE_METACARPAL);
                if (angle < targetAngle)
                {
                    return true;
                }

            }
        }
        return false;
    }

    private bool IsAllFingersBend(float targetAngle = -30)
    {
        for (int i = 1; i < HandModel.NUM_FINGERS; i++)
        {
            float angle = handmodel.fingers[i]
					.GetFingerJointStretchMecanim((int)Bone.BoneType.TYPE_METACARPAL);
            if (angle > targetAngle)
            {
                return false;
            }
        }
        return true;
    }

    public float GetTriger()
    {
        return IsAllFingersBend() ? 1.0f : 0.0f;
    }

    public float GetHorizental()
    {
        return 0;
    }

    public float GetVertical()
    {
        return 0;
    }

    public Vector3 GetVelocity()
    {
    	return velocity;
    }

    public virtual void OnGrab(GameObject grabedObj)
    {
        Utils.IgnoreCollisions(gameObject, grabedObj);
    }

    public virtual void OnDrop(GameObject grabedObj)
    {
        Utils.IgnoreCollisions(gameObject, grabedObj,false);

    }

    void Update()
    {
        UpdateButton();
        UpdateVelocity();
    }


    private void UpdateButton()
    {
        FingerModel thumb = handmodel.fingers[(int)Finger.FingerType.TYPE_THUMB];
        for (int i = 1; i < HandModel.NUM_FINGERS; i++)
        {
            FingerModel tf = handmodel.fingers[i];
            float distance = Vector3.Distance(tf.GetTipPosition(), thumb.GetTipPosition());
            bool isPressed = distance <= grabTriggerDistance;
            int btnIndex = i - 1;
            preBtn[btnIndex] = curBtn[btnIndex];
            curBtn[btnIndex] = isPressed;
        }
        // Debug.LogWarning(preBtn[0]+ " " + preBtn[1] + " " + preBtn[2]);
        // Debug.LogError(curBtn[0]+ " " + curBtn[1] + " " + curBtn[2]);
        // Debug.LogWarning(hand_model.fingers[1].GetFingerJointStretchMecanim((int)Bone.BoneType.TYPE_METACARPAL));
    }

    private void UpdateVelocity()
    {
    	Vector3 curPalmPosition = handmodel.GetPalmPosition();
    	velocity = (curPalmPosition - palmPostion) / Time.deltaTime;
    	palmPostion = curPalmPosition;
    }

}
