using System;
using System.Collections;
using System.Collections.Generic;
using Factory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private GameObject _frame;

    [SerializeField]
    private Slider _progressBar;

    [SerializeField]
    private TMP_Text _progressText;

    [SerializeField]
    private TMP_Text _levelText;

    [SerializeField]
    private Image _itemIcon;
    [SerializeField]
    private Image _backgroundImage;

    [SerializeField]
    private Sprite _iconQuestion;

    public ParticleSystem sparkleEffect;

    public InventoryItemData itemData;
    public bool isItemActive = true; // Track if item is active

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
        SetLevelText(itemData.GetLevel());
        UpdateProgressbar();
        SetItemIcon(itemData.GetIcon());
        SetBackgroundImage();
    }

    public void SetLevelText(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = $"{level}";
            itemData.maxStackSize = (int)Mathf.Max(1, Mathf.Pow(2, level - 1));
        }
    }

    public void SetItemIcon(Sprite icon)
    {
        if (_itemIcon != null)
        {
            _itemIcon.sprite = icon;
        }
    }

    public void SetBackgroundImage()
    {
        if (_backgroundImage != null && itemData != null && itemData.gearData != null)
        {
            _backgroundImage.sprite = GameManager
                .Instance.GetGearRarityData(itemData.gearData.rarity)
                .icon;
        }
    }

    public void UpdateProgressbar()
    {
        if (_progressBar != null)
        {
            _progressBar.value = Mathf.Clamp01(
                (float)itemData.currentStackSize / itemData.maxStackSize
            );
        }
        if (_progressText != null)
        {
            _progressText.text = $"{itemData.currentStackSize}/{itemData.maxStackSize}";
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
        if (!isItemActive)
        {
            // If item is inactive, do not process click
            return;
        }
        InventoryManager.Instance.ItemClicked(this);
    }

    public void SetInactive()
    {
        // Make item visually inactive but still clickable
        if (_itemIcon != null)
        {
            _itemIcon.color = new Color(1f, 1f, 1f, 1f);
            if (_iconQuestion != null)
            {
                _itemIcon.sprite = _iconQuestion;
            }
        }
        if (_progressBar != null)
        {
            _progressBar.gameObject.SetActive(false);
        }
        if (_progressText != null)
        {
            _progressText.gameObject.SetActive(false);
        }
        if (_levelText != null)
        {
            _levelText.gameObject.SetActive(false);
        }
    }

    public void SetActive()
    {
        // Restore item to normal visual state
        if (_itemIcon != null)
        {
            _itemIcon.color = Color.white;
            SetItemIcon(itemData.GetIcon());
        }
        if (_progressBar != null)
        {
            _progressBar.gameObject.SetActive(true);
        }
        if (_progressText != null)
        {
            _progressText.gameObject.SetActive(true);
        }
        if (_levelText != null)
        {
            _levelText.gameObject.SetActive(true);
        }
    }
}
