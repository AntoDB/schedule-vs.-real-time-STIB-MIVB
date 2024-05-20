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
