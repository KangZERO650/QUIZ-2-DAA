using UnityEngine;
using TMPro;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance;
    
    public static int FinalScore { get; private set; }
    
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    private GameObject currentSpawnedItem;

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;

    [Header("Prefabs")]
    public GameObject normalPrefab;
    public GameObject[] powerUpPrefabs;

    [Header("Settings")]
    [Range(0, 100)] public float powerUpChance = 30f;
    
    private int currentScore = 0;

    void Awake() 
    {
        Instance = this;
        currentScore = 0;
        FinalScore = 0;
    }

    void Start() => SpawnNext();

    public void SpawnNext()
    {
        if (currentSpawnedItem != null) return;

        Node playerNode = null;
        if (playerTransform != null)
        {
            playerNode = GridManager.Instance.GetNodeFromWorldPoint(playerTransform.position);
        }

        Node spawnNode = GridManager.Instance.GetRandomWalkableNode(playerNode);
        
        if (spawnNode == null) return;

        GameObject toSpawn = normalPrefab;

        if (powerUpPrefabs != null && powerUpPrefabs.Length > 0)
        {
            if (Random.value <= powerUpChance / 100f)
            {
                toSpawn = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            }
        }

        if (toSpawn != null)
        {
            currentSpawnedItem = Instantiate(toSpawn, spawnNode.worldPosition + Vector3.up * 0.5f, Quaternion.identity);
        }
    }

    public void OnCollected(Collectible item)
    {
        currentScore += item.scoreValue;
        scoreText.text = currentScore.ToString();
        
        FinalScore = currentScore;

        if (item.type != CollectibleType.Normal)
        {
            PowerUpManager.Instance.ActivatePowerUp(item.type);
        }

        currentSpawnedItem = null; 
        SpawnNext();
    }
}