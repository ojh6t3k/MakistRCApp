using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

namespace Ardunity
{
    public enum StreamClass
    {
        Serial,
        Serial1,
        Serial2,
        Serial3,
        SoftwareSerial,
		AltSoftSerial,
        Bridge
    }
        
	public enum Resolution
	{
		bit8 = 256,
		bit10 = 1024,
		bit12 = 4096
	}
	
	public enum CheckEdge
	{
		RisingEdge,
		FallingEdge
	}
	
	public enum Axis
	{
		X,
		Y,
		Z
	}
    
    public class Trigger
    {
        private bool _value = true;
        
        public bool value
        {
            get
            {
                if(_value)
                {
                    _value = false;
                    return true;
                }
                else
                    return false;
            }            
        }
        
        public void Reset()
        {
            _value = true;
        }
        
        public void Clear()
        {
            _value = false;
        }
    }
	
	[Serializable]
	public class BoolEvent : UnityEvent<bool> {}

	[Serializable]
	public class IntEvent : UnityEvent<int> {}
	
	[Serializable]
	public class FloatEvent : UnityEvent<float> {}

	[Serializable]
	public class ColorEvent : UnityEvent<Color> {}

	[Serializable]
	public class QuaternionEvent : UnityEvent<Quaternion> {}
	
	[Serializable]
	public class CommDeviceEvent : UnityEvent<CommDevice> {}
}
