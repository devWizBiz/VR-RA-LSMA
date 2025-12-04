/*
 * ROM.cs
 * -------
 * This component guides a patient through a simple Range of Motion (ROM) assessment.
 * It tracks the hand’s angle relative to a reference, displays progress with a circular bar,
 * provides step-by-step instructions, and gives feedback on repetitions and maximum angle achieved.
 */

using UnityEngine;
using TMPro;

public class ROM : MonoBehaviour
{
    [Header("Tracking")]
    public Transform handTracker;      // Transform representing the patient’s hand
    public Transform reference;        // Reference orientation (e.g., torso or neutral forward)

    [Header("Target Range")]
    public Vector2 targetRange = new Vector2(0, 90); // degrees (min, max)

    [Header("UI Elements")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI angleText;
    public TextMeshProUGUI feedbackText;

    [Header("Progress Bar")]
    public CircularProgressBar circularBar; // visual progress indicator

    private int stepIndex = 0;
    private string[] steps = {
        "Step 1: Place your hand at your side.",
        "Step 2: Raise your hand slowly forward until comfortable.",
        "Step 3: Lower your hand back down.",
        "Step 4: Repeat this movement three times.",
        "Step 5: Try to match the green arc shown in front of you."
    };

    private float maxAngleAchieved = 0f; // highest angle reached during assessment
    private int repetitions = 0;         // count of completed repetitions
    private bool assessmentComplete = false;

    void Start()
    {
        ShowInstruction();               // display first step
        feedbackText.text = "";
        circularBar.SetFill(0f);         // start progress bar empty
    }

    void Update()
    {
        if (assessmentComplete) return;

        // Measure hand angle relative to reference forward vector
        float angle = Vector3.Angle(reference.forward, handTracker.forward);
        angleText.text = $"Hand Angle: {angle:F1}°";

        // Track maximum angle achieved so far
        if (angle > maxAngleAchieved) {
            maxAngleAchieved = angle;
            feedbackText.text = "Great job — new max angle!";
        }

        // Update progress bar based on target range
        float progress = Mathf.InverseLerp(targetRange.x, targetRange.y, angle);
        circularBar.SetFill(progress);

        // Detect repetitions when angle exceeds ~80% of target max
        if (angle > targetRange.y * 0.8f) {
            repetitions++;
            feedbackText.text = $"Repetition {repetitions} complete!";
            if (repetitions >= 3) {
                CompleteAssessment(); // finish after 3 reps
            }
        }
    }

    // Advance to next instruction step
    public void NextStep()
    {
        stepIndex++;
        ShowInstruction();
    }

    // Display current step instruction or fallback message
    void ShowInstruction()
    {
        if (stepIndex < steps.Length) {
            instructionText.text = steps[stepIndex];
        } else {
            instructionText.text = "Follow the arc and raise your hand.";
        }
    }

    // Finalize assessment and show results
    void CompleteAssessment()
    {
        assessmentComplete = true;
        instructionText.text = "Assessment complete!";
        feedbackText.text = $"Max Angle: {maxAngleAchieved:F1}°\nWell done!";
        circularBar.SetFill(1f); // fill bar to indicate completion
    }
}