using Leap;
using UnityEngine;

public class LeapmotionGestureFine : MonoBehaviour 
{
	public bool circleGesture;
	public float circleMinRadius = 50f;
	public float circleMinArc = 3.0f;

	public bool swipeGesture;
	public float swipeMinLength = 200f;
	public float swipeMinVelocity = 700f;

	public bool screenTapGesture;
	public float ScreenTapMinForwardVelocity = 30.0f;
	public float ScreenTapHistorySeconds = .5f;
	public float ScreenTapMinDistance = 1.0f;
	public bool keyTapGesture;
	public float KeyTapMinDownVelocity = 40.0f;
	public float KeyTapHistorySeconds = .2f;
	public float KeyTapMinDistance = 1.0f;
	// Use this for initialization
	void Start () 
	{
		HandController handController = GetComponent<HandController>();
		if(handController != null)
		{
			Controller controller = handController.GetLeapController();
			if(circleGesture)
			{
				ActiveCircleGestture(controller);
			}
			if(swipeGesture)
			{
				ActiveSwipeGesture(controller);
			}
			if(keyTapGesture)
			{
				ActiveKeyTapGesture(controller);
			}
			if(screenTapGesture)
			{
				ActiveScreenTapGesture(controller);
			}
		}
	}

	private void ActiveCircleGestture(Controller controller)
	{
		controller.EnableGesture (Gesture.GestureType.TYPE_CIRCLE);
		controller.Config.SetFloat ("Gesture.Circle.MinRadius", circleMinRadius);
		controller.Config.SetFloat ("Gesture.Circle.MinArc", circleMinArc);
		controller.Config.Save ();
	}

	private void ActiveSwipeGesture(Controller controller)
	{
		controller.EnableGesture (Gesture.GestureType.TYPE_SWIPE);
		controller.Config.SetFloat ("Gesture.Swipe.MinLength", swipeMinLength);
		controller.Config.SetFloat ("Gesture.Swipe.MinVelocity", swipeMinVelocity);
		controller.Config.Save ();
	}

	private void ActiveKeyTapGesture(Controller controller)
	{
		controller.EnableGesture (Gesture.GestureType.TYPE_KEY_TAP);
		controller.Config.SetFloat ("Gesture.KeyTap.MinDownVelocity", KeyTapMinDownVelocity);
		controller.Config.SetFloat ("Gesture.KeyTap.HistorySeconds", KeyTapHistorySeconds);
		controller.Config.SetFloat ("Gesture.KeyTap.MinDistance", KeyTapMinDistance);
		controller.Config.Save ();
	}

	private void ActiveScreenTapGesture(Controller controller)
	{
		controller.EnableGesture (Gesture.GestureType.TYPE_SCREEN_TAP);
		controller.Config.SetFloat ("Gesture.ScreenTap.MinForwardVelocity", ScreenTapMinForwardVelocity);
		controller.Config.SetFloat ("Gesture.ScreenTap.HistorySeconds", ScreenTapHistorySeconds);
		controller.Config.SetFloat ("Gesture.ScreenTap.MinDistance", ScreenTapMinDistance);
		controller.Config.Save ();
	}
}
