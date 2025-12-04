/*
 * LoginSceneManager.cs
 * --------------------
 * Login manager for patients or guests.
 * - Username and password fields are handled via TMP_InputFields.
 * - Password input is masked (characters hidden when typed).
 * - Supports "Remember Me" toggle using PlayerPrefs.
 * - Validates credentials (Sarah Johnson).
 * - Loads the next scene on successful login or guest access.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginSceneManager : MonoBehaviour
{
    [Header("TMP Input Fields")]
    public TMP_InputField usernameField;     // Patient ID field
    public TMP_InputField passwordField;     // Password field (masked)

    [Header("UI Controls")]
    public Toggle rememberMeToggle;
    public Button loginButton;
    public Button guestButton;

    [Header("Optional Feedback")]
    public TMP_Text messageText; // Label for warnings or status messages

    [Header("Scene Settings")]
    public string nextScene = "LobbyScene";

    // --- Demo User (Sarah Johnson) ---
    public string demoPatientID = "RA-PT-102945";
    public string demoPassword = "DemoPass!23";
    public string demoName = "Sarah Johnson";

    private void Start()
    {
        // Pre-fill demo credentials for testing
        usernameField.text = demoPatientID;
        passwordField.text = demoPassword;

        // Ensure password input is masked
        if (passwordField != null)
            passwordField.contentType = TMP_InputField.ContentType.Password;

        // Restore saved login data if "Remember Me" was checked
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

        // Attach button listeners
        loginButton.onClick.AddListener(OnLoginPressed);
        guestButton.onClick.AddListener(OnGuestPressed);

        if (messageText != null)
            messageText.text = "";
    }

    public void OnLoginPressed()
    {
        string user = usernameField.text.Trim();
        string pass = passwordField.text.Trim();

        // Basic validation
        if (user == "" || pass == "")
        {
            ShowMessage("Please enter Patient ID and Password.");
            return;
        }

        // --- Check Demo User ---
        if (user == demoPatientID && pass == demoPassword)
        {
            ShowMessage("Login Successful!");

            // Save login info if "Remember Me" is enabled
            if (rememberMeToggle.isOn)
            {
                PlayerPrefs.SetInt("rememberMe", 1);
                PlayerPrefs.SetString("savedUsername", user);
                PlayerPrefs.SetString("savedPassword", pass); // plain text is fine for demo
            }
            else
            {
                PlayerPrefs.SetInt("rememberMe", 0);
                PlayerPrefs.DeleteKey("savedUsername");
                PlayerPrefs.DeleteKey("savedPassword");
            }

            // Store current user info for session
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