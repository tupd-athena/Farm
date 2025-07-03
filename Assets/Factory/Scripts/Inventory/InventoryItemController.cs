using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factory;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryItemController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private GameObject _frame;
    [SerializeField]
    private Slider _progressBar;
    [SerializeField]
    private TMP_Text _progressText;
    [SerializeField]
    private Image _itemIcon;

    public InventoryItemData itemData;

    public Slider ProgressBar
    {
        get { return _progressBar; }
        set { _progressBar = value; }
    }

    public void Init(InventoryItemData data)
    {
        if (data == null)
        {
            Debug.LogError("InventoryItemData is null");
            return;
        }
        itemData.CopyFrom(data);
        HideFrame();
        UpdateProgressbar(itemData.currentStackSize, itemData.maxStackSize);
        SetItemIcon(itemData.GetIcon());
    }

    public void SetItemIcon(Sprite icon)
    {
        if (_itemIcon != null)
        {
            _itemIcon.sprite = icon;
        }
    }

    public void UpdateProgressbar(int currentValue, int maxValue)
    {
        if (_progressBar != null)
        {
            _progressBar.value = (float)currentValue / maxValue;
        }
        if (_progressText != null)
        {
            _progressText.text = $"{currentValue}/{maxValue}";
        }
    }

    public void HideFrame()
    {
        if (_frame != null)
        {
            _frame.SetActive(false);
        }
    }
    public void ShowFrame()
    {
        if (_frame != null)
        {
            _frame.SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.ItemClicked(this);
    }
}
