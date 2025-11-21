using UnityEngine;
using UnityEngine.UI;

public class PanelSurveyController : MonoBehaviour
{
    [Header("Sliders")]
    public Slider sliderA;
    public Slider sliderB;
    public Slider sliderC;

    [Header("Buttons")]
    public Button submitButton;
    public Button skipButton;

    [Header("Panels")]
    public GameObject currentPanel;     // The assessment panel (this panel)
    public GameObject mainMenuPanel;    // The panel you want to show next

    [Header("User Data Target")]
    public UserData User;           // A scriptable object or a component holding user values

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitPressed);
        skipButton.onClick.AddListener(OnSkipPressed);
    }

    private void OnSubmitPressed()
    {
        // Save slider values to user data
        User.valueA = sliderA.value;
        User.valueB = sliderB.value;
        User.valueC = sliderC.value;

        // Switch UI panels
        currentPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        Debug.Log("Submitted slider values and moved to main menu.");
    }

    private void OnSkipPressed()
    {
        // No saving, just switch panels
        currentPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        Debug.Log("Skipped and moved to main menu.");
    }
}
