using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Factory
{
    public class ItemController : MonoBehaviour
    {
        public bool mergeable = false;
        public bool isCollected = false;

        public Rigidbody2D rb;

        [SerializeField]
        private SpriteRenderer _itemIcon;

        public ItemData itemData;

        public bool dropCompleted = false;

        public Tween moveTween;

        public AnimationCurve leafDropCurve;

        public bool isInWater = false;

        private List<Task> _asyncTasks = new List<Task>();

        public bool canCollect = false;
        public ParticleSystem waterSplashVFX;
        public System.Action OnDropToSurface;
        public System.Action OnSpawn;

        private List<GameObject> _objectsToDestroy = new List<GameObject>();

        public float existTime = 10f;

        private void KillAllTweens()
        {
            // Kill any DOTween animations on the item icon
            _itemIcon.DOKill();

            // Kill any material tweens
            var material = GetComponent<SpriteRenderer>().material;
            material.DOKill();

            // Kill any transform tweens
            transform.DOKill();

            // Kill any move tween
            moveTween?.Kill();

            //remove all async await
            foreach (var task in _asyncTasks)
            {
                if (!task.IsCompleted && task.Status != TaskStatus.RanToCompletion) { }
            }
            _asyncTasks.Clear();
        }

        void OnEnable()
        {
            // Reset all states
            isCollected = false;
            mergeable = false;
            dropCompleted = false;
            isInWater = false;

            rb = GetComponent<Rigidbody2D>();

            // Kill all active tweens
            KillAllTweens();

            // Cancel all pending invokes
            CancelInvoke(nameof(SetMergeable));
            CancelInvoke(nameof(SetGravityInAir));
            CancelInvoke(nameof(SetGravityInLiquid));
            CancelInvoke(nameof(CollectItem));

            // Reset constraints and visibility
            UnfreezeConstrain();
            _itemIcon.color = new Color(1, 1, 1, 1);
            _itemIcon.gameObject.SetActive(true);
            // SetGravityInAir();
            OnDropToSurface = null;
            OnSpawn = null;
        }

        void OnDisable()
        {
            KillAllTweens();
            // Clear the list of objects to destroy
            foreach (var obj in _objectsToDestroy)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }

        private void SetMergeable()
        {
            mergeable = true;
        }

        private void SetGravityInAir()
        {
            dropCompleted = false;
            GetComponent<Collider2D>().isTrigger = false;
            GetComponent<Rigidbody2D>().gravityScale = 1f;
        }

        protected virtual async Task SetGravityInLiquid()
        {
            var delayTask = Task.Delay(2000);
            _asyncTasks.Add(delayTask);
            await delayTask;
            GetComponent<Rigidbody2D>().gravityScale = 0f; // Reduced gravity for slower fall
            GetComponent<Collider2D>().isTrigger = true;

            // Kill any existing move tween
            moveTween?.Kill();
            transform
                .DORotate(new Vector3(0, 0, 90), 4f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo);
            DOVirtual.DelayedCall(
                1f,
                () =>
                {
                    isInWater = true;
                    canCollect = true;
                }
            );
            if (itemData.dropType == DropType.Leaf)
            {
                var leafTask = SetLeafDrop();
                _asyncTasks.Add(leafTask);
                await leafTask;
            }
            else if (itemData.dropType == DropType.Standard)
            {
                var standardTask = SetStandardDrop();
                _asyncTasks.Add(standardTask);
                await standardTask;
            }
        }

        public async Task SetStandardDrop()
        {
            moveTween = null;
            transform
                .DOMoveY(GameManager.Instance.GetBottomYWithOffset(), 15f / itemData.dropSpeed)
                .OnComplete(async () =>
                {
                    var delayTask = Task.Delay(1000);
                    _asyncTasks.Add(delayTask);
                    await delayTask;
                    var collectTask = CollectItem();
                    _asyncTasks.Add(collectTask);
                    await collectTask;
                });
        }

        public async Task SetLeafDrop()
        {
            bool moveLeft = Random.Range(0, 2) == 0;

            // Create the zigzag sequence
            moveTween = transform
                .DOMoveY(GameManager.Instance.GetBottomYWithOffset(), 15f / itemData.dropSpeed)
                .OnComplete(async () =>
                {
                    var delayTask = Task.Delay(1000);
                    _asyncTasks.Add(delayTask);
                    await delayTask;
                    var collectTask = CollectItem();
                    _asyncTasks.Add(collectTask);
                    await collectTask;
                });
            for (int i = 0; i < 5; i++)
            {
                if (transform.localPosition.y <= GameManager.Instance.GetBottomYWithOffset() * 0.8f)
                {
                    moveTween.Kill();
                    var delayTask = Task.Delay(1000);
                    _asyncTasks.Add(delayTask);
                    await delayTask;
                    var collectTask = CollectItem();
                    _asyncTasks.Add(collectTask);
                    await collectTask;
                    return;
                }
                var moveTask = transform
                    .DOLocalMoveX(transform.localPosition.x + (moveLeft ? -0.5f : 0.5f), 1.5f)
                    .SetLoops(2, LoopType.Yoyo)
                    .AsyncWaitForCompletion();
                _asyncTasks.Add(moveTask);
                await moveTask;
                moveLeft = !moveLeft;
            }
        }

        public async Task CollectItem()
        {
            if (isCollected)
            {
                return;
            }
            var delayTask = Task.Delay(2000);
            _asyncTasks.Add(delayTask);
            await delayTask;
            if (isCollected)
            {
                return;
            }
            isCollected = true;
            var fadeTask = _itemIcon.DOFade(0, 1f).AsyncWaitForCompletion();
            _asyncTasks.Add(fadeTask);
            await fadeTask;
            GameManager.Instance.CollectItem(this);
        }

        public void SetItemData(ItemData itemData, float cost = 0)
        {
            this.itemData.Copy(itemData);
            existTime = 0;
            if (cost > 0)
            {
                this.itemData.cost = cost;
            }
            UpdateItem();
            AddSpecialComponent();
            OnSpawn?.Invoke();
        }

        public void AddSpecialComponent()
        {
            OnSpawn += SetGravityInAir;
            switch (itemData.itemName)
            {
                case "HomingBait":
                    var gearData0 = GameManager.Instance.GetGearDataByID(itemData.gearId);
                    if (!isInWater)
                    {
                        if (GetComponent<HomingBait>() == null)
                        {
                            gameObject.AddComponent<HomingBait>();
                        }
                        OnDropToSurface = null;
                        OnDropToSurface += () =>
                        {
                            GetComponent<HomingBait>().Active();
                        };
                        GetComponent<HomingBait>().enabled = true;
                        GetComponent<HomingBait>().itemController = this;
                        var pudding = Instantiate(
                            Resources.Load<GameObject>("Prefabs/Item/Pudding"),
                            transform
                        );
                        _objectsToDestroy.Add(pudding);
                        SetSprite(Resources.Load<Sprite>("Sprites/" + gearData0.iconName));
                        _itemIcon.gameObject.SetActive(false);
                        FreezeConstrain();
                        canCollect = false;
                    }
                    else
                    {
                        SetSprite(Resources.Load<Sprite>("Sprites/" + itemData.iconName));
                    }
                    break;
                case "HeartBait":
                    if (GetComponent<HeartBait>() == null)
                    {
                        gameObject.AddComponent<HeartBait>();
                    }
                    GetComponent<HeartBait>().enabled = true;
                    GetComponent<HeartBait>().itemController = this;
                    OnDropToSurface = null;
                    OnDropToSurface += () =>
                    {
                        GetComponent<HeartBait>().Active();
                    };
                    canCollect = false;
                    break;
                case "TheTwin":
                    if (GetComponent<TheTwinFood>() == null)
                    {
                        gameObject.AddComponent<TheTwinFood>();
                    }
                    GetComponent<TheTwinFood>().enabled = true;
                    GetComponent<TheTwinFood>().itemController = this;
                    var gearData1 = GameManager.Instance.GetGearDataByID(itemData.gearId);
                    if (!isInWater)
                    {
                        SetSprite(Resources.Load<Sprite>("Sprites/" + gearData1.iconName));
                    }
                    OnDropToSurface = null;
                    OnDropToSurface += () =>
                    {
                        SetSprite(Resources.Load<Sprite>("Sprites/" + itemData.iconName));
                        GetComponent<TheTwinFood>().Active();
                    };
                    canCollect = false;
                    break;
                case "TheTripple":
                    var gearData2 = GameManager.Instance.GetGearDataByID(itemData.gearId);
                    if (!isInWater)
                    {
                        OnDropToSurface = null;
                        OnDropToSurface += () =>
                        {
                            GetComponent<TheTrippleFood>().Active();
                        };
                        if (GetComponent<TheTrippleFood>() == null)
                        {
                            gameObject.AddComponent<TheTrippleFood>();
                        }
                        GetComponent<TheTrippleFood>().enabled = true;
                        GetComponent<TheTrippleFood>().itemController = this;
                        var pudding = Instantiate(
                            Resources.Load<GameObject>("Prefabs/Item/Pudding"),
                            transform
                        );
                        _objectsToDestroy.Add(pudding);
                        SetSprite(Resources.Load<Sprite>("Sprites/" + gearData2.iconName));
                        _itemIcon.gameObject.SetActive(false);
                        FreezeConstrain();
                        canCollect = false;
                    }
                    else
                    {
                        SetSprite(Resources.Load<Sprite>("Sprites/" + itemData.iconName));
                    }
                    break;
                default:
                    OnDropToSurface = null;
                    OnDropToSurface += () =>
                    {
                        SetGravityInLiquid();
                    };
                    break;
            }
        }

        public void UpdateItem()
        {
            if (itemData == null)
            {
                return;
            }
            var sprite = Resources.Load<Sprite>("Sprites/" + itemData.iconName);
            if (sprite != null)
            {
                _itemIcon.sprite = sprite;
            }
            transform.localScale = Vector3.one * itemData.size;
        }

        public void SetSprite(Sprite sprite)
        {
            if (_itemIcon == null)
            {
                _itemIcon = GetComponent<SpriteRenderer>();
            }
            _itemIcon.sprite = sprite;
        }

        public void Dissolve()
        {
            var material = GetComponent<SpriteRenderer>().material;
            material.SetFloat("_Dissolve", 1f);
            material
                .DOFloat(0f, "_Dissolve", 1f)
                .SetEase(Ease.InExpo)
                .OnComplete(() =>
                {
                    _itemIcon.DOKill();
                    _itemIcon
                        .DOColor(new Color(0, 0.5f, 1, 0), 1f)
                        .SetEase(Ease.InExpo)
                        .OnComplete(() =>
                        {
                            GameManager.Instance.CollectItem(this);
                        });
                });
        }

        public void Clear()
        {
            // Clear the list of objects to destroy
            if(GetComponent<HomingBait>() != null)
            {
                Destroy(GetComponent<HomingBait>());
            }
            if(GetComponent<HeartBait>() != null)
            {
                Destroy(GetComponent<HeartBait>());
            }
            if(GetComponent<TheTwinFood>() != null)
            {
                Destroy(GetComponent<TheTwinFood>());
            }
            if(GetComponent<TheTrippleFood>() != null)
            {
                Destroy(GetComponent<TheTrippleFood>());
            }
            
            foreach (var obj in _objectsToDestroy)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            _objectsToDestroy.Clear();
        }

        void FixedUpdate()
        {
            existTime += Time.fixedDeltaTime;
            if (existTime >= 20)
            {
                GameManager.Instance.CollectItem(this);
                return;
            }
            if (!dropCompleted)
            {
                UsingRaycast();
            }
            // if (transform.position.y <= -5 && GetComponent<Collider2D>().isTrigger)
            // {
            //     GetComponent<Rigidbody2D>().gravityScale = 0f;
            //     GetComponent<Collider2D>().isTrigger = false;
            //     CollectItem();
            // }
        }

        void FreezeConstrain()
        {
            // GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        void UnfreezeConstrain()
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }

        void UsingRaycast()
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                transform.position,
                Vector2.down,
                itemData.size * 0.3f
            );
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    if (hit.collider.CompareTag("Liquid") && !dropCompleted)
                    {
                        OnDropToSurface?.Invoke();
                        if (dropCompleted)
                        {
                            return;
                        }
                        dropCompleted = true;
                        waterSplashVFX?.Play();
                        // FreezeConstrain();
                    }
                    if (hit.collider.CompareTag("Item") && !isCollected && mergeable)
                    {
                        var item = hit.collider.GetComponent<ItemController>();
                        var outputItemId = GetOutputItemId(item.itemData);
                        var outputItemData = GameManager.Instance.GetItemDataByItemID(outputItemId);
                        if (outputItemData == null)
                        {
                            return;
                        }
                        var outputItemDataCopy = new ItemData();
                        outputItemDataCopy.Copy(outputItemData);
                        // item.Dissolve();
                        item.isCollected = true;
                        GameManager.Instance.CollectItem(item);
                        outputItemDataCopy.cost =
                            (item.itemData.cost + itemData.cost) * outputItemData.cost;
                        SetItemData(outputItemDataCopy);
                        transform
                            .DOScale(transform.localScale * 1.5f, 0.1f)
                            .SetLoops(2, LoopType.Yoyo);
                        return;
                    }
                }
            }
        }

        public int GetOutputItemId(ItemData checkItemData)
        {
            if (checkItemData == null)
            {
                return -1;
            }
            foreach (var mergeableItem in this.itemData.mergeableItems)
            {
                if (mergeableItem.itemId == checkItemData.itemId)
                {
                    return mergeableItem.outputItemId;
                }
            }
            return -1;
        }
    }
}
