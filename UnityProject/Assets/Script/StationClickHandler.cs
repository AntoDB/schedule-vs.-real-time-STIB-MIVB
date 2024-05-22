using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        string[] currentServiceIDs = GetCurrentServiceIDs();

        if (currentServiceIDs == null || currentServiceIDs.Length == 0)
        {
            Debug.Log("No service IDs available for today.");
            return;
        }

        Debug.Log("Service IDs for today:");
        foreach (string serviceID in currentServiceIDs)
        {
            Debug.Log(serviceID);
        }
    }

    string[] GetCurrentServiceIDs()
    {
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
        List<string> serviceIDs = new List<string>();

        try
        {
            Debug.Log("TEST");
            string filePath = Path.Combine(Application.dataPath, "Imports/stib_data/schedule_25-05-2024/calendar.txt");
            string[] calendarLines = File.ReadAllLines(filePath);
            foreach (string line in calendarLines)
            {
                string[] fields = line.Split(',');
                if (fields.Length >= 10)
                {
                    string startDate = fields[8];
                    string endDate = fields[9];

                    if (currentDate.CompareTo(startDate) >= 0 && currentDate.CompareTo(endDate) <= 0)
                    {
                        serviceIDs.Add(fields[0]);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading calendar file: " + e.Message);
        }

        return serviceIDs.ToArray();
    }
}
