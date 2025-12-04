/*
 * TrainingUIManager.cs
 * --------------------
 * Manages navigation between different training modules in the app.
 * Controls which UI canvases are visible (Start, Menu, Guided Joint, Typing Ergonomics,
 * Guided Stretch) and handles transitions such as opening modules, returning to the menu,
 * loading a separate scene, or exiting the application.
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingUIManager : MonoBehaviour
{
    [Header("Menu & Panels")]
    public GameObject startCanvas;             // Initial start screen
    public GameObject menuCanvas;              // Main menu
    public GameObject guidedJointCanvas;       // Guided Joint Exercises module
    public GameObject typingErgonomicsCanvas;  // Typing Ergonomics Simulation module
    public GameObject guidedStretchCanvas;     // Guided Stretch Routine module

    void Start()
    {
        // Show start canvas first, hide all others
        startCanvas.SetActive(true);
        menuCanvas.SetActive(false);
        guidedJointCanvas.SetActive(false);
        typingErgonomicsCanvas.SetActive(false);
        guidedStretchCanvas.SetActive(false);
    }

    // --- Start button ---
    public void ShowMenu()
    {
        // Transition from start screen to main menu
        startCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }

    public void HideMenu()
    {
        menuCanvas.SetActive(false);
    }

    // --- Main menu buttons ---
    public void OpenGuidedJointExercises()
    {
        HideMenu();
        guidedJointCanvas.SetActive(true);
    }

    public void OpenTypingErgonomics()
    {
        HideMenu();
        typingErgonomicsCanvas.SetActive(true);
    }

    public void OpenGuidedStretchRoutine()
    {
        HideMenu();
        guidedStretchCanvas.SetActive(true);
    }

    public void OpenDailyActivitySimulation()
    {
        // Loads a separate training scene (not just a canvas swap)
        SceneManager.LoadScene("dastrainingScene");
    }

    // --- Back buttons inside each panel ---
    public void BackToMenuFromPanel()
    {
        // Hide all module canvases and return to main menu
        guidedJointCanvas.SetActive(false);
        typingErgonomicsCanvas.SetActive(false);
        guidedStretchCanvas.SetActive(false);
        ShowMenu();
    }

    // --- Exit button ---
    public void ExitApplication()
    {
        // Quits the application (works in build, ignored in editor)
        Application.Quit();
    }
}