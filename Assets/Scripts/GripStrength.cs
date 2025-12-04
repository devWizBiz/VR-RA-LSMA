/*
 * GripStrength.cs
 * ----------------
 * This component guides a patient through a grip strength assessment in VR.
 * It progresses through phases (Intro, Warmup, Calibration, Trials, Rest, Complete),
 * records peak grip values, calculates averages and endurance, and provides
 * real-time feedback via UI text.
 */

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class GripStrength : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty gripAction; // bind to right-hand grip or trigger (0..1)

    [Header("UI")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI peakText;
    public TextMeshProUGUI avgText;
    public TextMeshProUGUI enduranceText;

    [Header("Timing")]
    public float trialDuration = 2.0f; // seconds to hold squeeze
    public float restDuration = 3.0f;  // seconds between trials
    public int totalTrials = 3;

    [Header("Calibration")]
    public float minCalGrip = 0.2f;            // minimum signal considered a valid warm-up/calibration
    private float calibratedMax01 = 0.8f;     // updated during calibration    

    private enum Phase { Intro, Warmup, Calibrate, Ready, Squeezing, Rest, Complete }
    private Phase phase = Phase.Intro;

    // Metrics across session
    private List<float> trialPeaks01 = new List<float>();
    private int trialIndex = 0;
    private float phaseTimer = 0f;

    // Live rolling stats (for display while squeezing)
    private float livePeak01 = 0f;
    private float liveSum01 = 0f;
    private int liveCount = 0;

    void OnEnable()
    {
        // Reset state when component is enabled
        ResetLiveStats();
        trialPeaks01.Clear();
        trialIndex = 0;
        phase = Phase.Intro;

        if (instructionText) instructionText.text = "We’re going to measure your grip strength. Squeeze when prompted.";
        if (feedbackText) feedbackText.text = "Tap Begin or wait...";
        UpdateHUD(0f, 0f);
        phaseTimer = 0f;
    }

    void Update()
    {
        float g = ReadGrip01();

        // Phase machine controls flow of assessment
        switch (phase)
        {
            case Phase.Intro:
                phaseTimer += Time.deltaTime;
                if (phaseTimer > 2f) TransitionTo(Phase.Warmup, "Warm-up: make a gentle squeeze, then relax.", "");
                break;

            case Phase.Warmup:
                UpdateHUD(g, calibratedMax01);
                if (g > minCalGrip) feedbackText.text = "Nice gentle squeeze.";
                if (g < 0.05f && Time.timeSinceLevelLoad > 4f)                   // Once relaxed after warmup, move to calibration
                    TransitionTo(Phase.Calibrate, "Calibration: squeeze firmly once, then release.", "");
                break;

            case Phase.Calibrate:
                UpdateHUD(g, calibratedMax01);
                calibratedMax01 = Mathf.Max(calibratedMax01, g);
                if (g < 0.05f && Time.timeSinceLevelLoad > 6f)         // Track maximum grip during calibration
                    TransitionTo(Phase.Ready, $"Trial {trialIndex + 1} of {totalTrials}: get ready.", "Calibration saved.");
                break;

            case Phase.Ready:
             // Short pause before trial begins
                phaseTimer += Time.deltaTime;
                if (phaseTimer > 1.0f)
                {
                    phaseTimer = 0f;
                    ResetLiveStats();
                    TransitionTo(Phase.Squeezing, "Squeeze now and hold for 2 seconds.", "Go!");
                }
                break;

            case Phase.Squeezing:
                // Collect live stats during squeeze
                livePeak01 = Mathf.Max(livePeak01, g);
                liveSum01 += g;
                liveCount++;

                // Feedback based on relative effort vs calibration
                float rel = RelToCal(g);
                if (rel > 0.9f) feedbackText.text = "Great peak! Hold…";
                else if (rel > 0.6f) feedbackText.text = "Good squeeze — a bit more if comfortable.";
                else if (rel > 0.3f) feedbackText.text = "Keep squeezing…";

                UpdateHUD(g, calibratedMax01);

                phaseTimer += Time.deltaTime;
                if (phaseTimer >= trialDuration)
                {
                    phaseTimer = 0f;
                    // Record peak for this trial
                    EnsureTrialIndex();
                    trialPeaks01[trialIndex] = Mathf.Max(trialPeaks01[trialIndex], livePeak01);

                    // Move to rest or complete
                    feedbackText.text = "Release and relax.";
                    phase = (trialIndex + 1 < totalTrials) ? Phase.Rest : Phase.Complete;
                }
                break;

            case Phase.Rest:
                instructionText.text = "Rest for a moment. Next trial starts soon.";
                phaseTimer += Time.deltaTime;
                if (phaseTimer >= restDuration)
                {
                    phaseTimer = 0f;
                    trialIndex++;
                    TransitionTo(Phase.Ready, $"Trial {trialIndex + 1} of {totalTrials}: get ready.", "");
                }
                break;

            case Phase.Complete:
                Summarize();         // Final results
                break;
        }
    }

    // --- Helpers ---

    // Read grip input normalized to 0..1
    float ReadGrip01()
    {
        return gripAction.action != null ? Mathf.Clamp01(gripAction.action.ReadValue<float>()) : 0f;
    }

    // Convert grip to relative effort vs calibration max
    float RelToCal(float g)
    {
        return Mathf.Clamp01(calibratedMax01 > 0.1f ? (g / calibratedMax01) : g);
    }

    void ResetLiveStats()
    {
        livePeak01 = 0f;
        liveSum01 = 0f;
        liveCount = 0;
    }

    // Ensure trial list has entry for current index
    void EnsureTrialIndex()
    {
        if (trialPeaks01.Count <= trialIndex) trialPeaks01.Add(0f);
    }

    // Transition helper: updates phase and UI text
    void TransitionTo(Phase next, string instruction, string feedback)
    {
        phase = next;
        phaseTimer = 0f;
        if (instructionText) instructionText.text = instruction;
        if (feedbackText) feedbackText.text = feedback;
    }

    // Update HUD with live stats
    void UpdateHUD(float currentGrip01, float calMax01)
    {
        float peak01 = Mathf.Max(livePeak01, currentGrip01);
        float avg01 = (liveCount > 0) ? (liveSum01 / liveCount) : currentGrip01;
        float endurance = Mathf.Clamp01((calMax01 > 0.1f ? avg01 / calMax01 : avg01)) * 100f;

        if (peakText) peakText.text = $"Peak: {(peak01 * 100f):F0}%";
        if (avgText) avgText.text = $"Average: {(avg01 * 100f):F0}%";
        if (enduranceText) enduranceText.text = $"Endurance: {endurance:F0}%";
    }

    // Summarize results across all trials
    void Summarize()
    {
        // session peak & average across trials
        float peak = 0f, sum = 0f;
        for (int i = 0; i < trialPeaks01.Count; i++)
        {
            peak = Mathf.Max(peak, trialPeaks01[i]);
            sum += trialPeaks01[i];
        }
        int n = Mathf.Max(1, trialPeaks01.Count);
        float avg = sum / n;

        float endurance = Mathf.Clamp01((calibratedMax01 > 0.1f ? avg / calibratedMax01 : avg)) * 100f;

        instructionText.text = "Assessment complete.";
        feedbackText.text = peak >= calibratedMax01 * 0.9f
            ? "Excellent effort!"
            : "Good work! You can improve with practice.";

        if (peakText) peakText.text = $"Peak: {(peak * 100f):F0}%";
        if (avgText) avgText.text = $"Average: {(avg * 100f):F0}%";
        if (enduranceText) enduranceText.text = $"Endurance: {endurance:F0}%";
    }
}