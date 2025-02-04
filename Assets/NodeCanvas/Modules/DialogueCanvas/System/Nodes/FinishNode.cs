using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.DialogueTrees{

	[Name("Finish")]
	[Description("End the dialogue in Success or Failure.\nNote: A Dialogue will anyway End in Succcess if it has reached a node without child connections.")]
	public class FinishNode : DTNode {

		public DialogueTree.EndState endState = DialogueTree.EndState.Success;

		public override string name{
			get {return "FINISH";}
		}

		public override int maxOutConnections{
			get {return 0;}
		}

		protected override Status OnExecute(Component agent, IBlackboard bb){
			status = (Status)endState;
			DLGTree.Stop();
			return status;
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){
			GUILayout.Label("<b>" + endState + "</b>");
		}

		protected override void OnNodeInspectorGUI(){
			DrawDefaultInspector();
		}

		#endif
	}
}