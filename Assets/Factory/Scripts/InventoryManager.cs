using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Factory;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Transform gui;

    [SerializeField]
    private GameObject _inventoryTopPopup;

    [SerializeField]
    private Button _levelUpButton;

    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private TMP_Text _levelText;

    [SerializeField]
    private TMP_Text _nextLevelText;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _descriptionText;

    [SerializeField]
    private TMP_Text _giftText;

    public Button buttonClose;
    public GameObject _inventoryBottomPopup;
    public GameObject _mask;
    public Slider slider;

    public List<InventoryItemController> inventoryItemControllers =
        new List<InventoryItemController>();
    public List<InventoryItemCard> inventoryItemCards = new List<InventoryItemCard>();

    public ListInventoryItemData inventoryData;
    public InventoryItemController itemPrefab;
    public InventoryItemCard cardPrefab;
    public InventoryItemCard coinPrefab;
    public Transform itemContainer;
    public Transform cardContainer;
    private string saveFilePath;
    public InventoryItemController currentItemController;

    public ParticleSystem levelUpEffect;
    public ParticleSystem giftEffect;
    public System.Action<InventoryItemController> OnItemClicked;
    public System.Action<InventoryItemCard> OnCardClicked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            saveFilePath = Path.Combine(Application.persistentDataPath, "inventory.json");
            _levelUpButton.onClick.AddListener(LevelUpButtonClicked);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        LoadData();
        LoadInventory();
    }

    public void SetStackSizeOfItem(InventoryItemController itemController, float valueChange = 1)
    {
        PlayerPrefs.Save();

        if (itemController != null && itemController.itemData != null)
        {
            itemController.itemData.currentStackSize += (int)valueChange;

            // Activate the item if it was inactive and now has stack size > 0
            if (!itemController.isItemActive && itemController.itemData.currentStackSize > 0)
            {
                itemController.SetActive();
                itemController.isItemActive = true;
            }

            itemController.UpdateProgressbar();
            SaveInventory();
        }
    }

    void Start()
    {
        UpdateGiftText();
        OnItemClicked += SetCurrentItem;
        if (_inventoryTopPopup != null)
        {
            HideTopPopup();
        }
        OnCardClicked += (card) =>
        {
            if (card != null && card.itemData != null)
            {
                if (card.isCoin)
                {
                    System.Random random = new System.Random();
                    int gold = random.Next(1, 6);
                    GameManager.Instance.AddGold(gold);
                }
                else
                {
                    InventoryItemController itemController = inventoryItemControllers.Find(item =>
                        item.itemData.id == card.itemData.id
                    );
                    if (itemController != null)
                    {
                        SetStackSizeOfItem(itemController);
                        itemController.UpdateProgressbar();
                        itemController.sparkleEffect.Play();
                    }
                }
                SaveInventory();
                DOVirtual.Float(
                    1,
                    0,
                    0.1f,
                    (value) =>
                    {
                        card.cardImage.color = new Color(
                            card.cardImage.color.r,
                            card.cardImage.color.g,
                            card.cardImage.color.b,
                            value
                        );
                        card.iconImage.color = new Color(
                            card.iconImage.color.r,
                            card.iconImage.color.g,
                            card.iconImage.color.b,
                            value
                        );
                    }
                );
                card.GetComponent<CanvasGroup>().blocksRaycasts = false;
                card.GetComponent<CanvasGroup>().interactable = false;
                card.transform.DOScale(Vector3.zero, 0f)
                    .SetEase(Ease.InCirc)
                    .SetDelay(1f)
                    .OnComplete(() =>
                    {
                        inventoryItemCards.Remove(card);
                        Destroy(card.gameObject);
                        if (inventoryItemCards.Count < 1)
                        {
                            ShowTopPopup();
                        }
                    });
            }
        };
    }

    public void SetSliderValue(float value)
    {
        value = Mathf.Clamp01(value);
        if (slider != null)
        {
            slider.value = value;
            _levelUpButton.interactable = value >= 1f;
        }
    }

    public void LevelUpButtonClicked()
    {
        if (inventoryItemControllers.Count == 0)
        {
            Debug.LogWarning("No items in inventory to level up");
            return;
        }
        InventoryItemController itemController = currentItemController;
        if (itemController != null && itemController.itemData != null)
        {
            if (itemController.itemData.currentStackSize >= itemController.itemData.maxStackSize)
            {
                SetStackSizeOfItem(itemController, -itemController.itemData.maxStackSize);
                itemController
                    .itemData.gearData.customValues.FirstOrDefault(c => c.id == "level")
                    .customValue++;
                itemController.SetLevelText(itemController.itemData.GetLevel());
                itemController.UpdateProgressbar();
                SetLevelText(itemController.itemData.GetLevel());
                SetSliderValue(
                    (float)itemController.itemData.currentStackSize
                        / itemController.itemData.maxStackSize
                );
                SaveInventory();
                levelUpEffect.Play();
            }
        }
    }

    public void ShowInventory()
    {
        gui.gameObject.SetActive(true);
        if (_inventoryBottomPopup != null)
        {
            _inventoryBottomPopup.SetActive(true);
            _mask.SetActive(true);
            _mask.GetComponent<Image>().color = new Color32(100, 75, 30, 255);
            _mask.GetComponent<Image>().DOFade(0.95f, 0.4f).SetEase(Ease.InSine);
            _inventoryBottomPopup.transform.localScale = Vector3.one;
            _inventoryBottomPopup.transform.DOLocalMoveX(0, 0.4f).SetEase(Ease.InSine);
            LoadInventory();
        }
    }

    public async Task Close()
    {
        HideTopPopup();
        buttonClose
            .transform.DOLocalRotate(new Vector3(0, 0, 360), 0.3f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InSine);
        inventoryItemControllers.ForEach(item => item.HideFrame());
        inventoryItemCards.ForEach(card => Destroy(card.gameObject));
        inventoryItemCards.Clear();
        _inventoryBottomPopup
            .transform.DOLocalMoveX(Screen.width * 2 + 100, 0.4f)
            .SetEase(Ease.InSine);
        _mask
            .GetComponent<Image>()
            .DOFade(0, 0.4f)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                _mask.SetActive(false);
                _inventoryBottomPopup.SetActive(false);
            });
        await Task.Delay(500);
        gui.gameObject.SetActive(false);
    }

    public void ShowTopPopup()
    {
        if (_inventoryTopPopup != null)
        {
            _inventoryTopPopup.transform.DOLocalMoveX(0, 0.4f).SetEase(Ease.InSine);

            // Find the first active item to set as current
            InventoryItemController firstActiveItem = inventoryItemControllers.FirstOrDefault(
                item => item.isItemActive
            );
            if (firstActiveItem != null)
            {
                SetCurrentItem(firstActiveItem);
            }
            else if (inventoryItemControllers.Count > 0)
            {
                // If no active items, still set the first one as current
                SetCurrentItem(inventoryItemControllers[0]);
            }

            buttonClose.gameObject.SetActive(true);
        }
    }

    public void HideTopPopup()
    {
        if (_inventoryTopPopup != null)
        {
            _inventoryTopPopup.transform.DOLocalMoveX(Screen.width * 2, 0.4f).SetEase(Ease.InSine);
        }
    }

    public void SetCurrentItem(InventoryItemController itemController)
    {
        if (itemController == null)
        {
            Debug.LogError("Item controller is null");
            return;
        }
        if (itemController.itemData == null)
        {
            Debug.LogError("Item data is null in item controller");
            return;
        }
        inventoryItemControllers.ForEach(item =>
        {
            if (item != null)
            {
                item.HideFrame();
            }
        });
        currentItemController = itemController;
        itemController.ShowFrame();
        SetIcon(itemController.itemData.GetIcon());
        SetLevelText(itemController.itemData.GetLevel());
        SetNameText(itemController.itemData.name);
        SetSliderValue(
            (float)itemController.itemData.currentStackSize / itemController.itemData.maxStackSize
        );
        SetDescriptionText(itemController.itemData.description);
    }

    public void SetIcon(Sprite icon)
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = icon;
        }
    }

    public void SetLevelText(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = "" + level;
            _nextLevelText.text = "" + (level + 1);
        }
    }

    public void SetNameText(string name)
    {
        if (_nameText != null)
        {
            _nameText.text = name;
        }
    }

    public void SetDescriptionText(string description)
    {
        if (_descriptionText != null)
        {
            _descriptionText.text = description;
        }
    }

    void OnDisable()
    {
        SaveInventory();
    }

    public void ItemClicked(InventoryItemController itemController)
    {
        // If item is inactive and has level 1 with 0 stack, activate it when clicked
        if (
            !itemController.isItemActive
            && itemController.itemData.GetLevel() == 1
            && itemController.itemData.currentStackSize == 0
        )
        {
            itemController.SetActive();
            itemController.isItemActive = true;
        }

        OnItemClicked?.Invoke(itemController);
    }

    public bool LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                inventoryData = JsonUtility.FromJson<ListInventoryItemData>(jsonData);
                Debug.Log("Inventory loaded successfully from JSON");
                if (inventoryData != null)
                {
                    return true;
                }
                else
                {
                    Debug.LogError("Inventory data is null after loading from JSON");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading inventory: " + e.Message);
                return false;
            }
        }
        else
        {
            Debug.Log("Inventory file not found, creating new inventory");
            return false;
        }
    }

    public void LoadInventory()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                inventoryData = JsonUtility.FromJson<ListInventoryItemData>(jsonData);
                Debug.Log("Inventory loaded successfully from JSON");
                if (inventoryData != null)
                {
                    InitInventoryItemControllers();
                }
                else
                {
                    Debug.LogError("Inventory data is null after loading from JSON");
                    CreateNewInventory();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading inventory: " + e.Message);
                CreateNewInventory();
            }
        }
        else
        {
            Debug.Log("Inventory file not found, creating new inventory");
            CreateNewInventory();
        }
    }

    public async void InitInventoryItemControllers()
    {
        foreach (var itemData in inventoryData.items)
        {
            if (
                inventoryItemControllers.FirstOrDefault(item => item.itemData.id == itemData.id)
                != null
            )
            {
                continue; // Skip if item already exists
            }
            InventoryItemController itemController = Instantiate(itemPrefab, itemContainer);
            itemController.Init(itemData);

            // Check if item should be inactive (level == 1 and currentStackSize == 0)
            if (itemData.GetLevel() == 1 && itemData.currentStackSize == 0)
            {
                itemController.SetInactive();
                itemController.isItemActive = false;
            }

            inventoryItemControllers.Add(itemController);
        }
    }

    private void CreateNewInventory()
    {
        inventoryData = new ListInventoryItemData();
        var gearDataSO = GameManager.Instance.gearDataSO;
        foreach (var gearData in gearDataSO.gearDataList)
        {
            if (gearData.hideInInventory)
                continue; // Skip items that should not be shown in inventory
            InventoryItemData newItem = new InventoryItemData
            {
                id = gearData.id,
                name = gearData.itemName,
                description = gearData.description,
                maxStackSize = 1,
                currentStackSize = gearData.id < 4 ? 1 : 0,
                gearData = new GearData(),
            };
            newItem.gearData.Copy(gearData);
            inventoryData.items.Add(newItem);
        }
        InitInventoryItemControllers();
        SaveInventory();
    }

    public void SaveInventory()
    {
        try
        {
            int giftAmount = inventoryData.giftAmount;
            inventoryData = new ListInventoryItemData();
            foreach (var item in inventoryItemControllers)
            {
                if (item != null && item.itemData != null)
                {
                    inventoryData.items.Add(item.itemData);
                }
            }
            inventoryData.giftAmount = giftAmount;
            string jsonData = JsonUtility.ToJson(inventoryData, true);
            File.WriteAllText(saveFilePath, jsonData);
            Debug.Log("Inventory saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving inventory: " + e.Message);
        }
    }

    public bool HasInventoryData(GearData gearData)
    {
        if (gearData == null)
        {
            Debug.LogError("GearData is null");
            return false;
        }
        if (inventoryData == null)
        {
            Debug.LogError("Inventory data is null");
            return false;
        }
        var existingItem = inventoryData.items.FirstOrDefault(item =>
            item.gearData.id == gearData.id
        );
        if (existingItem == null)
        {
            Debug.LogWarning($"No item found in inventory with GearData ID: {gearData.id}");
            return false;
        }
        // Check if the item has a stack size greater than 0 or level greater than 1
        if (existingItem.currentStackSize > 0 || existingItem.GetLevel() > 1)
        {
            return true;
        }
        return false;
    }

    [ContextMenu("Update Gift Text")]
    public void UpdateGiftText()
    {
        if (_giftText != null && inventoryData != null)
        {
            _giftText.text =
                inventoryData.giftAmount > 0 ? inventoryData.giftAmount.ToString() : "";
            Debug.Log("Gift text updated: " + inventoryData.giftAmount);
        }
    }

    public ListInventoryItemData GetInventoryData()
    {
        return inventoryData;
    }

    public void AddGift(int amount)
    {
        int currentAmount = inventoryData.giftAmount;
        inventoryData.giftAmount = Mathf.Clamp(currentAmount + amount, 0, 999);
        SaveInventory();

        Invoke(nameof(UpdateGiftText), 1f);
        giftEffect.emission.SetBurst(0, new ParticleSystem.Burst(amount, 1));
        giftEffect.Play();
    }

    public async Task ShowRewardCards()
    {
        System.Random random = new System.Random();
        List<InventoryItemData> rewardItems = new List<InventoryItemData>();
        int cardAmount = random.Next(0, inventoryData.giftAmount); // Randomly choose between 1 and 10 cards
        Debug.Log($"Showing {cardAmount} reward cards of {inventoryData.giftAmount} total gifts");
        if (inventoryData != null)
        {
            // Check if there are any items with level > 1 and current stack > 0
            bool hasLeveledItems = false;
            foreach (var item in inventoryData.items)
            {
                if (item.GetLevel() > 1 || item.currentStackSize > 0)
                {
                    hasLeveledItems = true;
                    break;
                }
            }

            if (!hasLeveledItems)
            {
                // Spawn cards with IDs from 0 to 3
                for (int id = 0; id <= 3; id++)
                {
                    var item = inventoryData.items.FirstOrDefault(i => i.id == id);
                    if (item != null)
                    {
                        rewardItems.Add(item);
                    }
                }
            }
            else
            {
                for (int i = 0; i < cardAmount; i++)
                {
                    int randomIndex = random.Next(inventoryData.items.Count);
                    InventoryItemData itemData = inventoryData.items[randomIndex];
                    if (!rewardItems.Exists(item => item.id == itemData.id))
                    {
                        rewardItems.Add(itemData);
                    }
                }
            }
        }
        if (rewardItems.Count > 0)
        {
            rewardItems.Sort((a, b) => b.gearData.rarity.CompareTo(a.gearData.rarity));
            for (int i = 0; i < rewardItems.Count; i++)
            {
                await Task.Delay(50); // Delay to simulate card reveal
                InventoryItemData itemData = rewardItems[i];
                InventoryItemCard card = Instantiate(cardPrefab, cardContainer)
                    .GetComponent<InventoryItemCard>();
                card.itemData.CopyFrom(itemData);
                card.ShowCard(itemData);
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    -1000,
                    random.Next(-200, 200)
                );
                card.GetComponent<RectTransform>()
                    .DOAnchorPos(new Vector2(random.Next(-200, 100), random.Next(-200, 200)), 0.7f)
                    .SetEase(Ease.OutQuint);
                card.transform.DOLocalRotate(new Vector3(0, 0, random.Next(-50, 50)), 0.4f)
                    .SetEase(Ease.OutBack);
                inventoryItemCards.Add(card);
                inventoryData.giftAmount--;
                UpdateGiftText();
                SaveInventory();
            }
        }
        int randomCoin = inventoryData.giftAmount;
        Debug.Log($"Random coins to spawn: {randomCoin} while {inventoryData.giftAmount} cards exist");
        if (randomCoin <= 0 && inventoryItemCards.Count == 0)
        {
            ShowTopPopup();
            return; // No coins if no items to show
        }
        for (int i = 0; i < randomCoin; i++)
        {
            var coin = Instantiate(coinPrefab, cardContainer);
            coin.transform.SetParent(cardContainer);
            coin.transform.localScale = Vector3.one;
            coin.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                -1000,
                random.Next(-200, 200)
            );
            coin.GetComponent<RectTransform>()
                .DOAnchorPos(new Vector2(random.Next(-200, 100), random.Next(-200, 200)), 0.7f)
                .SetEase(Ease.OutQuint);
            coin.transform.DOLocalRotate(new Vector3(0, 0, random.Next(-50, 50)), 0.4f)
                .SetEase(Ease.OutBack);
            coin.isCoin = true;
            coin.canClick = true;
            inventoryItemCards.Add(coin);
            inventoryData.giftAmount--;
            UpdateGiftText();
            SaveInventory();
        }
    }
}

[Serializable]
public class ListInventoryItemData
{
    public List<InventoryItemData> items = new List<InventoryItemData>();
    public int giftAmount;
}

[Serializable]
public class InventoryItemData
{
    public int id;
    public string name;
    public string description;
    public int maxStackSize;
    public int currentStackSize;
    public GearData gearData;

    public void CopyFrom(InventoryItemData other)
    {
        id = other.id;
        name = other.name;
        description = other.description;
        maxStackSize = other.maxStackSize;
        currentStackSize = other.currentStackSize;
        gearData.Copy(other.gearData);
    }

    public int GetLevel()
    {
        if (gearData == null)
        {
            return 0;
        }
        return (int)gearData.GetCustomValue("level");
    }

    public Sprite GetIcon()
    {
        if (gearData == null)
        {
            return null;
        }
        return Resources.Load<Sprite>("Sprites/" + gearData.iconName);
    }
}
