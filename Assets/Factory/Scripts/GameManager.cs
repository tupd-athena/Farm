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
        private GameObject _duneObject;

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
        public GearDataSO gearDataSO => _gearDataSO;
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
        public GameObject DuneObject => _duneObject;

        public System.Action OnGameStart;

        public bool isStop = false;

        public Tween airPumpTween;

        public DayConfiguration dayConfiguration;

        public long sumOfFishesHealth = 0;

        public int totalHP;

        public List<GearController> sacrificeGears;
        public int SacrificePoint = 0;

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
            homeUI = AppManager.Instance.ShowSafeOverlayUI<HomeUI>("Factory/HomeUI");
            if (homeUI == null)
            {
                return;
            }
            // homeUI.StartButton.onClick.AddListener(StartGame);
            // _circle.rotation = new Vector3(0, 0, _circleSpeed);
            // NewGame();
            OnGameStart = null;
            _isFirstOpenShop = false;
            isStop = true;
            _currentLevelConfig = GetCurrentLevelConfig();
            currentDay = 0;
            totalHP = 0;
            SacrificePoint = 0;
            _gold = 0;
            _duneObject.SetActive(false);
            Debug.Log("new: 1");
            CustomValueManager.Instance.ClearCustomValueInGame();
            homeUI.HideArtifactPopup();
            homeUI.HideShopPopup();
            homeUI.WavePanel.SetActive(false);
        }

        public void ResetArtifactTweens()
        {
            airPumpTween?.Kill();
            airPumpTween = null;
            CancelInvoke(nameof(SpawnPearl));
        }

        public async Task NewGame()
        {
            homeUI.TicketPanel.SetActive(false);
            homeUI.WavePanel.SetActive(true);
            homeUI.GoldContainer.gameObject.SetActive(true);
            homeUI.buttonQuit.gameObject.SetActive(true);
            OnGameStart = null;
            _isFirstOpenShop = false;
            isStop = true;
            _currentLevelConfig = GetCurrentLevelConfig();
            currentDay = 0;
            totalHP = 0;
            SacrificePoint = 0;
            dayConfiguration = GetDayConfig();
            totalHP = dayConfiguration.maxInPool;
            _gold = 0;
            GamePlayTracking.Instance.ResetCurrentRunStats();
            _duneObject.SetActive(true);
            Debug.Log("new: 1");
            CustomValueManager.Instance.ClearCustomValueInGame();
            InitGears(_currentLevelConfig.gridSize);
            Debug.Log("new: 2");
            InitFishes(dayConfiguration.fishConfigs);
            Debug.Log("new: 3");
            UpdateGold(_gold + _currentLevelConfig.initialLevelCurrency);
            artifacts.ForEach(a => a.SetActive(false));
            Debug.Log("new: 4");
            // homeUI.ShowInventory();
            // homeUI.HideArtifactPopup();
            Debug.Log("new: 5");
            // RandomArtifactPopup();
            Debug.Log("new: 6");
            ChangeGameState(GameStateType.Shop);
            homeUI.UpdateDay();
            homeUI.HideArtifactPopup();
            Debug.Log("new: 7");
            // SacrificeRandomGears();
            // SeagullRandomGears();
            Debug.Log("new: 8");
        }

        public LevelConfiguration GetCurrentLevelConfig()
        {
            return _levelConfigSO.levelConfigs[0];
        }

        public DayConfiguration GetDayConfig()
        {
            System.Random random = new System.Random();
            DayConfiguration dayConfig = null;
            if (currentDay < _levelConfigSO.fixedDayConfigurations.Count)
            {
                return dayConfig = _levelConfigSO.GetDayConfig(
                    _levelConfigSO.fixedDayConfigurations[currentDay],
                    ComputeTotalFishHP(),
                    GetMaxCoinDrop()
                );
            }
            if (currentDay % 10 != 4 && currentDay % 10 != 9 || currentDay == 0)
            {
                dayConfig = _levelConfigSO.GetDayConfig(
                    _levelConfigSO.normalDayConfigurations[
                        random.Next(0, _levelConfigSO.normalDayConfigurations.Count)
                    ],
                    ComputeTotalFishHP(),
                    GetMaxCoinDrop()
                );
            }
            else if (currentDay % 10 == 4)
            {
                dayConfig = _levelConfigSO.GetDayConfig(
                    _levelConfigSO.specialDayConfigurations[
                        random.Next(0, _levelConfigSO.bossDayConfigurations.Count)
                    ],
                    ComputeTotalFishHP(),
                    GetMaxCoinDrop()
                );
            }
            else if (currentDay % 10 == 9 && currentDay != 0)
            {
                dayConfig = _levelConfigSO.GetDayConfig(
                    _levelConfigSO.bossDayConfigurations[
                        random.Next(0, _levelConfigSO.specialDayConfigurations.Count)
                    ],
                    ComputeTotalFishHP(),
                    GetMaxCoinDrop()
                );
            }
            return dayConfig;
        }

        public long ComputeTotalFishHP()
        {
            sumOfFishesHealth =
                _currentLevelConfig.maxTotalFishHP
                * (1 + (long)(_levelConfigSO.scaleTotalHP * Mathf.Pow(2, currentDay)));
            return sumOfFishesHealth;
        }

        public int GetMaxCoinDrop()
        {
            return (int)(
                _currentLevelConfig.maxCoinDrop
                * (float)Mathf.Clamp(Mathf.Pow(_levelConfigSO.scaleTotalHP, currentDay), 1, 5)
            );
        }

        public void InitFishes(List<FishConfigDay> fishConfigs)
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
            dayConfiguration = GetDayConfig();

            // Give 5 diamonds every 5 days
            if (currentDay % 5 == 0 && currentDay > 0)
            {
                for (int i = 0; i < 1; i++)
                {
                    var diamond = Instantiate(Resources.Load<GameObject>("Prefabs/Diamond"));
                    diamond.transform.position = transform.position;
                    diamond.SetActive(true);
                    diamond.GetComponent<CoinController>().value = 1;
                    diamond.GetComponent<CoinController>().Active();
                    diamond.GetComponent<CoinController>().OnComplete = () =>
                    {
                        InventoryManager.Instance.AddDiamonds(1);
                        AudioManager.Instance.PlaySound("Coin");
                        Destroy(diamond); // Clean up the ticket object after use
                    };
                }
                Debug.Log(
                    $"Day {currentDay}: Player received 5 diamonds for reaching day milestone!"
                );
            }

            if (currentDay % 10 == 5)
            {
                RandomArtifactPopup();
            }
            Debug.Log($"NextDay {currentDay}");
            isStop = true;
            sumOfFishesHealth =
                _currentLevelConfig.maxTotalFishHP * (1 + (long)(0.3f * Mathf.Pow(2, currentDay)));
            totalHP += dayConfiguration.maxInPool;
            Debug.Log($"Max Total Fish HP: {_currentLevelConfig.maxTotalFishHP}");
            // await homeUI.ShowGameStartPanel();
            InitFishes(dayConfiguration.fishConfigs);
            Debug.Log($"UpdateGold + {_currentLevelConfig.initialLevelCurrency}");
            homeUI.UpdateDay();
            // SacrificeRandomGears();

            ChangeGameState(GameStateType.Shop);
        }

        public async Task ShowLosePanel()
        {
            // Stop all game activities immediately
            isStop = true;

            // Clear all active game objects
            await FishManager.Instance.ClearFishes();
            CancelInvoke(nameof(SpawnPearl));
            ResetArtifactTweens();
            ClearAllGears();
            ClearItems();

            // Reset all game state variables
            currentDay = 0;
            currentLevel = 0;
            totalHP = 0;
            sumOfFishesHealth = 0;
            SacrificePoint = 0;
            _gold = 0;
            _isFirstOpenShop = false;

            // Clear sacrifice gears list
            if (sacrificeGears != null)
            {
                foreach (var gear in sacrificeGears)
                {
                    if (gear != null && gear.selectedSignImage != null)
                    {
                        gear.selectedSignImage.gameObject.SetActive(false);
                    }
                }
                sacrificeGears.Clear();
            }

            // Deactivate all artifacts
            foreach (var artifact in artifacts)
            {
                if (artifact != null)
                {
                    artifact.SetActive(false);
                }
            }

            // Reset custom values
            CustomValueManager.Instance.ClearCustomValueInGame();

            // Reset game tracking
            GamePlayTracking.Instance.ResetCurrentRunStats();

            // Reset UI state
            homeUI.UpdateGoldText(_gold);
            homeUI.WavePanel.SetActive(false);
            homeUI.GoldContainer.gameObject.SetActive(false);
            homeUI.buttonQuit.gameObject.SetActive(false);
            homeUI.HideArtifactPopup();
            homeUI.HideShopPopup();

            // Deactivate dune object
            _duneObject.SetActive(false);

            // Reset game state to initial state
            ChangeGameState(GameStateType.None);

            // Clear any remaining event subscriptions
            OnGameStart = null;

            Debug.Log("Game completely ended and reset to initial state");
            homeUI.NavigationBar.ShowAllTabsAndBar();
        }

        public void StartGame()
        {
            ChangeGameState(GameStateType.Main);
            CancelInvoke(nameof(SpawnPearl));
            OnGameStart?.Invoke();
            FishManager.Instance.SpawnFish(dayConfiguration.fishConfigs);
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

        public ListInventoryItemData GetInventoryData()
        {
            return homeUI.InventoryManager.GetInventoryData();
        }

        public float GetCustomValueOfInventoryByID(int itemId, string paraName)
        {
            return GetInventoryData()
                .items.FirstOrDefault(item => item.id == itemId)
                .gearData.GetCustomValue(paraName);
        }

        public float GetCustomValueForMultiplyByLevel(int itemId)
        {
            return 1
                + (
                    GetInventoryData()
                        .items.FirstOrDefault(item => item.id == itemId)
                        .gearData.GetCustomValue("level") - 1
                )
                    * gearDataSO
                        .gearDataList.FirstOrDefault(g => g.id == itemId)
                        .customValues.FirstOrDefault(c => c.id == "mult")
                        .customValue;
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
            return _activeItems.Count > 350;
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
                item.transform.DOScale(Vector3.zero, 0.5f)
                    .OnComplete(() =>
                    {
                        _itemPool.ReturnObject(item, ITEM_POOL_ID);
                    });
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
            GamePlayTracking.Instance.TrackGold(gold - _gold);
            _gold = gold;
            homeUI.UpdateGoldText(gold);
        }

        public void AddGold(int gold)
        {
            GamePlayTracking.Instance.TrackGold(gold);
            _gold += gold;
            homeUI.UpdateGoldText(_gold);
            CheckGoldAllGearsInShop();
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
                    gearController.gameObject.name = "Gear_" + i + "_" + j;
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
            var currentLevelConfig = GetCurrentLevelConfig();
            var gridSize = currentLevelConfig.gridSize;

            do
            {
                gearindex = random.Next(0, _gearControllers.Count);
                var gear = _gearControllers[gearindex];
                var coord = gear.gridCoordinate;

                // Check if gear is on edge or corner
                bool isOnEdge =
                    coord.x == 0
                    || coord.y == gridSize.x - 1
                    || coord.y == 0
                    || coord.x == gridSize.y - 1;

                // Exit loop if this gear is valid (not head, not on edge, and passes existing conditions)
                if (
                    !gear.isHead
                    && !isOnEdge
                    && !(
                        (coord.x % 2 == 0 && coord.y % 2 == 1)
                        || (coord.x % 2 == 1 && coord.y % 2 == 0)
                    )
                )
                {
                    break;
                }
            } while (true);

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
            if (
                homeUI.gameObject.activeInHierarchy == false
                || homeUI.ShopItems == null
                || homeUI.ShopItems.Count == 0
            )
            {
                return;
            }
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
            List<GearData> availableGears = new List<GearData>();
            availableGears = _gearDataSO
                .gearDataList.FindAll(g => InventoryManager.Instance.HasInventoryData(g))
                .ToList();
            availableGears.CopyTo(availableGears.ToArray());
            foreach (var gearData in availableGears)
            {
                totalWeight += gearData.weight;
            }

            foreach (var shopItem in homeUI.ShopItems)
            {
                System.Random random = new System.Random();
                float randomValue = random.Next(0, (int)totalWeight);
                float currentWeight = 0f;

                foreach (var gearData in availableGears)
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
                    OnGameStart += () => SpawnPearl();
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
                    FishManager.Instance.UpdateFishCountText();
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
            try
            {
                System.Random random = new System.Random();
                int amount = random.Next(1, 4);
                List<int> currentIndexes = new List<int>();
                List<ArtifactData> currentArtifacts = new List<ArtifactData>();
                int totalWeight = 0;
                foreach (var artifactData in _artifactConfigSO.artifactDatas)
                {
                    totalWeight += artifactData.weight;
                }
                for (int i = 0; i < _artifactConfigSO.artifactDatas.Count; i++)
                {
                    currentIndexes.Add(i);
                }
                for (int i = 0; i < amount; i++)
                {
                    int randomWeight = random.Next(0, totalWeight);
                    int currentWeight = 0;
                    int artifactIndex = 0;
                    foreach (var index in currentIndexes)
                    {
                        currentWeight += _artifactConfigSO.artifactDatas[index].weight;
                        if (randomWeight <= currentWeight)
                        {
                            artifactIndex = index;
                            break;
                        }
                    }
                    Debug.Log("RandomArtifactPopup: " + artifactIndex);
                    artifactIndex = Mathf.Clamp(
                        artifactIndex,
                        0,
                        _artifactConfigSO.artifactDatas.Count - 1
                    );
                    ArtifactData artifactData = _artifactConfigSO.artifactDatas[artifactIndex];
                    currentArtifacts.Add(artifactData);
                    currentIndexes.Remove(artifactIndex);
                    totalWeight -= artifactData.weight;
                }
                homeUI.ShowArtifactPopup(currentArtifacts);
            }
            catch (System.Exception e)
            {
                Debug.LogError("RandomArtifactPopup Error: " + e.Message);
            }
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
            pearl.GetComponent<CoinController>().canClick = false;
            pearl.transform.localScale = Vector3.zero;
            pearl.GetComponent<CoinController>().sparkleEffect.gameObject.SetActive(false);
            pearl
                .transform.DOScale(Vector3.one * 0.5f, artifact.GetValueByName("cooldown"))
                .OnComplete(() =>
                {
                    pearl.GetComponent<CoinController>().canClick = true;
                    pearl.GetComponent<CoinController>().sparkleEffect.gameObject.SetActive(true);
                });
            pearl.GetComponent<CoinController>().OnCollect += () =>
            {
                CollectPearl(artifact);
            };
        }

        public void CollectPearl(ArtifactData artifact)
        {
            SpawnPearl();
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

        public GearRarityData GetGearRarityData(GearRarity rarityType)
        {
            return _gearDataSO.gearRarityDataList.Find(r => r.rarity == rarityType);
        }

        public void SacrificeRandomGears()
        {
            return;
            System.Random random = new System.Random();
            int count = random.Next(0, 3);
            foreach (var gear in sacrificeGears)
            {
                gear.selectedSignImage.gameObject.SetActive(false);
                if (gear.gearData != null && !string.IsNullOrEmpty(gear.gearData.itemName))
                {
                    gear.Hide();
                    SacrificePoint++;
                }
            }
            sacrificeGears = new List<GearController>();
            if (_gearControllers.Count == 0)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var listGears = _gearControllers.FindAll(g =>
                    g.isHead == false && sacrificeGears.Contains(g) == false
                );
                var gear = listGears[random.Next(0, listGears.Count)];
                gear.selectedSignImage.gameObject.SetActive(true);
                sacrificeGears.Add(gear);
            }
        }

        public void SeagullRandomGears()
        {
            System.Random random = new System.Random();
            int count = random.Next(0, 3);
            foreach (var gear in sacrificeGears)
            {
                gear.selectedSignImage.gameObject.SetActive(false);
                if (gear.gearData != null && !string.IsNullOrEmpty(gear.gearData.itemName))
                {
                    gear.Hide();
                    SacrificePoint++;
                }
            }
            sacrificeGears = new List<GearController>();
            if (_gearControllers.Count == 0)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var listGears = _gearControllers.FindAll(g =>
                    g.isHead == false && sacrificeGears.Contains(g) == false
                );
                var gear = listGears[random.Next(0, listGears.Count)];
                gear.selectedSignImage.gameObject.SetActive(true);
                sacrificeGears.Add(gear);
            }
        }

        public void CalculateSpeedOfGears()
        {
            _gearControllers.ForEach(g => g.currentSpeed = 0);
            float headGearSpeed = 0;
            float multiplier = 1;
            float multiplierBySpeedUp = CustomValueManager.Instance.GetCustomValueInGame(
                CustomValueManager.MULTIPLIER_HEAD_GEAR_BY_SPEEDUP
            );
            float multiplierByScaredTotem = CustomValueManager.Instance.GetCustomValueInGame(
                CustomValueManager.MULTIPLIER_HEAD_GEAR_BY_SCARED_TOTEM
            );
            multiplier += multiplierBySpeedUp + multiplierByScaredTotem;
            headGearSpeed = 0.5f / multiplier / 4f;
            Debug.Log(
                "CalculateSpeedOfGears: "
                    + headGearSpeed
                    + " multiplier: "
                    + multiplier
                    + " multiplierBySpeedUp: "
                    + multiplierBySpeedUp
                    + " multiplierByScaredTotem: "
                    + multiplierByScaredTotem
            );
            List<GearController> headGears = _gearControllers.FindAll(g => g.isHead);
            foreach (var headGear in headGears)
            {
                foreach (var connectedGear in headGear.connectedGears)
                {
                    var listGear = connectedGear.gear.FindAllConnectedGearsOnBoard();
                    foreach (var gear in listGear)
                    {
                        if (gear.isHead || gear.gearData == null)
                        {
                            continue;
                        }
                        gear.currentSpeed =
                            gear.gearData.tickValue * headGearSpeed / gear.gearData.maxValue;
                    }
                }
            }
            _gearControllers.ForEach(g => g.UpdateVelocity(g.currentSpeed));
        }
        //
    }
}
