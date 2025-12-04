/*
 * TherapistGuide.cs
 * -----------------
 * This component is a placeholder for therapist guidance animations or logic.
 * It provides an API surface (PlayStretch, PlayIdle) so other scripts can call
 * these methods without breaking when the implementation is added later.
 * Currently, the methods do nothing but preserve stability for integration.
 */

using UnityEngine;

public class TherapistGuide : MonoBehaviour
{
    // Called when a stretch routine begins; intended to trigger therapist animation or guidance
    public void PlayStretch(string name) { }

    // Called when routine is idle or complete; intended to reset therapist to neutral state
    public void PlayIdle() { }
}