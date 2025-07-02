using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atom;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Factory
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public Camera mainCamera;
        public HomeUI homeUI;

        [SerializeField]
        private GameState _gameState;
        public GameState GameState => _gameState;

        [SerializeField]
        private Rotate _circle;

        [SerializeField]
        private float _circleSpeed;

        [SerializeField]
        private GameObject _itemContainer;

        [SerializeField]
        private PoolSystem _itemPool;

        [SerializeField]
        private GearDataSO _gearDataSO;

        [SerializeField]
        private ItemDataSO _itemDataSO;

        [SerializeField]
        private ArtifactConfigSO _artifactConfigSO;

        [SerializeField]
        private GridLayoutGroup _gearItemContainer;
        public GameObject TempContainerUI => homeUI.BoardTempContainer.gameObject;
        public GameObject TempContainer;

        [SerializeField]
        private LevelConfigSO _levelConfigSO;

        public Transform bottom;

        private List<GameObject> _activeItems = new List<GameObject>();
        public List<GearController> _gearControllers = new List<GearController>();

        public List<GameObject> artifacts = new List<GameObject>();
        public GameObject ItemContainer => _itemContainer;
        public HomeUI HomeUI => homeUI;
        private const string ITEM_POOL_ID = "Item";

        private LevelConfiguration _currentLevelConfig;
        public int currentLevel = 0;
        public int currentDay = 0;
        private bool _isFirstOpenShop = false;
        private int _gold = 0;
        public GridLayoutGroup GearItemContainer => _gearItemContainer;
        public List<GameObject> ActiveItems => _activeItems;

        public System.Action OnGameStart;

        public bool isStop = false;

        public Tween airPumpTween;

        public float GetBottomYWithOffset(float offset = 0.1f)
        {
            return bottom.position.y + offset;
        }

        public float Scale()
        {
            return bottom.position.y / -8f;
        }

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            _gameState = GetComponent<GameState>();
            OnGameStart = null;
            Debug.Log("Bottom Y: " + GetBottomYWithOffset());
        }

        public void DisableGearTrigger()
        {
            foreach (var gear in _gearControllers)
            {
                gear.GetComponent<Image>().raycastTarget = false;
            }
        }

        public void EnableGearTrigger()
        {
            foreach (var gear in _gearControllers)
            {
                gear.GetComponent<Image>().raycastTarget = true;
            }
        }

        public void Start()
        {
            currentLevel = 0;
            _currentLevelConfig = GetCurrentLevelConfig();

            homeUI = AppManager.Instance.ShowSafeOverlayUI<HomeUI>("Factory/HomeUI");
            if (homeUI == null)
            {
                return;
            }
            homeUI.StartButton.onClick.AddListener(StartGame);
            // _circle.rotation = new Vector3(0, 0, _circleSpeed);
            LoadLevel(0);
        }

        public void ResetArtifactTweens()
        {
            airPumpTween?.Kill();
            airPumpTween = null;
            CancelInvoke(nameof(SpawnPearl));
        }

        public async Task LoadLevel(int level)
        {
            OnGameStart = null;
            _isFirstOpenShop = false;
            isStop = true;
            currentLevel = level;
            _currentLevelConfig = GetCurrentLevelConfig();
            currentDay = 0;
            _gold = 0;
            CustomValueManager.Instance.ClearCustomValueInGame();
            Debug.Log($"LoadLevel {level}");
            InitGears(_currentLevelConfig.gridSize);
            InitBoxes(GetCurrentDayConfig().fishConfigs);
            UpdateGold(_gold + _currentLevelConfig.initialLevelCurrency);
            artifacts.ForEach(a => a.SetActive(false));
            // homeUI.HideArtifactPopup();
            RandomArtifactPopup();
            ChangeGameState(GameStateType.Shop);
            homeUI.UpdateDay();
            await homeUI.ShowGameStartPanel();
        }

        public LevelConfiguration GetCurrentLevelConfig()
        {
            if (currentLevel < 0 || currentLevel >= _levelConfigSO.levelConfigs.Count)
            {
                return null;
            }
            return _levelConfigSO.levelConfigs[currentLevel];
        }

        public DayConfiguration GetCurrentDayConfig()
        {
            int dayNumber = currentDay;
            if (
                _currentLevelConfig == null
                || _currentLevelConfig.dayConfigurations == null
                || dayNumber < 0
                || dayNumber >= _currentLevelConfig.dayConfigurations.Count
            )
            {
                return null;
            }
            return _currentLevelConfig.dayConfigurations[dayNumber];
        }

        public void InitBoxes(List<FishConfigDay> fishConfigs)
        {
            FishManager.Instance.Init(fishConfigs);
        }

        public async Task NextDay()
        {
            await Task.Delay(1000);
            await FishManager.Instance.ClearFishes();
            CancelInvoke(nameof(SpawnPearl));
            ClearItems();
            currentDay++;
            Debug.Log($"NextDay {currentDay}");
            isStop = true;
            if (GetCurrentDayConfig() == null)
            {
                await ShowWinPanel();
                return;
            }
            _currentLevelConfig = GetCurrentLevelConfig();
            await homeUI.ShowGameStartPanel();
            InitBoxes(GetCurrentDayConfig().fishConfigs);
            UpdateGold(_gold + GetCurrentDayConfig().initialDayCurrency);
            Debug.Log($"UpdateGold + {_currentLevelConfig.initialLevelCurrency}");
            homeUI.UpdateDay();
            ChangeGameState(GameStateType.Shop);
        }

        public async Task NextLevel()
        {
            await Task.Delay(1000);
            await FishManager.Instance.ClearFishes();
            CancelInvoke(nameof(SpawnPearl));
            isStop = true;
            currentLevel++;
            Debug.Log($"NextLevel {currentLevel}");
            if (GetCurrentLevelConfig() == null)
            {
                currentLevel = 1;
            }
            currentDay = 0;
            ClearAllGears();
            ClearItems();
            await LoadLevel(currentLevel);
        }

        public async Task ShowWinPanel()
        {
            await Task.Delay(1000);
            await FishManager.Instance.ClearFishes();
            CancelInvoke(nameof(SpawnPearl));
            isStop = true;
            Debug.Log($"ShowWinPanel");
            await homeUI.ShowWinPanel();
            await NextLevel();
            await Task.Delay(2000);
        }

        public async Task ShowLosePanel()
        {
            await FishManager.Instance.ClearFishes();
            CancelInvoke(nameof(SpawnPearl));
            await Task.Delay(1000);
            isStop = true;
            Debug.Log($"ShowLosePanel");
            await homeUI.ShowLosePanel();
            currentDay = 0;
            ClearAllGears();
            ClearItems();
            LoadLevel(currentLevel);
        }

        public void StartGame()
        {
            ChangeGameState(GameStateType.Main);
            CancelInvoke(nameof(SpawnPearl));
            OnGameStart?.Invoke();
            FishManager.Instance.SpawnFish(GameManager.Instance.GetCurrentDayConfig().fishConfigs);
            // ActivateAllHeadGears();
        }

        public void ChangeGameState(GameStateType state)
        {
            _gameState.SetState(state);
        }

        public void ActivateAllHeadGears()
        {
            foreach (var gear in _gearControllers)
            {
                if (gear.isHead)
                {
                    gear.Rotate();
                }
            }
        }

        public void SetFillAllGearsInShop()
        {
            foreach (var gear in _gearControllers)
            {
                gear.FillItemIcon(1);
            }
        }

        public GameObject SpawnItem(
            Vector2 position,
            GearController gearController,
            float Amplifier
        )
        {
            var checkItemData = GetItemDataByItemID(gearController.itemData.itemId);
            if (checkItemData == null)
            {
                return null;
            }
            var item = _itemPool.GetObject(ITEM_POOL_ID);
            item.GetComponent<Collider2D>().enabled = true;
            item.GetComponent<SpriteRenderer>().material = new Material(
                item.GetComponent<SpriteRenderer>().material
            );
            item.GetComponent<SpriteRenderer>().material.SetFloat("_Dissolve", 1f);
            item.transform.SetParent(FishManager.Instance._fishParent.transform);
            position = position * mainCamera.orthographicSize / 6.4f;
            item.transform.position = new Vector3(position.x, position.y, 0);
            item.transform.localEulerAngles = new Vector3(0, 0, 0);
            item.transform.localScale = new Vector3(1, 1, 1);
            float cost = (float)(checkItemData.cost * gearController.gearData.level + Amplifier);
            item.GetComponent<ItemController>().SetItemData(checkItemData, cost);
            System.Random random = new System.Random();
            float randomX = random.Next(-3, 3);
            item.GetComponent<Rigidbody2D>().AddForce(new Vector2(randomX, 0), ForceMode2D.Impulse);
            _activeItems.Add(item);
            return item;
        }

        public GameObject SpawnItem(Vector2 position, ItemController itemController)
        {
            var checkItemData = itemController;
            if (checkItemData == null)
            {
                return null;
            }
            var item = _itemPool.GetObject(ITEM_POOL_ID);
            item.GetComponent<Collider2D>().enabled = true;
            item.GetComponent<SpriteRenderer>().material = new Material(
                item.GetComponent<SpriteRenderer>().material
            );
            item.GetComponent<SpriteRenderer>().material.SetFloat("_Dissolve", 1f);
            item.transform.SetParent(FishManager.Instance._fishParent.transform);
            position = position * mainCamera.orthographicSize / 6.4f;
            item.transform.position = new Vector3(position.x, position.y, 0);
            item.transform.localEulerAngles = new Vector3(0, 0, 0);
            item.transform.localScale = new Vector3(1, 1, 1);
            float cost = (float)(checkItemData.itemData.cost);
            item.GetComponent<ItemController>().SetItemData(checkItemData.itemData, cost);
            System.Random random = new System.Random();
            float randomX = random.Next(-3, 3);
            item.GetComponent<Rigidbody2D>().AddForce(new Vector2(randomX, 0), ForceMode2D.Impulse);
            _activeItems.Add(item);
            return item;
        }

        public GameObject SpawnItem(Vector2 position, ItemData itemData)
        {
            var item = _itemPool.GetObject(ITEM_POOL_ID);
            item.GetComponent<Collider2D>().enabled = true;
            item.GetComponent<SpriteRenderer>().material = new Material(
                item.GetComponent<SpriteRenderer>().material
            );
            item.GetComponent<SpriteRenderer>().material.SetFloat("_Dissolve", 1f);
            item.transform.SetParent(FishManager.Instance._fishParent.transform);
            position = position * mainCamera.orthographicSize / 6.4f;
            item.transform.position = new Vector3(position.x, position.y, 0);
            item.transform.localEulerAngles = new Vector3(0, 0, 0);
            item.transform.localScale = new Vector3(1, 1, 1);
            float cost = (float)(itemData.cost);
            item.GetComponent<ItemController>().SetItemData(itemData, cost);
            System.Random random = new System.Random();
            float randomX = random.Next(-3, 3);
            item.GetComponent<Rigidbody2D>().AddForce(new Vector2(randomX, 0), ForceMode2D.Impulse);
            _activeItems.Add(item);
            return item;
        }

        public bool CheckActiveItemCount()
        {
            return _activeItems.Count > 50;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearItems();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                ChangeGameState(GameStateType.Shop);
            }
        }

        [ContextMenu("Clear Items")]
        public void ClearItems()
        {
            foreach (var item in _activeItems)
            {
                _itemPool.ReturnObject(item, ITEM_POOL_ID);
            }
            _activeItems.Clear();
        }

        public void ClearAllGears()
        {
            foreach (var gearController in _gearControllers)
            {
                Destroy(gearController.gameObject);
            }
            _gearControllers.Clear();
        }

        public void UpdateGold(int gold)
        {
            _gold = gold;
            homeUI.UpdateGoldText(gold);
        }

        public void AddGold(int gold)
        {
            _gold += gold;
            homeUI.UpdateGoldText(_gold);
        }

        public void InitGears(Vector2 gridSize)
        {
            ClearAllGears();
            float size = 110;
            _gearItemContainer.cellSize = new Vector2(size, size);
            _gearItemContainer.spacing = new Vector2(-5, -5);
            _gearItemContainer.constraintCount = (int)gridSize.x;
            for (int i = 0; i < gridSize.y; i++)
            {
                for (int j = 0; j < gridSize.x; j++)
                {
                    var gearController = Instantiate(
                            Resources.Load<GameObject>("Prefabs/Gear"),
                            _gearItemContainer.transform
                        )
                        .GetComponent<GearController>();
                    _gearControllers.Add(gearController);
                    if ((i % 2 == 0 && j % 2 == 0) || (i % 2 == 1 && j % 2 == 1))
                    {
                        gearController.startAngle = (45 / 2f);
                    }
                    else
                    {
                        gearController.startAngle = -3.5f;
                    }
                    gearController.gridCoordinate = new Vector2(i, j);
                    gearController.Hide();
                    gearController.SetGear(new List<GearType> { GearType.Text });
                    gearController.OnRotate += (float Amplifier) =>
                    {
                        if (
                            !gearController.isHead
                            && gearController.itemData != null
                            && gearController.gearData.gearTypes.Contains(GearType.Image)
                            && !isStop
                            && !CheckActiveItemCount()
                        )
                        {
                            Debug.Log("Spawn Item");
                            var screenPosition = gearController.transform.position;
                            SpawnItem(screenPosition, gearController, Amplifier);
                        }
                    };
                }
            }
            for (int i = 0; i < GetCurrentLevelConfig().maxHeadGearSlots; i++)
            {
                SetRandomHeadGear();
            }
            CalculateGearNeighbors();
        }

        public void SetRandomHeadGear()
        {
            System.Random random = new System.Random();
            int gearindex = random.Next(0, _gearControllers.Count);
            do
            {
                gearindex = random.Next(0, _gearControllers.Count);
            } while (
                _gearControllers[gearindex].isHead
                || (
                    (
                        _gearControllers[gearindex].gridCoordinate.x % 2 == 0
                        && _gearControllers[gearindex].gridCoordinate.y % 2 == 1
                    )
                    || (
                        _gearControllers[gearindex].gridCoordinate.x % 2 == 1
                        && _gearControllers[gearindex].gridCoordinate.y % 2 == 0
                    )
                )
            );
            _gearControllers[gearindex].Rotate();
            _gearControllers[gearindex].Show();
            _gearControllers[gearindex].isHead = true;
            _gearControllers[gearindex].SetGear(new List<GearType> { GearType.HeadGear });
        }

        public void CalculateGearNeighbors()
        {
            foreach (var gear in _gearControllers)
            {
                Vector2 coord = gear.gridCoordinate;
                int direction = 0;
                // Check all adjacent positions (up, down, left, right)
                Vector2[] adjacentPositions = new Vector2[]
                {
                    new Vector2(coord.x + 1, coord.y), // right - direction 3
                    new Vector2(coord.x - 1, coord.y), // left - direction 1
                    new Vector2(coord.x, coord.y + 1), // up - direction 0
                    new Vector2(coord.x, coord.y - 1), // down - direction 2
                };

                foreach (var pos in adjacentPositions)
                {
                    var neighbor = _gearControllers.Find(g => g.gridCoordinate == pos);
                    if (neighbor != null)
                    {
                        switch (pos)
                        {
                            case Vector2 v when v.x == coord.x && v.y == coord.y + 1:
                                SetGearConnection(gear, neighbor, 3);
                                break;
                            case Vector2 v when v.x == coord.x - 1 && v.y == coord.y:
                                SetGearConnection(gear, neighbor, 0);
                                break;
                            case Vector2 v when v.x == coord.x && v.y == coord.y - 1:
                                SetGearConnection(gear, neighbor, 1);
                                break;
                            case Vector2 v when v.x == coord.x + 1 && v.y == coord.y:
                                SetGearConnection(gear, neighbor, 2);
                                break;
                        }
                        direction++;
                    }
                }
            }
        }

        public void SetGearConnection(
            GearController gearController,
            GearController connectedGearController,
            int direction
        )
        {
            if (gearController.connectedGears.Find(g => g.gear == connectedGearController) != null)
            {
                return;
            }
            gearController.Connect(connectedGearController, direction);
        }

        public void CollectItem(ItemController item)
        {
            item.Clear();
            _itemPool.ReturnObject(item.gameObject, ITEM_POOL_ID);
            if (_activeItems.Contains(item.gameObject))
            {
                _activeItems.Remove(item.gameObject);
            }
        }

        public void DisconnectGear(
            GearController gearController,
            GearController connectedGearController,
            int direction
        )
        {
            gearController.Disconnect(connectedGearController, direction);
        }

        public void ForceGearsInShop()
        {
            var totalWeight = 0f;

            var gearTypes = new List<GearType> { GearType.Food, GearType.Text, GearType.Food };
            var index = 0;
            foreach (var shopItem in homeUI.ShopItems)
            {
                totalWeight = 0;
                var currentGearType = gearTypes[index];
                foreach (
                    var gearData in _gearDataSO.gearDataList.FindAll(g =>
                        g.gearTypes.Contains(currentGearType)
                    )
                )
                {
                    totalWeight += gearData.weight;
                }
                System.Random random = new System.Random();
                float randomValue = random.Next(0, (int)totalWeight);
                float currentWeight = 0f;
                Debug.Log(
                    "Current Gear Type: "
                        + currentGearType
                        + _gearDataSO
                            .gearDataList.FindAll(g => g.gearTypes.Contains(currentGearType))
                            .Count
                );
                foreach (
                    var gearData in _gearDataSO.gearDataList.FindAll(g =>
                        g.gearTypes.Contains(currentGearType)
                    )
                )
                {
                    currentWeight += gearData.weight;
                    if (randomValue <= currentWeight)
                    {
                        shopItem.gear.SetGearData(gearData);
                        shopItem.gear.SetItemData(gearData);
                        shopItem.gear.isInShop = true;
                        homeUI.UpdateCostText(shopItem, (int)gearData.cost);
                        shopItem.gear.Show();
                        shopItem.gear.FillItemIcon(1);
                        shopItem.gear.SetGear(gearData.gearTypes);
                        shopItem.gear.OnDropShop = null;
                        shopItem.gear.OnDropShop += (gearData) =>
                        {
                            if (_gold >= (int)gearData.cost)
                            {
                                _gold -= (int)gearData.cost;
                                UpdateGold(_gold);
                                shopItem.purchased = true;
                            }
                            CheckGoldAllGearsInShop();
                        };
                        shopItem.purchased = false;
                        break;
                    }
                }
                index++;
            }
            CheckGoldAllGearsInShop();
        }

        public bool CheckGold(int cost)
        {
            return _gold >= cost;
        }

        [ContextMenu("CheckGoldAllGearsInShop")]
        public void CheckGoldAllGearsInShop()
        {
            foreach (var shopItem in homeUI.ShopItems)
            {
                if (shopItem.gear.isInShop)
                {
                    if (
                        shopItem.gear.gearData == null
                        || !CheckGold(shopItem.gear.gearData.cost)
                        || String.IsNullOrEmpty(shopItem.gear.gearData.itemName)
                        || shopItem.purchased
                    )
                    {
                        // homeUI.StrikethroughCostText(shopItem, true);
                        homeUI.UpdatePanelImage(shopItem, true);
                    }
                    else
                    {
                        // homeUI.StrikethroughCostText(shopItem, false);
                        homeUI.UpdatePanelImage(shopItem, false);
                    }
                }
            }
            homeUI.SetLockRerollButton(CheckGold(5));
        }

        public void SetAllGearFillEmpty()
        {
            foreach (var gear in _gearControllers)
            {
                if (gear.gearData == null || !gear.gearData.gearTypes.Contains(GearType.Image))
                {
                    continue;
                }
                gear.FillItemIcon(0);
                gear.currentTotalTickValue = 0;
            }
        }

        public bool CheckItemInShop(GearController gear)
        {
            return !CheckGold(gear.gearData.cost) || String.IsNullOrEmpty(gear.gearData.itemName);
        }

        public void CheckFirstOpenShop()
        {
            if (_isFirstOpenShop && GameState.Instance.CurrentState == GameStateType.Shop)
            {
                var shopItems = homeUI.ShopItems.FindAll(g =>
                    g.gear.gearData != null && !g.gear.isInShop
                );
                if (shopItems.Count < 2)
                {
                    homeUI.UnlockStartButton();
                }
            }
        }

        public void RandomGearsInShop(bool isFree = false)
        {
            if (!_isFirstOpenShop)
            {
                ForceGearsInShop();
                homeUI.LockStartButton();
                _isFirstOpenShop = true;
                return;
            }
            if (!isFree)
            {
                _gold -= 5;
                UpdateGold(_gold);
            }
            var totalWeight = 0f;
            foreach (var gearData in _gearDataSO.gearDataList)
            {
                totalWeight += gearData.weight;
            }

            foreach (var shopItem in homeUI.ShopItems)
            {
                System.Random random = new System.Random();
                float randomValue = random.Next(0, (int)totalWeight);
                float currentWeight = 0f;

                foreach (var gearData in _gearDataSO.gearDataList)
                {
                    currentWeight += gearData.weight;
                    if (randomValue <= currentWeight)
                    {
                        shopItem.gear.SetGearData(gearData);
                        shopItem.gear.SetItemData(gearData);
                        shopItem.gear.isInShop = true;
                        shopItem.gear.Show();
                        homeUI.UpdateCostText(shopItem, (int)shopItem.gear.gearData.cost);
                        shopItem.gear.FillItemIcon(1);
                        shopItem.purchased = false;
                        break;
                    }
                }
            }
            CheckGoldAllGearsInShop();
        }

        public void ActiveArtifact(ArtifactData artifactData)
        {
            string artifactName = artifactData.artifactId;
            homeUI.HideArtifactPopup();
            switch (artifactName)
            {
                case "sacredTotem": // Artifact 1
                    CustomValueManager.Instance.AddCustomValueInGame(
                        CustomValueManager.MULTIPLIER_HEAD_GEAR_BY_SCARED_TOTEM,
                        artifactData.GetValueByName("percentIncrease")
                    );
                    artifacts[0].SetActive(true);
                    break;
                case "airPump": // Artifact 2
                    ActiveTheAirPump(artifactData);
                    break;
                case "clam": // Artifact 3
                    OnGameStart += () =>
                        Invoke(nameof(SpawnPearl), artifactData.GetValueByName("cooldown"));
                    artifacts[2].SetActive(true);
                    break;
                case "treasure": // Artifact 4
                    AddGold((int)artifactData.GetValueByName("value"));
                    artifacts[3].SetActive(true);
                    break;
                case "bank": // Artifact 5
                    CustomValueManager.Instance.AddCustomValueInGame(
                        CustomValueManager.FISH_GOLD_BONUS,
                        artifactData.GetValueByName("value")
                    );
                    artifacts[6].SetActive(true);
                    break;
                case "fillFull":
                    FillFull();
                    artifacts[4].SetActive(true);
                    break;
                case "heartBonus": // Artifact 6
                    CustomValueManager.Instance.AddCustomValueInGame(
                        CustomValueManager.HEART_BONUS,
                        artifactData.GetValueByName("value")
                    );
                    FishManager.Instance.UpdateFishCountText(
                        GameManager.Instance.GetCurrentDayConfig().maxInPool
                            + (int)
                                CustomValueManager.Instance.GetCustomValueInGame(
                                    CustomValueManager.HEART_BONUS
                                )
                    );
                    artifacts[5].SetActive(true);
                    break;
                default:
                    break;
            }
        }

        public void FillFull()
        {
            foreach (var gear in _gearControllers)
            {
                if (gear.gearData == null && !gear.isHead)
                {
                    gear.SetGearData(_gearDataSO.gearDataList[0]);
                    gear.SetItemData(_gearDataSO.gearDataList[0]);
                    gear.Show();
                }
            }
            homeUI.HideArtifactPopup();
        }

        public void ActiveTheAirPump(ArtifactData artifactData)
        {
            artifacts[1].SetActive(true);
            airPumpTween?.Kill();
            airPumpTween = DOVirtual.DelayedCall(
                artifactData.GetValueByName("cooldown"),
                () =>
                {
                    artifacts[1].GetComponentInChildren<ParticleSystem>().Stop();
                    CustomValueManager.Instance.AddCustomValueInGame(
                        CustomValueManager.REDUCE_FISH_TICK_RATE,
                        artifactData.GetValueByName("percentDecrease")
                    );
                    var main = artifacts[1].GetComponentInChildren<ParticleSystem>().main;
                    main.loop = false;
                    main.playOnAwake = false;
                    main.duration = artifactData.GetValueByName("duration") * 0.8f;
                    artifacts[1].GetComponentInChildren<ParticleSystem>().Play();

                    airPumpTween = DOVirtual.DelayedCall(
                        artifactData.GetValueByName("duration"),
                        () =>
                        {
                            CustomValueManager.Instance.RemoveCustomValueInGame(
                                CustomValueManager.REDUCE_FISH_TICK_RATE,
                                artifactData.GetValueByName("percentDecrease")
                            );
                            ActiveTheAirPump(artifactData);
                        }
                    );
                }
            );
        }

        public void RandomArtifactPopup()
        {
            System.Random random = new System.Random();
            int amount = random.Next(1, _artifactConfigSO.artifactDatas.Count + 1);
            List<int> currentIndexes = new List<int>();
            List<ArtifactData> currentArtifacts = new List<ArtifactData>();
            for (int i = 0; i < _artifactConfigSO.artifactDatas.Count; i++)
            {
                currentIndexes.Add(i);
            }
            for (int i = 0; i < amount; i++)
            {
                int randomIndex = random.Next(0, currentIndexes.Count);
                int artifactIndex = currentIndexes[randomIndex];
                ArtifactData artifactData = _artifactConfigSO.artifactDatas[artifactIndex];
                currentArtifacts.Add(artifactData);
                currentIndexes.RemoveAt(randomIndex);
            }
            homeUI.ShowArtifactPopup(currentArtifacts);
        }

        public void SpawnPearl()
        {
            CancelInvoke(nameof(SpawnPearl));
            var pearl = Instantiate(
                Resources.Load<GameObject>("Prefabs/Pearl"),
                _itemContainer.transform
            );
            var artifact = _artifactConfigSO.artifactDatas.FirstOrDefault(a =>
                a.artifactId == "clam"
            );
            pearl.transform.position = artifacts[2].transform.position;
            pearl.GetComponent<CoinController>().value = (int)artifact.GetValueByName("gold");
            Invoke(nameof(SpawnPearl), artifact.GetValueByName("cooldown"));
        }

        public ItemData GetItemDataByGearID(int id)
        {
            return _itemDataSO.itemDataList.Find(item => item.gearId == id);
        }

        public ItemData GetItemDataByItemID(int id)
        {
            return _itemDataSO.itemDataList.Find(item => item.itemId == id);
        }

        public GearData GetGearDataByID(int id)
        {
            return _gearDataSO.gearDataList.Find(gear => gear.id == id);
        }

        public Sprite GetArtifactRaritySprite(ArtifactType artifactType)
        {
            return _artifactConfigSO
                .artifactTypeDatas.Find(a => a.artifactType == artifactType)
                .artifactFrame;
        }

        public Sprite GetGearBaseColorSprite(GearBaseColorType type)
        {
            return _gearDataSO.gearBaseColors.Find(c => c.type == type).sprite;
        }
    }
}
