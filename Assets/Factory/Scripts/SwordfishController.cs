using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Factory;
using UnityEngine;

public class SwordfishController : FishController
{
    #region Constants
    private const float SCREEN_LEFT_THRESHOLD = -0.2f;
    private const float SCREEN_RIGHT_THRESHOLD = 1.2f;
    private const int INITIAL_SORTING_ORDER = 100;
    private const float FADE_DURATION = 3f;
    private const float DEATH_MOVE_Y = -1f;
    private const float SWAY_SPEED_ACTIVE = 1f;
    private const float SWAY_SPEED_INACTIVE = 0f;
    private const float MOVE_AREA_MULTIPLIER = 10f;
    private const float POSITION_MULTIPLIER = 0.1f;
    private const int RANDOM_RANGE_MIN = -30;
    private const int RANDOM_RANGE_MAX = 30;
    private const float TARGET_X_LEFT = -15f;
    private const float TARGET_X_RIGHT = 15f;
    private const int SPEED_FAST = 5;
    private const int SPEED_SLOW = 2;
    #endregion

    #region Public Fields
    [Header("Swordfish Behavior")]
    public float range = 10f;
    public bool showVFX = true;

    [Header("VFX References")]
    public Transform vfx;
    public Transform vfxFlip;
    #endregion

    #region Private Fields
    [SerializeField]
    private int swordfishState = 0;
    private bool canAttack = false;
    private bool isAttacking = false;

    // Performance optimizations
    private List<FishController> attackedFishes = new List<FishController>();
    private System.Random random = new System.Random();
    private HomeUI cachedHomeUI;
    private Camera cachedCamera;

    // Attack optimization
    private float lastAttackCheckTime;
    private const float ATTACK_CHECK_INTERVAL = 0.1f; // Check for attacks 10 times per second instead of every frame
    #endregion

    #region Properties
    public int SwordfishState
    {
        get => swordfishState;
        private set => swordfishState = value;
    }

    public bool CanAttack => canAttack;
    public bool IsAttacking => isAttacking;
    #endregion

    #region Initialization
    private void Start()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (cachedCamera == null)
            cachedCamera = Camera.main;
        if (cachedHomeUI == null && GameManager.Instance != null)
            cachedHomeUI = GameManager.Instance.homeUI;
    }
    #endregion

    #region Health and Death Management
    public void DecreaseHPByClick()
    {
        if (state != FishState.Moving)
            return;

        float damage = fishConfig.fishCurrencyValue * fishConfig.percentDecrease / 100f;
        currentTotalTickValue -= damage;
        currentTotalTickValue = Mathf.Max(currentTotalTickValue, 0);

        UpdateHpBar();
        SetDamageColor();

        if (currentTotalTickValue <= 0)
        {
            HandleDeath();
        }
    }

    private void SetDamageColor()
    {
        SetColor(new Color32(255, 125, 125, 255));
    }

    private void HandleDeath()
    {
        state = FishState.Dead;
        _moveTween.Kill();
        FlipWithDirection(new Vector3(-1, -1, 1));

        var deathSequence = DOTween.Sequence();
        deathSequence.Append(_spriteRenderer.DOFade(0, FADE_DURATION));
        deathSequence.Join(transform.DOLocalMoveY(DEATH_MOVE_Y, FADE_DURATION));
        deathSequence.OnComplete(() => gameObject.SetActive(false));

        _spriteRenderer.material.SetFloat("_SwaySpeed", SWAY_SPEED_INACTIVE);
        FishManager.Instance.CheckWinLose();
        // InventoryManager.Instance.AddTickets(1);
        var ticket = Instantiate(Resources.Load<GameObject>("Prefabs/Ticket"));
        ticket.transform.position = transform.position;
        ticket.SetActive(true);
        ticket.GetComponent<CoinController>().value = 1;
        ticket.GetComponent<CoinController>().Active();
        ticket.GetComponent<CoinController>().OnComplete = () =>
        {
            InventoryManager.Instance.AddTickets(1);
            AudioManager.Instance.PlaySound("Coin");
            Destroy(ticket); // Clean up the ticket object after use
        };
        GamePlayTracking.Instance.AddByKey(GamePlayTracking.CURRENT_BOSSES, 1);
    }
    #endregion

    #region Update and Screen Management
    public override void Update()
    {
        HandleScreenIndicators();

        if (canAttack && Time.time >= lastAttackCheckTime + ATTACK_CHECK_INTERVAL)
        {
            lastAttackCheckTime = Time.time;
            CheckForAttackTargets();
        }
    }

    private void HandleScreenIndicators()
    {
        if (cachedCamera == null || cachedHomeUI == null)
        {
            CacheComponents();
            return;
        }

        Vector3 screenPosition = cachedCamera.WorldToViewportPoint(transform.position);

        bool isOffScreenLeft = screenPosition.x < SCREEN_LEFT_THRESHOLD;
        bool isOffScreenRight = screenPosition.x > SCREEN_RIGHT_THRESHOLD;

        if (isOffScreenLeft)
        {
            SetIndicatorActive(cachedHomeUI.IndicatorLeft, true);
            UpdateIndicatorPosition(cachedHomeUI.IndicatorLeft);
            SetIndicatorActive(cachedHomeUI.IndicatorRight, false);
        }
        else if (isOffScreenRight)
        {
            SetIndicatorActive(cachedHomeUI.IndicatorRight, true);
            UpdateIndicatorPosition(cachedHomeUI.IndicatorRight);
            SetIndicatorActive(cachedHomeUI.IndicatorLeft, false);
        }
        else
        {
            SetIndicatorActive(cachedHomeUI.IndicatorLeft, false);
            SetIndicatorActive(cachedHomeUI.IndicatorRight, false);
        }
    }

    private void SetIndicatorActive(RectTransform indicator, bool active)
    {
        if (indicator != null && indicator.gameObject.activeSelf != active)
        {
            indicator.gameObject.SetActive(active);
        }
    }

    private void UpdateIndicatorPosition(RectTransform indicator)
    {
        if (indicator != null)
        {
            var indicatorTransform = indicator.transform;
            indicatorTransform.position = new Vector3(
                indicatorTransform.position.x,
                transform.position.y,
                indicatorTransform.position.z
            );
        }
    }

    private void CheckForAttackTargets()
    {
        var hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(range, range),
            0,
            Vector2.zero
        );

        foreach (var hit in hits)
        {
            if (ShouldAttackFish(hit, out var fishController))
            {
                AttackFish(fishController);
            }
        }
    }

    private bool ShouldAttackFish(RaycastHit2D hit, out FishController fishController)
    {
        fishController = null;

        if (hit.collider == null || !hit.collider.CompareTag("Fish"))
            return false;

        fishController = hit.collider.GetComponent<FishController>();

        return fishController != null
            && fishController.state == FishState.Moving
            && !fishController.fishConfig.isBoss
            && !attackedFishes.Contains(fishController);
    }

    private void AttackFish(FishController fishController)
    {
        float damage =
            fishController.fishConfig.fishCurrencyValue * fishConfig.percentDecrease / 100f;
        fishController.TakeDamage(damage, transform);
        attackedFishes.Add(fishController);

        Debug.Log($"Swordfish attacked {fishController.name} for {damage} damage");
    }
    #endregion

    #region Fish Initialization and Setup
    public override void Init(FishConfig fishConfig, int index)
    {
        Debug.Log("Swordfish Init");
        this.fishConfig = fishConfig;

        SetupSortingOrders(index);
        InitializeState();
        CacheComponents();

        // Initialize the attacked fishes list
        attackedFishes.Clear();

        Move();

        currentTotalTickValue = fishConfig.fishCurrencyValue;
        SetupMaterial();
        _currentTargetItem = null;
    }

    private void SetupSortingOrders(int index)
    {
        int baseSortingOrder = INITIAL_SORTING_ORDER + index;
        _hpBarMask.sortingOrder = baseSortingOrder;
        hpBar.GetComponent<SpriteRenderer>().sortingOrder = baseSortingOrder + 1;
        _spriteMask.frontSortingOrder = baseSortingOrder + 2;
        _spriteMask.backSortingOrder = baseSortingOrder;
        SetLinesSortingOrder(baseSortingOrder + 2);
    }

    private void InitializeState()
    {
        currentTotalTickValue = 0;
        state = FishState.Moving;
        SetSprite(0);
        targetPosition = transform.position;
        SwordfishState = 0;
    }

    private void SetupMaterial()
    {
        _spriteRenderer.material.SetFloat("_SwaySpeed", SWAY_SPEED_ACTIVE);
        _spriteRenderer.material.SetColor("_Color", Color.white);
    }
    #endregion

    #region Movement and AI Logic
    public override void Move()
    {
        int previousState = SwordfishState;
        var movementData = CalculateMovement();

        ApplyMovementData(movementData);
        UpdateVFXState();
        ClearAttackedFishesOnStateChange(previousState);

        targetPosition = CalculateTargetPosition(movementData.targetX);
        base.Move();
    }

    private MovementData CalculateMovement()
    {
        return SwordfishState switch
        {
            0 => new MovementData(GetRandomX(), 1, SPEED_FAST, false),
            1 => new MovementData(GetRandomX(), 2, SPEED_SLOW, false),
            2 => new MovementData(GetRandomX(), 3, SPEED_SLOW, false),
            3 => new MovementData(GetRandomX(), 4, SPEED_SLOW, false),
            4 => new MovementData(GetRandomX(), 5, SPEED_SLOW, false),
            5 => new MovementData(TARGET_X_LEFT, 6, SPEED_FAST, false),
            6 => new MovementData(TARGET_X_RIGHT, 7, SPEED_FAST, true),
            7 => new MovementData(TARGET_X_LEFT, 8, SPEED_FAST, true),
            8 => new MovementData(TARGET_X_RIGHT, 0, SPEED_FAST, true),
            _ => new MovementData(transform.localPosition.x, 0, SPEED_FAST, false),
        };
    }

    private float GetRandomX()
    {
        return random.Next(RANDOM_RANGE_MIN, RANDOM_RANGE_MAX) * POSITION_MULTIPLIER;
    }

    private void ApplyMovementData(MovementData data)
    {
        SwordfishState = data.nextState;
        fishConfig.speed = data.speed;
        canAttack = data.canAttack;
        showVFX = data.canAttack;
    }

    private void UpdateVFXState()
    {
        if (!showVFX)
        {
            SetVFXActive(false, false);
            return;
        }

        bool facingLeft = _fishBody.transform.localScale.x < 0;
        SetVFXActive(facingLeft, !facingLeft);
    }

    private void SetVFXActive(bool vfxActive, bool vfxFlipActive)
    {
        if (vfx != null)
            vfx.gameObject.SetActive(vfxActive);
        if (vfxFlip != null)
            vfxFlip.gameObject.SetActive(vfxFlipActive);
    }

    private void ClearAttackedFishesOnStateChange(int previousState)
    {
        if (previousState != SwordfishState)
        {
            attackedFishes.Clear();
        }
    }

    private Vector3 CalculateTargetPosition(float targetX)
    {
        int moveArea = (int)(fishConfig.moveArea * MOVE_AREA_MULTIPLIER);
        moveArea = Mathf.Abs(moveArea);
        float randomY = fishConfig.depth + random.Next(-moveArea, moveArea) * POSITION_MULTIPLIER;
        return new Vector3(targetX, randomY, 0);
    }
    #endregion

    #region Helper Structures
    private struct MovementData
    {
        public readonly float targetX;
        public readonly int nextState;
        public readonly int speed;
        public readonly bool canAttack;

        public MovementData(float targetX, int nextState, int speed, bool canAttack)
        {
            this.targetX = targetX;
            this.nextState = nextState;
            this.speed = speed;
            this.canAttack = canAttack;
        }
    }
    #endregion

    #region Public Methods
    public void SetColor(Color color, float duration = 0.1f)
    {
        _spriteRenderer.DOComplete();
        _spriteRenderer
            .DOColor(color, duration)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() =>
            {
                _spriteRenderer.DOColor(Color.white, duration);
            });
    }

    public void StopAttack()
    {
        isAttacking = false;
        canAttack = false;
    }

    public override void OnClick()
    {
        Debug.Log("Swordfish OnClick");
        DecreaseHPByClick();
    }

    public override void CheckFull() { }

    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        attackedFishes?.Clear();
        _spriteRenderer?.DOKill();
        transform?.DOKill();
    }
    #endregion
}
