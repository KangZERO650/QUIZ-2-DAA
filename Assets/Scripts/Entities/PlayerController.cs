using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Color normalBarColor = Color.cyan;
    [SerializeField] private Color cooldownBarColor = Color.gray; 
    [SerializeField] private Color unlimitedBarColor = Color.yellow;

    [Header("Movement Settings")]
    [SerializeField] private float sprintMultiplier = 2f;
    
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float drainRate = 40f; 
    [SerializeField] private float refillRate = 20f;
    
    [Header("Cooldown Settings")]
    [SerializeField] private float sprintCooldownDuration = 1.5f; 
    [SerializeField] private float refillDelay = 0.5f;           
    
    private float currentStamina;
    private float refillTimer;
    private float sprintCooldownTimer;
    private bool canSprint = true;
    private bool wasSprintingLastFrame;
    
    private Image sliderFillImage;
    private CubeRoller roller;
    private bool isSprinting;

    void Start()
    {
        roller = GetComponent<CubeRoller>();
        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
            sliderFillImage = staminaSlider.fillRect.GetComponent<Image>();
            sliderFillImage.color = normalBarColor;
        }
    }

    void Update()
    {
        if (CountdownManager.Instance != null && !CountdownManager.Instance.IsGameStarted) return;

        HandleTimers();
        UpdateUI();

        if (roller.IsRolling) return;

        Vector3 moveDir = GetInputDirection();

        if (moveDir != Vector3.zero)
        {
            Vector3 targetPos = transform.position + moveDir;

            Node targetNode = GridManager.Instance.GetNodeFromWorldPoint(targetPos);

            if (targetNode != null && targetNode.isWalkable)
            {
                float speedMult = 1f;

                bool isUnlimited = PowerUpManager.Instance != null && PowerUpManager.Instance.IsUnlimitedStamina;
                bool wantToSprint = Input.GetKey(KeyCode.LeftShift) && (currentStamina > 0 || isUnlimited);

                if (canSprint && wantToSprint) 
                {
                    speedMult = sprintMultiplier;
                    isSprinting = true;
                    refillTimer = refillDelay; 
                }
                else
                {
                    if (wasSprintingLastFrame) StartSprintCooldown();
                    isSprinting = false;
                }

                roller.Roll(moveDir, speedMult);
            }
            else
            {
                if (wasSprintingLastFrame) StartSprintCooldown();
                isSprinting = false;
            }
        }
        else
        {
            if (wasSprintingLastFrame) StartSprintCooldown();
            isSprinting = false;
        }

        wasSprintingLastFrame = isSprinting;
    }

    void HandleTimers()
    {
        bool isUnlimited = PowerUpManager.Instance != null && PowerUpManager.Instance.IsUnlimitedStamina;

        if (!canSprint)
        {
            sprintCooldownTimer -= Time.deltaTime;
            if (sprintCooldownTimer <= 0)
            {
                canSprint = true;
                if (sliderFillImage != null) sliderFillImage.color = isUnlimited ? unlimitedBarColor : normalBarColor;
            }
        }

        if (isSprinting && roller.IsRolling)
        {
            if (!isUnlimited)
            {
                currentStamina -= drainRate * Time.deltaTime;
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    isSprinting = false; 
                }
            }
            else
            {
                currentStamina = maxStamina;
            }
        }
        else if (currentStamina < maxStamina)
        {
            if (refillTimer > 0) refillTimer -= Time.deltaTime;
            else currentStamina += refillRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    void StartSprintCooldown()
    {
        canSprint = false;
        sprintCooldownTimer = sprintCooldownDuration;
        if (sliderFillImage != null) sliderFillImage.color = cooldownBarColor;
    }

    Vector3 GetInputDirection()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) return Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) return Vector3.back;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) return Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) return Vector3.right;
        return Vector3.zero;
    }

    void UpdateUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
            if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsUnlimitedStamina)
                sliderFillImage.color = unlimitedBarColor;
            else if (canSprint)
                sliderFillImage.color = normalBarColor;
        }
    }
}