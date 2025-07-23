using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Athena.Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Factory
{
    /// <summary>
    /// Refactored HomeUI controller with improved organization, performance, and maintainability.
    ///
    /// Key Improvements:
    /// - Organized code into logical regions
    /// - Cached frequently accessed components
    /// - Eliminated magic numbers with constants
    /// - Created reusable animation methods
    /// - Reduced memory allocations with StringBuilder
    /// - Simplified conditional logic with ternary operators
    /// - Added proper async/await for inventory operations
    /// - Consistent naming conventions
    /// </summary>
    public class HomeUI : UIController
    {
        #region Animation Constants
        private const float FADE_DURATION = 0.5f;
        private const float SCALE_DURATION = 0.1f;
        private const float SCALE_INTENSITY = 1.05f;
        private const int WARNING_LOOPS = 2;
        private const int TYPING_DELAY_MS = 50;
        private const int REROLL_COST = 5;
        private const int MAX_FISH_DISPLAY = 9999;
        private const float GEAR_OFFSET_MAIN = -500f;
        private const float GEAR_OFFSET_SHOP = -550f;
        #endregion

        #region UI References - Core
        [Header("Core UI")]
        [SerializeField]
        private GameObject _shopPopup;

        [SerializeField]
        private InventoryManager _inventoryManager;

        public Button buttonQuit;

        [SerializeField]
        private RecycleBin _recycleBin;

        [SerializeField]
        private Button _startButton;

        [SerializeField]
        private Button _rerollButton;

        [SerializeField]
        private GameObject _artifactPopup;
        [SerializeField]
        private NavigationBar _navigationBar;

        [SerializeField]
        private StartPopupController _startPopupController;

        [SerializeField]
        private GameObject _wavePanel;
        [SerializeField]
        private GameObject _ticketPanel;
        [SerializeField]
        private EndGamePopupController _endGamePopupController;
        #endregion

        #region UI References - Text Elements
        [Header("Text Elements")]
        [SerializeField]
        private TMP_Text _goldText;
        [SerializeField]
        private TMP_Text _diamondText;

        [SerializeField]
        private TMP_Text _totalFishText;

        [SerializeField]
        private TMP_Text _notReadyFishAmountText;

        [SerializeField]
        private TMP_Text _waveText;

        [SerializeField]
        private TMP_Text _gameStartLevelText;

        [SerializeField]
        private TMP_Text _gameStartDayText;
        #endregion

        #region UI References - Visual Elements
        [Header("Visual Elements")]
        [SerializeField]
        private Slider _levelProgressSlider;

        [SerializeField]
        private RectTransform _goldContainer;

        [SerializeField]
        private Image _warningEffect;

        [SerializeField]
        private Transform _boardTempContainer;

        [SerializeField]
        private RectTransform _gameStartPanel;

        [SerializeField]
        private RectTransform _indicatorLeft;

        [SerializeField]
        private RectTransform _indicatorRight;
        #endregion

        #region UI References - Collections
        [Header("Collections")]
        [SerializeField]
        private List<ShopItem> _shopItems = new List<ShopItem>();

        [SerializeField]
        private List<ArtifactController> _artifacts = new List<ArtifactController>();
        #endregion

        #region Cached Components
        private Image _goldContainerImage;
        private RectTransform _gearItemContainerTransform;
        #endregion

        #region Properties
        public InventoryManager InventoryManager => _inventoryManager;
        public Button StartButton => _startButton;
        public List<ShopItem> ShopItems => _shopItems;
        public List<ArtifactController> Artifacts => _artifacts;
        public TMP_Text TotalFishText => _totalFishText;
        public TMP_Text NotReadyFishAmountText => _notReadyFishAmountText;
        public Transform BoardTempContainer => _boardTempContainer;
        public TMP_Text TotalGoldText => _goldText;
        public TMP_Text GameStartLevelText => _gameStartLevelText;
        public TMP_Text GameStartDayText => _gameStartDayText;
        public RectTransform IndicatorLeft => _indicatorLeft;
        public RectTransform IndicatorRight => _indicatorRight;
        public RectTransform GameStartPanel => _gameStartPanel;
        public NavigationBar NavigationBar => _navigationBar;
        public StartPopupController StartPopupController => _startPopupController;
        public GameObject WavePanel => _wavePanel;
        public RectTransform GoldContainer => _goldContainer;
        public GameObject TicketPanel => _ticketPanel;
        public EndGamePopupController EndGamePopupController => _endGamePopupController;
        #endregion

        #region State
        private bool _isLockFirstOpenShop = false;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        #endregion

        #region Unity Lifecycle
        void Start()
        {
            InitializeComponents();
            SetupEventListeners();
        }

        private void InitializeComponents()
        {
            // Cache frequently accessed components
            _goldContainerImage = _goldContainer.GetComponent<Image>();
            _gearItemContainerTransform =
                GameManager.Instance.GearItemContainer.GetComponent<RectTransform>();
        }

        private void SetupEventListeners()
        {
            _rerollButton.onClick.AddListener(OnRerollButtonClick);
            _startButton.onClick.AddListener(OnStartButtonClick);
        }
        #endregion

        #region Button State Management
        public void LockStartButton()
        {
            _isLockFirstOpenShop = true;
            SetButtonsInteractable(false);
        }

        public void UnlockStartButton()
        {
            _isLockFirstOpenShop = false;
            SetButtonsInteractable(true);
            SetLockRerollButton(GameManager.Instance.CheckGold(REROLL_COST));
        }

        private void SetButtonsInteractable(bool interactable)
        {
            _startButton.interactable = interactable;
            _rerollButton.interactable = interactable;
        }

        public void SetLockRerollButton(bool state)
        {
            if (_isLockFirstOpenShop)
                return;
            _rerollButton.interactable = state;
        }
        #endregion

        #region Warning Effects & Animations
        public void ShowWarningEffect()
        {
            _warningEffect.DOComplete();
            _warningEffect.color = new Color(1, 0.2f, 0.2f, 0);
            _warningEffect.gameObject.SetActive(true);

            // Chain warning animations more efficiently
            var sequence = DOTween.Sequence();
            for (int i = 0; i < 3; i++)
            {
                sequence
                    .AppendCallback(() => AudioManager.Instance.PlaySound("Warning"))
                    .Append(
                        _warningEffect
                            .DOFade(1, FADE_DURATION)
                            .SetEase(Ease.InSine)
                            .SetLoops(WARNING_LOOPS, LoopType.Yoyo)
                    );
            }
            sequence.OnComplete(() => _warningEffect.gameObject.SetActive(false));
        }

        public void OffsetGearItemContainer()
        {
            var targetY =
                GameManager.Instance.GameState.CurrentState == GameStateType.Main
                    ? GEAR_OFFSET_MAIN
                    : GEAR_OFFSET_SHOP;

            _gearItemContainerTransform.DOAnchorPosY(targetY, FADE_DURATION);
        }

        private void AnimateUIElement(
            RectTransform target,
            float scale = SCALE_INTENSITY,
            float duration = SCALE_DURATION
        )
        {
            target.DOComplete();
            target
                .DOScale(scale, duration)
                .SetEase(Ease.InSine)
                .SetLoops(WARNING_LOOPS, LoopType.Yoyo);
        }

        private void AnimateUIElementWithColor(
            RectTransform target,
            Image image,
            Color color,
            float scale = SCALE_INTENSITY,
            float duration = SCALE_DURATION
        )
        {
            target.DOComplete();
            AnimateUIElement(target, scale, duration);
            image
                .DOColor(color, duration)
                .SetEase(Ease.InSine)
                .SetLoops(WARNING_LOOPS, LoopType.Yoyo);
        }
        #endregion

        #region Popup Management
        public void ShowShopPopup()
        {
            _shopPopup.SetActive(true);
            _recycleBin.gameObject.SetActive(true);
        }

        public void HideShopPopup()
        {
            _shopPopup.SetActive(false);
            _recycleBin.gameObject.SetActive(false);
        }

        public void ShowArtifactPopup(List<ArtifactData> datas)
        {
            _artifacts.ForEach(artifact => artifact.gameObject.SetActive(false));

            for (int i = 0; i < datas.Count && i < _artifacts.Count; i++)
            {
                _artifacts[i].gameObject.SetActive(true);
                _artifacts[i].Initialize(datas[i]);
            }

            _artifactPopup.SetActive(true);
        }

        public void HideArtifactPopup()
        {
            _artifactPopup.SetActive(false);
        }
        #endregion

        #region UI Text Updates
        public void UpdateDay()
        {
            UpdateWaveText();
            UpdateLevelProgressSlider();
        }

        public void UpdateWaveText()
        {
            _waveText.text = $"Wave {GameManager.Instance.currentDay + 1}";
        }

        public void UpdateLevelProgressSlider()
        {
            _levelProgressSlider.value = (GameManager.Instance.currentDay + 1) % 10 / 10f;
        }

        public void UpdateGoldText(int gold)
        {
            _goldText.text = gold.ToString();
            AnimateUIElementWithColor(
                _goldContainer,
                _goldContainerImage,
                new Color(1, 1, 1, 0.5f)
            );
        }

        public void UpdateTotalFishText(int totalFish)
        {
            _totalFishText.text = Mathf.Clamp(totalFish, 0, MAX_FISH_DISPLAY).ToString();
        }

        public void WarningGoldPanel()
        {
            AnimateUIElementWithColor(
                _goldContainer,
                _goldContainerImage,
                new Color(1, 0.7f, 0.7f, 1)
            );
        }
        #endregion

        #region Game Start/End Panels
        public async Task ShowGameStartPanel()
        {
            _gameStartPanel.gameObject.SetActive(true);
            _gameStartPanel.DOComplete();
            _gameStartPanel.anchoredPosition = Vector2.zero;
            ClearGameStartTexts();

            await ShowGameStartText();
            await Task.Delay(2000);
            await HideGameStartPanel();
        }

        private void ClearGameStartTexts()
        {
            _gameStartLevelText.text = "";
            _gameStartDayText.text = "";
        }

        public async Task ShowGameStartText()
        {
            var levelText = GetLevelText();
            var fullText = $"Level {levelText}";

            await TypeText(_gameStartLevelText, fullText, TYPING_DELAY_MS);
            await Task.Delay(500);
        }

        private string GetLevelText()
        {
            return GameManager.Instance.currentLevel == 0
                ? "tutorial"
                : GameManager.Instance.currentLevel.ToString();
        }

        private async Task TypeText(TMP_Text textComponent, string text, int delayMs)
        {
            _stringBuilder.Clear();
            for (int i = 0; i < text.Length; i++)
            {
                await Task.Delay(delayMs);
                _stringBuilder.Append(text[i]);
                textComponent.text = _stringBuilder.ToString();
            }
            textComponent.text = text; // Ensure final text is complete
        }

        public async Task ShowWinPanel()
        {
            _gameStartPanel.gameObject.SetActive(true);
            _gameStartPanel.DOComplete();
            _gameStartPanel.anchoredPosition = Vector2.zero;
            ClearGameStartTexts();

            var levelText = GetLevelText();
            var congratsText = $"Congratulations! You have completed level {levelText}!";
            await TypeText(_gameStartLevelText, congratsText, TYPING_DELAY_MS);
            await Task.Delay(500);
        }

        public async Task ShowLosePanel()
        {
            _gameStartPanel.gameObject.SetActive(true);
            _gameStartPanel.DOComplete();
            _gameStartPanel.anchoredPosition = Vector2.zero;
            ClearGameStartTexts();

            const string loseText = "You have lost the game!";
            await TypeText(_gameStartLevelText, loseText, TYPING_DELAY_MS);
            await Task.Delay(500);
        }

        public async Task HideGameStartPanel()
        {
            _gameStartPanel.DOComplete();
            _gameStartPanel.gameObject.SetActive(false);
        }
        #endregion

        #region Shop Item Management
        public void UpdateCostText(ShopItem shopItem, int cost)
        {
            shopItem.price.text = cost.ToString();
        }

        public void UpdatePanelImage(ShopItem shopItem, bool isPurchased = false)
        {
            shopItem.panelImage.color = isPurchased
                ? new Color(0.6f, 0.6f, 0.6f, 1)
                : new Color(1, 1, 1, 1);
            shopItem.priceImage.color = isPurchased
                ? new Color(0.6f, 0.6f, 0.6f, 1)
                : new Color(1, 1, 1, 1);
        }

        public void StrikethroughCostText(ShopItem shopItem, bool isStrikethrough = false)
        {
            if (shopItem == null)
            {
                return;
            }
            shopItem.price.fontStyle = isStrikethrough
                ? FontStyles.Strikethrough
                : FontStyles.Normal;
            shopItem.priceStrikethrough = isStrikethrough;
        }
        #endregion


        #region Event Handlers
        public void OnRerollButtonClick()
        {
            if (!GameManager.Instance.CheckGold(REROLL_COST))
            {
                WarningGoldPanel();
                return;
            }
            GameManager.Instance.RandomGearsInShop();
        }

        public void OnStartButtonClick()
        {
            GameManager.Instance.StartGame();
        }
        #endregion

        #region Inventory Management
        public async void HideInventory()
        {
            await _inventoryManager.Close();
        }

        public void ShowInventory()
        {
            _inventoryManager.gameObject.SetActive(true);
            _inventoryManager.ShowInventory();
        }
        #endregion

        public void QuitToHomeMenu()
        {
            HideShopPopup();
            HideArtifactPopup();
            GameManager.Instance.ShowLosePanel();
        }
    }

    [System.Serializable]
    public class ShopItem
    {
        public GearController gear;
        public TMP_Text price;
        public bool purchased = false;
        public bool priceStrikethrough = false;
        public Image panelImage;
        public Image priceImage;
    }
}
