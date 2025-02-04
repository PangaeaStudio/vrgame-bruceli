﻿using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{

	[Category("✫ Utility")]
	[Description("Display a UI label on the agent's position if seconds to run is not 0, else simply logs the message")]
	public class DebugLogText : ActionTask<Transform>{

        [RequiredField]
		public BBParameter<string> log = "Hello World";
		public float labelYOffset = 0;
		public float secondsToRun = 1f;
		public CompactStatus finishStatus = CompactStatus.Success;

		protected override string info{
			get {return "Log " + log.ToString() + (secondsToRun > 0? " for " + secondsToRun + " sec." : ""); }
		}

		protected override void OnExecute(){
			Debug.Log(log.value);
			if (secondsToRun > 0)
				MonoManager.AddGUIMethod(OnGUI);
		}

		protected override void OnStop(){
			if (secondsToRun > 0)
				MonoManager.RemoveGUIMethod(OnGUI);
		}

		protected override void OnUpdate(){
			if (elapsedTime >= secondsToRun)
				EndAction(finishStatus == CompactStatus.Success? true : false );
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
			
		private Texture2D _tex;
		private Texture2D tex{
			get
			{
				if (_tex == null){
					_tex = new Texture2D(1,1);
					_tex.SetPixel(0, 0, Color.white);
					_tex.Apply();
				}
				return _tex;			
			}
		}

		void OnGUI(){

			if (Camera.main == null)
				return;

			var point = Camera.main.WorldToScreenPoint(agent.position + new Vector3(0, labelYOffset, 0));
			var size = new GUIStyle("label").CalcSize(new GUIContent(log.value));
			var r = new Rect(point.x - size.x /2, Screen.height - point.y, size.x +10, size.y);
			GUI.color = new Color(1f,1f,1f,0.5f);
			GUI.DrawTexture(r, tex);
			GUI.color = new Color(0.2f, 0.2f, 0.2f, 1);
			r.x += 4;
			GUI.Label(r, log.value);
			GUI.color = Color.white;
		}
	}
}