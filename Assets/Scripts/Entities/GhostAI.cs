using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostAI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float decisionDelay = 0.01f;

    [Header("Speed Boost Settings")]
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float boostedSpeed = 2f;
    [SerializeField] private float boostDuration = 2f;
    [SerializeField] private float minWaitBoost = 5f;
    [SerializeField] private float maxWaitBoost = 15f;

    private float ghostSpeedMultiplier;
    private CubeRoller roller;
    private List<Node> currentPath = new List<Node>();
    
    private Renderer ghostRenderer;
    private Color normalColor;
    private Color boostColor = new Color(0.5f, 0f, 0.5f);

    void Start()
    {
        roller = GetComponent<CubeRoller>();
        ghostRenderer = GetComponent<Renderer>();
        
        ColorUtility.TryParseHtmlString("#F33434", out normalColor);
        if (ghostRenderer != null) ghostRenderer.material.color = normalColor;

        ghostSpeedMultiplier = normalSpeed;

        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;

        StartCoroutine(WaitUntilGameStarts());
        StartCoroutine(RandomSpeedBoostRoutine());
    }

    IEnumerator WaitUntilGameStarts()
    {
        while (CountdownManager.Instance == null || !CountdownManager.Instance.IsGameStarted)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
        MakeNextMove();
    }
    
    private IEnumerator RandomSpeedBoostRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minWaitBoost, maxWaitBoost));
            
            ghostSpeedMultiplier = boostedSpeed;
            if (ghostRenderer != null) ghostRenderer.material.color = boostColor;
            
            yield return new WaitForSeconds(boostDuration);
            
            ghostSpeedMultiplier = normalSpeed;
            if (ghostRenderer != null) ghostRenderer.material.color = normalColor;
        }
    }

    void MakeNextMove()
    {
        if (PowerUpManager.Instance.IsGhostFrozen)
        {
            Invoke(nameof(MakeNextMove), 0.5f);
            return;
        }

        else if (playerTransform == null || roller.IsRolling) return;

        Node currentNode = GridManager.Instance.GetNodeFromWorldPoint(transform.position);
        currentPath = DijkstraPathfinding.FindPath(transform.position, playerTransform.position);

        if (currentPath != null && currentPath.Count > 0)
        {
            Node nextNode = currentPath[0];

            int diffX = nextNode.gridPosition.x - currentNode.gridPosition.x;
            int diffY = nextNode.gridPosition.y - currentNode.gridPosition.y;

            Vector3 moveDir = Vector3.zero;

            if (diffX != 0) 
                moveDir = new Vector3(Mathf.Clamp(diffX, -1, 1), 0, 0);
            else if (diffY != 0) 
                moveDir = new Vector3(0, 0, Mathf.Clamp(diffY, -1, 1));

            if (moveDir != Vector3.zero)
            {
                roller.Roll(moveDir, ghostSpeedMultiplier, () => {
                    StartCoroutine(WaitAndMove());
                });
            }
            else
            {
                Invoke(nameof(MakeNextMove), 0.05f);
            }
        }
        else
        {
            Invoke(nameof(MakeNextMove), 0.2f);
        }
    }

    private System.Collections.IEnumerator WaitAndMove()
    {
        yield return new WaitForSeconds(decisionDelay);
        MakeNextMove();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsInvincible)
            {
                Destroy(gameObject);
            }
            else
            {
                SceneManager.LoadScene("GameOver");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = ghostSpeedMultiplier > normalSpeed ? boostColor : normalColor;
            Vector3 lastPos = transform.position;
            foreach (Node n in currentPath)
            {
                Gizmos.DrawLine(lastPos, n.worldPosition);
                lastPos = n.worldPosition;
                Gizmos.DrawSphere(n.worldPosition, 0.2f);
            }
        }
    }
}