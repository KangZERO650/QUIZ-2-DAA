using System.Collections;
using UnityEngine;

public class CubeRoller : MonoBehaviour
{
    [SerializeField] private float baseRollSpeed = 300f;
    private bool isRolling = false;

    public bool IsRolling => isRolling;

    public void Roll(Vector3 direction, float speedMultiplier = 1f, System.Action onComplete = null)
    {
        if (isRolling) return;
        StartCoroutine(RollCoroutine(direction, speedMultiplier, onComplete));
    }

    private IEnumerator RollCoroutine(Vector3 direction, float speedMultiplier, System.Action onComplete)
    {
        isRolling = true;
    
        float remainingAngle = 90f;
        float currentSpeed = baseRollSpeed * speedMultiplier;
        
        Vector3 halfExtents = transform.localScale * 0.5f;
        Vector3 pivot = transform.position + (direction * halfExtents.x) + (Vector3.down * halfExtents.y);
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);

        while (remainingAngle > 0)
        {
            float angle = Mathf.Min(remainingAngle, currentSpeed * Time.deltaTime);
            transform.RotateAround(pivot, rotationAxis, angle);
            remainingAngle -= angle;
            yield return null;
        }

        transform.position = new Vector3(
            Mathf.Round(transform.position.x * 2) / 2f,
            transform.position.y,
            Mathf.Round(transform.position.z * 2) / 2f
        );

        isRolling = false;
        onComplete?.Invoke();
    }
}