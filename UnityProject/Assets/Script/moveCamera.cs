using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class moveCamera : MonoBehaviour
{
    // Variable en public pour pouvoir changer sur la partie visuelle
    public float zoomSpeed = 0.01f;
    public float moveSpeed = 5f;
    public float mousepadSpeed = 150;

    void SizeCamera(float zoomInput)
    {
        // Récupérer la taille actuelle de la caméra
        float currentSize = Camera.main.orthographicSize;

        // Calculer la nouvelle taille en fonction de l'entrée de zoom
        float newSize = Mathf.Clamp(currentSize + zoomInput * zoomSpeed, 2f, 6f);

        // Affecter la nouvelle taille à la caméra
        Camera.main.orthographicSize = newSize;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Gérer le zoom avec la molette de la souris
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        SizeCamera(-zoomInput * mousepadSpeed);

        // Gérer le zoom avec les touches "+" et "-"
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
        {
            SizeCamera(-1);
        }
        else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            SizeCamera(1);
        }

        // Gérer les déplacements
        float horizontalInput = Input.GetAxis("Horizontal");
        //float verticalInput = Input.GetAxis("Vertical");
        float verticalInput = 0f;

        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
}
