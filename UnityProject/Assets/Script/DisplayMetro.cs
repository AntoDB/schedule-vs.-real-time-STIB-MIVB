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

    void GetData()
    {
        //myObject = JsonUtility.FromJson<MyClass>(json);
    }

    IEnumerator FetchDataFromSTIB()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        // Ajout de l'en-t�te Authorization
        request.SetRequestHeader("Authorization", "Apikey " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // Convertir la r�ponse JSON en objet JObject
            JObject responseJson = JObject.Parse(request.downloadHandler.text);

            // Extraire les enregistrements
            JArray results = (JArray)responseJson["results"];

            if (results != null)
            {
                // Parcourir tous les r�sultats
                foreach (var result in results)
                {
                    // R�cup�rer les informations sur la ligne (lineid)
                    string lineID = result["lineid"].ToString();

                    // Filtrer les lignes 1, 2, 5 et 6
                    if (lineID == "1" || lineID == "2" || lineID == "5" || lineID == "6")
                    {
                        Debug.Log("Line ID: " + lineID);

                        // R�cup�rer les positions des v�hicules
                        JArray vehiclePositions = JArray.Parse(result["vehiclepositions"].ToString());

                        // Parcourir toutes les positions des v�hicules
                        foreach (var vehiclePosition in vehiclePositions)
                        {
                            Debug.Log("Vehicle Position: " + vehiclePosition.ToString());
                            // Vous pouvez extraire d'autres informations n�cessaires ici
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Aucun r�sultat trouv� dans la r�ponse.");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FetchDataFromSTIB());

        // Instantiate at position (0, 0, 0) and zero rotation.
        Instantiate(rame, new Vector3(9.836f, 1.656f, 0f), Quaternion.identity);
        Instantiate(rame, new Vector3(9.836f, 1.656f, 0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
