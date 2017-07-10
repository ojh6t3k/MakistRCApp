using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine.Events;
using Ardunity;


public class MakistRCApp : MonoBehaviour
{
	public HM10 hm10;
	public Button connect;
	public Button disconnect;
	public Button quit;

	public Canvas popupCanvas;
	public RectTransform settingCommSocket;
	public Button ok;
	public Button cancel;
	public ListView deviceList;
	public ListItem deviceItem;

	public Canvas messageCanvas;
	public RectTransform msgConnecting;
	public RectTransform msgConnectionFailed;
	public RectTransform msgLostConnection;
	public RectTransform msgNotSupport;
	public Button okConnectionFailed;
	public Button okLostConnection;
	public Button okNotSupport;

	public Slider steering;
	public Slider speed;
	public Image image_left;
	public Image image_center;
	public Image image_right;
	public Image image_d5;
	public Image image_d4;
	public Image image_d3;
	public Image image_d2;
	public Image image_d1;
	public Image image_n;
	public Image image_r1;
	public Image image_r2;
	public Image image_r3;
	public Image image_r4;
	public Image image_r5;
	public Toggle lightControl;
	public Toggle soundControl;
	public Toggle gyroControl;
	public Toggle autoDriveOn;
	public Image batteryHigh;
	public Image batteryMid;
	public Image batteryLow;
	public Text batteryText;

	private float _time = 0f;
	private bool _first = true;
	private bool _connected = false;
	private bool _autoDrive = false;
	private bool _oldLight = false;
	private bool _oldSound = false;
	private float _oldSteering = 0f;
	private float _oldSpeed = 0f;
	private float _newSteering = 0f;
	private float _newSpeed = 0f;
	private List<byte> _rxDataBytes = new List<byte>();

	void Awake()
	{
		ok.onClick.AddListener(OnOKClick);
		cancel.onClick.AddListener(OnCancelClick);

		hm10.OnStartSearch.AddListener(OnStartSearch);
		hm10.OnFoundDevice.AddListener(OnFoundDevice);
		hm10.OnStopSearch.AddListener(OnStopSearch);
		hm10.OnOpen.AddListener(OnBleOpen);
		hm10.OnClose.AddListener(OnBleClose);
		hm10.OnOpenFailed.AddListener(OnBleOpenFailed);
		hm10.OnErrorClosed.AddListener(OnBleErrorClosed);

		deviceList.OnSelectionChanged.AddListener(OnSelectionChanged);

		connect.onClick.AddListener(OnConnectClick);
		disconnect.onClick.AddListener(OnDisconnectClick);
		quit.onClick.AddListener(OnQuitClick);

		okConnectionFailed.onClick.AddListener(OnMessageOKClick);
		okLostConnection.onClick.AddListener(OnMessageOKClick);
		okNotSupport.onClick.AddListener(OnMessageOKClick);

		steering.onValueChanged.AddListener(OnSteeringChanged);
		speed.onValueChanged.AddListener(OnSpeedChanged);
		gyroControl.onValueChanged.AddListener(OnGyroChanged);
	}

	// Use this for initialization
	void Start ()
	{
		popupCanvas.gameObject.SetActive(false);
		settingCommSocket.gameObject.SetActive(false);
		deviceList.ClearItem();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(gyroControl.isOn)
		{
			Vector2 origin = new Vector2(0f, 1f);
			Vector2 current = new Vector2(Input.acceleration.x, -Input.acceleration.y);
			float angle = Vector2.Angle(origin, current);
			if(current.x > 0f)
				angle = -angle;

			float temp = Mathf.Clamp(-angle / 90f, -1f, 1f);
			_newSteering = Mathf.Round(temp * 10f) / 10f;
			UpdateSteeringUI();
		}

		_time += Time.deltaTime;
		if(_time >= 0.03f) // per 30msec
		{
			_time = 0;
			if(_connected)
			{
				if(((_newSteering != _oldSteering) && !_autoDrive) || _first)
				{
					hm10.Write(Encoding.ASCII.GetBytes(string.Format("T:{0:f1}\n", _newSteering)));
					_oldSteering = _newSteering;
				}

				if(((_newSpeed != _oldSpeed) && !_autoDrive) || _first)
				{
					hm10.Write(Encoding.ASCII.GetBytes(string.Format("P:{0:f1}\n", _newSpeed)));
					_oldSpeed = _newSpeed;
				}

				if(((_oldLight != lightControl.isOn) && !_autoDrive) || _first)
				{
					if(lightControl.isOn)
						hm10.Write(Encoding.ASCII.GetBytes("L:1\n"));
					else
						hm10.Write(Encoding.ASCII.GetBytes("L:0\n"));
					_oldLight = lightControl.isOn;
				}

				if(((_oldSound != soundControl.isOn) && !_autoDrive) || _first)
				{
					if(soundControl.isOn)
						hm10.Write(Encoding.ASCII.GetBytes("S:1\n"));
					else
						hm10.Write(Encoding.ASCII.GetBytes("S:0\n"));
					_oldSound = soundControl.isOn;
				}

				if((_autoDrive != autoDriveOn.isOn) || _first)
				{
					if(autoDriveOn.isOn)
					{
						hm10.Write(Encoding.ASCII.GetBytes("A:1\n"));
						soundControl.isOn = false;
						lightControl.isOn = false;
					}
					else
						hm10.Write(Encoding.ASCII.GetBytes("A:0\n"));
					
					_autoDrive = autoDriveOn.isOn;
				}

				_first = false;

				byte[] data = hm10.Read();
				if(data != null)
				{
					_rxDataBytes.AddRange(data);
					for(int i = 0; i < _rxDataBytes.Count; i++)
					{
						if(_rxDataBytes[i] == '\n')
						{
							string s1 = Encoding.ASCII.GetString(_rxDataBytes.GetRange(0, i).ToArray());
							_rxDataBytes.RemoveRange(0, i + 1);
							string[] tokens = s1.Split(new char[] { ':' });
							if(tokens.Length == 2)
							{
								if(tokens[0].Equals("B2") || tokens[0].Equals("B3"))
								{
									float voltage = float.Parse(tokens[1]);
									batteryText.gameObject.SetActive(true);
									batteryText.text = string.Format("{0:F1}v", voltage);
									float percent = 0f;
									if(tokens[0].Equals("B2"))
										percent = (voltage - 5.4f) / (7.4f - 5.4f);
									else if(tokens[0].Equals("B3"))
										percent = (voltage - 8.1f) / (11.1f - 8.1f);
									percent = Mathf.Clamp(percent * 100f, 0f, 100f);
									if(percent > 75f)
									{
										batteryHigh.gameObject.SetActive(true);
										batteryMid.gameObject.SetActive(false);
										batteryLow.gameObject.SetActive(false);
									}
									else if(percent > 25f)
									{
										batteryHigh.gameObject.SetActive(false);
										batteryMid.gameObject.SetActive(true);
										batteryLow.gameObject.SetActive(false);
									}
									else if(percent > 0f)
									{
										batteryHigh.gameObject.SetActive(false);
										batteryMid.gameObject.SetActive(false);
										batteryLow.gameObject.SetActive(true);
									}
									else
									{
										batteryHigh.gameObject.SetActive(false);
										batteryMid.gameObject.SetActive(false);
										batteryLow.gameObject.SetActive(false);
									}
								}
							}
						}
					}
				}
			}
		}
	}

	private void OnConnectClick()
	{
		if(hm10.isSupport)
		{
			popupCanvas.gameObject.SetActive(true);
			settingCommSocket.gameObject.SetActive(true);
			deviceList.ClearItem();
			ok.interactable = false;
			hm10.StartSearch();
		}
		else
		{
			messageCanvas.gameObject.SetActive(true);
			msgConnecting.gameObject.SetActive(false);
			msgConnectionFailed.gameObject.SetActive(false);
			msgLostConnection.gameObject.SetActive(false);
			msgNotSupport.gameObject.SetActive(true);
		}
	}

	private void OnDisconnectClick()
	{
		hm10.Close();
	}

	private void OnQuitClick()
	{
		hm10.Close();
		Application.Quit();
	}

	private void OnOKClick()
	{
		ListItem selectedItem = deviceList.selectedItem;
		if(selectedItem != null)
		{
			hm10.device = new CommDevice((CommDevice)selectedItem.data);
			hm10.Open();
		}

		popupCanvas.gameObject.SetActive(false);
		settingCommSocket.gameObject.SetActive(false);

		messageCanvas.gameObject.SetActive(true);
		msgConnecting.gameObject.SetActive(true);
		msgConnectionFailed.gameObject.SetActive(false);
		msgLostConnection.gameObject.SetActive(false);
	}

	private void OnCancelClick()
	{
		popupCanvas.gameObject.SetActive(false);
		settingCommSocket.gameObject.SetActive(false);
		hm10.StopSearch();
	}

	private void OnSelectionChanged(ListItem item)
	{
		if(item != null)
		{
			ok.interactable = true;
		}
		else
		{
			ok.interactable = false;
		}
	}

	private void OnStartSearch()
	{
		deviceList.ClearItem();
		ok.interactable = false;
	}

	private void OnFoundDevice(CommDevice device)
	{
		ListItem item = GameObject.Instantiate(deviceItem);
		item.gameObject.SetActive(true);
		item.textList[0].text = device.name;
		if(item.textList.Length > 1)
			item.textList[1].text = device.address;
		item.data = device;

		deviceList.AddItem(item);

		if(deviceList.selectedItem == null && hm10.device != null)
		{
			if(hm10.device.Equals(device))
			{
				deviceList.selectedItem = item;
				ok.interactable = true;
			}
		}			
	}

	private void OnMessageOKClick()
	{
		messageCanvas.gameObject.SetActive(false);
	}

	private void OnStopSearch()
	{
	}

	private void OnBleOpen()
	{
		disconnect.gameObject.SetActive(true);
		connect.gameObject.SetActive(false);

		messageCanvas.gameObject.SetActive(false);
		msgConnecting.gameObject.SetActive(false);
		msgConnectionFailed.gameObject.SetActive(false);
		msgLostConnection.gameObject.SetActive(false);
		msgNotSupport.gameObject.SetActive(false);

		_connected = true;
		_first = true;
		_newSteering = steering.value;
		_newSpeed = speed.value;
	}

	private void OnBleClose()
	{
		disconnect.gameObject.SetActive(false);
		connect.gameObject.SetActive(true);

		_connected = false;
		_autoDrive = false;
		autoDriveOn.isOn = false;
		batteryHigh.gameObject.SetActive(false);
		batteryMid.gameObject.SetActive(false);
		batteryLow.gameObject.SetActive(false);
		batteryText.gameObject.SetActive(false);
		_rxDataBytes.Clear();
	}

	private void OnBleOpenFailed()
	{
		messageCanvas.gameObject.SetActive(true);
		msgConnecting.gameObject.SetActive(false);
		msgConnectionFailed.gameObject.SetActive(true);
		msgLostConnection.gameObject.SetActive(false);
		msgNotSupport.gameObject.SetActive(false);
	}

	private void OnBleErrorClosed()
	{
		messageCanvas.gameObject.SetActive(true);
		msgConnecting.gameObject.SetActive(false);
		msgConnectionFailed.gameObject.SetActive(false);
		msgLostConnection.gameObject.SetActive(true);
		msgNotSupport.gameObject.SetActive(false);

		OnBleClose();
	}

	private void OnSteeringChanged(float value)
	{
		_newSteering = Mathf.Round(value * 10f) / 10f;
		UpdateSteeringUI();
	}

	private void UpdateSteeringUI()
	{
		if(_newSteering > 0.1f)
		{
			image_left.gameObject.SetActive(false);
			image_center.gameObject.SetActive(false);
			image_right.gameObject.SetActive(true);
		}
		else if(_newSteering < -0.1f)
		{
			image_left.gameObject.SetActive(true);
			image_center.gameObject.SetActive(false);
			image_right.gameObject.SetActive(false);
		}
		else
		{
			image_left.gameObject.SetActive(false);
			image_center.gameObject.SetActive(true);
			image_right.gameObject.SetActive(false);
		}
	}

	private void OnSpeedChanged(float value)
	{
		if(value > 0.8)
		{
			image_d5.gameObject.SetActive(true);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value > 0.6)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(true);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value > 0.4)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(true);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value > 0.2)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(true);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value >= 0.05f)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(true);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value > -0.05f)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(true);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value > -0.2)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(true);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value > -0.4)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(true);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value > -0.6)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(true);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(false);
		}
		else if(value > -0.8)
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(true);
			image_r5.gameObject.SetActive(false);
		}
		else
		{
			image_d5.gameObject.SetActive(false);
			image_d4.gameObject.SetActive(false);
			image_d3.gameObject.SetActive(false);
			image_d2.gameObject.SetActive(false);
			image_d1.gameObject.SetActive(false);
			image_n.gameObject.SetActive(false);
			image_r1.gameObject.SetActive(false);
			image_r2.gameObject.SetActive(false);
			image_r3.gameObject.SetActive(false);
			image_r4.gameObject.SetActive(false);
			image_r5.gameObject.SetActive(true);
		}

		_newSpeed = Mathf.Round(value * 10f) / 10f;
	}

	private void OnGyroChanged(bool value)
	{
		steering.interactable = !value;

		if(!value)
		{
			_newSteering = steering.value;
			UpdateSteeringUI();
		}
	}
}
