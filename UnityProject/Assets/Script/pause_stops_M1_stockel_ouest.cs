using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pause_stops_M1_stockel_ouest : MonoBehaviour
{
    private Animator animator;
    private Renderer renderer;
    private GameObject animatedObject;
    private string directionID;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        renderer = GetComponent<Renderer>();
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

    public void AtStop(string s)
    {
        //Debug.Log(gameObject.GetInstanceID());
        Debug.Log(s);
        Debug.Log(directionID);

        if (s == "Kraainem")
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
        }
    }
}
