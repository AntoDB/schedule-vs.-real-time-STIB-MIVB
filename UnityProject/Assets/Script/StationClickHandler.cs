using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// For STIB-MIVB API
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;
using System.IO; // Importation nécessaire pour File

public class StationClickHandler : MonoBehaviour
{
    private string apiUrl = "https://stibmivb.opendatasoft.com/api/explore/v2.1/catalog/datasets/waiting-time-rt-production/records?limit=-1";
    private string apiKey = "8351f946e8d149daf4ed2778963c30b4b9706c7944a1a9118bb023aa";

    public GameObject uiPanel; // Reference to the UI panel to display station info
    public Text stationNameText; // Reference to the Text component for displaying station name
    public Transform scheduleContainer; // Parent container for schedule timetables
    public Transform realtimeContainer; // Parent container for real-time timetables
    public GameObject timetablePrefab; // Prefab for a timetable entry
    public UIPanelManager panelManager;
    private Dictionary<string, StopData> stopDataDict;

    private string stationName;

    void Start()
    {
        stationName = gameObject.name;

        string filePath = Path.Combine(Application.dataPath, "Imports/stib_data/stop-details-production.json");
        stopDataDict = StopDataLoader.LoadStopData(filePath);

        if (stopDataDict.Count == 0)
        {
            Debug.LogError("Stop data dictionary is empty. Check if the JSON file is correctly loaded and parsed.");
        }
    }

    void OnMouseDown()
    {
        DisplayStationInfo();

        Debug.Log("Station " + stationName + " clicked");
        
        panelManager.OpenPanel();
    }

    void DisplayStationInfo()
    {
        stationNameText.text = stationName;
        DisplayScheduleInfo();
    }

    void DisplayScheduleInfo()
    {
        // Example: This should be replaced with actual data fetching
        string[] directions = { "Direction 1", "Direction 2" }; // Example directions
        string[][] schedules = {
            new string[] { "Train A: 5 min", "Train B: 10 min" },
            new string[] { "Train C: 8 min", "Train D: 15 min" }
        };

        foreach (Transform child in scheduleContainer)
        {
            Destroy(child.gameObject); // Clear existing timetable entries
        }

        for (int i = 0; i < directions.Length; i++)
        {
            GameObject timetableEntry = Instantiate(timetablePrefab, scheduleContainer);
            Text entryText = timetableEntry.GetComponent<Text>();
            if (entryText != null)
            {
                entryText.text = $"{directions[i]}:\n" + string.Join("\n", schedules[i]);
            }
            else
            {
                Debug.LogError("Text component not found on timetablePrefab.");
            }
        }
    }
}
