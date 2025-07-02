using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Factory
{
    public class FishManager : MonoBehaviour
    {
        public static FishManager Instance;
        public List<FishController> _fishes;
        public GameObject _fishParent;
        public GameObject _gate;

        public Material _waterMaterial;

        public float _fishMoveDistance = 3;

        public int currentFishIndex = 0;

        public bool isFishClosing = false;

        public Transform _fishSpawnPoint;

        public Slider _patienceSlider;
        public int totalFish = 0;

        public int indexFish = 0;

        public System.Action<FishController> OnFishFull;

        void Awake()
        {
            Instance = this;
            _waterMaterial.SetColor("_Color", new Color(0, 1, 0.979797f, 1f));
            OnFishFull = null;
        }

        void Start()
        {
            Vector3 screenPosition = UIManager.Instance.CameraUI.WorldToScreenPoint(
                _fishSpawnPoint.position
            );
            screenPosition.z = 0;
            Vector3 worldPosition = GameManager.Instance.mainCamera.ScreenToWorldPoint(
                screenPosition
            );
            worldPosition.z = 0;
            Debug.Log("screenPosition: " + screenPosition);
            Debug.Log("worldPosition: " + worldPosition);
            transform.position = worldPosition;
        }

        public void SpawnTextFloating(string text, Vector3 position)
        {
            var textObject = PoolSystem.Instance.GetObject("TextFloating");
            textObject.transform.SetParent(GameManager.Instance.homeUI.TotalGoldText.transform);
            text = float.Parse(text).ToString("0");
            var value = float.Parse(text);
            textObject.GetComponentInChildren<TMP_Text>().text = "+" + text;
            textObject.SetActive(true);
            textObject.transform.localPosition = Vector3.zero;
            textObject.transform.localScale = Vector3.one;
            float moveY = 50;
            textObject
                .transform.DOLocalMoveY(moveY, 0.3f)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    textObject.transform.localScale = Vector3.one * 0.01f;
                    PoolSystem.Instance.ReturnObject(textObject, "TextFloating");
                });
        }

        void Update()
        {
            if (
                GameManager.Instance.homeUI != null
                && !isFishClosing
                && !GameManager.Instance.isStop
            )
            {
                _patienceSlider.value = Mathf.Clamp(
                    _patienceSlider.value + Time.deltaTime,
                    0,
                    _patienceSlider.maxValue
                );
                float value = Mathf.Clamp01(
                    (float)_patienceSlider.value / _patienceSlider.maxValue
                );
                _patienceSlider.fillRect.GetComponent<Image>().color = new Color(
                    1,
                    1 - value,
                    1 - value,
                    1
                );
            }
        }

        public void UpdatePatienceSlider(int patience, int maxPatience = -1)
        {
            if (maxPatience != -1)
            {
                _patienceSlider.maxValue = maxPatience;
            }
            _patienceSlider.DOValue(patience, 0.3f).SetEase(Ease.InSine);
            float value = Mathf.Clamp01((float)patience / _patienceSlider.maxValue);
            _patienceSlider.fillRect.GetComponent<Image>().color = new Color(
                1,
                1 - value,
                1 - value,
                1
            );
            Debug.Log(1 - value);
        }

        [ContextMenu("MoveNextFish")]
        public async Task MoveNextFish()
        {
            isFishClosing = true;
            var currentFish = _fishes[currentFishIndex];
            await _gate
                .transform.DOLocalMoveX(-2, 0.3f)
                .SetEase(Ease.OutCirc)
                .AsyncWaitForCompletion();
            if (currentFishIndex >= _fishes.Count - 1)
            {
                await ClearFishes();
                await Task.Delay(1000);
                await GameManager.Instance.NextDay();
                return;
            }
            currentFishIndex++;
            float moveDistance = _fishMoveDistance * currentFishIndex;
            await transform
                .DOLocalMoveX(-moveDistance, 1f)
                .SetEase(Ease.OutCirc)
                .AsyncWaitForCompletion();
            await _gate
                .transform.DOLocalMoveX(0, 0.3f)
                .SetEase(Ease.OutCirc)
                .AsyncWaitForCompletion();
            isFishClosing = false;
        }

        public void GetItem(ItemController item)
        {
            if (currentFishIndex >= _fishes.Count || item == null || isFishClosing)
            {
                return;
            }
            SpawnTextFloating(item.itemData.cost.ToString(), item.transform.position);
        }

        public async Task ClearFishes()
        {
            DestroyFishes();
            await Task.Delay(100);
            transform.DOKill();
            isFishClosing = true;
            _gate.transform.DOLocalMoveX(-2, 1f).SetEase(Ease.InSine);
            transform.DOLocalMoveX(0, 1.0f).SetEase(Ease.InSine);
        }

        public void OpenGate()
        {
            _gate.transform.DOLocalMoveX(0, 1f).SetEase(Ease.InSine);
        }

        public void DestroyFishes()
        {
            foreach (var fish in _fishes)
            {
                fish.gameObject.SetActive(false);
                Destroy(fish.gameObject, 2f);
            }
            _fishes.Clear();
        }

        public async Task UpdateFishCountText(int count)
        {
            // Debug.Log("UpdateFishCountText: " + count);
            GameManager.Instance.homeUI.UpdateTotalFishText(count);
            if (count <= 2)
            {
                // _waterMaterial.DOKill();
                // _waterMaterial
                //     .DOColor(new Color(1, 0.6f, 0.6f, 1), 2f)
                //     .SetEase(Ease.OutExpo)
                //     .SetLoops(1, LoopType.Yoyo);

                GameManager.Instance.homeUI.TotalFishText.DOComplete();
                GameManager.Instance.homeUI.TotalFishText.transform.DOComplete();
                GameManager.Instance.homeUI.TotalFishText.transform.localScale = Vector3.one;
                GameManager.Instance.homeUI.TotalFishText.color = Color.white;
                GameManager
                    .Instance.homeUI.TotalFishText.DOColor(new Color(1, 0, 0, 1), 0.4f)
                    .SetEase(Ease.OutExpo)
                    .SetLoops(4, LoopType.Yoyo);
                GameManager
                    .Instance.homeUI.TotalFishText.transform.DOScale(1.2f, 0.4f)
                    .SetEase(Ease.OutExpo)
                    .SetLoops(4, LoopType.Yoyo);
                await Task.Delay(3000);
                await UpdateFishCountText(
                    GameManager.Instance.GetCurrentDayConfig().maxInPool
                        + (int)
                            CustomValueManager.Instance.GetCustomValueInGame(
                                CustomValueManager.HEART_BONUS
                            )
                        - _fishes.FindAll(x => x.state == FishState.Dead).Count
                );
            }
            else
            {
                GameManager.Instance.homeUI.TotalFishText.DOKill();
                GameManager.Instance.homeUI.TotalFishText.transform.DOKill();
                GameManager.Instance.homeUI.TotalFishText.transform.localScale = Vector3.one;
                GameManager.Instance.homeUI.TotalFishText.color = Color.white;
                _waterMaterial.DOKill();
                _waterMaterial
                    .DOColor(new Color(0, 1, 0.979797f, 1f), 2f)
                    .SetEase(Ease.OutExpo)
                    .SetLoops(1, LoopType.Yoyo);
            }
        }

        public async Task SpawnFish(List<FishConfigDay> fishConfigs)
        {
            await ClearFishes();
            System.Random random = new System.Random();
            isFishClosing = false;
            foreach (var fishConfig in fishConfigs)
            {
                var index = 0;
                for (int i = 0; i < fishConfig.amount; i++, index++)
                {
                    if (fishConfig.fishConfig.isBoss)
                    {
                        GameManager.Instance.homeUI.ShowWarningEffect();
                    }
                    await Task.Delay(fishConfig.fishConfig.patienceValue * 1000);
                    if (
                        GameManager.Instance.GameState.CurrentState == GameStateType.Shop
                        || isFishClosing
                    )
                    {
                        return;
                    }
                    var fishPrefab = Resources.Load<GameObject>(
                        "Prefabs/Fish/" + fishConfig.fishConfig.fishPrefabName
                    );
                    var fish = Instantiate(fishPrefab, _fishParent.transform);
                    _fishes.Add(fish.GetComponent<FishController>());
                    fish.transform.localPosition = new Vector3(
                        _fishMoveDistance * (random.Next(0, 2) == 0 ? -1 : 1),
                        random.Next(-4, -2),
                        0
                    );
                    fish.transform.DOScale(fish.transform.localScale * 1.5f, 0.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(2, LoopType.Yoyo);
                    fish.GetComponent<FishController>().Init(fishConfig.fishConfig, indexFish);
                    indexFish += 2;
                    Debug.Log("InitFish: " + fishConfig.fishConfig.fishCurrencyValue);
                    fish.gameObject.name = "Fish" + index;
                    GameManager.Instance.homeUI.NotReadyFishAmountText.text = (
                        this.totalFish - _fishes.Count
                    ).ToString();
                }
            }
            await Task.Delay(10000);
            // CheckWinLose();
        }

        public void CheckWinLose()
        {
            UpdateFishCountText(
                GameManager.Instance.GetCurrentDayConfig().maxInPool
                    + (int)
                        CustomValueManager.Instance.GetCustomValueInGame(
                            CustomValueManager.HEART_BONUS
                        )
                    - _fishes.FindAll(x => x.state == FishState.Dead && !x.fishConfig.isBoss).Count
            );
            if (
                _fishes.FindAll(x => x.state == FishState.Dead).Count
                >= GameManager.Instance.GetCurrentDayConfig().maxInPool
            )
            {
                Debug.Log("Lose");
                GameManager.Instance.ShowLosePanel();
            }
            else if (_fishes.Count == totalFish)
            {
                if (_fishes.FindAll(x => x.state == FishState.Moving).Count == 0)
                {
                    Debug.Log("Win" + _fishes.FindAll(x => x.state == FishState.Dead).Count);
                    GameManager.Instance.NextDay();
                }
            }
        }

        public async Task Init(List<FishConfigDay> fishConfigs)
        {
            await ClearFishes();
            currentFishIndex = 0;
            totalFish = 0;
            isFishClosing = true;
            _fishes = new List<FishController>();
            foreach (var fishConfig in fishConfigs)
            {
                this.totalFish += fishConfig.amount;
            }
            GameManager.Instance.homeUI.NotReadyFishAmountText.text = (
                this.totalFish - _fishes.Count
            ).ToString();
            UpdateFishCountText(
                GameManager.Instance.GetCurrentDayConfig().maxInPool
                    + (int)
                        CustomValueManager.Instance.GetCustomValueInGame(
                            CustomValueManager.HEART_BONUS
                        )
            );
        }

        public void UseDopamine(GearData gearData)
        {
            foreach (var fish in _fishes.FindAll(x => x.state == FishState.Moving))
            {
                fish.EnableTempImmortal(gearData);
            }
        }

        public FishController GetFishLowHp()
        {
            return _fishes
                .FindAll(x =>
                    x.state == FishState.Moving
                    && x.fishConfig.isBoss == false
                    && x.currentTotalTickValue > 0
                    && x.currentTotalTickValue < x.fishConfig.fishCurrencyValue * 0.2f
                )
                .OrderBy(x => x.currentTotalTickValue)
                .FirstOrDefault();
        }

        public FishController GetHungriest()
        {
            return _fishes
                .FindAll(x => x.state == FishState.Moving && x.fishConfig.isBoss == false)
                .OrderBy(x => x.currentTotalTickValue)
                .FirstOrDefault();
        }

        public FishController GetRandomFish()
        {
            if (_fishes.Count == 0)
            {
                return null;
            }
            System.Random random = new System.Random();
            var fishes = _fishes.FindAll(x =>
                x.state == FishState.Moving
                && x.fishConfig.isBoss == false
                && x.currentTotalTickValue < x.fishConfig.fishCurrencyValue
            );
            if (fishes.Count == 0)
            {
                return null;
            }
            int index = random.Next(0, fishes.Count);
            return fishes[index];
        }
    }
}
