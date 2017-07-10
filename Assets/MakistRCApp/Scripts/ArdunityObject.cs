using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


namespace Ardunity
{
	public enum NodeType
	{
		None,
		WireTo,
		WireFrom
	}
    
    [Serializable]
	public class Node
	{
		public string name;
        public string text;
        public NodeType nodeType;
		public Type objectType;		
        public ArdunityObject objectTarget;
        public string tooltip;
		public Rect rect;
		public bool updated;
        
        public Node(string name, string text, Type objectType, NodeType nodeType, string tooltip = "")
		{
			this.name = name;
            this.text = text;
			this.objectType = objectType;
			this.nodeType = nodeType;
            this.tooltip = tooltip;
            this.objectTarget = null;
			this.updated = false;
		}
        
        public GUIContent guiContent
        {
            get
            {
                if(nodeType == NodeType.None)
                    return new GUIContent(string.Format("{0}", text), tooltip);
                else if(nodeType == NodeType.WireFrom)
                {
                    if(objectType.IsSubclassOf(typeof(ArdunityObject)))
                        return new GUIContent(string.Format("● {0} ●", text), tooltip);
                    else if(objectType.GetInterface("IWireInput") != null)
                        return new GUIContent(string.Format("→ {0} ←", text), tooltip);
                    else if(objectType.GetInterface("IWireOutput") != null)
                        return new GUIContent(string.Format("← {0} →", text), tooltip);
                }
                else if(nodeType == NodeType.WireTo)
                {
                    if(objectType.IsSubclassOf(typeof(ArdunityObject)))
                        return new GUIContent(string.Format("○ {0} ○", text), tooltip);
                    else if(objectType.GetInterface("IWireInput") != null)
                        return new GUIContent(string.Format("← {0} →", text), tooltip);
                    else if(objectType.GetInterface("IWireOutput") != null)
                        return new GUIContent(string.Format("→ {0} ←", text), tooltip);
                }
                
                return null;
            }
        }
	}

	[AddComponentMenu("ARDUnity/Internal/ArdunityObject")]
	public class ArdunityObject : MonoBehaviour
	{
		[HideInInspector]
		public Rect windowRect;
		[HideInInspector]
		public Node[] nodes;
        
        protected virtual void Awake()
        {
            InitializeNode();
        }

		protected virtual void Reset()
		{
			InitializeNode();
		}
        
        public Node FindNode(string name)
        {
            if(nodes != null)
            {
                foreach(Node node in nodes)
                {
                    if(node.name.Equals(name))
                        return node;
                }
            }
            
            return null;
        }
		
		public void InitializeNode()
		{
            List<Node> nodes = new List<Node>();
            
            AddNode(nodes);
            
            Type t = GetType();
            nodes.Add(new Node(t.Name, t.Name, t, NodeType.WireTo, t.Name));
            
            // Sort
            List<Node> displays = new List<Node>();
            List<Node> references = new List<Node>();
            List<Node> interfaces = new List<Node>();
            
            for(int i=0; i<nodes.Count; i++)
            {
                if(nodes[i].nodeType == NodeType.None)
                    displays.Add(nodes[i]);
                else if(nodes[i].nodeType == NodeType.WireFrom)
                    references.Add(nodes[i]);
                else if(nodes[i].nodeType == NodeType.WireTo)
                    interfaces.Add(nodes[i]);
            }
            
            nodes.Clear();
            nodes.AddRange(displays.ToArray());
            nodes.AddRange(references.ToArray());
            nodes.AddRange(interfaces.ToArray());
            
            if(this.nodes == null)
                this.nodes = nodes.ToArray();
            else
            {
                int count = 0;
                for(int i=0; i<nodes.Count; i++)
                {
                    for(int j=0; j<this.nodes.Length; j++)
                    {
                        if(nodes[i].name.Equals(this.nodes[j].name))
                        {
                            this.nodes[j].text = nodes[i].text;
                            this.nodes[j].objectType = nodes[i].objectType;
                            this.nodes[j].tooltip = nodes[i].tooltip;
                            nodes[i].objectTarget = this.nodes[j].objectTarget;
                            count++;
                            break;
                        }
                    }
                }
                
                if(count != nodes.Count)
                    this.nodes = nodes.ToArray();
            }
            
            UpdateNode();
		}
		
		protected virtual void AddNode(List<Node> nodes)
		{
			
		}
        
        public void UpdateNode()
        {
			Type t = GetType();
			List<Node> list = new List<Node>(nodes);
			for(int i = 0; i < list.Count; i++)
			{
				list[i].updated = false;
				if(list[i].name.Equals(t.Name))
					list[i].updated = true;
				else
					UpdateNode(list[i]);
				
				if(!list[i].updated)
				{
					list.RemoveAt(i);
					i--;
				}
			}

			nodes = list.ToArray();
        }
        
        protected virtual void UpdateNode(Node node)
        {
        }
	}
}

