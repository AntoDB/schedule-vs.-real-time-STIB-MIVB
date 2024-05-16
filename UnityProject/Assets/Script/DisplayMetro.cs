using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

public class DisplayMetro : MonoBehaviour
{
    private string apiUrl = "https://stibmivb.opendatasoft.com/api/explore/v2.1/catalog/datasets/vehicle-position-rt-production/records?limit=-1";
    private string apiKey = "8351f946e8d149daf4ed2778963c30b4b9706c7944a1a9118bb023aa";

    public GameObject rame;

    // Class for storing vehicle data
    private class VehicleData
    {
        public string lineID;
        public string directionID;
        public Vector3 position;

        public VehicleData(string lineID, string directionID, Vector3 position)
        {
            this.lineID = lineID;
            this.directionID = directionID;
            this.position = position;
        }
    }

    // List for storing vehicle data
    private List<VehicleData> vehicles = new List<VehicleData>();

    // Mapping from directionId to type_animation
    private Dictionary<string, int> directionToAnimation = new Dictionary<string, int>
    {
        { "8731", 0 }, // 1 Direction Gare de l'ouest (platform 1)
        { "8733", 0 }, // 1 Direction Gare de l'ouest (platform 2)
        { "8161", 1 }, // 1 Direction Stockel (platform 1)
        { "8162", 1 }, // 1 Direction Stockel (platform 2)
        { "8641", 2 }, // 5 Direction Erasme (platform 1)
        { "8642", 2 }, // 5 Direction Erasme (platform 2)
        { "8261", 3 }, // 5 Direction Hermann-Debroux (platform 1)
        { "8262", 3 }, // 5 Direction Hermann-Debroux (platform 2)
        { "8763", 4 }, // 2 Direction Simonis (platform 1) [Include in the line 6]
        { "8764", 4 }, // 2 Direction Simonis (platform 2) [Include in the line 6]
        { "8471", 5 }, // 2 Direction Elisabeth (platform 1) [Include in the line 6]
        { "8472", 5 }, // 2 Direction Elisabeth (platform 2) [Include in the line 6]
        { "8833", 4 }, // 6 Direction Roi Baudouin (platform 1)
        { "8834", 4 }, // 6 Direction Roi Baudouin (platform 2)
        //{ "8471", 5 }, // 6 Direction Elisabeth (platform 1)
        //{ "8472", 5 }, // 6 Direction Elisabeth (platform 2)
    };

    // Coroutine to fetch data from STIB API
    IEnumerator FetchDataFromSTIB()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        // Add Authorization header
        request.SetRequestHeader("Authorization", "Apikey " + apiKey);

        // Wait for the response
        yield return request.SendWebRequest();

        // Handle potential errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // Parse the JSON response
            JObject responseJson = JObject.Parse(request.downloadHandler.text);

            // Extract records/results
            JArray results = (JArray)responseJson["results"];

            if (results != null)
            {
                // Iterate through each record
                foreach (var result in results)
                {
                    // Retrieve information about the line
                    // Get line ID
                    string lineID = result["lineid"].ToString();

                    // Filter lines 1, 2, 5, and 6
                    if (lineID == "1" || lineID == "2" || lineID == "5" || lineID == "6")
                    {
                        Debug.Log("Line ID: " + lineID);

                        // Get vehicle positions
                        JArray vehiclePositions = JArray.Parse(result["vehiclepositions"].ToString());

                        // Iterate through each vehicle position
                        foreach (var vehiclePosition in vehiclePositions)
                        {
                            Debug.Log("Vehicle Position: " + vehiclePosition.ToString());

                            // // Extract pointId and directionId and convert it to a Vector3 for the position
                            string pointId = vehiclePosition["pointId"].ToString();
                            string directionID = vehiclePosition["directionId"].ToString();

                            // Convert pointId to position (example conversion, replace with actual logic)
                            Vector3 position = PointIdToPosition(pointId);
                            //Debug.Log(position);

                            // Store the vehicle data
                            vehicles.Add(new VehicleData(lineID, directionID, position));
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("No records found in the response.");
            }
        }
    }

    // Function to convert pointId to Vector3 (replace with actual conversion logic)
    Vector3 PointIdToPosition(string pointId)
    {
        // This is a dummy conversion, replace it with actual logic
        // For example, use a dictionary to map pointId to coordinates
        float parsedValue;
        if (float.TryParse(pointId, out parsedValue))
        {
            return new Vector3(parsedValue, 0, 0);
        }
        else
        {
            // Handle parsing error, return a default or error position
            return Vector3.zero;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FetchDataAndInstantiateVehicles());
    }

    // Coroutine to fetch data and instantiate vehicles
    IEnumerator FetchDataAndInstantiateVehicles()
    {
        // Fetch the data
        yield return StartCoroutine(FetchDataFromSTIB());

        // Instantiate all vehicles at their positions
        foreach (var vehicle in vehicles)
        {
            GameObject newRame = Instantiate(rame, vehicle.position, Quaternion.identity);

            // Access the Animator and set the type_animation parameter
            Animator animator = newRame.GetComponent<Animator>();
            if (animator != null)
            {
                int typeAnimation;
                if (directionToAnimation.TryGetValue(vehicle.directionID, out typeAnimation))
                {
                    animator.SetInteger("type_animation", typeAnimation);
                }
                else
                {
                    // Handle case where directionID is not in the dictionary
                    Debug.LogWarning("Unknown directionID: " + vehicle.directionID);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update logic if needed
    }
}
