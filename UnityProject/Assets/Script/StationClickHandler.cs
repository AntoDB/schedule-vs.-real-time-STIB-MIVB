using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// For STIB-MIVB API
using UnityEngine.Networking;
using System.Linq;
using System;
using System.IO; // Importation nécessaire pour File

public class StationClickHandler : MonoBehaviour
{
    public GameObject uiPanel; // Reference to the UI panel to display station info
    public Text stationNameText; // Reference to the Text component for displaying station name
    //public Text scheduleTimetableText; // Reference to the Text component for displaying the schedule timetable information
    //public Text realtimeTimetableText; // Reference to the Text component for displaying the real time timetable information
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

    // Update is called once per frame
    void Update()
    {

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
        StartCoroutine(DisplayRealtimeInfo());
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

    IEnumerator DisplayRealtimeInfo()
    {
        // Get station IDs from StopData
        var stationIDs = GetStationIDs(stationName);
        if (stationIDs.Count == 0)
        {
            Debug.LogError("No station IDs found for station: " + stationName);
            yield break;
        }

        // Fetch real-time data
        string apiUrl = "https://stibmivb.opendatasoft.com/api/explore/v2.1/catalog/datasets/waiting-time-rt-production/records?limit=-1";
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "8351f946e8d149daf4ed2778963c30b4b9706c7944a1a9118bb023aa");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching realtime data: " + request.error);
            yield break;
        }

        Debug.Log("Parsing real-time data..."); // to debug

        // Parse the response
        var jsonResponse = request.downloadHandler.text;
        Debug.Log("Raw API response: " + jsonResponse); // to debug
        RealtimeData data = JsonUtility.FromJson<RealtimeData>(jsonResponse);


        // === TO DEBUG
        if (data == null || data.results == null)
        {
            Debug.LogError("Failed to parse real-time data.");
            yield break;
        }

        Debug.Log($"Parsed {data.results.Length} results from the API response.");
        // === END TO DEBUG

        // Filter results by station IDs
        var stationData = data.results.Where(r => stationIDs.Contains(r.pointid)).ToList();

        if (stationData.Count == 0)
        {
            Debug.LogError("No real-time data found for station: " + stationName);
            yield break;
        }

        Debug.Log($"Found real-time data for station: {stationName}"); // to debug

        foreach (Transform child in realtimeContainer)
        {
            Destroy(child.gameObject); // Clear existing timetable entries
        }

        foreach (var station in stationData)
        {
            Debug.Log($"Processing station: {station.pointid} with {station.GetPassingTimes().Length} passing times."); // to debug
            foreach (var passingTime in station.GetPassingTimes())
            {
                Debug.Log($"Processing passing time: {passingTime.destination.fr}, arrival: {passingTime.expectedArrivalTime}"); // to debug
                GameObject timetableEntry = Instantiate(timetablePrefab, realtimeContainer);
                Text entryText = timetableEntry.GetComponent<Text>();
                if (entryText != null)
                {
                    entryText.text = $"{passingTime.destination.fr}: {passingTime.expectedArrivalTime}";
                    Debug.Log($"Added real-time entry: {entryText.text}"); // to debug
                }
                else
                {
                    Debug.LogError("Text component not found on timetablePrefab.");
                }
            }
        }
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

    [Serializable]
    public class RealtimeData
    {
        public int total_count;
        public Result[] results;

        [Serializable]
        public class Result
        {
            public string pointid;
            public string lineid;
            public string passingtimes;

            public PassingTime[] GetPassingTimes()
            {
                return JsonHelper.FromJson<PassingTime>(passingtimes);
            }

            [Serializable]
            public class PassingTime
            {
                public Destination destination;
                public string expectedArrivalTime;
                public string lineId;
            }

            [Serializable]
            public class Destination
            {
                public string fr;
                public string nl;
            }
        }
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
