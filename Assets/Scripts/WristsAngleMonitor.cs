/*
 * WristsAngleMonitor.cs
 * ---------------------
 * Monitors wrist rotation in a typing ergonomics module using XR input.
 * Compares current wrist rotation against a neutral calibration, calculates deviation,
 * updates progress bars and feedback text, and triggers haptic warnings when posture
 * exceeds safe thresholds.
 */

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class WristsAngleMonitor : MonoBehaviour
{
    [Header("XR Input")]
    public InputActionProperty wristRotationRightAction;   // Right hand rotation input
    public InputActionProperty wristRotationLeftAction;    // Left hand rotation input

    [Header("UI References - General")]
    public TextMeshProUGUI instructionText;                // Static guidance text

    [Header("UI References - Right Wrist")]
    public ClassicProgressBar rightBar;
    public TextMeshProUGUI rightFeedbackText;
    public TextMeshProUGUI rightAngleValueText;

    [Header("UI References - Left Wrist")]
    public ClassicProgressBar leftBar;
    public TextMeshProUGUI leftFeedbackText;
    public TextMeshProUGUI leftAngleValueText;

    [Header("Thresholds (degrees)")]
    public float safeAngle = 15f;  // deviation considered safe
    public float maxAngle = 45f;   // maximum deviation before unsafe

    private Quaternion neutralRight; // calibrated neutral rotation for right wrist
    private Quaternion neutralLeft;  // calibrated neutral rotation for left wrist

    void OnEnable()
    {
        // Enable XR input actions when script is active
        wristRotationRightAction.action.Enable();
        wristRotationLeftAction.action.Enable();
    }

    void OnDisable()
    {
        // Disable XR input actions when script is inactive
        wristRotationRightAction.action.Disable();
        wristRotationLeftAction.action.Disable();
    }

    void Start()
    {
        // Capture initial neutral rotations
        neutralRight = wristRotationRightAction.action.ReadValue<Quaternion>();
        neutralLeft = wristRotationLeftAction.action.ReadValue<Quaternion>();

        // Initialize progress bars and UI
        rightBar.SetFillColor(Color.green);
        leftBar.SetFillColor(Color.green);

        instructionText.text = "Place hands on home row. Press 'Set Neutral' to calibrate your wrist posture.";
        rightFeedbackText.text = "";
        leftFeedbackText.text = "";
        rightAngleValueText.text = "Right deviation: 0°";
        leftAngleValueText.text = "Left deviation: 0°";
    }

    // Called when user presses "Set Neutral" to calibrate posture
    public void SetNeutral()
    {
        neutralRight = wristRotationRightAction.action.ReadValue<Quaternion>();
        neutralLeft = wristRotationLeftAction.action.ReadValue<Quaternion>();

        instructionText.text = "Neutral posture saved. Begin typing while keeping wrists aligned.";
        rightFeedbackText.text = "Right wrist calibrated.";
        leftFeedbackText.text = "Left wrist calibrated.";
    }

    void Update()
    {
        // Continuously monitor both wrists each frame
        UpdateWrist(wristRotationRightAction, neutralRight, rightBar, rightFeedbackText, rightAngleValueText, "Right");
        UpdateWrist(wristRotationLeftAction, neutralLeft, leftBar, leftFeedbackText, leftAngleValueText, "Left");
    }

    // Core wrist monitoring logic: calculates deviation, updates UI, triggers haptics
    private void UpdateWrist(InputActionProperty wristAction, Quaternion neutral, ClassicProgressBar bar,
                             TextMeshProUGUI feedbackText, TextMeshProUGUI angleText, string label)
    {
        Quaternion current = wristAction.action.ReadValue<Quaternion>();
        float deviation = Quaternion.Angle(neutral, current); // angular difference from neutral
        float normalized = Mathf.Clamp01(deviation / maxAngle);

        bar.FillAmount = normalized; // update progress bar fill

        // Feedback based on thresholds
        if (deviation <= safeAngle)
        {
            bar.SetFillColor(Color.green);
            feedbackText.text = $"{label} wrist neutral — great!";
        }
        else if (deviation <= maxAngle)
        {
            bar.SetFillColor(Color.yellow);
            feedbackText.text = $"{label} wrist caution — reduce bend.";
            TriggerHaptics(0.25f, 0.1f); // mild haptic warning
        }
        else
        {
            bar.SetFillColor(Color.red);
            feedbackText.text = $"{label} wrist unsafe — straighten!";
            TriggerHaptics(0.5f, 0.15f); // stronger haptic warning
        }

        angleText.text = $"{label} deviation: {Mathf.RoundToInt(deviation)}°";
    }

    private void TriggerHaptics(float amplitude, float duration)
    {
        // Placeholder for XR controller haptics
        // Example: XRBaseController.SendHapticImpulse(amplitude, duration);
    }
}