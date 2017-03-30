﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasControlsController : MonoBehaviour
    {
        public Toggle rayTracerToggle;
        public Toggle carControlToggle;
        public InputField networkSendRateInput;
        public InputField rayTracerGapSizeInput;
        public InputField networkedObjectSendRateInput;

        // Use this for initialization
        void Start()
        {
            rayTracerToggle.onValueChanged.AddListener(RayTracerChanged);
            carControlToggle.onValueChanged.AddListener(CarControlChanged);
            networkedObjectSendRateInput.onValueChanged.AddListener(NetworkedObjectSendRateInput);

            DataController.Instance.meshSendRate = PlayerPrefs.GetFloat("MeshSendRate", DataController.Instance.meshSendRate);
            DataController.Instance.rayTracerGap = PlayerPrefs.GetFloat("RayTracerGap", DataController.Instance.rayTracerGap);
            DataController.Instance.networkedObjectSendRate = PlayerPrefs.GetFloat("NetworkedObjectSendRate", DataController.Instance.networkedObjectSendRate);

            networkSendRateInput.text = DataController.Instance.meshSendRate.ToString();
            rayTracerGapSizeInput.text = DataController.Instance.rayTracerGap.ToString();
            networkedObjectSendRateInput.text = DataController.Instance.networkedObjectSendRate.ToString();
        }

        public void MeshSendRateChanged(string newVal)
        {
            float parsedVal;
            if(float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.meshSendRate = parsedVal;
                PlayerPrefs.SetFloat("NetworkSendRate", parsedVal);
                PlayerPrefs.Save();
                if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdateNetworkSendRate, newVal);
                }
            }
            else
            {
                networkSendRateInput.text = DataController.Instance.meshSendRate.ToString();
            }
        }

        private void RayTracerChanged(bool arg0)
        {
            if (arg0)
            {
                if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                {
                    ClientController.Instance.StartRayTracer();
                }
                else
                {
                    HostController.Instance.StartRayTracer();
                }
            }
            else
            {
                if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                {
                    ClientController.Instance.StopRayTracer();
                }
                else
                {
                    HostController.Instance.StopRayTracer();
                }
            }
        }

        private void CarControlChanged(bool arg0)
        {
            DataController.Instance.carControl = arg0;
        }

        private void NetworkedObjectSendRateInput(string newVal)
        {
            float parsedVal;
            if (float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.networkedObjectSendRate = parsedVal;
                PlayerPrefs.SetFloat("NetworkedObjectSendRate", parsedVal);
                PlayerPrefs.Save();
                if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdateNetworkedObjectSendRate, newVal);
                }
            }
            else
            {
                networkSendRateInput.text = DataController.Instance.meshSendRate.ToString();
            }
        }

        public void RayTracerGapSizeChanged(string newVal)
        {
            float parsedVal;
            if (float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.rayTracerGap = parsedVal;
                PlayerPrefs.SetFloat("RayTracerGap", parsedVal);
                PlayerPrefs.Save();
                if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdateNetworkSendRate, newVal);
                }
            }
            else
            {
                rayTracerGapSizeInput.text = DataController.Instance.rayTracerGap.ToString();
            }
        }
    }
}
