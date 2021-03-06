﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class PointCloudController : MonoBehaviour
    {
        #region Singleton

        private static PointCloudController _instance;

        public static PointCloudController Instance { get { return _instance; } }

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

        public Mesh instanceMesh;
        public Material instanceMaterial;

        private int instanceCount = -1;
        private ComputeBuffer positionBuffer;
        private ComputeBuffer argsBuffer;
        private ComputeBuffer colorBuffer;

        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        private Vector3 oldCentralCarPos = Vector3.zero;

        void Update()
        {
            if (instanceCount <= 0 || argsBuffer == null)
                return;

            if (oldCentralCarPos == Vector3.zero)
            {
                instanceMaterial.SetVector("centralCarDifference", Vector3.zero);
            }
            else
            {
                instanceMaterial.SetVector("centralCarDifference", (oldCentralCarPos - DataController.Instance.centralCar.transform.position));
            }
            

            // Render
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f)), argsBuffer, 0, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 9);
        }

        public void UpdatePositions(List<Vector3> positionData)
        {
            instanceCount = positionData.Count;
            oldCentralCarPos = Vector3.zero;

            // Positions & Colors
            if (positionBuffer != null) positionBuffer.Release();
            if (colorBuffer != null) colorBuffer.Release();

            positionBuffer = new ComputeBuffer(instanceCount, 16);
            colorBuffer = new ComputeBuffer(instanceCount, 4 * 4);

            Vector4[] positions = new Vector4[instanceCount];
            Vector4[] colors = new Vector4[instanceCount];
            float distanceFromCentre;
            Color pointColour;

            for (int i = 0; i < instanceCount; i++)
            {
                positions[i] = new Vector4(positionData[i].x, positionData[i].y, positionData[i].z, DataController.Instance.pointCloudPointSize);

                distanceFromCentre = (DataController.Instance.centralCar.transform.position - (Vector3)positions[i]).sqrMagnitude;
                pointColour = Color.Lerp(Color.red, Color.yellow, distanceFromCentre / (DataController.Instance.updateDistance * 4f));
                colors[i] = new Vector4(pointColour.r, pointColour.g, pointColour.b, 1f);
            }

            positionBuffer.SetData(positions);
            colorBuffer.SetData(colors);

            instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
            instanceMaterial.SetBuffer("colorBuffer", colorBuffer);

            // indirect args
            uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
            args[0] = numIndices;
            args[1] = (uint)instanceCount;
            argsBuffer.SetData(args);
        }

        public void UpdatePositions(Vector3[] positionData, Vector3 centralCarPos)
        {
            instanceCount = positionData.Length;
            oldCentralCarPos = centralCarPos;

            // Positions & Colors
            if (positionBuffer != null) positionBuffer.Release();
            if (colorBuffer != null) colorBuffer.Release();

            positionBuffer = new ComputeBuffer(instanceCount, 16);
            colorBuffer = new ComputeBuffer(instanceCount, 4 * 4);

            Vector4[] positions = new Vector4[instanceCount];
            Vector4[] colors = new Vector4[instanceCount];
            float distanceFromCentre;
            Color pointColour;

            for (int i = 0; i < instanceCount; i++)
            {
                positions[i] = new Vector4(positionData[i].x, positionData[i].y, positionData[i].z, DataController.Instance.pointCloudPointSize);

                distanceFromCentre = (centralCarPos - (Vector3)positions[i]).sqrMagnitude;
                pointColour = Color.Lerp(Color.red, Color.yellow, distanceFromCentre / (DataController.Instance.updateDistance * 3f));
                colors[i] = new Vector4(pointColour.r, pointColour.g, pointColour.b, 1f);
            }

            positionBuffer.SetData(positions);
            colorBuffer.SetData(colors);

            instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
            instanceMaterial.SetBuffer("colorBuffer", colorBuffer);
            instanceMaterial.SetVector("oldCentralCarPos", centralCarPos);

            // indirect args
            uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
            args[0] = numIndices;
            args[1] = (uint)instanceCount;
            argsBuffer.SetData(args);

            //GC.Collect();
        }

        public void StartRendering()
        {
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        public void StopRendering()
        {
            if (positionBuffer != null) positionBuffer.Release();
            positionBuffer = null;

            if (colorBuffer != null) colorBuffer.Release();
            colorBuffer = null;

            if (argsBuffer != null) argsBuffer.Release();
            argsBuffer = null;

            instanceCount = -1;
        }

        void OnDisable()
        {
            if (positionBuffer != null) positionBuffer.Release();
            positionBuffer = null;

            if (colorBuffer != null) colorBuffer.Release();
            colorBuffer = null;

            if (argsBuffer != null) argsBuffer.Release();
            argsBuffer = null;
        }
    }
}