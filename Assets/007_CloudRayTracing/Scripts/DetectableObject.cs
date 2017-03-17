﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableObject : MonoBehaviour
{
    private Vector3 oldKey;

    void Start()
    {
        oldKey = transform.position;
    }

	// Update is called once per frame
	void Update ()
    {
        if (GlobalVariables.isClient && GlobalVariables.activated)
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;

                ClientController.Instance.UpdateObjectPositionOnServer(oldKey, transform.position, transform.eulerAngles, transform.localScale);

                oldKey = transform.position;
            }
        }
    }
}