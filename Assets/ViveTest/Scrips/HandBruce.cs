using UnityEngine;
using System.Collections;
using RootMotion.Demos;
using Pangaea.Input;

public class HandBruce : MonoBehaviour 
{
	public HandType handType;
	public GameObject inputObject;

	private IPanInputDevice input;
	private CharactorBruce bruce;
	private GameObject currentWeapon;

	void Start () {
		InitInputDevice();
		
		bruce = GameObject.FindWithTag("Player").GetComponent<CharactorBruce>();
		if(null == currentWeapon)
		{
			SwapWeapon();
		}
	}

	private void InitInputDevice()
	{
		if(null != inputObject)
			input = inputObject.GetComponent<IPanInputDevice>();
		if(input == null)
			throw new UnityException("Can not Init InputDevice");
		handType = input.GetHandType();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(input.GetBtnDown(PanButton.BTN0))
		{
			SwapWeapon();
		}
	}

	private void SwapWeapon()
	{
		GameObject obj;
		if(handType == HandType.Left)
		{
			obj = bruce.GetLNextWeapon();
		}
		else
		{
			obj = bruce.GetRNextWeapon();
		}

		if(null != currentWeapon)
		{
			currentWeapon.GetComponent<HitTrigger>().OnDrop(input, GetComponent<FixedJoint>());
			input.OnDrop(currentWeapon);
		}

		currentWeapon = Instantiate(obj);
		var rig = GetComponent<Rigidbody>();
        HitTrigger hittrigger = currentWeapon.GetComponent<HitTrigger>();
        if (hittrigger.IsUsingJoint)
        {

            currentWeapon.transform.position = rig.transform.position;
            currentWeapon.transform.rotation = rig.transform.rotation;
            hittrigger.OnPickUp(input, GetComponent<FixedJoint>());
            input.OnGrab(currentWeapon);
        }
        else
        {
            hittrigger.transform.SetParent(transform, false);
            hittrigger.OnPickUp(input, GetComponent<FixedJoint>());
            input.OnGrab(currentWeapon);
        }
	}
}
