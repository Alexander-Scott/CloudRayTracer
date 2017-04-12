﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ServerController : MonoBehaviour
    {
        #region Singleton

        private static ServerController _instance;

        public static ServerController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        #endregion

        private Server server;

        private int transmissionID = 0;

        // Use this for initialization
        void Start()
        {
            server = new Server();

            server.OnPeerConnected += Server_OnPeerConnected;
        }

        public void StartServer()
        {
            server.StartServer(7777);

            foreach (NetworkedObject netObj in DataController.Instance.networkedObjectDictionary.Values)
            {
                netObj.ServerStart();
            }

            MenuController.Instance.UpdateSubTitleText("You are the SERVER");
        }

        public void UpdateObjectPosition(int objectID, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (DataController.Instance.networkedObjectDictionary.ContainsKey(objectID))
            {
                GameObject go = DataController.Instance.networkedObjectDictionary[objectID].gameObject;
                go.transform.position = position;
                go.transform.eulerAngles = rotation;
                go.transform.localScale = localScale;
            }
            else
            {
                Debug.Log("Object with ID " + objectID + " not found in UpdateObjectPosition");
            }
        }

        public void UpdateObjectState(int objectID, bool active)
        {
            if (DataController.Instance.networkedObjectDictionary.ContainsKey(objectID))
            {
                DataController.Instance.networkedObjectDictionary[objectID].gameObject.SetActive(active);
            }
            else
            {
                Debug.Log("Object with ID " + objectID + " not found in UpdateObjectState");
            }
        }

        public void UpdateObjectStateAndPosition(int objectID, bool active, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (DataController.Instance.networkedObjectDictionary.ContainsKey(objectID))
            {
                GameObject go = DataController.Instance.networkedObjectDictionary[objectID].gameObject;
                go.SetActive(active);
                go.transform.position = position;
                go.transform.eulerAngles = rotation;
                go.transform.localScale = localScale;
            }
            else
            {
                Debug.Log("Object with ID " + objectID + " not found in UpdateObjectStateAndPosition");
            }
        }

        public void SendPacket(DataController.PacketType packetType, string contents)
        {
            server.Connection.SendPacket((int)packetType, contents);
        }

        public void PacketRecieved(DataController.PacketType packetType, string contents)
        {
            switch (packetType)
            {
                case DataController.PacketType.StartRayTracer:
                    Debug.Log("Raytrace start");
                    RayTraceController.Instance.StartRayTracing();
                    break;

                case DataController.PacketType.StopRayTracer:
                    Debug.Log("Raytrace stop");
                    RayTraceController.Instance.StopRayTracing();
                    break;

                case DataController.PacketType.UpdateHitPositionsSendRate:
                    float parseHitPositionsSendRate;
                    if (float.TryParse(contents, out parseHitPositionsSendRate))
                    {
                        Debug.Log("Hit positions send rate set to " + parseHitPositionsSendRate);
                        DataController.Instance.hitPositionsSendRate = parseHitPositionsSendRate;
                    }
                    break;

                case DataController.PacketType.UpdateRayTracerGap:
                    float parseRayTracerGap;
                    if (float.TryParse(contents, out parseRayTracerGap))
                    {
                        Debug.Log("Ray tracer gap set to " + parseRayTracerGap);
                        DataController.Instance.rayTracerGap = parseRayTracerGap;
                    }
                    break;

                case DataController.PacketType.UpdateNetworkedObjectSendRate:
                    float parseNetworkObjectSendRate;
                    if (float.TryParse(contents, out parseNetworkObjectSendRate))
                    {
                        Debug.Log("Networked Object send rate set to " + parseNetworkObjectSendRate);
                        DataController.Instance.networkedObjectSendRate = parseNetworkObjectSendRate;
                    }
                    break;

                case DataController.PacketType.FinishedSyncing:
                    Debug.Log(DataController.Instance.networkedObjectDictionary.Count + " objects synced");
                    Timing.RunCoroutine(SendPerformanceData(), "SendPerformanceData");
                    DataController.Instance.applicationState = DataController.ApplicationState.Server;
                    break;

                case DataController.PacketType.UpdateCentralCar:
                    int parseObjID;
                    if (int.TryParse(contents, out parseObjID))
                    {
                        Debug.Log("Central car set to network object with ID of " + parseObjID);
                        DataController.Instance.centralCar = DataController.Instance.networkedObjectDictionary[parseObjID].GetComponent<CarController>();
                        SensorManager.Instance.transform.parent = DataController.Instance.centralCar.transform;
                        SensorManager.Instance.transform.localPosition = Vector3.zero;
                        SensorManager.Instance.transform.localEulerAngles = Vector3.zero;
                    }
                        
                    break;

                case DataController.PacketType.SetSensorDisabled:
                    int parseSensorID;
                    if (int.TryParse(contents, out parseSensorID))
                    {
                        Debug.Log("Sensor " + parseSensorID + " set to disabled");
                        DataController.Instance.activeSensors[(DataController.SensorType)parseSensorID] = false;
                        SensorManager.Instance.ToggleSensor((DataController.SensorType)parseSensorID, false);
                    }
                    break;

                case DataController.PacketType.SetSensorEnabled:
                    int parseSensorID2;
                    if (int.TryParse(contents, out parseSensorID2))
                    {
                        Debug.Log("Sensor " + parseSensorID2 + " set to enabled");
                        DataController.Instance.activeSensors[(DataController.SensorType)parseSensorID2] = true;
                        SensorManager.Instance.ToggleSensor((DataController.SensorType)parseSensorID2, true);
                    }
                    break;
            }
        }

        public void SendHitPositionsToClient(List<Vector3> hitPostions)
        {
            if (server.NumberOfPeers > 0)
            {
                byte[] result = VectorsToBytes(hitPostions);
                StartCoroutine(server.Connection.SendBytesToClientsRoutine(transmissionID, result));

                transmissionID++;
            }
            else
            {
                RayTraceController.Instance.StopRayTracing();
            }
        }

        //byte[] SerializeObject<_T>(_T objectToSerialize)
        ////same as above, but should technically work anyway
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    MemoryStream memStr = new MemoryStream();
        //    bf.Serialize(memStr, objectToSerialize);
        //    memStr.Position = 0;
        //    return memStr.ToArray();
        //}

        private byte[] VectorsToBytes(List<Vector3> hitPositions)
        {
            byte[] buff = new byte[(sizeof(float) * 3) * hitPositions.Count];
            int buffIndex = 0;

            for (int i = 0; i < hitPositions.Count; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(hitPositions[i].x), 0, buff, buffIndex * sizeof(float), sizeof(float));
                buffIndex++;
                Buffer.BlockCopy(BitConverter.GetBytes(hitPositions[i].y), 0, buff, buffIndex * sizeof(float), sizeof(float));
                buffIndex++;
                Buffer.BlockCopy(BitConverter.GetBytes(hitPositions[i].z), 0, buff, buffIndex * sizeof(float), sizeof(float));
                buffIndex++;
            }

            return buff;
        }

        public void Server_OnPeerConnected(Peer obj)
        {
            Debug.Log("Peer connected!");

            DataController.Instance.applicationState = DataController.ApplicationState.ServerSynchronising;
        }

        private void Server_OnPeersDisconnected()
        {
            Debug.Log("Peer disconnected");

            for (int i = 0; i < TrafficController.Instance.trafficCars.Count; i++)
            {
                Destroy(TrafficController.Instance.trafficCars[i]);
            }

            TrafficController.Instance.trafficCars.Clear();
        }

        public IEnumerator<float> SendPerformanceData()
        {
            while (server.NumberOfPeers > 0)
            {
                server.Connection.SendPerformanceDictionary((int)DataController.StatisticType.FPS, Mathf.Floor(1.0f / Time.deltaTime));
                server.Connection.SendPerformanceDictionary((int)DataController.StatisticType.MEM, GC.GetTotalMemory(false));

                yield return Timing.WaitForSeconds(0.5f);
            }

            Server_OnPeersDisconnected();
        }
    }
}
