using UnityEngine;
using System.Collections;
using RootMotion.Demos;

public class HandBruce : MonoBehaviour {

	SteamVR_TrackedObject trackedObj;
	private CharactorBruce bruce;
	public bool isLeft = false;
	private GameObject currentWeapon;

	public static bool isDebug = false;
	// Use this for initialization
	void Start () {
		trackedObj = transform.parent.GetComponent<SteamVR_TrackedObject>();
		bruce = GameObject.FindWithTag("Player").GetComponent<CharactorBruce>();
		if(null == currentWeapon)
		{
			SwapWeapon();
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(IsPressUP())
		{
			SwapWeapon();
		}
	}

	private bool IsTouchUP()
	{
		if(isDebug)
		{
			return Input.GetKeyDown("a");
		}
		var device = SteamVR_Controller.Input((int)trackedObj.index);
		return device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger);
	}

	private bool IsPressUP()
	{
		if(isDebug)
		{
			return Input.GetKeyUp("a");
		}
		var device = SteamVR_Controller.Input((int)trackedObj.index);
		return device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger);
	}

	private void SwapWeapon()
	{
		GameObject obj;
		if(isLeft)
		{
			obj = bruce.GetLNextWeapon();
		}
		else
		{
			obj = bruce.GetRNextWeapon();
		}

		if(null != currentWeapon)
		{
			currentWeapon.GetComponent<HitTrigger>().OnDrop(trackedObj, GetComponent<FixedJoint>());
		}

		currentWeapon = Instantiate(obj);
		var rig = GetComponent<Rigidbody>();
		currentWeapon.transform.position = rig.transform.position;
		currentWeapon.transform.rotation = rig.transform.rotation;
		currentWeapon.GetComponent<HitTrigger>().OnPickUp(trackedObj, GetComponent<FixedJoint>());
	}
}
