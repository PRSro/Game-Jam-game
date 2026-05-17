using UnityEngine;
using System.Collections;

/// <summary>Handles phase transitions with a countdown per phase.</summary>
public class TurnPhaseManager : MonoBehaviour
{
    public static TurnPhaseManager Instance { get; private set; }

    public const float PHASE_TIME_LIMIT = 45f;

    Coroutine phaseTimerCoroutine;
    System.Action onTimerExpired;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Starts a countdown for the current phase. Calls onExpired when time runs out.</summary>
    public void StartPhaseTimer(System.Action onExpired)
    {
        StopPhaseTimer();
        onTimerExpired = onExpired;
        phaseTimerCoroutine = StartCoroutine(PhaseTimerCoroutine());
    }

    /// <summary>Stops the phase countdown and hides the UI bar.</summary>
    public void StopPhaseTimer()
    {
        if (phaseTimerCoroutine != null)
        {
            StopCoroutine(phaseTimerCoroutine);
            phaseTimerCoroutine = null;
        }
        onTimerExpired = null;
        GameManager.Instance?.OnPhaseTimerUpdate?.Invoke(0f);
    }

    IEnumerator PhaseTimerCoroutine()
    {
        float timeLeft = PHASE_TIME_LIMIT;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            GameManager.Instance?.OnPhaseTimerUpdate?.Invoke(timeLeft);
            yield return null;
        }
        GameManager.Instance?.OnPhaseTimerUpdate?.Invoke(0f);

        System.Action expired = onTimerExpired;
        onTimerExpired = null;
        expired?.Invoke();
    }
}
