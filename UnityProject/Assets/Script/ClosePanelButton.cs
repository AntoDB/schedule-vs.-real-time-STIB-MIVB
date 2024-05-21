using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosePanelButton : MonoBehaviour
{
    public GameObject uiPanel;

    public void ClosePanel()
    {
        uiPanel.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
