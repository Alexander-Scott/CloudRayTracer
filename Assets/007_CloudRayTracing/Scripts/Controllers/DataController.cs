﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class DataController : MonoBehaviour
    {
        public class TransmissionData
        {
            public int curDataIndex; // Current position in the array of data already received.
            public byte[] data;

            public TransmissionData(byte[] _data)
            {
                curDataIndex = 0;
                data = _data;
            }
        }

        private static DataController _instance;
        public static DataController Instance { get { return _instance; } }

        [HideInInspector]
        public CarController centralCar;
        [HideInInspector]
        public ApplicationState applicationState = ApplicationState.Undefined;
        [HideInInspector]
        public bool aiMovement = false;
        [HideInInspector]
        public bool firstPerson = false;
        [HideInInspector]
        public float meshSendRate = 1f;
        [HideInInspector]
        public float networkedObjectSendRate = 0.3f;
        [HideInInspector]
        public float rayTracerGap = 0.02f; // The gap between each ray fired in the sensor bounds

        [Header("Config")]

        public string ipAddress;
        public int defaultBufferSize = 1300; // Max ethernet MTU is ~1400
        public float updateDistance = 5f;
        public float objectSyncDelay = 0.01f;

        public Dictionary<SensorType, bool> activeSensors = new Dictionary<SensorType, bool>();
        
        public Dictionary<int, NetworkedObject> networkedObjectDictionary = new Dictionary<int, NetworkedObject>();

        public enum PacketType { StartRayTracer, StopRayTracer, UpdateNetworkSendRate, UpdateRayTracerGap, UpdateNetworkedObjectSendRate, FinishedSyncing, }
        public enum ApplicationState { Undefined, Client, ClientSynchronising, Server, ServerSynchronising, Host, }
        public enum StatisticType { FPS, MEM, }
        public enum ClientCanvasButtonType { Information, Controls, Viewports, Performance, Sensors, Disconnect, }
        public enum SensorType
        {
            FrontLong,
            FrontShort,
            FrontLeft,
            FrontRight,
            BackLong,
            BackShort,
            BackLeft,
            BackRight,
            RightLong,
            RightShort,
            LeftLong,
            LeftShort,
        }

        public Dictionary<StatisticType, float> performanceDictionary = new Dictionary<DataController.StatisticType, float>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                GetPlayerPrefs();
                _instance = this;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = new Ray();
                RaycastHit hit;

                if (CameraController.Instance.CameraDefault.pixelRect.Contains(Input.mousePosition))
                {
                    ray = CameraController.Instance.CameraDefault.ScreenPointToRay(Input.mousePosition);
                }
                else if (CameraController.Instance.CameraEverything.pixelRect.Contains(Input.mousePosition))
                {
                    ray = CameraController.Instance.CameraEverything.ScreenPointToRay(Input.mousePosition);
                }
                else if (CameraController.Instance.CameraPCOnly.pixelRect.Contains(Input.mousePosition))
                {
                    ray = CameraController.Instance.CameraPCOnly.ScreenPointToRay(Input.mousePosition);
                }
                else if (CameraController.Instance.CameraWireframe.pixelRect.Contains(Input.mousePosition))
                {
                    ray = CameraController.Instance.CameraWireframe.ScreenPointToRay(Input.mousePosition);
                }

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.tag == "Car")
                    {
                        centralCar.GetComponent<CarController>().isFocusCar = false;

                        centralCar = hit.transform.GetComponent<CarController>();
                        centralCar.isFocusCar = true;

                        SensorManager.Instance.transform.parent = centralCar.transform;
                        SensorManager.Instance.transform.localPosition = Vector3.zero;
                        SensorManager.Instance.transform.localEulerAngles = Vector3.zero;
                    }
                }
            }
        }

        public string LocalIPAddress()
        {
#if UNITY_EDITOR_OSX
            return "NULL";
#else
            System.Net.IPHostEntry host;
            string localIP = "";
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
#endif
        }

        public void GetPlayerPrefs()
        {
            ipAddress = PlayerPrefs.GetString("IPAddress", "127.0.0.1");

            int numOfSensorTypes = Enum.GetNames(typeof(SensorType)).Length;

            for (int i = 0; i < numOfSensorTypes; i++)
            {
                string key = "SensorState" + ((SensorType)i).ToString();
                bool active = bool.Parse(PlayerPrefs.GetString(key, "true"));
                activeSensors[(SensorType)i] = active;
            }
        }

        public void SaveSensorState(SensorType sensorType, bool state)
        {
            activeSensors[sensorType] = state;
            SensorManager.Instance.ToggleSensor(sensorType, state);
            PlayerPrefs.SetString("SensorState" + sensorType.ToString(), state.ToString());
            PlayerPrefs.Save();
        }
    }
}
