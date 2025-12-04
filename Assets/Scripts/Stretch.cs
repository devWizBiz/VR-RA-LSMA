/*
 * Stretch.cs
 * ----------
 * Defines a stretch exercise as a reusable ScriptableObject asset.
 * Each stretch contains display information, RA-safe instructions,
 * and parameters for either timed holds or repetitions with breathing cues.
 * These assets are loaded by the routine manager to guide patients.
 */

using UnityEngine;

[CreateAssetMenu(menuName = "Training/Stretch")]
public class Stretch : ScriptableObject
{
    public string displayName;             // Name shown in UI (e.g., "Wrist Flexor Stretch")

    [TextArea] 
    public string instruction;             // Therapist/RA-safe step-by-step instructions

    public float holdDuration = 20f;       // Duration in seconds for static hold (set reps = 0)
    public int reps = 0;                   // Number of repetitions (set holdDuration = 0)

    // Breathing cues used during repetitions or holds
    public float inhaleSeconds = 3f;       
    public float exhaleSeconds = 4f;
}
