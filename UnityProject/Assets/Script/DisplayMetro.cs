using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DisplayMetro : MonoBehaviour
{
    public GameObject rame;

    void GetData()
    {
        //myObject = JsonUtility.FromJson<MyClass>(json);
    }


    // Start is called before the first frame update
    void Start()
    {
        // Instantiate at position (0, 0, 0) and zero rotation.
        Instantiate(rame, new Vector3(9.836f, 1.656f, 0f), Quaternion.identity);
        Instantiate(rame, new Vector3(9.836f, 1.656f, 0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
