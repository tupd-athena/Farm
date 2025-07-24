using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartPopupController : MonoBehaviour
{
    public Transform popupPanel;
    public TMP_Text dayText;
    public TMP_Text fishText;
    public TMP_Text goldText;
    public TMP_Text bossText;
    public TMP_Text progressText;

    public System.Action OnPlayButtonClick;

    public void Start()
    {
        UpdateTexts();
    }

    public void ShowPopup()
    {
        popupPanel.gameObject.SetActive(true);
        UpdateTexts();
    }

    public void HidePopup()
    {
        popupPanel.gameObject.SetActive(false);
    }

    public void PlayButtonClicked()
    {
        HidePopup();
        OnPlayButtonClick?.Invoke();
    }

    public void UpdateTexts()
    {
        var highestDay = GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.HIGHEST_DAY_KEY);
        var highestFish = GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.HIGHEST_FISH);
        var highestGold = GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.HIGHEST_GOLD);
        var highestBosses = GamePlayTracking.Instance.GetValueByKey(
            GamePlayTracking.HIGHEST_BOSSES
        );
        dayText.text = $"Highest Day: {highestDay}";
        fishText.text = $"Highest Fish: {highestFish}";
        goldText.text = $"Highest Gold: {highestGold}";
        bossText.text = $"Highest Bosses: {highestBosses}";
        progressText.text = $"{Mathf.Clamp(highestDay, 1, 50)} / {50}";
    }
}
