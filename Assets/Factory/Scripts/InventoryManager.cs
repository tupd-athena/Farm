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

    public Transform treasuresGUI;
    public Transform normalTreasuresPopup;
    public Transform specialTreasuresPopup;

    public Transform inventoryGUI;

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
    private TMP_Text _ticketAmountText;

    [SerializeField]
    private TMP_Text _diamondText;

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

    // Properties for tickets and diamonds using JSON save system
    public int Tickets
    {
        get { return inventoryData?.tickets ?? 0; }
        private set
        {
            if (inventoryData != null)
            {
                inventoryData.tickets = value;
                SaveInventory();
            }
        }
    }

    public int Diamonds
    {
        get { return inventoryData?.diamonds ?? 0; }
        private set
        {
            if (inventoryData != null)
            {
                inventoryData.diamonds = value;
                SaveInventory();
            }
        }
    }

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
        UpdateTicketText();
        UpdateDiamondText();
        OnItemClicked += SetCurrentItem;
        OnCardClicked += (card) =>
        {
            if (card != null && card.itemData != null)
            {
                // COMMENTED OUT GOLD/COIN LOGIC - Only item rewards now
                // if (card.isCoin)
                // {
                //     System.Random random = new System.Random();
                //     int gold = random.Next(1, 6);
                //     GameManager.Instance.AddGold(gold);
                // }
                // else
                // {
                InventoryItemController itemController = inventoryItemControllers.Find(item =>
                    item.itemData.id == card.itemData.id
                );
                if (itemController != null)
                {
                    SetStackSizeOfItem(itemController);
                    itemController.UpdateProgressbar();
                    itemController.sparkleEffect.Play();
                }
                // }
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
                card.transform.DOScale(Vector3.zero, 0f).SetEase(Ease.InCirc).SetDelay(1f);
                
                // Check if there are any more clickable cards after a delay
                DOVirtual.DelayedCall(1.5f, () =>
                {
                    CheckAndShowTreasuresIfNoClickableCards();
                });
            }
        };
    }

    public void SwapTreasuresPopup(bool isSpecial)
    {
        if (isSpecial)
        {
            normalTreasuresPopup.gameObject.SetActive(false);
            specialTreasuresPopup.gameObject.SetActive(true);
        }
        else
        {
            normalTreasuresPopup.gameObject.SetActive(true);
            specialTreasuresPopup.gameObject.SetActive(false);
        }
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
        inventoryGUI.gameObject.SetActive(true);
        if (_inventoryBottomPopup != null)
        {
            _inventoryBottomPopup.SetActive(true);
            _inventoryBottomPopup.transform.localScale = Vector3.one;
            LoadInventory();

            // Set first item as current item and show top popup
            if (inventoryItemControllers.Count > 0)
            {
                SetCurrentItem(inventoryItemControllers[0]);
                ShowTopPopup();
            }
        }
    }

    public async Task Close()
    {
        inventoryItemControllers.ForEach(item => item.HideFrame());
        inventoryItemCards.ForEach(card => Destroy(card.gameObject));
        inventoryItemCards.Clear();

        _inventoryBottomPopup.SetActive(false);
        inventoryGUI.gameObject.SetActive(false);
    }

    public void ShowTopPopup()
    {
        if (_inventoryTopPopup != null)
        {
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

    public void InitInventoryItemControllers()
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
            // Preserve current tickets and diamonds values
            int currentTickets = inventoryData?.tickets ?? 0;
            int currentDiamonds = inventoryData?.diamonds ?? 0;

            inventoryData = new ListInventoryItemData();
            foreach (var item in inventoryItemControllers)
            {
                if (item != null && item.itemData != null)
                {
                    inventoryData.items.Add(item.itemData);
                }
            }

            // Restore tickets and diamonds
            inventoryData.tickets = currentTickets;
            inventoryData.diamonds = currentDiamonds;

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

    [ContextMenu("Update Ticket Text")]
    public void UpdateTicketText()
    {
        if (_ticketAmountText != null && inventoryData != null)
        {
            _ticketAmountText.text = Tickets > 0 ? Tickets.ToString() : "0";
            Debug.Log("Ticket text updated: " + Tickets);
        }
    }

    [ContextMenu("Update Diamond Text")]
    public void UpdateDiamondText()
    {
        if (_diamondText != null && inventoryData != null)
        {
            _diamondText.text = Diamonds > 0 ? Diamonds.ToString() : "0";
            Debug.Log("Diamond text updated: " + Diamonds);
        }
    }

    public ListInventoryItemData GetInventoryData()
    {
        return inventoryData;
    }

    public void RollNormalCards(int amount = 1)
    {
        // Check if we have enough tickets
        if (Tickets < amount)
        {
            Debug.LogWarning($"Not enough tickets to roll {amount} cards. Available: {Tickets}");
            return;
        }

        // Spend tickets based on amount
        if (SpendTickets(amount))
        {
            if (amount == 1)
            {
                Roll1Card();
            }
            else if (amount == 10)
            {
                Roll10Cards();
            }
        }
    }

    public void RollDiamondCards(int amount)
    {
        int diamondCost = amount * 10; // Each roll costs 10 diamonds
        
        // Check if we have enough diamonds
        if (Diamonds < diamondCost)
        {
            Debug.LogWarning($"Not enough diamonds to roll {amount} cards. Need: {diamondCost}, Available: {Diamonds}");
            return;
        }

        // Spend diamonds based on amount x 10
        if (SpendDiamonds(diamondCost))
        {
            if (amount == 1)
            {
                Roll1Card();
            }
            else if (amount == 10)
            {
                Roll10Cards();
            }
        }
    }

    public async Task Roll1Card()
    {
        // Hide treasure popups when rolling
        HidePopups();

        System.Random random = new System.Random();

        // 50% chance for gold, 50% chance for card - COMMENTED OUT GOLD LOGIC
        // bool isGold = random.Next(0, 2) == 0;

        // if (isGold)
        // {
        //     // Give random gold between 1-10
        //     int goldAmount = random.Next(1, 11);
        //     GameManager.Instance.AddGold(goldAmount);
        //     Debug.Log($"Rolled gold: {goldAmount}");

        //     // Create and show coin card
        //     var coin = Instantiate(coinPrefab, cardContainer);
        //     coin.transform.SetParent(cardContainer);
        //     coin.transform.localScale = Vector3.one;
        //     coin.GetComponent<RectTransform>().anchoredPosition = new Vector2(
        //         -1000,
        //         random.Next(-200, 200)
        //     );
        //     coin.GetComponent<RectTransform>()
        //         .DOAnchorPos(new Vector2(random.Next(-200, 100), random.Next(-200, 200)), 0.7f)
        //         .SetEase(Ease.OutQuint);
        //     coin.transform.DOLocalRotate(new Vector3(0, 0, random.Next(-50, 50)), 0.4f)
        //         .SetEase(Ease.OutBack);
        //     coin.isCoin = true;
        //     coin.canClick = true;
        //     inventoryItemCards.Add(coin);
        // }
        // else
        // {
        // Always give card instead of random gold/card
        int randomIndex = random.Next(inventoryData.items.Count);
        InventoryItemData itemData = inventoryData.items[randomIndex];

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

        Debug.Log($"Rolled card: {itemData.name}");
        // }

        await Task.Delay(100); // Small delay for animation
    }

    public async Task Roll10Cards()
    {
        // Hide treasure popups when rolling
        HidePopups();

        System.Random random = new System.Random();

        for (int i = 0; i < 10; i++)
        {
            // 50% chance for gold, 50% chance for card for each roll - COMMENTED OUT GOLD LOGIC
            // bool isGold = random.Next(0, 2) == 0;

            // if (isGold)
            // {
            //     // Give random gold between 1-10
            //     int goldAmount = random.Next(1, 11);
            //     GameManager.Instance.AddGold(goldAmount);
            //     Debug.Log($"Roll {i + 1}: Rolled gold: {goldAmount}");

            //     // Create and show coin card
            //     var coin = Instantiate(coinPrefab, cardContainer);
            //     coin.transform.SetParent(cardContainer);
            //     coin.transform.localScale = Vector3.one;
            //     coin.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            //         -1000,
            //         random.Next(-200, 200)
            //     );
            //     coin.GetComponent<RectTransform>()
            //         .DOAnchorPos(new Vector2(random.Next(-200, 100), random.Next(-200, 200)), 0.7f)
            //         .SetEase(Ease.OutQuint);
            //     coin.transform.DOLocalRotate(new Vector3(0, 0, random.Next(-50, 50)), 0.4f)
            //         .SetEase(Ease.OutBack);
            //     coin.isCoin = true;
            //     coin.canClick = true;
            //     inventoryItemCards.Add(coin);
            // }
            // else
            // {
            // Always give card instead of random gold/card
            int randomIndex = random.Next(inventoryData.items.Count);
            InventoryItemData itemData = inventoryData.items[randomIndex];

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

            Debug.Log($"Roll {i + 1}: Rolled card: {itemData.name}");
            // }

            // Add delay between each card reveal
            await Task.Delay(150);
        }
    }

    public async Task ShowRewardCards()
    {
        // Hide treasure popups when showing reward cards
        HidePopups();
        
        System.Random random = new System.Random();
        List<InventoryItemData> rewardItems = new List<InventoryItemData>();
        int cardAmount = random.Next(0, Diamonds); // Use new Diamonds property
        Debug.Log($"Showing {cardAmount} reward cards of {Diamonds} total diamonds");
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
                SpendDiamonds(1); // Use new diamond system
            }
        }
        // COMMENTED OUT GOLD/COIN LOGIC - Only item rewards now
        // int remainingDiamonds = Diamonds;
        // Debug.Log($"Random coins to spawn: {remainingDiamonds} while {Diamonds} diamonds exist");
        // if (remainingDiamonds <= 0 && inventoryItemCards.Count == 0)
        // {
        //     ShowTopPopup();
        //     return; // No coins if no items to show
        // }
        // for (int i = 0; i < remainingDiamonds; i++)
        // {
        //     var coin = Instantiate(coinPrefab, cardContainer);
        //     coin.transform.SetParent(cardContainer);
        //     coin.transform.localScale = Vector3.one;
        //     coin.GetComponent<RectTransform>().anchoredPosition = new Vector2(
        //         -1000,
        //         random.Next(-200, 200)
        //     );
        //     coin.GetComponent<RectTransform>()
        //         .DOAnchorPos(new Vector2(random.Next(-200, 100), random.Next(-200, 200)), 0.7f)
        //         .SetEase(Ease.OutQuint);
        //     coin.transform.DOLocalRotate(new Vector3(0, 0, random.Next(-50, 50)), 0.4f)
        //         .SetEase(Ease.OutBack);
        //     coin.isCoin = true;
        //     coin.canClick = true;
        //     inventoryItemCards.Add(coin);
        // }
        // // Spend all remaining diamonds at once
        // if (remainingDiamonds > 0)
        // {
        //     SpendDiamonds(remainingDiamonds);
        // }
    }

    // Methods to manage tickets
    public void AddTickets(int amount)
    {
        if (amount > 0)
        {
            int newAmount = Mathf.Clamp(Tickets + amount, 0, 999999);
            Tickets = newAmount;
            Debug.Log($"Added {amount} tickets. Total: {Tickets}");
            UpdateTicketText(); // Update UI when tickets change
        }
    }

    public bool SpendTickets(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot spend negative or zero tickets");
            return false;
        }

        if (Tickets >= amount)
        {
            Tickets = Tickets - amount;
            Debug.Log($"Spent {amount} tickets. Remaining: {Tickets}");
            UpdateTicketText(); // Update UI when tickets change
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough tickets. Need: {amount}, Have: {Tickets}");
            return false;
        }
    }

    // Methods to manage diamonds
    public void AddDiamonds(int amount)
    {
        if (amount > 0)
        {
            int newAmount = Mathf.Clamp(Diamonds + amount, 0, 999999);
            Diamonds = newAmount;
            Debug.Log($"Added {amount} diamonds. Total: {Diamonds}");
            UpdateDiamondText(); // Update UI when diamonds change
        }
    }

    public bool SpendDiamonds(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot spend negative or zero diamonds");
            return false;
        }

        if (Diamonds >= amount)
        {
            Diamonds = Diamonds - amount;
            Debug.Log($"Spent {amount} diamonds. Remaining: {Diamonds}");
            UpdateDiamondText(); // Update UI when diamonds change
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough diamonds. Need: {amount}, Have: {Diamonds}");
            return false;
        }
    }

    // Get current amounts
    public int GetTickets()
    {
        return Tickets;
    }

    public int GetDiamonds()
    {
        return Diamonds;
    }

    // Context menu methods for testing
    [ContextMenu("Add 10 Tickets")]
    public void DebugAddTickets()
    {
        AddTickets(10);
    }

    [ContextMenu("Add 5 Diamonds")]
    public void DebugAddDiamonds()
    {
        AddDiamonds(5);
    }

    [ContextMenu("Show Currency")]
    public void DebugShowCurrency()
    {
        Debug.Log($"Tickets: {Tickets}, Diamonds: {Diamonds}");
    }

    public void ShowTreasures()
    {
        if (treasuresGUI != null)
        {
            treasuresGUI.gameObject.SetActive(true);
            normalTreasuresPopup.gameObject.SetActive(true);
            specialTreasuresPopup.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Treasures GUI is not set up correctly.");
        }
    }

    public void HideTreasures()
    {
        if (treasuresGUI != null)
        {
            treasuresGUI.gameObject.SetActive(false);
            normalTreasuresPopup.gameObject.SetActive(false);
            specialTreasuresPopup.gameObject.SetActive(false);
        }
    }

    public void HidePopups() // normal and special treasures
    {
        if (treasuresGUI != null)
        {
            normalTreasuresPopup.gameObject.SetActive(false);
            specialTreasuresPopup.gameObject.SetActive(false);
        }
    }

    public void ShowSpecialTreasures()
    {
        if (treasuresGUI != null)
        {
            treasuresGUI.gameObject.SetActive(true);
            normalTreasuresPopup.gameObject.SetActive(false);
            specialTreasuresPopup.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Treasures GUI is not set up correctly.");
        }
    }

    public void HideSpecialTreasures()
    {
        if (treasuresGUI != null)
        {
            specialTreasuresPopup.gameObject.SetActive(false);
        }
    }

    // Check if there are any clickable cards remaining, if not show treasure popups
    private void CheckAndShowTreasuresIfNoClickableCards()
    {
        // Count clickable cards (cards that have canClick = true and are interactable)
        int clickableCards = 0;
        foreach (var card in inventoryItemCards)
        {
            if (card != null && card.canClick && card.GetComponent<CanvasGroup>().interactable)
            {
                clickableCards++;
            }
        }

        // If no clickable cards remain, show treasure popups
        if (clickableCards == 0)
        {
            Debug.Log("No more clickable cards found, showing treasure popups");
            ShowTreasures();
        }
    }
}

[Serializable]
public class ListInventoryItemData
{
    public List<InventoryItemData> items = new List<InventoryItemData>();
    public int diamonds;
    public int tickets;
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
