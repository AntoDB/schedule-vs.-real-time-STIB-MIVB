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
        // R�cup�rer la taille actuelle de la cam�ra
        float currentSize = Camera.main.orthographicSize;

        // Calculer la nouvelle taille en fonction de l'entr�e de zoom
        float newSize = Mathf.Clamp(currentSize + zoomInput * zoomSpeed, 2f, 6f);

        // Affecter la nouvelle taille � la cam�ra
        Camera.main.orthographicSize = newSize;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // G�rer le zoom avec la molette de la souris
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        SizeCamera(-zoomInput * mousepadSpeed);

        // G�rer le zoom avec les touches "+" et "-"
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
        {
            SizeCamera(-1);
        }
        else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            SizeCamera(1);
        }

        // G�rer les d�placements
        float horizontalInput = Input.GetAxis("Horizontal");
        //float verticalInput = Input.GetAxis("Vertical");
        float verticalInput = 0f;

        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
}
