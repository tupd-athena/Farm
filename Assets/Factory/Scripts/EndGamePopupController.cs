using System.Collections;
using System.Collections.Generic;
using Factory;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePopupController : MonoBehaviour
{
    [SerializeField]
    private Transform _popupPanel;

    [SerializeField]
    private TMPro.TMP_Text _timeText;

    [SerializeField]
    private TMPro.TMP_Text _fedfishText;

    [SerializeField]
    private TMPro.TMP_Text _starvedFishText;

    [SerializeField]
    private TMPro.TMP_Text _coinAccumulatedText;

    [SerializeField]
    private TMPro.TMP_Text _ticketAmountText;

    [SerializeField]
    private TMPro.TMP_Text _diamondAmountText;

    [SerializeField]
    private GameObject _ticketObject;

    [SerializeField]
    private GameObject _diamondObject;

    [SerializeField]
    private Button endGameButton;

    [SerializeField]
    private Button x2RewardsButton;

    public System.Action OnEndGameButtonClick;
    public System.Action OnX2RewardsButtonClick;

    private void Start()
    {
        endGameButton.onClick.AddListener(OnEndGameButtonClicked);
        x2RewardsButton.onClick.AddListener(OnX2RewardsButtonClicked);
    }

    public void ShowPopup()
    {
        _popupPanel.gameObject.SetActive(true);
        UpdateTexts();
    }

    public void HidePopup()
    {
        _popupPanel.gameObject.SetActive(false);
    }

    private void UpdateTexts()
    {
        _timeText.text = $"Time: {GamePlayTracking.Instance.GetTotalTime()}";
        _fedfishText.text =
            $"Fed Fish: {GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.FED_FISH_COUNT)}";
        _starvedFishText.text =
            $"Starved Fish: {GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.STARVED_FISH_COUNT)}";
        _coinAccumulatedText.text =
            $"Coins: {GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.TOTAL_COINS)}";

        _ticketAmountText.text =
            $"{GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.TICKET_AMOUNT)}";
        _diamondAmountText.text =
            $"{GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.DIAMOND_AMOUNT)}";
        _ticketObject.SetActive(
            GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.TICKET_AMOUNT) > 0
        );
        _diamondObject.SetActive(
            GamePlayTracking.Instance.GetValueByKey(GamePlayTracking.DIAMOND_AMOUNT) > 0
        );
    }

    private void OnEndGameButtonClicked()
    {
        HidePopup();
        OnEndGameButtonClick?.Invoke();
        GameManager.Instance.ChangeGameState(GameStateType.None);
        GameManager.Instance.OnGameStart = null;

        Debug.Log("Game completely ended and reset to initial state");

        GameManager.Instance.homeUI.NavigationBar.ShowAllTabsAndBar();
    }

    private void OnX2RewardsButtonClicked()
    {
        HidePopup();
        OnX2RewardsButtonClick?.Invoke();
    }
}
