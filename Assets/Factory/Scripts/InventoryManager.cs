using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Factory;
using TMPro;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

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

    public List<InventoryItemController> inventoryItemControllers =
        new List<InventoryItemController>();

    private ListInventoryItemData inventoryData;
    public InventoryItemController itemPrefab;
    public Transform itemContainer;
    private string saveFilePath;

    public System.Action<InventoryItemController> OnItemClicked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            saveFilePath = Path.Combine(Application.persistentDataPath, "inventory.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        OnItemClicked += SetCurrentItem;
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
        itemController.ShowFrame();
        SetIcon(itemController.itemData.GetIcon());
        SetLevelText(itemController.itemData.GetLevel());
        SetNameText(itemController.itemData.name);
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

    void OnEnable()
    {
        LoadInventory();
    }

    public void ItemClicked(InventoryItemController itemController)
    {
        OnItemClicked?.Invoke(itemController);
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
        inventoryItemControllers.Clear();
        foreach (var itemData in inventoryData.items)
        {
            InventoryItemController itemController = Instantiate(itemPrefab, itemContainer);
            itemController.Init(itemData);
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
                description = gearData.itemName,
                maxStackSize = 1,
                currentStackSize = 0,
                gearData = new GearData(),
            };
            newItem.gearData.Copy(gearData);
            inventoryData.items.Add(newItem);
        }
        SaveInventory();
        LoadInventory();
    }

    public void SaveInventory()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(inventoryData, true);
            File.WriteAllText(saveFilePath, jsonData);
            Debug.Log("Inventory saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving inventory: " + e.Message);
        }
    }

    public ListInventoryItemData GetInventoryData()
    {
        return inventoryData;
    }
}

[Serializable]
public class ListInventoryItemData
{
    public List<InventoryItemData> items = new List<InventoryItemData>();
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
        return gearData.level;
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
