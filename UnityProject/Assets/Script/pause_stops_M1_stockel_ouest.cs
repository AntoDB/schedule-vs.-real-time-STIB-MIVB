using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // Importation nécessaire pour File
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class StopData
{
    public string gpscoordinates { get; set; }
    public string id { get; set; }
    public Name name { get; set; }

    public class Name
    {
        public string fr { get; set; }
        public string nl { get; set; }
    }

    public class StopDataJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            var stopData = new StopData
            {
                gpscoordinates = jsonObject["gpscoordinates"].ToString(),
                id = jsonObject["id"].ToString(),
                name = JsonConvert.DeserializeObject<Name>(jsonObject["name"].ToString())
            };

            return stopData;
        }

        public override bool CanConvert(System.Type objectType)
        {
            return objectType == typeof(StopData);
        }
    }
}

public static class StopDataLoader
{
    public static Dictionary<string, StopData> LoadStopData(string filePath)
    {
        Dictionary<string, StopData> stopDataDict = new Dictionary<string, StopData>();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            List<StopData> stopDataList = JsonConvert.DeserializeObject<List<StopData>>(json, new StopData.StopDataJsonConverter());
            foreach (var stopData in stopDataList)
            {
                stopDataDict[stopData.id] = stopData;
            }
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
        }
        return stopDataDict;
    }
}

public class pause_stops_M1_stockel_ouest : MonoBehaviour
{
    private Animator animator;
    private Renderer renderer;
    private GameObject animatedObject;
    private string directionID;
    private string pointID;
    private Dictionary<string, StopData> stopDataDict;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        renderer = GetComponent<Renderer>();

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
        // You can use the directionID in your update logic if needed
    }

    // Method to set the directionID
    public void SetDirection(string direction)
    {
        this.directionID = direction;
    }

    // Method to set the directionID
    public void SetPoint(string point)
    {
        this.pointID = point;
    }

    public void AtStop(string s)
    {
        //Debug.Log(gameObject.GetInstanceID());
        Debug.Log(s);
        Debug.Log(directionID);
        Debug.Log("Point ID " + pointID);

        if (string.IsNullOrEmpty(pointID))
        {
            Debug.LogError("Point ID is not set.");
            return;
        }

        // Stop the animation at each station (not terminal station)
        if (stopDataDict != null && stopDataDict.TryGetValue(pointID, out StopData stopData))
        {
            string stationName = stopData.name.fr.ToUpper();
            s = s.ToUpper();

            if (s == stationName)
            {
                if (animator != null)
                    animator.enabled = false;
            }
            else
            {
                // Debug all metros that must be stopped but continue due to bad spawn position
                if (renderer != null && animator.enabled)
                {
                    stopDataDict.TryGetValue(directionID, out StopData destinationData);
                    string destinationName = destinationData.name.fr.ToUpper();
                    Debug.LogWarning("Direction ID " + directionID + " " + destinationName + "    Point ID " + pointID + " " + stationName + "     Current station CATCH " + s);
                    renderer.material.color = Color.green;
                    animator.enabled = false;
                }
            }
        }
        else
        {
            Debug.LogError($"Stop data not found for pointID: {pointID}");
        }

        /*if (s == "Kraainem")
        {
            // Stop the animation
            if (animator != null)
                animator.enabled = false;

            // Change the color of the object to red
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
            else
            {
                Debug.LogError("L'objet ne possède pas de composant Renderer !");
            }
        }*/

        // If instance at the destination -> wait & destroy
        if ((directionID == "8731" && s == "Gare de l'Ouest") || (directionID == "8162" && s == "Stockel") || ((directionID == "8641" || directionID == "8642") && s == "Erasme") || (directionID == "8262" && s == "Hermann-Debroux") || (directionID == "8763" && s == "Simonis") || ((directionID == "8833" || directionID == "8834") && s == "Roi Baudouin") || (directionID == "8472" && s == "Elisabeth"))
        {
            // Stop the animation
            if (animator != null)
                animator.enabled = false;

            // Start the coroutine to destroy the instance after a delay
            StartCoroutine(DestroyAfterDelay(2f));
        }
    }

    // Coroutine to destroy the object after a delay
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Destroy instance
        Destroy(gameObject);
    }
}
