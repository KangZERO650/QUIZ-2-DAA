using UnityEngine;

public enum CollectibleType { Normal, Invincible, Freeze, UnlimitedStamina }

public class Collectible : MonoBehaviour
{
    public CollectibleType type;
    public int scoreValue = 10;
    
    private bool isCollected = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return; 

        if (other.CompareTag("Player"))
        {
            isCollected = true; 

            if (CollectibleManager.Instance != null)
            {
                CollectibleManager.Instance.OnCollected(this);
            }
            
            Destroy(gameObject);
        }
    }
}