using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pause_stops_M1_stockel_ouest : MonoBehaviour
{
    private Animator animator;
    private Renderer renderer;
    private GameObject animatedObject;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AtStop(string s)
    {
        //Debug.Log(gameObject.GetInstanceID());
        Debug.Log(s);

        if (s == "Kraainem")
        {
            // Arrêtez l'animation
            if (animator != null)
                animator.enabled = false;

            // Changez la couleur de l'objet en rouge
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