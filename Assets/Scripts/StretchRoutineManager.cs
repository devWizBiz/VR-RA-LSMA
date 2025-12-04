/*
 * StretchRoutineManager.cs
 * ------------------------
 * Manages Guided Strength Routine flow:
 *   - Start button launches routine and then hides itself
 *   - Patient first sees instructions (instruction preview phase)
 *   - Begin Exercise button starts the actual stretch routine
 *   - Breathing cues shown with CircularProgressBar
 *   - repsText shows repetition count
 *   - feedbackText gives patient encouragement during exercise
 *   - Pain panel with slider, confirm, and skip buttons after each stretch
 *   - Stretch instructions loaded from ScriptableObjects in Assets/Training/Stretch
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StretchRoutineManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI instructionText;    // Therapist/RA-safe instructions
    public TextMeshProUGUI feedbackText;       // Patient encouragement or guidance
    public TextMeshProUGUI repsText;           // Repetition counter
    public TextMeshProUGUI breathingText;      // Breathing cues
    public CircularProgressBar breathingBar;   // Animated breathing circle

    [Header("Pain Panel")]
    public GameObject painPanel;               // Pain rating panel
    public Slider painSlider;                  // Slider for pain rating
    public TextMeshProUGUI painLabel;          // Displays pain level

    [Header("Buttons")]
    public GameObject startButton;             // Global start button
    public GameObject beginExerciseButton;     // Button shown during instruction preview

    [Header("Routine Data")]
    public Stretch[] stretches;                // ScriptableObject assets for each stretch
    public TherapistGuide therapistGuide;      // Stub for therapist animations/guidance

    private bool routineRunning;               // Tracks if exercise coroutine is active
    private int currentIndex;                  // Current stretch index
    private int currentRep;                    // Current repetition count

    void Start()
    {
        // Initialize UI state
        painSlider.value = 0;
        painLabel.text = "Pain Level: 0";
        painPanel.SetActive(false);
        repsText.text = "";
        feedbackText.text = "Press Start to begin.";
        instructionText.text = "";
        breathingBar.SetFill(0f);

        // Attach slider event in code (avoids Inspector wiring issues)
        painSlider.onValueChanged.AddListener(OnPainSliderChanged);

        // Auto-load Stretch assets if none assigned
        if (stretches == null || stretches.Length == 0)
        {
            stretches = Resources.LoadAll<Stretch>("Training/Stretch");
        }

        // Hide Begin Exercise button until routine starts
        if (beginExerciseButton != null)
            beginExerciseButton.SetActive(false);
    }

    // Called when Start button is pressed
    public void StartRoutine()
    {
        routineRunning = false;
        currentIndex = 0;
        currentRep = 0;

        if (startButton != null)
            startButton.SetActive(false); // Hide global start button

        ShowInstructionPreview(); // Show first stretch instructions
    }

    // Instruction preview phase: patient reads before pressing Begin Exercise
    private void ShowInstructionPreview()
    {
        if (currentIndex < stretches.Length)
        {
            Stretch s = stretches[currentIndex];
            therapistGuide.PlayStretch(s.displayName);
            instructionText.text = FormatInstruction(s.instruction);

            // Reset UI for preview phase
            repsText.text = "";
            breathingText.text = "";
            breathingBar.SetFill(0f);
            feedbackText.text = "Read the instructions, then press Begin Exercise.";

            // Show Begin Exercise button only during preview
            beginExerciseButton.SetActive(true);
        }
        else
        {
            // Routine finished
            instructionText.text = "";
            feedbackText.text = "Routine complete!";
            repsText.text = "";
            therapistGuide.PlayIdle();
            beginExerciseButton.SetActive(false);
        }
    }

    // Called when patient presses Begin Exercise
    public void BeginExercise()
    {
        routineRunning = true;

        // Hide Begin button once exercise starts
        beginExerciseButton.SetActive(false);

        StartCoroutine(RunStretchRoutine());
    }

    // Main routine loop: runs one stretch, then pauses for pain check
    private IEnumerator RunStretchRoutine()
    {
        while (routineRunning && currentIndex < stretches.Length)
        {
            Stretch s = stretches[currentIndex];
            therapistGuide.PlayStretch(s.displayName);

            // Instructions already shown in preview, so just reset reps
            currentRep = 0;

            // Handle repetition-based stretches
            if (s.reps > 0)
            {
                while (currentRep < s.reps)
                {
                    breathingText.text = "Inhale...";
                    yield return AnimateBreathingBar(0f, 1f, s.inhaleSeconds);

                    breathingText.text = "Exhale...";
                    yield return AnimateBreathingBar(1f, 0f, s.exhaleSeconds);

                    currentRep++;
                    repsText.text = $"Rep {currentRep} of {s.reps}";

                    // Alternate encouragement messages
                    feedbackText.text = currentRep % 2 == 0
                        ? "Nice rhythm, keep it steady!"
                        : "Good work, stay relaxed.";
                }
            }
            // Handle hold-based stretches
            else if (s.holdDuration > 0)
            {
                breathingText.text = "Hold position...";
                yield return AnimateBreathingBar(0f, 1f, s.holdDuration);
            }

            // Pause routine for pain check before continuing
            routineRunning = false;
            ShowPainCheck();
            yield break;
        }

        // Routine complete
        breathingText.text = "";
        breathingBar.SetFill(0f);
        therapistGuide.PlayIdle();
    }

    // Breathing bar animation helper
    private IEnumerator AnimateBreathingBar(float startValue, float endValue, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = t / duration;
            breathingBar.SetFill(Mathf.Lerp(startValue, endValue, normalized));
            yield return null;
        }
    }

    // --- Pain Panel ---
    public void OnPainSliderChanged(float value)
    {
        // Update label live as slider moves
        painLabel.text = $"Pain Level: {Mathf.RoundToInt(value)}";
    }

    public void ConfirmPain()
    {
        // Log and hide panel, then move to next stretch
        int painLevel = Mathf.RoundToInt(painSlider.value);
        Debug.Log("Pain confirmed: " + painLevel);
        painPanel.SetActive(false);

        currentIndex++;
        routineRunning = false;
        ShowInstructionPreview(); // go back to preview phase
    }

    public void SkipPain()
    {
        // Skip pain check and continue routine
        Debug.Log("Pain skipped");
        painPanel.SetActive(false);

        currentIndex++;
        routineRunning = false;
        ShowInstructionPreview(); // go back to preview phase
    }

    private void ShowPainCheck()
    {
        // Show pain panel and reset slider/label
        painPanel.SetActive(true);
        instructionText.text = "";
        feedbackText.text = "Please rate your pain level.";
        repsText.text = "";
        painSlider.value = 0;
        painLabel.text = "Pain Level: 0";
    }

    // Format instructions into bullet points for VR readability
    private string FormatInstruction(string raw)
    {
        string[] parts = raw.Split('.');
        string formatted = "";
        foreach (string p in parts)
        {
            if (!string.IsNullOrWhiteSpace(p))
                formatted += "• " + p.Trim() + "\n";
        }
        return formatted.Trim();
    }
}