﻿#if UNITY_EDITOR

using System.Linq;
using NodeCanvas.Editor;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Framework{

	partial class Node{

		//Class for the nodeports GUI
		class GUIPort{

			public readonly int portIndex;
			public readonly Node parent;
			public readonly Vector2 pos;

			public GUIPort(int index, Node parent, Vector2 pos){
				this.portIndex = index;
				this.parent = parent;
				this.pos = pos;
			}
		}

		[SerializeField]
		private bool _collapsed = false;

		private Texture2D _icon;
		private bool iconLoaded = false;
		private string _nodeDescription;
		private bool nodeIsPressed{get;set;}
		private Vector2 size = new Vector2(100, 20);
		private static GUIPort clickedPort{get;set;}
		private static GUIStyle _centerLabel = null;
		private static int dragDropMisses;
		readonly public static Color successColor = new Color(0.4f, 0.7f, 0.2f);
		readonly public static Color failureColor = new Color(1.0f, 0.3f, 0.3f);
		readonly public static Color runningColor = Color.yellow;
		readonly public static Color restingColor = new Color(0.7f, 0.7f, 1f, 0.8f);
		readonly public static Vector2 minSize = new Vector2(100, 20);

		public static Node[] copiedNodes{get;set;}

		//This is to be able to work with rects which is easier in many cases.
		//Size is temporary to the node since it's auto adjusted thus no need to serialize it
		public Rect nodeRect{
			get { return new Rect(_position.x, _position.y, size.x, size.y); }
			set
			{
				_position = new Vector2(value.x, value.y);
				size = new Vector2(Mathf.Max(value.width, minSize.x), Mathf.Max(value.height, minSize.y) );
			}
		}

		private static GUIStyle centerLabel{
			get
			{
				if (_centerLabel == null){
					_centerLabel = new GUIStyle("label");
					_centerLabel.alignment = TextAnchor.UpperCenter;
					_centerLabel.richText = true;
				}
				return _centerLabel;
			}
		}

		//The help info of the node
		private string nodeDescription{
			get
			{
				if (string.IsNullOrEmpty(_nodeDescription)){
					var descAtt = this.GetType().RTGetAttribute<DescriptionAttribute>(false);
					_nodeDescription = descAtt != null? descAtt.description : "No Description";
				}
				return _nodeDescription;
			}
		}

		///Editor! Active is relevant to the input connections
		public bool isActive{
			get
			{
				for (var i = 0; i < inConnections.Count; i++)
					if (inConnections[i].isActive)
						return true;
				return inConnections.Count == 0;
			}
		}

		///Editor! Are children collapsed?
		public bool collapsed{
			get {return _collapsed;}
			set {_collapsed = value;}
		}

		///EDITOR! is the node hidden due to parent has children collapsed or is hidden itself?
		public bool isHidden {
			get
			{
				if (graph.autoSort){
					foreach (var parent in inConnections.Select(c => c.sourceNode)){
						if (parent.ID > this.ID)
							continue;
						if (parent.collapsed || parent.isHidden)
							return true;
					}
				}
				return false;
			}
		}

		private bool isSelected{
			get {return Graph.currentSelection == this || Graph.multiSelection.Contains(this);}
		}


		//The icon of the node
		private Texture2D icon{
			get
			{
				if (iconLoaded){
					return _icon;
				}

				if (this is ITaskAssignable){
					var assignable = this as ITaskAssignable;
					_icon = assignable.task != null? assignable.task.icon : null;
				}
				if (_icon == null){
					var iconAtt = this.GetType().RTGetAttribute<IconAttribute>(true);
					if (iconAtt != null) _icon = (Texture2D)Resources.Load(iconAtt.iconName);
				}
				iconLoaded = true;				
				return _icon;			
			}
		}

		//Is NC in icon mode && node has an icon?
		protected bool inIconMode{
			get {return NCPrefs.iconMode && icon != null;}
		}


		////////////////

		//Helper function to create a nested graph for a IGraphAssignable
		protected static void CreateNested<T>(IGraphAssignable parent) where T:Graph{
			
			var newGraph = EditorUtils.CreateAsset<T>(true);
			if (newGraph != null){
				Undo.RegisterCreatedObjectUndo(newGraph, "CreateNested");
				parent.nestedGraph = newGraph;
			}
		}

		///Get connection information node wise, to show on top of the connection
		virtual public string GetConnectionInfo(int index){
			return null;
		}

		///Extra inspector controls for the provided OUT connection
		virtual public void OnConnectionInspectorGUI(int index){

		}

		//The main function for drawing a node's gui.Fires off others.
		public void ShowNodeGUI(Vector2 canvasMousePos, float zoomFactor){

			if (isHidden)
				return;

			DrawNodeWindow(canvasMousePos, zoomFactor);
			DrawNodeTag();
			DrawNodeComments();
		}

		//Draw the window
		void DrawNodeWindow(Vector2 canvasMousePos, float zoomFactor){

			///un-colapse children ui
			if (collapsed){
				var r = new Rect(nodeRect.x, (nodeRect.yMax + 10), nodeRect.width, 20);
				EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
				if (GUI.Button(r, "COLLAPSED", "box"))
					collapsed = false;
			}

			GUI.color = isActive? Color.white : new Color(0.9f, 0.9f, 0.9f, 0.8f);
			GUI.color = Graph.currentSelection == this? new Color(0.9f, 0.9f, 1) : GUI.color;
			nodeRect = GUILayout.Window(ID, nodeRect, NodeWindowGUI, string.Empty, "window");

			GUI.Box(nodeRect, "", "windowShadow");
			GUI.color = new Color(1,1,1,0.5f);
			GUI.Box(new Rect(nodeRect.x+6, nodeRect.y+6, nodeRect.width, nodeRect.height), "", "windowShadow");

			if (Application.isPlaying && status != Status.Resting){

				if (status == Status.Success)
					GUI.color = successColor;
				else if (status == Status.Running)
					GUI.color = runningColor;
				else if (status == Status.Failure)
					GUI.color = failureColor;
				else if (status == Status.Error)
					GUI.color = Color.red;

				GUI.Box(nodeRect, "", "windowHighlight");
				
			} else {
				
				if (isSelected){
					GUI.color = restingColor;
					GUI.Box(nodeRect, "", "windowHighlight");
				}
			}

			GUI.color = Color.white;
			if (Graph.allowClick)
				EditorGUIUtility.AddCursorRect(new Rect(nodeRect.x*zoomFactor, nodeRect.y*zoomFactor, nodeRect.width*zoomFactor, nodeRect.height*zoomFactor ), MouseCursor.Link);
		}

		//removes the text color that some nodes add with html tags
		string StripNameColor(string name){
			if (string.IsNullOrEmpty(name))
				return name;
			if (name.StartsWith("<") && name.EndsWith(">")){
				name = name.Replace( name.Substring (0, name.IndexOf(">")+1), "" );
				name = name.Replace( name.Substring (name.IndexOf("<"), name.LastIndexOf(">")+1 - name.IndexOf("<")), "" );
			}
			return name;
		}


		//This is the callback function of the GUILayout.window. Everything here is called INSIDE the node Window callback.
		void NodeWindowGUI(int ID){

/*			//TODO? Background icon way
			var icon = ...
			GUI.color = new Color(1,1,1,0.1f);
			GUI.Box(new Rect(0,0, nodeRect.width, nodeRect.height ), icon);
			GUI.color = Color.white;
*/

			var e = Event.current;

			ShowTitleOrIcon();
			ShowPossibleErrors();
			HandleEvents(e);
			ShowStatusIcons();
			ShowBreakpointMark();
			ShowNodeContents();
			HandleContextMenu(e);
			HandleNodePosition(e);
		}

		//The title name or icon of the node
		void ShowTitleOrIcon(){
			
			if (inIconMode){ //prefs in icon mode AND has icon

				var assignable = this as ITaskAssignable;
				IconAttribute att = null;
				if (assignable != null && assignable.task != null){
					att = assignable.task.GetType().RTGetAttribute<IconAttribute>(true);
				}

				if (att == null){
					att = this.GetType().RTGetAttribute<IconAttribute>(true);
				}

				if (att.fixedColor == false){
					GUI.color = EditorGUIUtility.isProSkin? Color.white : new Color(0f,0f,0f, 0.7f);
				}
				
				GUI.backgroundColor = Color.clear;
				GUILayout.Box(icon, GUILayout.MaxHeight(50));
				GUI.backgroundColor = Color.white;
				GUI.color = Color.white;

			} else {

				if (name != null){
					var title = name;
					var defaultColor = "<color=#eed9a7>";
					if (!EditorGUIUtility.isProSkin){
						title = StripNameColor(title);
						defaultColor = "<color=#222222>";
					}
					GUILayout.Label("<b><size=12>" + defaultColor + title + "</color></size></b>", centerLabel);
				}
			}			
		}

		void ShowPossibleErrors(){
			
			var assignable = this as ITaskAssignable;
			if (assignable != null && assignable.task != null){
				var errors = assignable.task.GetErrors();
				if (errors != null){
			        var errorRect = new Rect(nodeRect.width - 21, 5, 16, 16);
		        	GUI.Box(errorRect, EditorUtils.warningIcon, GUIStyle.none);					
				}
			}
		}

		//Handles events, Mouse downs, ups etc.
		void HandleEvents(Event e){

			//delete shortcut
			if (isSelected && e.keyCode == KeyCode.Delete && e.type == EventType.KeyUp && GUIUtility.keyboardControl == 0){
				Graph.PostGUI += delegate{ graph.RemoveNode(this); };
				e.Use();
			}

			//Duplicate shortcut
			if (isSelected && e.keyCode == KeyCode.D && e.isKey && e.control){
				Graph.PostGUI += delegate{ Graph.currentSelection = Duplicate(graph); };
				e.Use();
			}

			//Node click
			if (Graph.allowClick && e.button != 2 && e.type == EventType.MouseDown){

				if (!e.control)
					Graph.currentSelection = this;

				nodeIsPressed = true;

				//Double click
				if (e.button == 0 && e.clickCount == 2){
		    		if (this is IGraphAssignable && (this as IGraphAssignable).nestedGraph != null ){
	    				graph.currentChildGraph = (this as IGraphAssignable).nestedGraph;
	    				nodeIsPressed = false;
		    		} else if (this is ITaskAssignable && (this as ITaskAssignable).task != null){
		    			EditorUtils.OpenScriptOfType((this as ITaskAssignable).task.GetType());
		    		} else {
			    		EditorUtils.OpenScriptOfType(this.GetType());
		    		}
		    		e.Use();
		    	}


				if (e.control){
					if (isSelected)
						Graph.multiSelection.Remove(this);
					else Graph.multiSelection.Add(this);
				}

		    	OnNodePicked();
			}

	    	//Mouse up
	    	if (e.type == EventType.MouseUp){
	    		nodeIsPressed = false;
	    		if (graph.autoSort)
	    			Graph.PostGUI += delegate { SortConnectionsByPositionX(); };
	    		OnNodeReleased();
	    	}			
		}

		//Shows the icons relative to the current node status
		void ShowStatusIcons(){
	        if (Application.isPlaying && status != Status.Resting){
		        var markRect = new Rect(5, 5, 15, 15);
		        if (status == Status.Success){
		        	GUI.color = successColor;
		        	GUI.Box(markRect, "", new GUIStyle("checkMark"));

		        } else if (status == Status.Running){
		        	GUI.Box(markRect, "", new GUIStyle("clockMark"));

		        } else if (status == Status.Failure){
		        	GUI.color = failureColor;
		        	GUI.Box(markRect, "", new GUIStyle("xMark"));
		        }
		    }
		}

		//Shows the breakpoint mark icon if node is set as a breakpoint
		void ShowBreakpointMark(){
			if (!isBreakpoint)
				return;
			var rect = new Rect(nodeRect.width-15, 5, 12, 12);
			GUI.DrawTexture(rect, EditorUtils.redCircle);
		}

		//Shows the actual node contents GUI
		void ShowNodeContents(){
	        
	        GUI.color = Color.white;
	        GUI.skin = null;
	        GUI.skin.label.richText = true;
	        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUILayout.BeginVertical();

			OnNodeGUI();

			if (this is ITaskAssignable){

				var task = (this as ITaskAssignable).task;
				if (task == null){
					GUILayout.Label("No Task");
				} else {
					GUILayout.Label(NCPrefs.showTaskSummary? task.summaryInfo : string.Format("<b>{0}</b>", task.name));
				}
			}

			GUILayout.EndVertical();
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
		}

		//Handles and shows the right click mouse button for the node context menu
		void HandleContextMenu(Event e){

		    if (Graph.allowClick && e.button == 1 && e.type == EventType.MouseUp){

		    	//Multiselection menu
		    	if (Graph.multiSelection.Count > 0){
		            var menu = new GenericMenu();
		            menu.AddItem (new GUIContent ("Duplicate Selected Nodes"), false, ()=>{ Graph.CopyNodesToGraph(Graph.multiSelection.OfType<Node>().ToList(), graph); });
					menu.AddItem (new GUIContent ("Copy Selected Nodes"), false, ()=>{ copiedNodes = Graph.multiSelection.OfType<Node>().ToArray(); });
		            menu.AddSeparator("/");
		            menu.AddItem (new GUIContent ("Delete Selected Nodes"), false, ()=>{ foreach (Node node in Graph.multiSelection.ToArray()) graph.RemoveNode(node); });
			        Graph.PostGUI += ()=> { menu.ShowAsContext(); }; //Post GUI cause of zoom
			        e.Use();
			        return;

		    	//Single node menu
		    	} else {

		            var menu = new GenericMenu();

		            if (graph.primeNode != this && allowAsPrime)
			            menu.AddItem (new GUIContent ("Set Start"), false, ()=> {graph.primeNode = this;});

			        if (this is IGraphAssignable)
			        	menu.AddItem (new GUIContent ("Edit Nested (Double Click)"), false, ()=> {graph.currentChildGraph = (this as IGraphAssignable).nestedGraph; } );

					
					menu.AddItem (new GUIContent ("Duplicate (CTRL+D)"), false, ()=> {Duplicate(graph);});
		            menu.AddItem( new GUIContent("Copy Node"), false, ()=> { copiedNodes = new Node[]{this}; } );

		            if (inConnections.Count > 0)
			            menu.AddItem (new GUIContent (isActive? "Disable" : "Enable"), false, ()=> {SetActive(!isActive);});

					if (graph.autoSort && outConnections.Count > 0)
						menu.AddItem (new GUIContent (collapsed? "Expand Children" : "Collapse Children"), false, ()=> { collapsed = !collapsed; });

					if (this is ITaskAssignable){
						var assignable = this as ITaskAssignable;
						if (assignable.task != null){
							menu.AddItem (new GUIContent("Copy Assigned Task"), false, ()=> { Task.copiedTask = assignable.task; });
						} else {
							menu.AddDisabledItem(new GUIContent("Copy Assigned Task"));
						}

						if (Task.copiedTask != null) {
							menu.AddItem (new GUIContent("Paste Assigned Task"), false, ()=>
							{
								if (assignable.task == Task.copiedTask)
									return;

								if (assignable.task != null){
									if (!EditorUtility.DisplayDialog("Paste Task", string.Format("Node already has a Task assigned '{0}'. Replace assigned task with pasted task '{1}'?", assignable.task.name, Task.copiedTask.name), "YES", "NO"))
										return;
								}

								try {assignable.task = Task.copiedTask.Duplicate(graph);}
								catch {Debug.LogWarning("Can't paste Task here. Incombatible Types");}
							});

						} else {
							menu.AddDisabledItem(new GUIContent("Paste Assigned Task"));
						}
					}
					
		            OnContextMenu(menu);

					menu.AddSeparator("/");
		            menu.AddItem (new GUIContent ("Delete (DEL)"), false, ()=> {graph.RemoveNode(this);});
		            Graph.PostGUI += ()=> { menu.ShowAsContext(); };
		            e.Use();
		    	}
		    }
		}

		//basicaly handles the node position and draging etc
		void HandleNodePosition(Event e){

		    if (Graph.allowClick && e.button != 2){

	    		//drag all selected nodes
	    		if (e.type == EventType.MouseDrag && Graph.multiSelection.Count > 1){
	    			for (var i = 0; i < Graph.multiSelection.Count; i++){
	    				((Node)Graph.multiSelection[i]).nodePosition += e.delta;
	    			}
	    			return;
	    		}

	    		if (nodeIsPressed){

		    		Undo.RecordObject(graph, "Move Move");

		    		var hierarchicalMove = NCPrefs.hierarchicalMove != e.shift;

		    		//snap to grid
			    	if (!hierarchicalMove && NCPrefs.doSnap && Graph.multiSelection.Count == 0){
			    		nodePosition = new Vector2( Mathf.Round(nodePosition.x/15) * 15, Mathf.Round(nodePosition.y/15) * 15 );
					}		

			    	//recursive drag
			    	if (graph.autoSort && e.type == EventType.MouseDrag){
				    	if (hierarchicalMove || collapsed )
				    		RecursivePanNode(e.delta);
			    	}

		    	}

		    	//this drag
		    	GUI.DragWindow();
		    }			
		}

		//The comments of the node sitting next or bottom of it
		void DrawNodeComments(){

			if (NCPrefs.showComments && !string.IsNullOrEmpty(nodeComment)){

				var commentsRect = new Rect();
				var size = new GUIStyle("textArea").CalcSize(new GUIContent(nodeComment));

				if (showCommentsBottom){
					size.y = new GUIStyle("textArea").CalcHeight(new GUIContent(nodeComment), nodeRect.width);
					commentsRect = new Rect(nodeRect.x, nodeRect.yMax + 5, nodeRect.width, size.y);
				} else {
					commentsRect = new Rect(nodeRect.xMax + 5, nodeRect.yMin, Mathf.Min(size.x, nodeRect.width * 2), nodeRect.height);
				}

				GUI.color = new Color(1,1,1,0.6f);
				GUI.backgroundColor = new Color(1f,1f,1f,0.2f);
				GUI.Box(commentsRect, nodeComment, "textArea");
				GUI.backgroundColor = Color.white;
				GUI.color = Color.white;
			}
		}

		//Shows the tag label on the left of the node if it is tagged
		void DrawNodeTag(){

			if (!string.IsNullOrEmpty(tag)){
				var size = new GUIStyle("label").CalcSize(new GUIContent(tag));
				var tagRect = new Rect(nodeRect.x - size.x -10, nodeRect.y, size.x, size.y);
				GUI.Label(tagRect, tag);
				tagRect.width = 12;
				tagRect.height = 12;
				tagRect.y += tagRect.height + 5;
				tagRect.x = nodeRect.x - 22;
				GUI.DrawTexture(tagRect, EditorUtils.tagIcon);
			}
		}

		//Function to pan the node with children recursively
		void RecursivePanNode(Vector2 delta){

			nodePosition += delta;

			for (var i= 0; i < outConnections.Count; i++){
				var node = outConnections[i].targetNode;
				if (node.ID > this.ID)
					node.RecursivePanNode(delta);
			}
		}

		//The inspector of the node shown in the editor panel or else.
		public void ShowNodeInspectorGUI(){

			UndoManager.CheckUndo(graph, "Node Inspector");

			if (NCPrefs.showNodeInfo){
				GUI.backgroundColor = new Color(0.8f,0.8f,1);
				EditorGUILayout.HelpBox(nodeDescription, MessageType.None);
				GUI.backgroundColor = Color.white;
			}

			GUILayout.BeginHorizontal();
			if (!inIconMode && allowAsPrime){
				customName = EditorGUILayout.TextField(customName);
				EditorUtils.TextFieldComment(customName, "Name...");
			}

			tag = EditorGUILayout.TextField(tag );
			EditorUtils.TextFieldComment(tag, "Tag...");
			GUILayout.EndHorizontal();

			GUI.color = new Color(1,1,1,0.5f);
			nodeComment = EditorGUILayout.TextField(nodeComment);
			GUI.color = Color.white;
			EditorUtils.TextFieldComment(nodeComment);

			EditorUtils.Separator();
			OnNodeInspectorGUI();
			TaskAssignableGUI();

			UndoManager.CheckDirty(graph);
		}


		//If the node implements ITaskAssignable this is shown for the user to assign a task
		void TaskAssignableGUI(){

			if (this is ITaskAssignable){

				var assignable = (this as ITaskAssignable);
				System.Type taskType = null;
				foreach(var iType in this.GetType().GetInterfaces()) {
				    if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(ITaskAssignable<>)){
				        taskType = iType.GetGenericArguments()[0];
				        break;
				    }
				}

				if (taskType != null){
					EditorUtils.TaskField(assignable.task, graph, taskType, (t)=> {iconLoaded = false; assignable.task = t;} );
				}
			}
		}


		//Activates/Deactivates all inComming connections
		void SetActive(bool active){

			if (isChecked)
				return;
			
			isChecked = true;

			//just for visual feedback
			if (!active)
				Graph.currentSelection = null;

			Undo.RecordObject(graph, "SetActive");

			//disable all incomming
			foreach (var cIn in inConnections)
				cIn.isActive = active;
			
			//disable all outgoing
			foreach (var cOut in outConnections)
				cOut.isActive = active;

			//if child is still considered active(= at least 1 incomming is active), continue else SetActive child as well
			foreach (var child in outConnections.Select(c => c.targetNode)){
				
				if (child.isActive == !active)
					continue;

				child.SetActive(active);
			}

			isChecked = false;
		}


		//Sorts the connections based on the child nodes and this node X position. Possible only when not in play mode
		void SortConnectionsByPositionX(){
			
			if (!Application.isPlaying){

				if (isChecked)
					return;

				isChecked = true;

				Undo.RecordObject(graph, "Re-Sort");
				outConnections = outConnections.OrderBy(c => c.targetNode.nodeRect.center.x ).ToList();
				foreach(var connection in inConnections)
					connection.sourceNode.SortConnectionsByPositionX();

				isChecked = false;
			}
		}


		///Draw an automatic editor inspector for this node.
		protected void DrawDefaultInspector(){
			EditorUtils.ShowAutoEditorGUI(this);
		}
		
		//Editor. When the node is picked
		virtual protected void OnNodePicked(){}
		//Editor. When the node is released (mouse up)
		virtual protected void OnNodeReleased(){}
		///Editor. Override to show controls within the node window
		virtual protected void OnNodeGUI(){}
		//Editor. Override to show controls within the inline inspector or leave it to show an automatic editor
		virtual protected void OnNodeInspectorGUI(){ DrawDefaultInspector(); }
		//Editor. Override to add more entries to the right click context menu of the node
		virtual protected GenericMenu OnContextMenu(GenericMenu menu){ return menu; }


		//Draw the connections line from this node, to all of its children. This is the default hierarchical style. Override in each system's base node class.
		virtual public void DrawNodeConnections(Vector2 canvasMousePos, float zoomFactor){

			if (isHidden)
				return;

			var e = Event.current;

			//Receive connections first
			if (clickedPort != null && e.type == EventType.MouseUp && e.button == 0){
		
				if (nodeRect.Contains(e.mousePosition)){
					graph.ConnectNodes(clickedPort.parent, this, clickedPort.portIndex);
					clickedPort = null;
					e.Use();

				} else {

					dragDropMisses ++;

					if (dragDropMisses == graph.allNodes.Count && clickedPort != null){

						var source = clickedPort.parent;
						var index = clickedPort.portIndex;
						var pos = e.mousePosition;						
						clickedPort = null;
						
						System.Action<System.Type> Selected = delegate(System.Type type){
							var newNode = graph.AddNode(type, pos);
							graph.ConnectNodes(source, newNode, index);
							newNode.SortConnectionsByPositionX();
							Graph.currentSelection = newNode;
						};

						//Show in PostGUI cause of zoom factors
						Graph.PostGUI += ()=> {
							EditorUtils.GetTypeSelectionMenu(graph.baseNodeType, Selected).ShowAsContext();
							e.Use();							
						};
					}
				}
			}



			if (maxOutConnections == 0)
				return;

			var nodeOutputBox = new Rect(nodeRect.x, nodeRect.yMax - 4, nodeRect.width, 12);
			GUI.Box(nodeOutputBox, "", (GUIStyle)"nodePortContainer");
			
			//draw the ports
			if (outConnections.Count < maxOutConnections || maxOutConnections == -1){
				for (var i = 0; i < outConnections.Count + 1; i++){
					var portRect = new Rect(0, 0, 10, 10);
					portRect.center = new Vector2(((nodeRect.width / (outConnections.Count + 1)) * (i + 0.5f)) + nodeRect.xMin, nodeRect.yMax + 6);
					GUI.Box(portRect, "", (GUIStyle)"nodePortEmpty");

					if (collapsed)
						continue;

					//start a connection by clicking a port
					EditorGUIUtility.AddCursorRect(portRect, MouseCursor.ArrowPlus);
					if (e.type == EventType.MouseDown && e.button == 0 && portRect.Contains(e.mousePosition)){
						dragDropMisses = 0;
						clickedPort = new GUIPort(i, this, portRect.center);
						e.Use();
					}
				}
			}

			//draw the new drag&drop connection line
			if (clickedPort != null && clickedPort.parent == this){
				var yDiff = (clickedPort.pos.y - e.mousePosition.y) * 0.5f;
				yDiff = e.mousePosition.y > clickedPort.pos.y? -yDiff : yDiff;
				var tangA = new Vector2(0, yDiff);
				var tangB = tangA * -1;
				Handles.DrawBezier(clickedPort.pos, e.mousePosition, clickedPort.pos + tangA , e.mousePosition + tangB, new Color(0.5f,0.5f,0.8f,0.8f), null, 3);
			}

			//draw all connected lines
			for (var connectionIndex = 0; connectionIndex < outConnections.Count; connectionIndex++){
				
				var connection = outConnections[connectionIndex];
				if (connection != null){

					var sourcePos = new Vector2(((nodeRect.width / (outConnections.Count + 1)) * (connectionIndex + 1) ) + nodeRect.xMin, nodeRect.yMax + 6);
					var targetPos = new Vector2(connection.targetNode.nodeRect.center.x, connection.targetNode.nodeRect.y);

					var connectedPortRect = new Rect(0,0,12,12);
					connectedPortRect.center = sourcePos;
					GUI.Box(connectedPortRect, "", (GUIStyle)"nodePortConnected");
			
					if (collapsed || connection.targetNode.isHidden)
						continue;

					connection.DrawConnectionGUI(sourcePos, targetPos);

					//On right click disconnect connection from the source.
					if (e.button == 1 && e.type == EventType.MouseDown && connectedPortRect.Contains(e.mousePosition)){
						graph.RemoveConnection(connection);
						e.Use();
						return;
					}
				}
			}
		}
	}
}

#endif
