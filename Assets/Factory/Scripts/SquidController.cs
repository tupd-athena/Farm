using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Factory;
using UnityEngine;

public class SquidController : FishController
{
    public Animator _animator;

    public bool isAttacking = false;

    public float attackTime = 15f;

    public List<SpriteRenderer> _spriteRenderers;

    public void DecreaseHPByClick()
    {
        // base.DecreaseHPByTime();
        if (state != FishState.Moving)
        {
            return;
        }
        currentTotalTickValue -= fishConfig.fishCurrencyValue * fishConfig.percentDecrease / 100;
        UpdateHpBar();
        SetColor(new Color32(255, 125, 125, 255));
        if (currentTotalTickValue < 0)
        {
            state = FishState.Dead;
            _moveTween.Kill();
            FlipWithDirection(new Vector3(-1, -1, 1));
            _spriteRenderer.DOFade(0, 3f);
            foreach (var spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.DOComplete();
                spriteRenderer.DOFade(0, 3f);
            }
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
        foreach (var spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.DOComplete();
            spriteRenderer
                .DOColor(color, duration)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    spriteRenderer.DOColor(new Color32(255, 255, 255, 255), 0.1f);
                });
        }
    }

    public override void Init(FishConfig fishConfig, int index)
    {
        Debug.Log("Squid Init");
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
        Move();
        currentTotalTickValue = fishConfig.fishCurrencyValue;
        _spriteRenderer.material.SetFloat("_SwaySpeed", 1);
        _spriteRenderer.material.SetColor("_Color", new Color32(255, 255, 255, 255));
        _currentTargetItem = null;
    }

    public void OnEnable()
    {
        isAttacking = true;
        Invoke(nameof(StopAttack), attackTime);
    }

    public override void Move()
    {
        System.Random random = new System.Random();
        if (!isAttacking)
        {
            isAttacking = true;
            _animator.Play("Attack");
            var particleSystem = PoolSystem.Instance.GetObject("Ink");
            particleSystem.transform.position = transform.position;
            var main = particleSystem.GetComponent<ParticleSystem>().main;
            particleSystem.SetActive(true);
            particleSystem.GetComponent<ParticleSystem>().Play();
            PoolSystem.Instance.ReturnObject(particleSystem, "Ink", attackTime);
            Invoke(nameof(StopAttack), attackTime);
            if (transform.localPosition.x + 5 > 4 || transform.localPosition.x - 5 < -4)
            {
                FlipWithDirection(new Vector3(_fishBody.localScale.x * -1, 1, 1));
            }
            FlipWithDirection(new Vector3(_fishBody.localScale.x * -1, 1, 1));
            transform
                .DOLocalMoveX(
                    transform.localPosition.x + 3 * -_fishBody.transform.localScale.x,
                    0.5f
                )
                .SetEase(Ease.InOutSine)
                .SetDelay(0.3f)
                .OnComplete(() =>
                {
                    float randomX = random.Next(-30, 30) * 0.1f;
                    int moveArea = (int)(fishConfig.moveArea * 10);
                    moveArea = Mathf.Abs(moveArea);
                    float randomY = fishConfig.depth + random.Next(-moveArea, moveArea) * 0.1f;
                    targetPosition = new Vector3(randomX, randomY, 0);
                    Move();
                });
            return;
        }
        if (targetPosition == transform.localPosition)
        {
            float randomX = random.Next(-30, 30) * 0.1f;
            int moveArea = (int)(fishConfig.moveArea * 10);
            moveArea = Mathf.Abs(moveArea);
            float randomY = fishConfig.depth + random.Next(-moveArea, moveArea) * 0.1f;
            targetPosition = new Vector3(randomX, randomY, 0);
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

    public override async Task FindTarget() { }

    public override void CheckFull() { }

    public override void Update() { }
}
