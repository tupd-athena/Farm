using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Factory
{
    public class GearController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        protected Image _gearIcon;

        public List<Sprite> _textGearIconsPerLevel = new List<Sprite>();

        [SerializeField]
        protected Image _gearItemIcon;

        [SerializeField]
        protected Image _gearItemIconBG;

        [SerializeField]
        protected RectTransform _gui;

        [SerializeField]
        protected TMP_Text _levelText;

        [SerializeField]
        protected Sprite _gear1;

        [SerializeField]
        protected Sprite _gear6ForItemIcon;

        public Vector2 gridCoordinate;

        public List<GearNeighbor> connectedGears = new List<GearNeighbor>();

        public int direction = 0;
        public float angle = 90;

        public float startAngle = 0;
        public bool isActive = false;
        public bool isHead = false;
        public bool isReverse = false;
        public bool isStop = false;
        public bool isNotAddTickValue = false;

        public bool isInShop = false;

        public System.Action<float> OnRotate;
        public System.Action<GearData> OnDropShop;

        protected GearController _tempGear;
        protected Canvas _canvas;
        protected RectTransform _rectTransform;

        public GearData gearData;
        public ItemData itemData;
        public float currentTotalTickValue = 0;

        public System.Action OnStopRotate;
        public System.Action OnStartRotate;
        public System.Action OnFillComplete;

        public System.Action OnDestroy;

        private Vector3 _originalScale;
        public TMP_Text LevelText => _levelText;

        protected void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _gearItemIcon.material = new Material(_gearItemIcon.material);
            _gearItemIcon.material.SetFloat("_Fill", 1f);
            _gearItemIconBG.material = new Material(_gearItemIconBG.material);
            _gearItemIconBG.material.SetFloat("_Fill", 1f);
            OnDropShop += (gearData) =>
            {
                Debug.Log("Default OnDropShop: " + (int)gearData.cost);
            };
        }

        public void FillItemIcon(float fillAmount)
        {
            _gearItemIcon?.material?.SetFloat("_Fill", fillAmount);
            // _gearItemIconBG?.material?.SetFloat("_Fill", fillAmount);
        }

        public void Show()
        {
            isActive = true;
            _gui.gameObject.SetActive(true);
            _gearIcon.transform.localEulerAngles = new Vector3(0, 0, startAngle);
        }

        public void Hide()
        {
            isActive = false;
            _gui.gameObject.SetActive(false);
            gearData = null;
            itemData = null;
            OnDestroy?.Invoke();
        }

        public void SetGearData(GearData data)
        {
            currentTotalTickValue = 0;
            FillItemIcon(1);
            if (gearData == null)
            {
                gearData = new GearData();
            }
            if (data == null)
            {
                return;
            }
            gearData.Copy(data);
            SetSizeDeltaIcon();
            if (gearData.gearTypes.Contains(GearType.Text))
            {
                _levelText.gameObject.SetActive(true);
                _gearItemIcon.gameObject.SetActive(false);
                SetTextGear();
                SetGear(gearData.gearTypes);
            }
            else if (gearData.gearTypes.Contains(GearType.Image))
            {
                _levelText.gameObject.SetActive(false);
                _gearItemIcon.gameObject.SetActive(true);
                var sprite = Resources.Load<Sprite>("Sprites/" + data.iconName);
                if (sprite != null)
                {
                    _gearItemIcon.sprite = sprite;
                    SetGear(gearData.gearTypes);
                }
                else
                {
                    _gearItemIcon.sprite = null;
                    SetGear(gearData.gearTypes);
                }
            }
            AddSpecialGear(data);
            _originalScale = transform.localScale;
        }

        public void SetSizeDeltaIcon()
        {
            _gearItemIcon.rectTransform.sizeDelta = new Vector2(gearData.size, gearData.size);
        }

        public void SetTextGear()
        {
            System.Random random = new System.Random();
            switch (gearData.itemName)
            {
                case "Multiplier":
                    var index = random.Next(0, gearData.customValues.Count);
                    var value = gearData.customValues[index].customValue;
                    if (gearData.baseValue != 1)
                    {
                        value = gearData.baseValue;
                    }
                    _levelText.text = "x" + value.ToString();
                    gearData.baseValue = value;
                    break;
                default:
                    _levelText.text = gearData.level.ToString();
                    break;
            }
        }

        public void DisableAllSpecialComponets()
        {
            if (GetComponent<SpeedUpGear>() != null)
            {
                Destroy(GetComponent<SpeedUpGear>());
            }
            if (GetComponent<HomingBait>() != null)
            {
                Destroy(GetComponent<HomingBait>());
            }
        }

        public void AddSpecialGear(GearData data)
        {
            DisableAllSpecialComponets();
            switch (data.itemName)
            {
                case "SpeedUP":
                    gameObject.AddComponent<SpeedUpGear>();
                    break;
                case "Dopamine":
                    OnFillComplete += () =>
                    {
                        FishManager.Instance.UseDopamine(gearData);
                    };
                    break;
                default:
                    break;
            }
        }

        public void SetItemData(GearData data)
        {
            ItemData item = GameManager.Instance.GetItemDataByGearID(data.id);
            if (item != null)
            {
                itemData = new ItemData();
                itemData.Copy(item);
            }
            else
            {
                itemData = null;
            }
        }

        public void SetGear(List<GearType> gearTypes)
        {
            if (gearTypes.Contains(GearType.HeadGear) || isHead)
            {
                _gearIcon.sprite = _gear1;
                return;
            }
            if (gearTypes.Contains(GearType.Text))
            {
                UpdateGearIconLevel();
            }
            else if (gearTypes.Contains(GearType.Image))
            {
                _gearIcon.sprite = gearData.gearBaseColor.sprite;
            }
        }

        public void Connect(GearController gear, int direction)
        {
            connectedGears.Add(new GearNeighbor { gear = gear, direction = direction });
        }

        public void Disconnect(GearController gear, int direction)
        {
            connectedGears.Remove(
                connectedGears.Find(g => g.gear == gear && g.direction == direction)
            );
        }

        public void Rotate() //Only for head gear
        {
            if (isStop)
            {
                return;
            }
            if (isHead)
            {
                _gearIcon.transform.DOKill();
            }

            Ease ease = Ease.Linear;
            if (connectedGears.Exists(x => x.direction == direction && x.gear.isActive))
            {
                ease = Ease.InSine;
            }
            float multiplier = 1;
            float multiplierBySpeedUp = CustomValueManager.Instance.GetCustomValueInGame(
                CustomValueManager.MULTIPLIER_HEAD_GEAR_BY_SPEEDUP
            );
            multiplierBySpeedUp = multiplierBySpeedUp == 0 ? 1 : multiplierBySpeedUp;
            float multiplierByScaredTotem = CustomValueManager.Instance.GetCustomValueInGame(
                CustomValueManager.MULTIPLIER_HEAD_GEAR_BY_SCARED_TOTEM
            );
            if (multiplierBySpeedUp > 1)
            {
                GetComponent<GearEffectComponent>().PlayEffect("Electric");
            }
            else
            {
                GetComponent<GearEffectComponent>().StopEffect("Electric");
            }
            multiplier = multiplierBySpeedUp + multiplierByScaredTotem;
            _gearIcon
                .transform.DORotate(
                    new Vector3(0, 0, angle * ((direction + 1) % 4)),
                    0.5f / multiplier
                )
                .SetEase(ease)
                .OnComplete(() =>
                {
                    Rotate();
                    NeighborRotate();
                });
        }

        public void UpdateGearIconLevel()
        {
            if (gearData == null)
            {
                _gearIcon.sprite = GameManager.Instance.GetGearBaseColorSprite(
                    GearBaseColorType.TEXT1
                );
                Debug.LogWarning("Gear data is null or not enough icons for levels.");
                return;
            }
            switch (gearData.level)
            {
                case 1:
                    _gearIcon.sprite = GameManager.Instance.GetGearBaseColorSprite(
                        GearBaseColorType.TEXT1
                    );
                    break;
                case 2:
                    _gearIcon.sprite = GameManager.Instance.GetGearBaseColorSprite(
                        GearBaseColorType.TEXT2
                    );
                    break;
                default:
                    _gearIcon.sprite = GameManager.Instance.GetGearBaseColorSprite(
                        GearBaseColorType.TEXT3
                    );
                    break;
            }
        }

        public virtual void Rotate(float angle, float Amplifier, float Multiplier)
        {
            if (isStop)
            {
                return;
            }
            _gearIcon.transform.DOComplete();
            _gearIcon
                .transform.DORotate(new Vector3(0, 0, angle + startAngle), 0.1f)
                .OnComplete(() =>
                {
                    UpdateRotationProgress(Amplifier, Multiplier);
                });
        }

        public virtual void UpdateRotationProgress(float Amplifier, float Multiplier)
        {
            _gearIcon.transform.localEulerAngles = new Vector3(0, 0, startAngle);
            if (gearData == null || gearData.id == 0 || isHead || isStop)
            {
                return;
            }
            if (GameManager.Instance.GameState.CurrentState != GameStateType.Main)
                return;
            float AmplifierToTick = Amplifier >= gearData.maxValue ? 0 : Amplifier;
            float AmplifierToCost =
                Amplifier >= gearData.maxValue ? Amplifier - gearData.maxValue : 0;
            int bonus = (int)(AmplifierToTick / gearData.maxValue);
            var tickValue = gearData.tickValue + (bonus >= 1 ? 0 : AmplifierToTick);
            tickValue *= Multiplier;
            if (isNotAddTickValue)
            {
                tickValue = 0;
                AmplifierToCost = 0;
                return;
            }
            currentTotalTickValue +=
                (
                    tickValue
                    + CustomValueManager.Instance.GetCustomValueInGame(
                        CustomValueManager.ADD_TICK_VALUE
                    )
                )
                * (
                    CustomValueManager.Instance.GetCustomValueInGame(
                        CustomValueManager.MULTIPLIER_TICK_VALUE
                    ) + 1
                );
            if (currentTotalTickValue >= gearData.maxValue)
            {
                int rotationCount = (int)(currentTotalTickValue / gearData.maxValue) + bonus;
                currentTotalTickValue = currentTotalTickValue % gearData.maxValue;
                OnFillComplete?.Invoke();
                StartCoroutine(InvokeRotateWithDelay(rotationCount, AmplifierToCost));
            }
            FillItemIcon(currentTotalTickValue / gearData.maxValue);
        }

        public IEnumerator InvokeRotateWithDelay(int rotationCount, float Amplifier)
        {
            for (int i = 0; i < rotationCount; i++)
            {
                yield return new WaitForSeconds(0.1f);
                OnRotate?.Invoke(Amplifier);
            }
        }

        public void NeighborRotate()
        {
            List<GearController> allConnectedGears = FindAllConnectedGearsOnBoard();
            direction = (direction + 1) % 4;
            float Amplifier = 0;
            float Multiplier = 1;

            foreach (var gear in allConnectedGears)
            {
                if (gear.gearData.itemName == "Amplifier")
                {
                    if (Amplifier == 0)
                    {
                        Amplifier = 1;
                    }
                    Amplifier += ((0.2f * Mathf.Pow(2, gear.gearData.level - 1)));
                }
                if (gear.gearData.itemName == "Multiplier")
                {
                    Multiplier += gear.gearData.baseValue - 1;
                }
            }
            Amplifier *= GameManager.Instance.GetGearDataByID(0).baseValue;
            foreach (var gear in allConnectedGears)
            {
                gear.Rotate(45 * (gear.isReverse ? -1 : 1), Amplifier, Multiplier);
            }
        }

        public List<GearController> FindAllConnectedGearsOnBoard(List<GearController> allGears)
        {
            List<GearController> gears = new List<GearController>();
            foreach (var gear in allGears)
            {
                foreach (var connectedGear in gear.connectedGears)
                {
                    if (
                        connectedGear.gear.isActive
                        && !connectedGear.gear.isHead
                        && !allGears.Contains(connectedGear.gear)
                    )
                    {
                        gears.Add(connectedGear.gear);
                        connectedGear.gear.isReverse = !gear.isReverse;
                    }
                }
            }
            if (gears.Count != 0)
            {
                allGears.AddRange(gears);
                allGears = FindAllConnectedGearsOnBoard(allGears);
            }
            return allGears;
        }

        public List<GearController> FindAllConnectedGearsOnBoard()
        {
            List<GearController> gears = new List<GearController>();
            foreach (var gear in connectedGears)
            {
                if (gear.gear.isActive && !gear.gear.isHead && gear.direction == direction)
                {
                    gear.gear.isReverse = !isReverse;
                    gears.Add(gear.gear);
                }
            }
            if (gears.Count != 0)
            {
                gears = FindAllConnectedGearsOnBoard(gears);
            }
            return gears;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (
                isHead
                || !isActive
                || GameManager.Instance.GameState.CurrentState == GameStateType.Main
            )
                return;

            var parent = isInShop
                ? GameManager.Instance.TempContainerUI.transform
                : GameManager.Instance.TempContainer.transform;
            // Create a temporary gear
            _tempGear = Instantiate(gameObject, parent).GetComponent<GearController>();
            _tempGear.transform.SetParent(parent);
            _tempGear.transform.localScale = Vector3.one * 1.5f;
            _tempGear.GetComponent<RectTransform>().sizeDelta = Vector2.one * 190;
            _tempGear.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f).SetEase(Ease.InSine);

            // Set the temporary gear's properties
            _tempGear._gearIcon.raycastTarget = false;
            _tempGear.GetComponent<CanvasGroup>().blocksRaycasts = false;
            _gui.transform.DOScale(Vector3.one * 0, 0.2f).SetEase(Ease.InSine);
            GetComponent<CanvasGroup>().alpha = 0.6f;
            GameManager.Instance.DisableGearTrigger();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (
                isHead
                || !isActive
                || GameManager.Instance.GameState.CurrentState == GameStateType.Main
            )
                return;
            if (_tempGear == null)
                return;

            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _tempGear.transform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out position
            );
            _tempGear.transform.localPosition = position + new Vector2(0, 100);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameManager.Instance.EnableGearTrigger();
            if (GameManager.Instance.GameState.CurrentState == GameStateType.Main)
                return;

            if (_tempGear != null)
            {
                Destroy(_tempGear.gameObject);
                _tempGear = null;
            }
            GetComponent<CanvasGroup>().alpha = 1f;
            _gui.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InSine);
        }
    }

    [System.Serializable]
    public class GearNeighbor
    {
        public GearController gear;
        public int direction;
    }
}
