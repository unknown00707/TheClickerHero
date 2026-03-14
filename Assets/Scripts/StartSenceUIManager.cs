using UnityEngine;

public class StartSenceUIManager : MonoBehaviour
{
    public GameObject languagePanel;
    public GameObject settingPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        languagePanel.SetActive(false);
        settingPanel.SetActive(false);
    }

    public void OpenLanguagePanel(bool isOpen)
    {
        languagePanel.SetActive(isOpen);
        settingPanel.SetActive(!isOpen);
    }
    public void ExitPanel()
    {
        languagePanel.SetActive(false);
        settingPanel.SetActive(false);
    }

}
