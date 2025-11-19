using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginSceneManager : MonoBehaviour
{
    [Header("TMP Input Fields")]
    public TMP_InputField usernameField;     // Patient ID field
    public TMP_InputField passwordField;

    [Header("UI Controls")]
    public Toggle rememberMeToggle;
    public Button loginButton;
    public Button guestButton;

    [Header("Optional Feedback")]
    public TMP_Text messageText; // Optional label for warnings

    [Header("Scene Settings")]
    public string nextScene = "AssessmentRoom";

    // --- Demo User (Sarah Johnson) ---
    private string demoPatientID = "RA-PT-102945";
    private string demoPassword = "DemoPass!23";
    private string demoName = "Sarah Johnson";

    private void Start()
    {
        // Restore saved login data
        if (PlayerPrefs.GetInt("rememberMe", 0) == 1)
        {
            rememberMeToggle.isOn = true;
            usernameField.text = PlayerPrefs.GetString("savedUsername", "");
            passwordField.text = PlayerPrefs.GetString("savedPassword", "");
        }
        else
        {
            rememberMeToggle.isOn = false;
        }

        // Button listeners
        loginButton.onClick.AddListener(OnLoginPressed);
        guestButton.onClick.AddListener(OnGuestPressed);

        if (messageText != null)
            messageText.text = "";
    }

    public void OnLoginPressed()
    {
        string user = usernameField.text.Trim();
        string pass = passwordField.text.Trim();

        if (user == "" || pass == "")
        {
            ShowMessage("Please enter Patient ID and Password.");
            return;
        }

        // --- Check Demo User ---
        if (user == demoPatientID && pass == demoPassword)
        {
            ShowMessage("Login Successful!");

            // Save login info
            if (rememberMeToggle.isOn)
            {
                PlayerPrefs.SetInt("rememberMe", 1);
                PlayerPrefs.SetString("savedUsername", user);
                PlayerPrefs.SetString("savedPassword", pass);
            }
            else
            {
                PlayerPrefs.SetInt("rememberMe", 0);
                PlayerPrefs.DeleteKey("savedUsername");
                PlayerPrefs.DeleteKey("savedPassword");
            }

            PlayerPrefs.SetString("currentUserName", demoName);
            PlayerPrefs.SetString("currentUserID", demoPatientID);

            SceneManager.LoadScene(nextScene);
            return;
        }

        // --- Invalid Login ---
        ShowMessage("Invalid Patient ID or Password.");
    }

    public void OnGuestPressed()
    {
        ShowMessage("Guest Access Selected.");

        // Clear saved login
        PlayerPrefs.SetInt("rememberMe", 0);
        PlayerPrefs.DeleteKey("savedUsername");
        PlayerPrefs.DeleteKey("savedPassword");

        PlayerPrefs.SetString("currentUserName", "Guest");
        PlayerPrefs.SetString("currentUserID", "GUEST");

        SceneManager.LoadScene(nextScene);
    }

    private void ShowMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;

        Debug.Log(msg);
    }
}
