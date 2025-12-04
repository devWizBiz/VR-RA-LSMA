/*
 * MoveAccConsist.cs
 * ------------------------------------------
 * Displays 2 hand-motion trails in VR:
 *   - IDEAL PATH (reference therapist motion) → drawn from the assigned LineRenderer points
 *   - ACTUAL TRAIL (patient movement) → drawn live from hand tracker samples
 *
 * After each repetition, press "Record Rep" button to:
 *   - Save accuracy distance for that repetition
 *   - Reset actual trail for next rep
 *   - Compute Accuracy, Smoothness, Consistency
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MoveAccConsist : MonoBehaviour
{
    [Header("Hand Tracking")]
    public Transform handTracker;

    [Header("Trails")]
    public LineRenderer idealPath;
    public LineRenderer actualTrail;

    [Header("UI Text")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI smoothnessText;
    public TextMeshProUGUI consistencyText;
    public TextMeshProUGUI feedbackText;

    private List<Vector3> samples = new List<Vector3>();
    private List<Vector3> currentRepTrail = new List<Vector3>();
    private List<float> repDistances = new List<float>();

    public int maxRepetitions = 3;
    private int repetitions = 0;
    private bool assessmentComplete = false;

    void Start()
    {
        // Enable LRs so they draw in 3D/VR
        idealPath.enabled = true;
        actualTrail.enabled = true;
        actualTrail.positionCount = 0;

        instructionText.text = "Trace the path.";
    }

    void Update()
    {
        if (assessmentComplete) return;

        Vector3 pos = handTracker.position;
        samples.Add(pos);
        currentRepTrail.Add(pos);

        // Draws only the active repetition segment
        actualTrail.positionCount = currentRepTrail.Count;
        actualTrail.SetPositions(currentRepTrail.ToArray());

        float acc = DistanceToPercent(ComputeAverageDistance(samples, idealPath));
        float smooth = ComputeSmoothnessPercent(samples);
        float consistency = ComputeConsistencyPercent(repDistances);

        accuracyText.text = $"Accuracy: {acc:F0}%";
        smoothnessText.text = $"Smoothness: {smooth:F0}%";
        consistencyText.text = $"Consistency: {consistency:F0}%";

        // Feedback is based on priority, not all metrics competing
        feedbackText.text = GetFeedback(acc, smooth, consistency);
    }

    // Converts average world distance into a 0–100% score
    float DistanceToPercent(float d) => Mathf.Clamp(100f - d * 10f, 0f, 100f);

    // Accuracy: average distance to closest path segment
    float ComputeAverageDistance(List<Vector3> s, LineRenderer lr)
    {
        if (s.Count == 0 || lr.positionCount < 2) return 0f;
        float sum = 0f;
        foreach (var p in s) sum += ClosestDist(lr, p);
        return sum / s.Count;
    }

    // Smoothness based on velocity variation (jerk proxy)
    float ComputeSmoothnessPercent(List<Vector3> s)
    {
        if (s.Count < 3) return 100f;
        float sum = 0f; int n = 0;

        for (int i = 2; i < s.Count; i++)
        {
            float v1 = (s[i-1] - s[i-2]).magnitude / Time.deltaTime;
            float v2 = (s[i] - s[i-1]).magnitude / Time.deltaTime;
            sum += Mathf.Abs(v2 - v1);
            n++;
        }

        float jerk = sum / Mathf.Max(1, n);

        // Scales jerk so values aren’t always near 100
        return Mathf.Clamp(100f - jerk * 20f, 0f, 100f);
    }

    // Consistency based on variance between repetitions
    float ComputeConsistencyPercent(List<float> distances)
    {
        if (distances.Count < 2) return 100f;

        float avg = 0f;
        foreach (float d in distances) avg += d;
        avg /= distances.Count;

        float var = 0f;
        foreach (float d in distances) var += Mathf.Pow(d - avg, 2);
        var /= distances.Count;

        return Mathf.Clamp(100f - var * 40f, 0f, 100f);
    }

    // Button-triggered repetition recording and reset
    public void RecordRepetition()
    {
        if (repetitions >= maxRepetitions || assessmentComplete) return;

        float d = ComputeAverageDistance(currentRepTrail, idealPath);
        repDistances.Add(d);
        repetitions++;

        // Reset only the actual trail segment
        currentRepTrail.Clear();
        samples.Clear();
        actualTrail.positionCount = 0;

        if (repetitions >= maxRepetitions)
            assessmentComplete = true;
    }

    // Returns only the most important feedback
    string GetFeedback(float a, float s, float c)
    {
        if (a < 60) return "Move closer to the reference path.";
        if (s < 60) return "Slow your hand movement for smoother motion.";
        if (c < 70) return "Try to repeat more consistently.";
        return "Excellent pacing and control!";
    }

    // Distance utility used inside accuracy
    float ClosestDist(LineRenderer lr, Vector3 p)
    {
        float best = float.MaxValue;
        for (int i = 0; i < lr.positionCount - 1; i++)
        {
            Vector3 a = lr.GetPosition(i), b = lr.GetPosition(i+1), ab = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
            Vector3 q = a + ab * t;
            best = Mathf.Min(best, Vector3.Distance(p, q));
        }
        return best;
    }
}