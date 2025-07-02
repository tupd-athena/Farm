using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Factory;
using UnityEngine;

public class SwordfishController : FishController
{
    public bool isAttacking = false;

    public int SwordfishState = 0;

    public float range = 10f;

    // List to track fishes that have been attacked by this swordfish
    private List<FishController> attackedFishes = new List<FishController>();

    public void DecreaseHPByClick()
    {
        // base.DecreaseHPByTime();
        if (state != FishState.Moving)
        {
            return;
        }
        currentTotalTickValue -= fishConfig.fishCurrencyValue * fishConfig.percentDecrease / 100;
        currentTotalTickValue = Mathf.Max(currentTotalTickValue, 0);
        UpdateHpBar();
        SetColor(new Color32(255, 125, 125, 255));
        if (currentTotalTickValue <= 0)
        {
            state = FishState.Dead;
            _moveTween.Kill();
            FlipWithDirection(new Vector3(-1, -1, 1));
            _spriteRenderer.DOFade(0, 3f);
            transform
                .DOLocalMoveY(-1, 3f)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            _spriteRenderer.material.SetFloat("_SwaySpeed", 0);
            FishManager.Instance.CheckWinLose();
        }
    }

    public override void Update()
    {
        //raycast to find fish
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(range, range),
            0,
            Vector2.zero
        );
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Fish"))
            {
                var fishController = hit.collider.GetComponent<FishController>();
                if (
                    fishController == null
                    || fishController.state == FishState.Dead
                    || fishController.fishConfig.isBoss
                )
                {
                    // Skip if the fish is null, dead, a boss, or already attacked
                    continue;
                }
                if (fishController.state != FishState.Moving)
                {
                    continue;
                }

                // Check if this fish has already been attacked
                if (attackedFishes.Contains(fishController))
                {
                    continue;
                }

                // Attack the fish and add it to the attacked list
                fishController.TakeDamage(
                    fishController.fishConfig.fishCurrencyValue * fishConfig.percentDecrease / 100
                );
                Debug.Log("Attack by Swordfish");
                attackedFishes.Add(fishController);
            }
        }
    }

    public void SetColor(Color color, float duration = 0.1f)
    {
        _spriteRenderer.DOComplete();
        _spriteRenderer
            .DOColor(color, duration)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() =>
            {
                _spriteRenderer.DOColor(new Color32(255, 255, 255, 255), duration);
            });
    }

    public override void Init(FishConfig fishConfig, int index)
    {
        Debug.Log("Swordfish Init");
        this.fishConfig = fishConfig;
        _hpBarMask.sortingOrder = 100 + index;
        hpBar.GetComponent<SpriteRenderer>().sortingOrder = 100 + index + 1;
        _spriteMask.frontSortingOrder = 100 + index + 2;
        _spriteMask.backSortingOrder = 100 + index;
        SetLinesSortingOrder(100 + index + 2);
        currentTotalTickValue = 0;
        state = FishState.Moving;
        SetSprite(0);
        targetPosition = transform.position;

        // Initialize the attacked fishes list
        attackedFishes.Clear();

        Move();
        currentTotalTickValue = fishConfig.fishCurrencyValue;
        _spriteRenderer.material.SetFloat("_SwaySpeed", 1);
        _spriteRenderer.material.SetColor("_Color", new Color32(255, 255, 255, 255));
        _currentTargetItem = null;
    }

    public override void Move()
    {
        System.Random random = new System.Random();
        float targetX = 0;
        int previousState = SwordfishState;

        switch (SwordfishState)
        {
            case 0: // Moving to left side
                targetX = -15;
                SwordfishState = 1;
                fishConfig.speed = 4;
                break;
            case 1: //Moving to right side
                targetX = 15;
                SwordfishState = 2;
                fishConfig.speed = 4;
                break;
            case 2: // Moving to left side
                targetX = -15;
                SwordfishState = 3;
                fishConfig.speed = 4;
                break;
            case 3: //Moving to right side
                targetX = 15;
                SwordfishState = 4;
                fishConfig.speed = 4;
                break;
            case 4:
                targetX = random.Next(-30, 30) * 0.1f;
                SwordfishState = 5;
                fishConfig.speed = 4;
                break;
            case 5:
                targetX = random.Next(-30, 30) * 0.1f;
                SwordfishState = 0;
                fishConfig.speed = 2;
                break;
            default:
                targetX = transform.localEulerAngles.z;
                break;
        }

        // Clear attacked fishes list when state changes
        if (previousState != SwordfishState)
        {
            attackedFishes.Clear();
        }

        if (targetPosition == transform.localPosition)
        {
            int moveArea = (int)(fishConfig.moveArea * 10);
            moveArea = Mathf.Abs(moveArea);
            float randomY = fishConfig.depth + random.Next(-moveArea, moveArea) * 0.1f;
            targetPosition = new Vector3(targetX, randomY, 0);
        }
        base.Move();
    }

    public void StopAttack()
    {
        isAttacking = false;
    }

    public override void OnClick()
    {
        // base.OnClick();
        Debug.Log("Squid OnClick");
        DecreaseHPByClick();
    }

    public override void CheckFull() { }
}
