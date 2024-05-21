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
