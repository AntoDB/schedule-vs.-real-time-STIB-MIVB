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
        string[] currentServiceIDs = GetCurrentServiceIDs();

        if (currentServiceIDs == null || currentServiceIDs.Length == 0)
        {
            Debug.Log("No service IDs available for today.");
            return;
        }

        Dictionary<string, List<string>> tripIDsByServiceID = GetTripIDsByServiceID(currentServiceIDs);

        // Affichez les tripIDs
        foreach (var kvp in tripIDsByServiceID)
        {
            Debug.Log("Service ID: " + kvp.Key);
            foreach (string tripID in kvp.Value)
            {
                Debug.Log("Trip ID: " + tripID);
            }
        }

        // Get station IDs from StopData
        var stationIDs = GetStationIDs(stationName);
        if (stationIDs.Count == 0)
        {
            Debug.LogError("No station IDs found for station: " + stationName);
        }
    }

    string[] GetCurrentServiceIDs()
    {
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
        string currentDayOfWeek = DateTime.Now.DayOfWeek.ToString().ToLower(); // Obtient le jour de la semaine actuel

        List<string> serviceIDs = new List<string>();

        try
        {
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
                        // Vérifie si le jour de la semaine actuel correspond à celui dans le calendrier
                        int dayIndex = GetDayIndex(currentDayOfWeek);
                        if (fields[dayIndex] == "1")
                        {
                            serviceIDs.Add(fields[0]);
                        }
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
    List<string> GetStationIDs(string stationName)
    {
        List<string> stationIDs = new List<string>();

        foreach (var stop in stopDataDict.Values)
        {
            if (stop.name.fr == stationName && stop.id.StartsWith("8"))
            {
                stationIDs.Add(stop.id);
            }
        }

        Debug.Log($"Station IDs for {stationName}: {string.Join(", ", stationIDs)}");
        return stationIDs;
    }

    int GetDayIndex(string dayOfWeek)
    {
        switch (dayOfWeek)
        {
            case "monday":
                return 1;
            case "tuesday":
                return 2;
            case "wednesday":
                return 3;
            case "thursday":
                return 4;
            case "friday":
                return 5;
            case "saturday":
                return 6;
            case "sunday":
                return 7;
            default:
                return -1; // Retourne -1 si le jour de la semaine n'est pas reconnu
        }
    }

    Dictionary<string, List<string>> GetTripIDsByServiceID(string[] serviceIDs)
    {
        Dictionary<string, List<string>> tripIDsByServiceID = new Dictionary<string, List<string>>();

        try
        {
            string filePath = Path.Combine(Application.dataPath, "Imports/stib_data/schedule_25-05-2024/trips.txt");
            string[] tripLines = File.ReadAllLines(filePath);
            foreach (string line in tripLines)
            {
                string[] fields = line.Split(',');
                if (fields.Length >= 3)
                {
                    string currentServiceID = fields[1];
                    string tripID = fields[2];

                    if (Array.Exists(serviceIDs, id => id == currentServiceID))
                    {
                        if (!tripIDsByServiceID.ContainsKey(currentServiceID))
                        {
                            tripIDsByServiceID[currentServiceID] = new List<string>();
                        }
                        tripIDsByServiceID[currentServiceID].Add(tripID);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading trips file: " + e.Message);
        }

        return tripIDsByServiceID;
    }
}
