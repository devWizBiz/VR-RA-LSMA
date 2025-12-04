/*
 * JointMobilityTraining.cs
 * ---------------------------------------------------------------
 * Tracks hand raise angle relative to a shoulder reference transform.
 * Progress bar fills as user raises hand toward target ROM (0–90°).
 * Reps are counted using a top→down movement cycle inside 2 sets.
 */

using UnityEngine;
using TMPro;
using System.Collections;

public class JointMobilityTraining : MonoBehaviour
{
    [Header("References")]
    public Transform handTracker;          
    public Transform shoulderReference;    
    public CircularProgressBar progressBar;

    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI progressText;

    [Header("Training Config")]
    public int sets = 2;
    public int repsPerSet = 6;
    public float targetAngle = 90f;        
    public float tolerance = 5f;          
    public float restDuration = 30f;       
    public float holdTopSeconds = 2f;

    private int currentSet = 0;
    private int currentRep = 0;
    private bool reachedTop = false;
    private bool holding = false;

    private float warmupTimer = 0f;
    private float restTimer = 0f;
    private float holdTimer = 0f;

    private enum Phase { Warmup, Training, HoldTop, Rest, Complete }
    private Phase phase = Phase.Warmup;

    private string[] steps = {
        "Relax arm by your side.",
        "Gently raise your hand toward shoulder height.",
        "Lift until you reach your comfortable limit (~90°).",
        "Hold briefly at the top.",
        "Lower your hand back down smoothly."
    };

    private int stepIndex = 0;

    void Start()
    {
        ResetBar();
        feedbackText.text = "";
        progressText.text = $"Set 1/{sets}, Rep 1/{repsPerSet}";
        instructionText.text = steps[stepIndex];
    }

    void Update()
    {
        if (phase == Phase.Complete) return;

        switch (phase)
        {
            case Phase.Warmup:   DoWarmup(); break;
            case Phase.Training: DoTraining(); break;
            case Phase.HoldTop:  DoHoldTop(); break;
            case Phase.Rest:     DoRest(); break;
        }
    }

    // --- WARMUP ---
    void DoWarmup()
    {
        warmupTimer += Time.deltaTime;
        float angle = GetCurrentShoulderAngle();

        // Warmup only fills halfway to avoid false "complete" look
        progressBar.SetFill(Mathf.Clamp01(angle / 40f));
        feedbackText.text = "Move slowly and comfortably.";

        if (warmupTimer > 6f)
        {
            ResetBar();
            BeginTraining();
        }
    }

    // --- TRAINING ---
    void BeginTraining()
    {
        currentSet = 0;
        currentRep = 0;
        reachedTop = false;
        holding = false;
        holdTimer = 0f;
        restTimer = 0f;
        warmupTimer = 0f;

        phase = Phase.Training;
        stepIndex = 1;
        instructionText.text = steps[stepIndex];
        progressText.text = $"Set {currentSet+1}/{sets}, Rep {currentRep+1}/{repsPerSet}";
    }

    void DoTraining()
    {
        float angle = GetCurrentShoulderAngle();
        bool nearTarget = angle >= (targetAngle - tolerance);
        bool nearStart  = angle <= tolerance + 2f;

        // When reaching top for first time → switch to hold
        if (nearTarget && !holding)
        {
            holding = true;
            reachedTop = true;
            phase = Phase.HoldTop;
            holdTimer = 0f;
            return;
        }

        // Fill only while raising (NOT when lowering)
        if (!holding)
            progressBar.SetFill(Mathf.Clamp01(angle / targetAngle));

        // When rep completes (top + return to start)
        if (reachedTop && nearStart)
        {
            currentRep++;
            reachedTop = false;
            holding = false;
            ResetBar();

            progressText.text = $"Set {currentSet+1}/{sets}, Rep {currentRep}/{repsPerSet}";
            instructionText.text = steps[4];
            feedbackText.text = "Good lowering motion.";

            if (currentRep >= repsPerSet)
            {
                currentSet++;
                currentRep = 0;

                if (currentSet >= sets)
                    CompleteTraining();
                else
                    BeginRest();
            }
            else
            {
                // Go back to "raise" instruction for next rep
                stepIndex = 2;
                instructionText.text = steps[stepIndex];
            }
        }

        feedbackText.text = GetFeedbackMessage(angle);
    }

    // --- HOLD TOP ---
    void DoHoldTop()
    {
        holdTimer += Time.deltaTime;
        progressBar.SetFill(1f);
        instructionText.text = "Hold position gently…";
        feedbackText.text = "Good hold, relax shoulder.";

        if (holdTimer >= holdTopSeconds)
        {
            holding = false;
            phase = Phase.Training;
            instructionText.text = steps[4]; // switch to lowering instruction
            progressBar.SetFill(0.85f); // buffer so lowering starts visibly below 1
        }
    }

    // --- REST PHASE ---
    void BeginRest()
    {
        phase = Phase.Rest;
        restTimer = 0f;
        reachedTop = false;
        holding = false;
        
        // reset progress visuals when resting
        progressBar.SetFill(0f);
        
        instructionText.text = "Rest — relax your arm and shoulder.";
        feedbackText.text = "You're doing well. Slow breathing.";
        progressText.text = $"Set {currentSet}/{sets} complete — rest before next set.";
    }

    void DoRest()
    {
        restTimer += Time.deltaTime;
        
        // show rest countdown progress (0 → 1 over restDuration)
        progressBar.SetFill(Mathf.Clamp01(restTimer / restDuration));
        
        if (restTimer >= restDuration)
        {
            // reset and resume training
            progressBar.SetFill(0f);
            phase = Phase.Training;
            instructionText.text = steps[1]; // back to raise instruction
            progressText.text = $"Set {currentSet+1}/{sets}, Rep 1/{repsPerSet}";
            feedbackText.text = "";
        }
    }
    
    // --- COMPLETE ---
    void CompleteTraining()
    {
        phase = Phase.Complete;
        instructionText.text = "Joint mobility exercise complete ✅";
        feedbackText.text = "Great job today. Rest now.";
        progressText.text = $"All {sets} sets finished";
        progressBar.SetFill(1f);
    }

    // --- HELPERS ---
    void ResetBar() => progressBar.SetFill(0f);

    float GetCurrentShoulderAngle()
    {
        return Vector3.Angle(
            shoulderReference.forward,
            handTracker.position - shoulderReference.position
        );
    }

    string GetFeedbackMessage(float angle)
    {
        if (holding) return "Holding at top...";
        if (angle >= targetAngle - tolerance) return "✅ Target reached!";
        if (angle > 50f) return "Nice lift — keep it smooth.";
        if (angle > 20f) return "Good start — gentle raise.";
        return "Begin the movement when ready.";
    }
}