using UnityEngine;
using System.Collections;

public class UniversalPortal : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform destination;
    [SerializeField] private float cooldown = 1.5f;
    
    [Header("Visual Effects")]
    [SerializeField] private bool useFadeEffect = true;

    private static bool isTeleporting = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportSequence(other.transform));
        }
    }

    private IEnumerator TeleportSequence(Transform playerTransform)
    {
        isTeleporting = true;

        CubeRoller roller = playerTransform.GetComponent<CubeRoller>();
        if (roller != null) roller.enabled = false; 

        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        Node targetNode = GridManager.Instance.GetNodeFromWorldPoint(destination.position);
        if (targetNode != null)
        {
            Vector3 exitDirection = destination.forward; 
            playerTransform.position = targetNode.worldPosition + Vector3.up * 0.5f + exitDirection;
        }

        if (roller != null) roller.enabled = true;

        yield return new WaitForSeconds(cooldown);
        isTeleporting = false;
    }

    private IEnumerator FadeOnly(float targetAlpha)
    {
        yield return new WaitForSeconds(0.2f); 
    }
}