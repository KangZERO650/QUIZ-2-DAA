using UnityEngine;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("Durations")]
    public float powerUpDuration = 5f;

    public bool IsInvincible { get; private set; }
    public bool IsGhostFrozen { get; private set; }
    public bool IsUnlimitedStamina { get; private set; }

    void Awake() => Instance = this;

    public void ActivatePowerUp(CollectibleType type)
    {
        StopCoroutine(type.ToString());
        StartCoroutine(type.ToString());
    }

    IEnumerator Invincible()
    {
        IsInvincible = true;
        Debug.Log("PowerUp: Invincible Start");
        yield return new WaitForSeconds(powerUpDuration);
        IsInvincible = false;
    }

    IEnumerator Freeze()
    {
        IsGhostFrozen = true;
        Debug.Log("PowerUp: Freeze Ghost Start");
        yield return new WaitForSeconds(powerUpDuration);
        IsGhostFrozen = false;
    }

    IEnumerator UnlimitedStamina()
    {
        IsUnlimitedStamina = true;
        Debug.Log("PowerUp: Unlimited Stamina Start");
        yield return new WaitForSeconds(powerUpDuration);
        IsUnlimitedStamina = false;
    }
}