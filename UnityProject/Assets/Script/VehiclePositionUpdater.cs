using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;

public class VehiclePositionUpdater : MonoBehaviour
{
    private string apiUrl = "https://stibmivb.opendatasoft.com/api/explore/v2.1/catalog/datasets/vehicle-position-rt-production/records?limit=-1";
    private string apiKey = "8351f946e8d149daf4ed2778963c30b4b9706c7944a1a9118bb023aa";
    public float updateInterval = 15f;

    private class VehicleData
    {
        public string lineID;
        public string directionID;
        public string pointID;
        public Vector3 position;

        public VehicleData(string lineID, string directionID, string pointID, Vector3 position)
        {
            this.lineID = lineID;
            this.directionID = directionID;
            this.pointID = pointID;
            this.position = position;
        }
    }

    private List<VehicleData> vehicles = new List<VehicleData>();

    void Start()
    {
        StartCoroutine(UpdateVehiclePositions());
    }

    IEnumerator UpdateVehiclePositions()
    {
        while (true)
        {
            yield return StartCoroutine(FetchVehiclePositions());
            yield return new WaitForSeconds(updateInterval);
        }
    }

    IEnumerator FetchVehiclePositions()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Apikey " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            yield break;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                JObject responseJson = JObject.Parse(request.downloadHandler.text);
                JArray results = (JArray)responseJson["records"];

                if (results != null)
                {
                    vehicles.Clear(); // Clear the previous data before adding new data
                    foreach (var result in results)
                    {
                        string lineID = result["fields"]?["lineid"]?.ToString();
                        if (lineID == null) continue; // Skip if lineID is null

                        if (lineID == "1" || lineID == "2" || lineID == "5" || lineID == "6")
                        {
                            var vehiclePositionsToken = result["fields"]?["vehiclepositions"];
                            if (vehiclePositionsToken == null) continue; // Skip if vehiclePositions is null

                            // Check if vehiclePositions is an array
                            if (vehiclePositionsToken.Type == JTokenType.Array)
                            {
                                JArray vehiclePositions = vehiclePositionsToken.ToObject<JArray>();
                                foreach (var vehiclePosition in vehiclePositions)
                                {
                                    string pointId = vehiclePosition["pointId"]?.ToString();
                                    string directionID = vehiclePosition["directionId"]?.ToString();
                                    if (pointId == null || directionID == null) continue; // Skip if pointId or directionID is null

                                    Vector3 position = PointIdToPosition(pointId);
                                    vehicles.Add(new VehicleData(lineID, directionID, pointId, position));
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Unexpected vehiclepositions type: " + vehiclePositionsToken.Type);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No records found in the response.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing JSON: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Unexpected error: " + request.result);
        }
    }

    Vector3 PointIdToPosition(string pointId)
    {
        // Example conversion logic
        return new Vector3(float.Parse(pointId), 0, 0);
    }
}
