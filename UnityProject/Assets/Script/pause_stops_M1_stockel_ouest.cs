using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pause_stops_M1_stockel_ouest : MonoBehaviour
{
    private Animator animator;
    private GameObject animatedObject;

    // Start is called before the first frame update
    void Start()
    {
        // Assurez-vous que l'objet animé possède un Animator
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("L'objet ne possède pas de composant Animator !");
            return;
        }

        // Obtenez la référence de l'objet animé
        animatedObject = animator.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AtStop(string s)
    {
        //Debug.Log(gameObject.GetInstanceID());
        Debug.Log(s);

        // Arrêtez l'animation
        animator.enabled = false;

        // Changez la couleur de l'objet en rouge
        Renderer renderer = animatedObject.GetComponent<Renderer>();
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