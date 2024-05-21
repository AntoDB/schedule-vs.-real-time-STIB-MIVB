using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationClickHandler : MonoBehaviour
{
    public GameObject uiPanel; // Reference to the UI panel to display station info
    public Text stationNameText; // Reference to the Text component for displaying station name
    public Text scheduleTimetableText; // Reference to the Text component for displaying the schedule timetable information
    public Text realtimeTimetableText; // Reference to the Text component for displaying the real time timetable information
    public UIPanelManager panelManager;

    private string stationName;

    void Start()
    {
        stationName = gameObject.name;
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
        panelManager.SetStationsActive(false);
    }

    void DisplayStationInfo()
    {
        // Fetch and display the station information
        stationNameText.text = stationName;

        // Example: Fetch timetable information (this should be replaced with actual data fetching)
        string scheduleTimetableInfo = "Next trains:\n- Train A: 5 min\n- Train B: 10 min";
        scheduleTimetableText.text = scheduleTimetableInfo;

        // Example: Fetch timetable information (this should be replaced with actual data fetching)
        string realtimeTimetableInfo = "Next trains:\n- Train A: 5 min\n- Train B: 10 min";
        realtimeTimetableText.text = realtimeTimetableInfo;

        // Show the UI panel
        uiPanel.SetActive(true);
    }
}
