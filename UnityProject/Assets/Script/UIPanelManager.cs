using UnityEngine;
using UnityEngine.UI;

public class UIPanelManager : MonoBehaviour
{
    public GameObject panel;
    public Button closeButton;
    public GameObject[] stations; // Station references (to disable/enable when panel open/close)

    void Start()
    {
        // Disables the panel on start-up
        panel.SetActive(false);

        // So that the button calls the ClosePanel() function when clicked
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    public void OpenPanel()
    {
        Debug.Log("OpenPanel called");
        panel.SetActive(true);    // (Re)activate panels
        SetStationsActive(false); // Deactivate stations
    }

    public void ClosePanel()
    {
        Debug.Log("ClosePanel called");
        panel.SetActive(false);  // Deactivate panel
        SetStationsActive(true); // Reactivate stations
    }

    public void SetStationsActive(bool isActive)
    {
        foreach (GameObject station in stations)
        {
            //Debug.Log("Setting station " + station.name + " active state to " + isActive);
            station.SetActive(isActive);
        }
    }
}
