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

    // List for storing vehicle data
    private List<VehicleData> vehicles = new List<VehicleData>();

    // Nested dictionary with mapping from lineId and directionId to type_animation
    private Dictionary<string, Dictionary<string, int>> lineDirectionToAnimation = new Dictionary<string, Dictionary<string, int>>
    {
        { "1", new Dictionary<string, int>
            {
                { "8731", 0 }, // 1 Direction Gare de l'ouest (platform 1)
                { "8733", 0 }, // 1 Direction Gare de l'ouest (platform 2)
                { "8161", 1 }, // 1 Direction Stockel (platform 1)
                { "8162", 1 }, // 1 Direction Stockel (platform 2)

                // ----- Special cases ----- //
                { "8641", 0 }, // 5 Direction Erasme (platform 1)
                { "8642", 0 }, // 5 Direction Erasme (platform 2)
            }
        },
        { "5", new Dictionary<string, int>
            {
                { "8641", 2 }, // 5 Direction Erasme (platform 1)
                { "8642", 2 }, // 5 Direction Erasme (platform 2)
                { "8261", 3 }, // 5 Direction Hermann-Debroux (platform 1)
                { "8262", 3 }, // 5 Direction Hermann-Debroux (platform 2)
                
                // ----- Special cases ----- //
                { "8731", 2 }, // 1 et 5 Direction Gare de l'ouest (platform 1)
                { "8733", 2 }, // 1 et 5 Direction Gare de l'ouest (platform 2)
            }
        },
        { "2", new Dictionary<string, int>
            {
                { "8763", 4 }, // 2 Direction Simonis (platform 1) [Same in the line 6 (this line is included)]
                { "8764", 4 }, // 2 Direction Simonis (platform 2) [Same in the line 6 (this line is included)]
                { "8471", 5 }, // 2 Direction Elisabeth (platform 1) [Same in the line 6 (this line is included)]
                { "8472", 5 }, // 2 Direction Elisabeth (platform 2) [Same in the line 6 (this line is included)]

                // ----- Special cases ----- //
            }
        },
        { "6", new Dictionary<string, int>
            {
                { "8833", 4 }, // 6 Direction Roi Baudouin (platform 1)
                { "8834", 4 }, // 6 Direction Roi Baudouin (platform 2)
                { "8471", 5 }, // 6 Direction Elisabeth (platform 1)
                { "8472", 5 }, // 6 Direction Elisabeth (platform 2)
                
                // ----- Special cases ----- //
                { "8371", 5 }, // 6 Direction Delacroix (platform 1)
            }
        },
    };

    // Mapping from pointId to animation time (in seconds)
    private Dictionary<string, Dictionary<string, float>> clipNameToPointIdToTime = new Dictionary<string, Dictionary<string, float>>
    {
        { "Train_anim_stockel_gareouest", new Dictionary<string, float>
            {
                // 1 Direction Gare de l'ouest
                { "8161", 0f },
                { "8162", 0f }, // Stockel (platform 2)
                { "8151", 2f },
                { "8141", 3f },
                { "8131", 3.8f },
                { "8121", 4.6f },
                { "8111", 5.5f },
                { "8101", 6.4f },
                { "8091", 8f },
                { "8081", 9f },
                { "8071", 10f },
                { "8061", 11f },
                { "8051", 12.25f },
                { "8041", 13.2f },
                { "8031", 13.9f },
                { "8021", 14.6f },
                { "8011", 15.7f },              // STIB API stop-details-production call : DE BROUCKERE (not De Brouckère)
                { "8271", 16.6f },
                { "8281", 17.5f },
                { "8291", 19f },                // STIB API stop-details-production call : JOSEPH.-CHARLOTTE (not Josephine-Charlotte)
                { "8741", 20f },
                { "8731", 21f },
                { "8732", 21f }, // Gare de l'Ouest (platform 2)
                { "8733", 21f }, // Gare de l'Ouest (platform 3)
            }
        },
        { "Train_anim_gareouest_stockel", new Dictionary<string, float>
            {
                // 1 Direction Stockel
                { "8731", 0f }, // Gare de l'Ouest (platform 1)    
                { "8732", 0f },
                { "8733", 0f }, // Gare de l'Ouest (platform 3)
                { "8742", 1f },
                { "8292", 2f },
                { "8282", 3.5f },
                { "8272", 4.4f },
                { "8012", 5.3f },
                { "8022", 6.15f },
                { "8032", 6.8f },
                { "8042", 7.5f },
                { "8052", 8.3f },
                { "8062", 9.2f },
                { "8072", 11f },
                { "8082", 12f },
                { "8092", 13f },                // STIB API stop-details-production call : JOSEPH.-CHARLOTTE (not Josephine-Charlotte)
                { "8102", 14.5f },
                { "8112", 15.4f },              // STIB API stop-details-production call : DE BROUCKERE (not De Brouckère)
                { "8122", 16.3f },
                { "8132", 17.1f },
                { "8142", 18f },
                { "8152", 18.8f },
                { "8161", 21f }, // Stockel (platform 1)
                { "8162", 21f },
                // Erasme -> Stockel
                { "8641", 0f }, // Erasme (platform 1)
                { "8642", 0f },
                { "8652", 0f },
                { "8662", 0f },
                { "8672", 0f },
                { "8682", 0f },
                { "8692", 0f },
                { "8702", 0f },
                { "8712", 0f },
                { "8722", 0f },
            }
        },
        { "Train_anim_hermann-debroux_erasme", new Dictionary<string, float>
            {
                // 5 Direction Erasme
                { "8261", 0f },
                { "8262", 0f }, // Hermann-Debreoux (platform 2)
                { "8251", 1.5f },
                { "8241", 2.25f },
                { "8231", 3.02f },
                { "8221", 3.9f },
                { "8211", 5f },                 // STIB API stop-details-production call : PETILLON (not Pétillon)
                { "8201", 6f },
                { "8071", 7f },
                { "8061", 8f },
                { "8051", 9.5f },
                { "8041", 10.3f },
                { "8031", 11.02f },
                { "8021", 11.8f },
                { "8011", 12.8f },
                { "8271", 13.8f },
                { "8281", 14.7f },
                { "8291", 16f },
                { "8741", 17f },
                { "8731", 18f },
                { "8721", 19f },
                { "8711", 20.3f },
                { "8701", 21.3f },
                { "8691", 22.03f },
                { "8681", 22.8f },
                { "8671", 23.5f },
                { "8661", 24.25f },
                { "8651", 25.3f },
                { "8641", 27f },
                { "8642", 0f }, // Erasme (platform 2)
                // Stockel -> Erasme
                { "8161", 0f },
                { "8162", 0f }, // Stockel (platform 2)
                { "8151", 0f },
                { "8141", 0f },
                { "8131", 0f },
                { "8121", 0f },
                { "8111", 0f },
                { "8101", 0f },
                { "8091", 0f },
                { "8081", 0f },
            }
        },
        { "Train_anim_erasme_hermann-debroux", new Dictionary<string, float>
            {
                // 5 Direction Hermann-Debroux
                { "8641", 0f }, // Erasme (platform 1)
                { "8642", 0f },
                { "8651", 1.8f }, // Eddy Merckx (platform 1)
                { "8652", 1.8f },
                { "8662", 2.8f },
                { "8672", 3.6f },
                { "8682", 4.5f },
                { "8692", 5.1f },
                { "8702", 6f },
                { "8712", 6.8f },
                { "8722", 8f },                 // STIB API stop-details-production call : PETILLON (not Pétillon)
                { "8732", 8.9f },
                { "8742", 10f },
                { "8292", 11f },
                { "8282", 12.5f },
                { "8272", 13.4f },
                { "8012", 14.3f },
                { "8022", 15.2f },
                { "8032", 16f },
                { "8042", 16.5f },
                { "8052", 17.3f },
                { "8062", 18.6f },
                { "8072", 20f },
                { "8202", 21f },
                { "8212", 22f },
                { "8222", 23.2f },
                { "8232", 24f },
                { "8242", 24.6f },
                { "8252", 25.4f },
                { "8261", 27f }, // Hermann-Debreoux (platform 1)
                { "8262", 27f },
            }
        },
        { "Train_anim_elisabeth_roiBaudouin", new Dictionary<string, float>
            {
                // 2 & 6 Direction Simonis or Roi Baudouin
                { "8471", 0f },
                { "8472", 0f }, // Elisabeth (platform 2)
                { "8461", 1.25f },
                { "8441", 2.1f },
                { "8431", 2.8f },
                { "8421", 4f },
                { "8411", 5f },
                { "8401", 6.1f },
                { "8301", 7f },
                { "8311", 8f },
                { "8321", 9.4f },
                { "8331", 10.1f },
                { "8341", 10.9f },
                { "8351", 11.8f },
                { "8361", 12.75f },
                { "8371", 14f },
                { "8381", 15f },
                { "8743", 15.6f },
                { "8753", 17f },
                { "8763", 18f },
                { "8773", 19f },
                { "8783", 20.15f },
                { "8793", 20.9f },
                { "8803", 21.6f },
                { "8813", 22.4f },
                { "8823", 23.15f },
                { "8833", 25f },
                { "8834", 25f }, // Roi Baudouin (platform 2)
            }
        },
        { "Train_anim_roiBaudouin_elisabeth", new Dictionary<string, float>
            {
                // 2 & 6 Direction Elisabeth
                { "8833", 0f }, // Roi Baudouin (platform 1)
                { "8834", 0f },
                { "8824", 2.1f },
                { "8814", 3f },
                { "8804", 3.7f },
                { "8794", 4.5f },
                { "8784", 5.3f },
                { "8774", 6f },
                { "8764", 7f },
                { "8763", 7f }, // Simonis -> Elisabeth
                { "8754", 8f },
                { "8744", 9.3f },
                { "8382", 10f },
                { "8371", 11f }, // Delacroix (platform 1)
                { "8372", 11f },
                { "8362", 12.4f },
                { "8352", 13.3f },
                { "8342", 14.2f },
                { "8332", 14.9f },
                { "8322", 15.6f },
                { "8312", 17f },
                { "8302", 18f },
                { "8402", 18.6f },
                { "8412", 20f },
                { "8422", 21f },
                { "8432", 22.25f },
                { "8442", 23f },
                { "8462", 23.8f },
                { "8471", 25f }, // Elisabeth (platform 1)
                { "8472", 25f },
            }
        },
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

                            // Extract pointId and directionId and convert it to a Vector3 for the position
                            string pointId = vehiclePosition["pointId"].ToString();
                            string directionID = vehiclePosition["directionId"].ToString();

                            // Convert pointId to position (example conversion, replace with actual logic)
                            Vector3 position = PointIdToPosition(pointId);
                            //Debug.Log(position);

                            // Store the vehicle data
                            vehicles.Add(new VehicleData(lineID, directionID, pointId, position));
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

            // Set the directionID on the instantiated vehicle
            pause_stops_M1_stockel_ouest pauseStopsScript = newRame.GetComponent<pause_stops_M1_stockel_ouest>();
            if (pauseStopsScript != null)
            {
                pauseStopsScript.SetDirection(vehicle.directionID);
            }

            // Access the Animator and set the type_animation parameter
            Animator animator = newRame.GetComponent<Animator>();
            if (animator != null)
            {
                int typeAnimation;
                Dictionary<string, int> directionToAnimation;
                if (lineDirectionToAnimation.TryGetValue(vehicle.lineID, out directionToAnimation) &&
                    directionToAnimation.TryGetValue(vehicle.directionID, out typeAnimation))
                {
                    animator.SetInteger("type_animation", typeAnimation);
                }
                else
                {
                    // Handle case where directionID or lineID is not in the dictionary
                    Debug.LogWarning("Unknown directionID or lineID: " + vehicle.directionID + ", " + vehicle.lineID);
                }

                // Add a small delay to ensure the animator is ready
                yield return new WaitForEndOfFrame();

                bool isAnimationSet = false;
                while (!isAnimationSet)
                {
                    AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfos.Length > 0)
                    {
                        string clipName = clipInfos[0].clip.name;
                        Debug.Log("clipInfo " + clipInfos + " ClipName " + clipName);
                        Dictionary<string, float> pointIdToTime;
                        if (clipNameToPointIdToTime.TryGetValue(clipName, out pointIdToTime))
                        {
                            float animationTime;
                            if (pointIdToTime.TryGetValue(vehicle.pointID, out animationTime))
                            {
                                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                                float animationLength = state.length;
                                float normalizedTime = animationTime / animationLength;
                                Debug.Log($"Animation clip name: {clipName}, length: {animationLength}, Animation time: {animationTime}, Normalized time: {normalizedTime}");
                                animator.Play(state.fullPathHash, -1, normalizedTime);
                                isAnimationSet = true;

                                // Transmit the pointID to the pause_stops_M1_stockel_ouest script attached to the vehicle (resumes import)
                                if (pauseStopsScript != null)
                                {
                                    pauseStopsScript.SetPoint(vehicle.pointID);
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Unknown pointID: " + vehicle.pointID);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Unknown clipName: " + clipName);
                        }
                    }
                    yield return null;
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
