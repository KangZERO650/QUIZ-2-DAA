using UnityEngine;

public class CollectibleRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0);
    [SerializeField] private float rotationSpeed = 100f;

    [Header("Bobbing Settings (Optional)")]
    [SerializeField] private bool useBobbing = true;
    [SerializeField] private float bobAmplitude = 0.1f;
    [SerializeField] private float bobFrequency = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        if (useBobbing)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}