﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasPerformanceController : MonoBehaviour
    {
        [Header("Client Label references")]
        public Text clientFpsLabel;
        public Text clientAvgFpsLabel;
        public Text clientMinFpsLabel;
        public Text clientMaxFpsLabel;
        public Text clientMemTotalLabel;
        public Text clientMemAllocLabel;

        [Space(10)]
        [Header("Server Label references")]
        public Text serverFpsLabel;
        public Text serverAvgFpsLabel;
        public Text serverMinFpsLabel;
        public Text serverMaxFpsLabel;
        public Text serverMemTotalLabel;
        public Text serverMemAllocLabel;

        [Space(10)]
        [Header("Other server references")]
        public GameObject labelParent;
        public GameObject naLabel;

        [Space(10)]
        [Header("Options")]

        public float refreshRate = 0.5f;

        private float count = 0f;
        private float totalFps = 0f;

        private float minFPS = 60f;
        private float maxFPS = 60f;

        private PerformanceCounter currentMemCounter;
        private PerformanceCounter ramCounter;

        private float totalMemory;

        void Start()
        {
            currentMemCounter = new PerformanceCounter("Memory", "Available MBytes");
            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            totalMemory = ramCounter.NextValue();

            clientMemTotalLabel.text = "MEM TOTAL: " + totalMemory + " MB";
            clientMemTotalLabel.color = Color.yellow;

            Timing.RunCoroutine(DisplayFPS(), "FPSCounter");

            if (DataController.Instance.applicationType != DataController.ApplicationType.Client)
            {
                labelParent.SetActive(false);
                naLabel.SetActive(true);
            }
        }

        private IEnumerator<float> DisplayFPS()
        {
            while (true)
            {
                float fpsVal = 1.0f / Time.deltaTime;

                CalcFPS(fpsVal);
                CalcAVG(fpsVal);
                CalcMINMAX(fpsVal);
                CalcMemAlloc();

                if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                {
                    DisplayServerInfo();
                }

                yield return Timing.WaitForSeconds(refreshRate);
            }
        }

        private void CalcFPS(float fpsVal)
        {
            clientFpsLabel.text = "FPS: " + Mathf.Floor(fpsVal);

            if (fpsVal > 60f)
            {
                clientFpsLabel.color = Color.green;
            }
            else if (fpsVal < 30f)
            {
                clientFpsLabel.color = Color.red;
            }
            else
            {
                clientFpsLabel.color = Color.yellow;
            }
        }

        private void CalcAVG(float fpsVal)
        {
            totalFps += fpsVal;

            count++;

            float avgFpsVal = totalFps / count;
            clientAvgFpsLabel.text = "AVG: " + Mathf.Floor(avgFpsVal);

            if (avgFpsVal > 60f)
            {
                clientAvgFpsLabel.color = Color.green;
            }
            else if (avgFpsVal < 30f)
            {
                clientAvgFpsLabel.color = Color.red;
            }
            else
            {
                clientAvgFpsLabel.color = Color.yellow;
            }
        }

        private void CalcMINMAX(float fpsVal)
        {
            if (fpsVal < minFPS)
            {
                minFPS = fpsVal;
            }

            if (fpsVal > maxFPS)
            {
                maxFPS = fpsVal;
            }

            if (minFPS > 60f)
            {
                clientMinFpsLabel.color = Color.green;
            }
            else if (minFPS < 30f)
            {
                clientMinFpsLabel.color = Color.red;
            }
            else
            {
                clientMinFpsLabel.color = Color.yellow;
            }

            if (maxFPS > 60f)
            {
                clientMaxFpsLabel.color = Color.green;
            }
            else if (maxFPS < 30f)
            {
                clientMaxFpsLabel.color = Color.red;
            }
            else
            {
                clientMaxFpsLabel.color = Color.yellow;
            }

            clientMinFpsLabel.text = "MIN: " + Mathf.Floor(minFPS);

            clientMaxFpsLabel.text = "MAX: " + Mathf.Floor(maxFPS);
        }

        private void CalcMemAlloc()
        {
            float memoryAlloc = totalMemory - currentMemCounter.NextValue();

            clientMemAllocLabel.text = "MEM ALLOC: " + memoryAlloc + " MB";
            clientMemAllocLabel.color = Color.yellow;
        }

        private void DisplayServerInfo()
        {
            if (DataController.Instance.performanceDictionary.Count > 0)
            {
                serverFpsLabel.text = "FPS: " + DataController.Instance.performanceDictionary[DataController.StatisticType.FPS];

                if (DataController.Instance.performanceDictionary[DataController.StatisticType.FPS] > 60)
                {
                    serverFpsLabel.color = Color.green;
                }
                else if (DataController.Instance.performanceDictionary[DataController.StatisticType.FPS] < 30)
                {
                    serverFpsLabel.color = Color.red;
                }
                else
                {
                    serverFpsLabel.color = Color.yellow;
                }

                serverAvgFpsLabel.text = "AVG: " + DataController.Instance.performanceDictionary[DataController.StatisticType.AVGFPS];

                if (DataController.Instance.performanceDictionary[DataController.StatisticType.AVGFPS] > 60)
                {
                    serverAvgFpsLabel.color = Color.green;
                }
                else if (DataController.Instance.performanceDictionary[DataController.StatisticType.AVGFPS] < 30)
                {
                    serverAvgFpsLabel.color = Color.red;
                }
                else
                {
                    serverAvgFpsLabel.color = Color.yellow;
                }

                serverMinFpsLabel.text = "MIN: " + DataController.Instance.performanceDictionary[DataController.StatisticType.MINFPS];

                if (DataController.Instance.performanceDictionary[DataController.StatisticType.MINFPS] > 60)
                {
                    serverMinFpsLabel.color = Color.green;
                }
                else if (DataController.Instance.performanceDictionary[DataController.StatisticType.MINFPS] < 30)
                {
                    serverMinFpsLabel.color = Color.red;
                }
                else
                {
                    serverMinFpsLabel.color = Color.yellow;
                }

                serverMinFpsLabel.text = "MAX: " + DataController.Instance.performanceDictionary[DataController.StatisticType.MAXFPS];

                if (DataController.Instance.performanceDictionary[DataController.StatisticType.MAXFPS] > 60)
                {
                    serverMaxFpsLabel.color = Color.green;
                }
                else if (DataController.Instance.performanceDictionary[DataController.StatisticType.MAXFPS] < 30)
                {
                    serverMaxFpsLabel.color = Color.red;
                }
                else
                {
                    serverMaxFpsLabel.color = Color.yellow;
                }
            }
        }
    }
}