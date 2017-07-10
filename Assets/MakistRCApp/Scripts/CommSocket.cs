using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;


namespace Ardunity
{
	public class ArdunityBLE
	{
		public static string serviceUUID = "6C1AD300-DEEF-4EDA-AFA5-28AEAFCCD7F5";
		public static string txUUID = "6C1AD301-DEEF-4EDA-AFA5-28AEAFCCD7F5";
		public static string rxUUID = "6C1AD302-DEEF-4EDA-AFA5-28AEAFCCD7F5";
		public static string nameUUID = "6C1AD305-DEEF-4EDA-AFA5-28AEAFCCD7F5";
	}

	[Serializable]
    public class CommDevice
    {
        public string name = "";
        public string address = "";
        public List<string> args = new List<string>();

        public CommDevice()
        {

        }

        public CommDevice(CommDevice device)
        {
            name = device.name;
            address = device.address;
            for (int i = 0; i < device.args.Count; i++)
                args.Add(device.args[i]);
        }

        public bool Equals(CommDevice device)
        {
            if (device == null)
                return false;

            if (!name.Equals(device.name))
                return false;

            if (!address.Equals(device.address))
                return false;

            if (args.Count != device.args.Count)
                return false;

            for (int i = 0; i < args.Count; i++)
            {
                if (!args[i].Equals(device.args[i]))
                    return false;
            }

            return true;
        }
    }
	
	[AddComponentMenu("ARDUnity/Internal/CommSocket")]
	public class CommSocket : ArdunityObject
	{
		[SerializeField]
        public List<CommDevice> foundDevices = new List<CommDevice>();
        [SerializeField]
        public CommDevice device = new CommDevice();

        public UnityEvent OnOpen;
        public UnityEvent OnClose;
        public UnityEvent OnOpenFailed;
        public UnityEvent OnErrorClosed;
        public UnityEvent OnStartSearch;
        public UnityEvent OnStopSearch;
		public UnityEvent OnWriteCompleted;
        public CommDeviceEvent OnFoundDevice;

		public virtual void Open()
        {
        }

        public virtual void Close()
        {
        }

        protected virtual void ErrorClose()
        {

        }

        public virtual void StartSearch()
        {
        }

        public virtual void StopSearch()
        {
        }

		public virtual void Write(byte[] data, bool getCompleted = false)
        {
        }

        public virtual byte[] Read()
        {
            return null;
        }

        public virtual bool IsOpen
        {
            get
            {
                return false;
            }
        }
        
        protected override void AddNode(List<Node> nodes)
        {
            base.AddNode(nodes);
            
            nodes.Add(new Node("CommSocket", "CommSocket", typeof(CommSocket), NodeType.WireTo, "CommSocket"));
        }

		protected override void UpdateNode(Node node)
		{
			if(node.name.Equals("CommSocket"))
			{
				node.updated = true;
				return;
			}

			base.UpdateNode(node);
		}
	}
}

