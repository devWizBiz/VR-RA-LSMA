/*
 * AssessmentUIManager.cs
 * ----------------------
 * Manages navigation between different assessment modules (ROM, Movement, Grip Strength).
 * Controls which canvases are visible at any given time, starting with the intro screen,
 * then switching to the menu or specific assessment panels. Provides back navigation
 * to return to the main menu.
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AssessmentUIManager : MonoBehaviour
{
    [Header("Menu & Panels")]
    public GameObject startCanvas;     // Initial start screen
    public GameObject menuCanvas;      // Main menu
    public GameObject romCanvas;       // Range of Motion assessment
    public GameObject movementCanvas;  // Movement assessment
    public GameObject gripCanvas;      // Grip Strength assessment

    void Start()
    {
        // Show start screen first, hide all other canvases
        startCanvas.SetActive(true);   
        menuCanvas.SetActive(false);
        romCanvas.SetActive(false);
        movementCanvas.SetActive(false);
        gripCanvas.SetActive(false);
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
    public void OpenROM()
    {
        HideMenu();
        romCanvas.SetActive(true);
    }

    public void OpenMovement()
    {
        HideMenu();
        movementCanvas.SetActive(true);
    }

    public void OpenGripStrength()
    {
        HideMenu();
        gripCanvas.SetActive(true);
    }

    // --- Back button inside assessment panels ---
    public void BackToMenuFromPanel()
    {
        // Hide all assessment canvases and return to main menu
        romCanvas.SetActive(false);
        movementCanvas.SetActive(false);
        gripCanvas.SetActive(false);
        ShowMenu();
    }
}