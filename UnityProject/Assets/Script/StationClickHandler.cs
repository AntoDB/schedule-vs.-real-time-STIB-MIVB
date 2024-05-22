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
    private Dictionary<string, List<Tuple<string, string>>> stopTimesIndex;

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

        // Charger l'index des stop_times
        string stopTimesFilePath = Path.Combine(Application.dataPath, "Imports/stib_data/schedule_25-05-2024/stop_times.txt");
        IndexStopTimes(stopTimesFilePath);
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
        ClearScheduleContainer();
        string[] currentServiceIDs = GetCurrentServiceIDs();

        if (currentServiceIDs == null || currentServiceIDs.Length == 0)
        {
            Debug.Log("No service IDs available for today.");
            return;
        }

        Dictionary<string, List<string>> tripIDsByServiceID = GetTripIDsByServiceID(currentServiceIDs);
        Dictionary<string, Dictionary<int, List<string>>> scheduleByStationAndHour = new Dictionary<string, Dictionary<int, List<string>>>();
        List<string> stationIDs = GetStationIDs(stationName);
        if (stationIDs.Count == 0)
        {
            Debug.LogError("No station IDs found for station: " + stationName);
        }

        // Parcourir les tripIDs
        foreach (var kvp in tripIDsByServiceID)
        {
            Debug.Log("Service ID: " + kvp.Key);
            foreach (string tripID in kvp.Value)
            {
                Debug.Log("Trip ID: " + tripID);
                foreach (string stationID in stationIDs)
                {
                    if (stopTimesIndex.ContainsKey(tripID))
                    {
                        foreach (var tuple in stopTimesIndex[tripID])
                        {
                            if (tuple.Item2 == stationID)
                            {
                                string departureTime = tuple.Item1;
                                Debug.Log("Departure Time: " + departureTime + " for Stop ID: " + stationID);
                                int hour = int.Parse(departureTime.Split(':')[0]);
                                if (hour == 24) { hour = 0; }
                                string minutesSeconds = departureTime.Substring(3); // Récupère les minutes et secondes

                                if (!scheduleByStationAndHour.ContainsKey(stationID))
                                {
                                    scheduleByStationAndHour[stationID] = new Dictionary<int, List<string>>();
                                }

                                if (!scheduleByStationAndHour[stationID].ContainsKey(hour))
                                {
                                    scheduleByStationAndHour[stationID][hour] = new List<string>();
                                }

                                scheduleByStationAndHour[stationID][hour].Add(minutesSeconds);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Trip ID not found in stop times index: " + tripID);
                    }
                }
            }
        }

        // Trier les stationIDs par ordre croissant
        List<string> sortedStationIDs = new List<string>(scheduleByStationAndHour.Keys);
        sortedStationIDs.Sort();

        // Ajouter les horaires au conteneur d'affichage par stationID trié
        foreach (var stationID in sortedStationIDs)
        {
            AddTimetableEntry("StationID " + stationID);
            foreach (var hourKvp in scheduleByStationAndHour[stationID])
            {
                string hourText = hourKvp.Key + "h | " + string.Join(" ", hourKvp.Value);
                AddTimetableEntry(hourText);
            }
        }
    }

    void AddTimetableEntry(string entryText)
    {
        GameObject timetableEntry = Instantiate(timetablePrefab, scheduleContainer);

        Text timetableText = timetableEntry.GetComponent<Text>();
        if (timetableText != null)
        {
            timetableText.text = entryText;
        }
        else
        {
            Debug.LogError("Timetable prefab does not have a Text component.");
        }
    }

    void ClearScheduleContainer()
    {
        foreach (Transform child in scheduleContainer)
        {
            Destroy(child.gameObject);
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

    void IndexStopTimes(string filePath)
    {
        Indexer indexer = new Indexer();
        stopTimesIndex = indexer.IndexStopTimes(filePath);
    }

    public class Indexer
    {
        public Dictionary<string, List<Tuple<string, string>>> IndexStopTimes(string filePath)
        {
            Dictionary<string, List<Tuple<string, string>>> index = new Dictionary<string, List<Tuple<string, string>>>();

            try
            {
                string[] stopTimeLines = File.ReadAllLines(filePath);
                foreach (string line in stopTimeLines)
                {
                    string[] fields = line.Split(',');
                    if (fields.Length >= 4)
                    {
                        string tripID = fields[0];
                        string departureTime = fields[2];
                        string stopID = fields[3];

                        if (!index.ContainsKey(tripID))
                        {
                            index[tripID] = new List<Tuple<string, string>>();
                        }
                        index[tripID].Add(new Tuple<string, string>(departureTime, stopID));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error indexing stop times file: " + e.Message);
            }

            return index;
        }
    }
}
