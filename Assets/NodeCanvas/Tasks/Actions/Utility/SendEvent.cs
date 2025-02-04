﻿using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Services;


namespace NodeCanvas.Tasks.Actions{

	[Category("✫ Utility")]
	[Description("Send a graph event. If global is true, all graph owners in scene will receive this event. Use along with the 'Check Event' Condition")]
	public class SendEvent : ActionTask<GraphOwner> {

		[RequiredField]
		public BBParameter<string> eventName;
		public BBParameter<float> delay;
		public bool sendGlobal;

		protected override string info{
			get{ return (sendGlobal? "Global " : "") + "Send Event [" + eventName + "]" + (delay.value > 0? " after " + delay + " sec." : "" );}
		}

		protected override string OnInit(){
			if (agent.GetComponent<MessageRouter>() == null)
				agent.gameObject.AddComponent<MessageRouter>();
			return null;
		}

		protected override void OnUpdate(){
			if (elapsedTime > delay.value){
				if (sendGlobal){
					Graph.SendGlobalEvent( new EventData(eventName.value) );
				} else {
					agent.SendEvent( new EventData(eventName.value) );
				}
				EndAction();
			}
		}
	}


	[Category("✫ Utility")]
	[Description("Send a graph event with T value. If global is true, all graph owners in scene will receive this event. Use along with the 'Check Event' Condition")]
	public class SendEvent<T> : ActionTask<GraphOwner> {
		
		[RequiredField]
		public BBParameter<string> eventName;
		public BBParameter<T> eventValue;
		public BBParameter<float> delay;
		public bool sendGlobal;

		protected override string info{
			get {return string.Format("{0} Event [{1}] ({2}){3}", (sendGlobal? "Global " : ""), eventName, eventValue, (delay.value > 0? " after " + delay + " sec." : "")  );}
		}

		protected override string OnInit(){
			if (agent.GetComponent<MessageRouter>() == null)
				agent.gameObject.AddComponent<MessageRouter>();
			return null;
		}

		protected override void OnUpdate(){
			if (elapsedTime > delay.value){
				if (sendGlobal){
					Graph.SendGlobalEvent( new EventData<T>(eventName.value, eventValue.value) );
				} else {
					agent.SendEvent( new EventData<T>(eventName.value, eventValue.value) );
				}
				EndAction();
			}
		}		
	}
}