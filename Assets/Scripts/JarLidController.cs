/*
 * JarLidController.cs
 * -------------------
 * Simulates opening a jar lid in a VR daily activity exercise.
 * - Uses XR Grip input to detect patient grip strength.
 * - Provides UI feedback for grip quality (too weak, good, too strong).
 * - Rotates the lid counter-clockwise until it reaches the target open angle.
 * - Once opened, detaches the lid by enabling physics so it can be picked up.
 */

using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class JarLidController : MonoBehaviour
{
    [Header("XR Input")]
    public InputActionProperty gripAction; // XR Grip Action assigned in Inspector

    [Header("Jar Config")]
    public float gripThreshold = 0.3f;      // minimum ergonomic grip required
    public float maxGrip = 0.7f;            // threshold for too much pressure
    public float openAngle = 90f;           // degrees needed to fully open lid
    public float rotationSpeed = 45f;       // rotation speed in degrees per second

    [Header("UI Feedback")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI progressText;

    private bool lidOpened = false;         // tracks if lid has been opened
    private float startAngle;               // initial rotation angle when grabbed
    private Rigidbody rb;                   // rigidbody for enabling physics after opening

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Attach grab/release events if XRGrabInteractable is present
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrab);   // record starting angle
            grab.selectExited.AddListener(OnRelease); // snap back if not opened
        }
    }

    void Start()
    {
        // Initial UI instructions
        instructionText.text = "Grip the jar.";
        feedbackText.text = "";
        progressText.text = "Step 1: Grip the jar.";
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Record the starting rotation angle when lid is grabbed
        startAngle = transform.localEulerAngles.y;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // If lid not opened, snap back to starting angle
        if (!lidOpened)
        {
            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x,
                startAngle,
                transform.localEulerAngles.z
            );
        }
    }

    void Update()
    {
        if (lidOpened) return; // stop processing once lid is opened

        float gripValue = gripAction.action.ReadValue<float>(); // read grip input

        // --- Grip within ergonomic range ---
        if (gripValue >= gripThreshold && gripValue <= maxGrip)
        {
            feedbackText.text = "Good grip — keep steady.";
            progressText.text = "Step 2: Twist the lid.";

            // Rotate lid counter-clockwise
            float step = rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, -step);

            // Measure rotation relative to starting angle
            float currentAngle = Mathf.DeltaAngle(startAngle, transform.localEulerAngles.y);

            // --- Lid fully opened ---
            if (currentAngle >= openAngle)
            {
                // Snap to exact open angle
                transform.localEulerAngles = new Vector3(
                    transform.localEulerAngles.x,
                    startAngle - openAngle,
                    transform.localEulerAngles.z
                );

                lidOpened = true;
                instructionText.text = "Daily Activity Simulation complete.";
                feedbackText.text = "Success — jar opened!";
                progressText.text = "Jar lid detached.";

                // Enable physics so lid can be picked up
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
        // --- Grip too strong ---
        else if (gripValue > maxGrip)
        {
            feedbackText.text = "Too much pressure — relax your grip.";
        }
        // --- Grip too weak ---
        else
        {
            feedbackText.text = "Grip not strong enough yet.";
        }
    }
}