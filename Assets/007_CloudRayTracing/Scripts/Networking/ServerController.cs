﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

	// Use this for initialization
	void Start ()
    {
        server = new Server();

        server.OnPeerConnected += Server_OnPeerConnected;
    }

    public void UpdateObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
    {
        GameObject go = ObjectManager.Instance.GetGameObject(oldKey);
        go.transform.position = position;
        go.transform.eulerAngles = rotation;
        go.transform.localScale = localScale;

        ObjectManager.Instance.UpdateKey(oldKey);
    }

    public void StartServer()
    {
        GlobalVariables.isClient = false;
        GlobalVariables.activated = true;
        server.StartServer(7777);

        UIManager.Instance.UpdateSubTitleText("You are the SERVER");
    }

    private void Server_OnPeerConnected(Peer obj)
    {
        Debug.Log("Peer connected!");
    }
}