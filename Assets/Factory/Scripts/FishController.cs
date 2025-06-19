using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coffee.UIExtensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Factory
{
    [System.Serializable]
    public class FishTweenParams
    {
        [Header("Movement")]
        public float moveToPositionDuration = 0.2f;
        public Ease moveToPositionEase = Ease.InOutSine;

        [Header("Item Collection")]
        public float itemCollectScaleDuration = 0.2f;
        public float itemMoveToFishDuration = 0.2f;
        public Ease itemMoveToFishEase = Ease.InOutSine;

        [Header("Fish Growth")]
        public float fishGrowScaleDuration = 0.1f;
        public float fishGrowScaleMultiplier = 1.1f;

        [Header("Death Animation")]
        public float fishDeathRotateDuration = 0.1f;
        public float fishDeathMoveXDuration = 2.5f;
        public float fishDeathRotateAngle = 30f;
        public int fishDeathRotateLoops = 6;
        public float fishDeathMoveXPosition = -15f;
        public Ease fishDeathRotateEase = Ease.InSine;
        public Ease fishDeathMoveEase = Ease.InSine;
    }

    public class FishController : MonoBehaviour
    {
        [SerializeField]
        protected FishTweenParams tweenParams;

        public Canvas _canvas;
        public Transform _fishBody;

        [SerializeField]
        protected SpriteRenderer _hpBarMask;

        [SerializeField]
        protected SpriteRenderer _spriteRenderer;

        public SpriteMask _spriteMask;
        public FishState state = FishState.Idle;
        public FishConfig fishConfig;
        public float currentTotalTickValue = 0;

        public RectTransform _confuseVFX;
        public Transform _immortalVFX;

        public Transform hpBar;

        public Vector3 targetPosition;

        public Tween _moveTween;

        protected int _indexSprite;

        public List<GameObject> _lines;

        protected ItemController _currentTargetItem;
        public SpriteRenderer spriteRenderer => _spriteRenderer;

        public bool tempImmortal = false;

        protected bool _lockTarget = false;

        private Vector3 _originalScale;

        public void Awake()
        {
            _canvas = GetComponentInChildren<Canvas>();
            _currentTargetItem = null;
            if (_canvas != null)
            {
                _canvas.worldCamera = GameManager.Instance.mainCamera;
            }
        }

        public void KillAllTween()
        {
            _moveTween.Kill();
        }

        public void SetSprite(int index)
        {
            _indexSprite = index;
            _spriteRenderer.sprite = fishConfig.sprites[index].sprite;
        }

        public virtual void Init(FishConfig fishConfig, int index)
        {
            _hpBarMask.sortingOrder = 100 + index;
            hpBar.GetComponent<SpriteRenderer>().sortingOrder = 100 + index + 1;
            _spriteMask.frontSortingOrder = 100 + index + 2;
            _spriteMask.backSortingOrder = 100 + index;
            SetLinesSortingOrder(100 + index + 2);
            this.fishConfig = fishConfig;
            currentTotalTickValue = 0;
            state = FishState.Moving;
            SetSprite(0);
            _spriteRenderer.transform.localScale = new Vector3(
                _spriteRenderer.transform.localScale.x * fishConfig.size,
                _spriteRenderer.transform.localScale.y * fishConfig.size,
                _spriteRenderer.transform.localScale.z * fishConfig.size
            );
            targetPosition = transform.position;
            Move();
            Debug.Log("Init: " + fishConfig.fishPrefabName);
            currentTotalTickValue = fishConfig.fishCurrencyValue * 0.5f;
            _spriteRenderer.material.SetFloat("_SwaySpeed", 1);
            _spriteRenderer.material.SetColor("_Color", new Color32(255, 255, 255, 255));
            _currentTargetItem = null;
            _hpBarMask.gameObject.SetActive(true);
            _originalScale = _spriteRenderer.transform.localScale;

            UpdateHpBar();
            if (fishConfig.isBoss)
            {
                return;
            }
            Invoke(nameof(DecreaseHPByTime), 1f);
        }

        protected void SetLinesSortingOrder(int index)
        {
            foreach (var line in _lines)
            {
                line.GetComponent<SpriteRenderer>().sortingOrder = index;
            }
            if (_immortalVFX != null)
            {
                _immortalVFX.GetComponent<ParticleSystemRenderer>().sortingOrder = index;
            }
        }

        public void UpdateHpBar()
        {
            float percent01 = currentTotalTickValue / fishConfig.fishCurrencyValue;
            hpBar.localScale = new Vector3(percent01 * 3.72f, 1, 1);
            if (percent01 > 0.2f)
            {
                hpBar.GetComponent<SpriteRenderer>().color = new Color32(160, 200, 0, 255);
                SetSprite(0);
            }
            else
            {
                hpBar.GetComponent<SpriteRenderer>().color = new Color32(210, 50, 20, 255);
                _spriteRenderer.material.DOComplete();
                _spriteRenderer
                    .material.DOColor(new Color32(255, 125, 125, 255), 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        _spriteRenderer.material.DOColor(new Color32(255, 255, 255, 255), 0.1f);
                    });
                // SetSprite(1);
            }
        }

        public virtual void DecreaseHPByTime()
        {
            if (!tempImmortal)
            {
                currentTotalTickValue -=
                    fishConfig.fishCurrencyValue * fishConfig.percentDecrease / 100;
            }

            UpdateHpBar();
            if (currentTotalTickValue < 0)
            {
                state = FishState.Dead;
                _moveTween.Kill();
                FlipWithDirection(new Vector3(-1, -1, 1));
                _hpBarMask.gameObject.SetActive(false);
                _spriteRenderer.material.DOFade(0, 3f);
                transform
                    .DOMoveY(-0.5f, 3f)
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                    });
                _spriteRenderer.material.SetFloat("_SwaySpeed", 0);
                FishManager.Instance.CheckWinLose();
            }
            if (state == FishState.Dead)
            {
                return;
            }
            Invoke(nameof(DecreaseHPByTime), 1f);
        }

        public virtual async Task FindTarget()
        {
            if (state != FishState.Moving || fishConfig.isBoss)
            {
                return;
            }
            if (
                _currentTargetItem == null
                || _currentTargetItem.isCollected
                || !_currentTargetItem.dropCompleted
                || !_currentTargetItem.gameObject.activeSelf
            )
            {
                _currentTargetItem = null;
            }

            // Use raycast to find items in range
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 2.5f, Vector2.zero);
            // Find the closest collectible item
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("Item"))
                {
                    ItemController item = hit.collider.GetComponent<ItemController>();
                    if (!item.isInWater || !item.canCollect)
                    {
                        continue;
                    }
                    if (
                        item != null
                        && item.dropCompleted
                        && !item.isCollected
                        && item.gameObject.activeSelf
                    )
                    {
                        if (Vector3.Distance(transform.position, item.transform.position) < 0.5f)
                        {
                            Debug.Log("Collect Item");
                            if (
                                item.gameObject.activeSelf == false
                                || string.IsNullOrEmpty(item.itemData.itemName)
                            )
                            {
                                return;
                            }
                            _spriteRenderer
                                .transform.DOScale(
                                    _spriteRenderer.transform.localScale
                                        * tweenParams.fishGrowScaleMultiplier,
                                    tweenParams.fishGrowScaleDuration
                                )
                                .SetLoops(2, LoopType.Yoyo);
                            Eat(item);
                        }
                        else
                        {
                            if (_lockTarget)
                            {
                                if (_currentTargetItem != null && _currentTargetItem != item)
                                {
                                    continue;
                                }
                            }
                            _currentTargetItem = item;
                            targetPosition = item.transform.localPosition;
                            _lockTarget = true;
                            Move();
                        }
                        break;
                    }
                    else
                    {
                        _currentTargetItem = null;
                    }
                }
            }
        }

        public void Eat(ItemController item)
        {
            item.isCollected = true;
            GameManager.Instance.CollectItem(item);
            if(state != FishState.Moving)
            {
                return;
            }
            currentTotalTickValue += item.itemData.cost;
            AudioManager.Instance.PlaySound("Eat");
            UpdateHpBar();
            CheckFull();
            CheckSpecialItem(item.itemData);
            _lockTarget = false;
        }

        public void CheckSpecialItem(ItemData itemData)
        {
            switch (itemData.itemName)
            {
                default:
                    break;
            }
        }

        public void EnableTempImmortal(GearData gearData)
        {
            tempImmortal = true;
            if (gearData == null)
            {
                return;
            }
            var duration = gearData.customValues.Find(x => x.id == "Duration").customValue;
            var main = _immortalVFX.GetComponent<ParticleSystem>().main;
            if (main.duration != Mathf.Max(duration - 0.5f, 0.5f))
            {
                main.duration = Mathf.Max(duration - 0.5f, 0.5f);
            }
            _immortalVFX.gameObject.SetActive(true);
            _immortalVFX.GetComponent<ParticleSystem>().Play();
            Invoke(nameof(ResetTempImmortal), duration);
        }

        public void ResetTempImmortal()
        {
            tempImmortal = false;
            _immortalVFX.gameObject.SetActive(false);
        }

        public virtual void CheckFull()
        {
            if (currentTotalTickValue >= fishConfig.fishCurrencyValue)
            {
                state = FishState.Full;
                _moveTween.Kill();
                FlipWithDirection(new Vector3(-1, 1, 1));
                transform.DOLocalMove(
                    new Vector3(-15, 0, 0),
                    Vector2.Distance(transform.localPosition, new Vector3(-15, 0, 0)) / 2
                );
                _spriteRenderer.material.SetFloat("_SwaySpeed", 3);
                var happyVFX = PoolSystem.Instance.GetObject("HappyVFX");
                happyVFX.transform.localPosition = transform.position;
                happyVFX.SetActive(true);
                PoolSystem.Instance.ReturnObject(happyVFX, "HappyVFX", 1f);
                AudioManager.Instance.PlaySound("Full");

                var coin = PoolSystem.Instance.GetObject("Coin");
                coin.transform.position = transform.position;
                coin.SetActive(true);
                coin.GetComponent<CoinController>().value = fishConfig.dropCoinValue;

                CancelInvoke(nameof(DecreaseHPByTime));
                FishManager.Instance.CheckWinLose();
            }
            else
            {
                targetPosition = transform.position;
                Move();
            }
        }

        public virtual void Update()
        {
            if (state == FishState.Moving)
            {
                FindTarget();
                ScaleFish();
            }
        }

        private void ScaleFish()
        {
            float value = currentTotalTickValue / fishConfig.fishCurrencyValue - 0.8f;
            if (value > 0)
            {
                _spriteRenderer.transform.localScale = _originalScale * 1.2f;
            }
            else
            {
                _spriteRenderer.transform.localScale = _originalScale;
            }
        }

        public virtual void OnClick()
        {
            if (state == FishState.Moving)
            {
                _currentTargetItem = null;
                targetPosition = transform.position;
                Move();
                Debug.Log("OnClick");
                Confuse();
            }
        }

        public void Confuse()
        {
            System.Random random = new System.Random();
            int randomInt = random.Next(0, 100);
            if (randomInt < 50)
            {
                _confuseVFX.transform.DOComplete();
                _confuseVFX.transform.localScale = Vector3.zero;
                _confuseVFX.gameObject.SetActive(true);
                _confuseVFX
                    .transform.DOScale(1, 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        _confuseVFX.gameObject.SetActive(false);
                    });
            }
        }

        public virtual void Move()
        {
            if (state == FishState.Dead || state == FishState.Full)
            {
                return;
            }
            _moveTween.Kill();
            _moveTween = null;
            if (targetPosition == transform.localPosition)
            {
                System.Random random = new System.Random();
                float randomX = random.Next(-30, 30) * 0.1f;
                int moveArea = (int)(fishConfig.moveArea * 10);
                Debug.Log("MoveArea: " + moveArea);
                moveArea = Mathf.Abs(moveArea);
                float randomY = fishConfig.depth + random.Next(-moveArea, moveArea) * 0.1f;
                targetPosition = new Vector3(randomX, randomY, 0);
            }
            float distance = Vector3.Distance(transform.localPosition, targetPosition);
            float time = distance / fishConfig.speed;
            time *= _currentTargetItem == null ? 1 : 0.75f;
            Vector3 direction = targetPosition - transform.localPosition;
            Vector3 outputDirection = new Vector3(direction.x >= 0 ? 1 : -1, 1, 1);
            FlipWithDirection(outputDirection);
            _moveTween = transform
                .DOLocalMove(targetPosition, time)
                .SetEase(tweenParams.moveToPositionEase)
                .OnComplete(() =>
                {
                    if (state == FishState.Moving)
                    {
                        _lockTarget = false;
                        Move();
                    }
                });
        }

        public void FlipWithDirection(Vector3 direction)
        {
            //using transform
            _fishBody.localScale = direction;
        }
    }

    [System.Serializable]
    public class FishSpriteByHP
    {
        public int percentHP;
        public Sprite sprite;
    }

    public enum FishState
    {
        Idle,
        Moving,
        Attacking,
        Dead,
        Full,
        Confused,
    }
}
