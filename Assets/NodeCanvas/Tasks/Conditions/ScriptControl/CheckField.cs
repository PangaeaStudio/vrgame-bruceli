﻿using System.Reflection;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category("✫ Script Control/Common")]
	[Description("Check a field on a script and return if it's equal or not to a value")]
	public class CheckField : ConditionTask {

		[SerializeField]
		private BBParameter checkValue;
		[SerializeField]
		private System.Type targetType;
		[SerializeField]
		private string fieldName;
		[SerializeField]
		private CompareMethod comparison;

		private FieldInfo field;

		public override System.Type agentType{
			get {return targetType != null? targetType : typeof(Transform);}
		}

		protected override string info{
			get
			{
				if (string.IsNullOrEmpty(fieldName))
					return "No Field Selected";
				return string.Format("{0}.{1}{2}", agentInfo, fieldName, checkValue.varType == typeof(bool)? "" : OperationTools.GetCompareString(comparison) + checkValue.ToString());
			}
		}

		//store the field info on agent set for performance
		protected override string OnInit(){
			field = agentType.RTGetField(fieldName);
			if (field == null)
				return "Missing Field Info";
			return null;
		}

		//do it by invoking field
		protected override bool OnCheck(){

			if (checkValue.varType == typeof(float))
				return OperationTools.Compare( (float)field.GetValue(agent), (float)checkValue.value, comparison, 0.05f );

			if (checkValue.varType == typeof(int))
				return OperationTools.Compare( (int)field.GetValue(agent), (int)checkValue.value, comparison );			

			return object.Equals( field.GetValue(agent), checkValue.value );
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnTaskInspectorGUI(){

			if (!Application.isPlaying && GUILayout.Button("Select Field")){
				System.Action<FieldInfo> FieldSelected = (field)=> {
					targetType = field.DeclaringType;
					fieldName = field.Name;
					checkValue = BBParameter.CreateInstance(field.FieldType, blackboard);
					comparison = CompareMethod.EqualTo;
				};

				if (agent != null){
					EditorUtils.ShowGameObjectFieldSelectionMenu(agent.gameObject, typeof(object), FieldSelected);
				} else {
					var menu = new UnityEditor.GenericMenu();
					foreach (var t in UserTypePrefs.GetPreferedTypesList(typeof(Component), true))
						menu = EditorUtils.GetFieldSelectionMenu(t, typeof(object), FieldSelected, menu);
					menu.ShowAsContext();
					Event.current.Use();
				}
			}

			if (agentType != null && !string.IsNullOrEmpty(fieldName)){
				GUILayout.BeginVertical("box");
				UnityEditor.EditorGUILayout.LabelField("Type", agentType.FriendlyName());
				UnityEditor.EditorGUILayout.LabelField("Field", fieldName);
				GUILayout.EndVertical();

				GUI.enabled = checkValue.varType == typeof(float) || checkValue.varType == typeof(int);
				comparison = (CompareMethod)UnityEditor.EditorGUILayout.EnumPopup("Comparison", comparison);
				GUI.enabled = true;
				EditorUtils.BBParameterField("Value", checkValue);
			}
		}

		#endif
	}
}