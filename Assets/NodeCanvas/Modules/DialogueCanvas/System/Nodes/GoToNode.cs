﻿using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.DialogueTrees{

	[Name("GO TO")]
	[Description("Jump to another Dialogue node")]
	public class GoToNode : DTNode {

		[SerializeField]
		private DTNode _targetNode;

		public override int maxOutConnections{
			get {return 0;}
		}

		public override string name{
			get{ return "<color=#00b9e8><GO TO></color>";}
		}

		protected override Status OnExecute(Component agent, IBlackboard bb){

			if (_targetNode == null)
				return Error("Target node of GOTO node is null");

			DLGTree.EnterNode(_targetNode);
			return Status.Success;
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){
			GUILayout.Label(string.Format("<b> {0} </b>", (_targetNode != null? _targetNode.name : "NONE")) );
		}

		protected override void OnNodeInspectorGUI(){
			if (GUILayout.Button("Set GO TO Target")){
				
				UnityEditor.GenericMenu.MenuFunction2 Selected = (object o)=> {
					_targetNode = (DTNode)o;
				};

				var menu = new UnityEditor.GenericMenu();
				foreach (DTNode node in graph.allNodes){
					if (node != this)
						menu.AddItem( new GUIContent(node.name), false, Selected, node );
				}
				menu.ShowAsContext();
				Event.current.Use();
			}

			if (_targetNode != null && GUILayout.Button("Select Target Node")){
				Graph.currentSelection = _targetNode;
			}
		}

		#endif
	}
}