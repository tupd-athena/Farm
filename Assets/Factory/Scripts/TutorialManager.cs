using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Factory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TutorialStep
{
    protected TutorialManager tutorialManager;
    public abstract int StepIndex { get; }

    public TutorialStep(TutorialManager manager)
    {
        tutorialManager = manager;
    }

    public abstract Task ExecuteAsync();

    public virtual void Cleanup() { }
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    private const string TUTORIAL_STEP_KEY = "TutorialStep";

    [Header("Tutorial UI Components")]
    public GameObject main;
    public RectTransform maskRect;
    public GameObject dialogPanel;
    public TMP_Text dialogText;
    public RectTransform handRect;
    public AnimationCurve handAnimationCurve;
    public ParticleSystem maskEffect;

    [Header("Tutorial Settings")]
    public int currentTutorialStep;
    
    [Header("Save Configuration")]
    [Tooltip("List of tutorial step indices that should save progress to PlayerPrefs")]
    public List<int> saveableSteps = new List<int> { 0, 2, 4, 6, 7 }; // Example: only steps 0, 2, 4, and 7 save

    public Action OnTouchScreen;
    public Action<int> OnTutorialStepChanged;
    public Action OnTutorialCompleted;

    private Dictionary<int, TutorialStep> tutorialSteps;
    private TutorialStep currentStep;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTutorialSteps();
        }
        else
        {
            Destroy(gameObject);
        }
        currentTutorialStep = PlayerPrefs.GetInt(TUTORIAL_STEP_KEY, 0);
    }

    private void InitializeTutorialSteps()
    {
        tutorialSteps = new Dictionary<int, TutorialStep>();

        // Register all tutorial steps
        RegisterTutorialStep(new PlayButtonTutorialStep(this));
        RegisterTutorialStep(new BuyGearTutorialStep(this));
        RegisterTutorialStep(new TreasuresButtonTutorialStep(this));
        RegisterTutorialStep(new RerollTicketTutorialStep(this));
        RegisterTutorialStep(new CardContainerTutorialStep(this));
        RegisterTutorialStep(new InventoryButtonTutorialStep(this));
        RegisterTutorialStep(new InventoryItemTutorialStep(this));
        RegisterTutorialStep(new LevelUpButtonTutorialStep(this));

        // Validate saveable steps list
        ValidateSaveableSteps();
    }

    /// <summary>
    /// Validate that all steps in saveableSteps list actually exist
    /// </summary>
    private void ValidateSaveableSteps()
    {
        var invalidSteps = new List<int>();
        foreach (var step in saveableSteps)
        {
            if (!tutorialSteps.ContainsKey(step))
            {
                invalidSteps.Add(step);
            }
        }

        if (invalidSteps.Count > 0)
        {
            Debug.LogWarning(
                $"Removing invalid saveable steps: [{string.Join(", ", invalidSteps)}]"
            );
            foreach (var invalidStep in invalidSteps)
            {
                saveableSteps.Remove(invalidStep);
            }
        }

        Debug.Log($"Current saveable steps: [{string.Join(", ", saveableSteps)}]");
    }

    /// <summary>
    /// Register a tutorial step with the manager
    /// </summary>
    /// <param name="step">The tutorial step to register</param>
    public void RegisterTutorialStep(TutorialStep step)
    {
        if (tutorialSteps == null)
            tutorialSteps = new Dictionary<int, TutorialStep>();

        tutorialSteps[step.StepIndex] = step;
    }

    /// <summary>
    /// Remove a tutorial step from the manager
    /// </summary>
    /// <param name="stepIndex">The step index to remove</param>
    public void UnregisterTutorialStep(int stepIndex)
    {
        if (tutorialSteps != null && tutorialSteps.ContainsKey(stepIndex))
        {
            tutorialSteps[stepIndex].Cleanup();
            tutorialSteps.Remove(stepIndex);
        }
    }

    /// <summary>
    /// Get the total number of registered tutorial steps
    /// </summary>
    public int TotalSteps => tutorialSteps?.Count ?? 0;

    /// <summary>
    /// Check if the tutorial is completed
    /// </summary>
    public bool IsCompleted => currentTutorialStep >= TotalSteps;

    /// <summary>
    /// Reset the tutorial to the beginning
    /// </summary>
    public void ResetTutorial()
    {
        currentStep?.Cleanup();
        currentStep = null;
        SetTutorialStep(0);
        ShowTutorialStep();
    }

    /// <summary>
    /// Skip to a specific tutorial step
    /// </summary>
    /// <param name="stepIndex">The step index to skip to</param>
    public void SkipToStep(int stepIndex)
    {
        currentStep?.Cleanup();
        currentStep = null;
        SetTutorialStep(stepIndex);
        ShowTutorialStep();
    }

    /// <summary>
    /// Pause the current tutorial step
    /// </summary>
    public void PauseTutorial()
    {
        currentStep?.Cleanup();
        if (maskRect != null)
            maskRect.gameObject.SetActive(false);
        HideDialog();
        HideTutorialSwipe();
    }

    /// <summary>
    /// Resume the current tutorial step
    /// </summary>
    public void ResumeTutorial()
    {
        ShowTutorialStep();
    }

    /// <summary>
    /// Check if a specific tutorial step should save its progress
    /// </summary>
    /// <param name="stepIndex">The step index to check</param>
    /// <returns>True if the step should save progress, false otherwise</returns>
    public bool ShouldSaveStep(int stepIndex)
    {
        return saveableSteps.Contains(stepIndex);
    }

    /// <summary>
    /// Add a step to the saveable steps list
    /// </summary>
    /// <param name="stepIndex">The step index to make saveable</param>
    public void AddSaveableStep(int stepIndex)
    {
        if (!saveableSteps.Contains(stepIndex))
        {
            saveableSteps.Add(stepIndex);
            Debug.Log($"Step {stepIndex} added to saveable steps");
        }
    }

    /// <summary>
    /// Remove a step from the saveable steps list
    /// </summary>
    /// <param name="stepIndex">The step index to make non-saveable</param>
    public void RemoveSaveableStep(int stepIndex)
    {
        if (saveableSteps.Remove(stepIndex))
        {
            Debug.Log($"Step {stepIndex} removed from saveable steps");
        }
    }

    /// <summary>
    /// Get all saveable step indices
    /// </summary>
    /// <returns>Copy of the saveable steps list</returns>
    public List<int> GetSaveableSteps()
    {
        return new List<int>(saveableSteps);
    }

    /// <summary>
    /// Set the entire saveable steps list
    /// </summary>
    /// <param name="steps">New list of saveable step indices</param>
    public void SetSaveableSteps(List<int> steps)
    {
        saveableSteps = new List<int>(steps);
        Debug.Log($"Saveable steps updated: [{string.Join(", ", saveableSteps)}]");
    }

    /// <summary>
    /// Clear all saveable steps (no steps will save)
    /// </summary>
    public void ClearSaveableSteps()
    {
        saveableSteps.Clear();
        Debug.Log("All saveable steps cleared - no steps will save progress");
    }

    /// <summary>
    /// Make all registered tutorial steps saveable
    /// </summary>
    public void MakeAllStepsSaveable()
    {
        saveableSteps.Clear();
        if (tutorialSteps != null)
        {
            saveableSteps.AddRange(tutorialSteps.Keys);
            Debug.Log($"All tutorial steps made saveable: [{string.Join(", ", saveableSteps)}]");
        }
    }

    // Add methods to manage tutorials here
    public void SetTutorialStep(int step)
    {
        currentTutorialStep = step;
        
        // Only save to PlayerPrefs if this step is in the saveable steps list
        if (ShouldSaveStep(step))
        {
            PlayerPrefs.SetInt(TUTORIAL_STEP_KEY, step);
            Debug.Log($"Tutorial step {step} saved to PlayerPrefs");
        }
        else
        {
            Debug.Log($"Tutorial step {step} not saved (not in saveable steps list)");
        }
    }

    void Start()
    {
        // PlayerPrefs.SetInt(TUTORIAL_STEP_KEY, 0); // Reset tutorial step for testing
        saveableSteps = new List<int> { 0, 2, 5, 8 };
        currentTutorialStep = PlayerPrefs.GetInt(TUTORIAL_STEP_KEY, 0);
        ShowTutorialStep();
    }

    public void NextStep()
    {
        currentStep?.Cleanup();
        currentTutorialStep++;
        SetTutorialStep(currentTutorialStep);

        OnTutorialStepChanged?.Invoke(currentTutorialStep);

        if (IsCompleted)
        {
            OnTutorialCompleted?.Invoke();
            Debug.Log("Tutorial completed!");
        }
        else
        {
            ShowTutorialStep();
        }
    }

    public async void ShowTutorialStep()
    {
        Debug.Log($"Showing tutorial step: {currentTutorialStep}");

        // Hide mask initially
        if (maskRect != null)
            maskRect.gameObject.SetActive(false);

        await Task.Delay(100); // Ensure the UI is ready

        if (IsCompleted)
        {
            Debug.Log("Tutorial has been completed.");
            return;
        }

        if (tutorialSteps.TryGetValue(currentTutorialStep, out var step))
        {
            currentStep = step;
            try
            {
                await step.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"Error executing tutorial step {currentTutorialStep}: {ex.Message}"
                );
                // Skip to next step on error
                NextStep();
            }
        }
        else
        {
            Debug.LogWarning(
                $"Tutorial step {currentTutorialStep} not found. Available steps: {string.Join(", ", tutorialSteps.Keys)}"
            );
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnTouchScreen?.Invoke();
        }

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                OnTouchScreen?.Invoke();
            }
        }
    }

    public void ShowTutorialSwipe(
        Vector2 from,
        Vector2 to,
        float duration = 0.5f,
        bool isLooping = false
    )
    {
        handRect.position = from;
        handRect.gameObject.SetActive(true);
        handRect
            .DOMove(to, duration)
            .SetEase(handAnimationCurve)
            .SetLoops(isLooping ? -1 : 1, LoopType.Restart)
            .OnComplete(() =>
            {
                handRect.gameObject.SetActive(false);
            });
    }

    public void HideTutorialSwipe()
    {
        handRect.DOComplete();
        handRect.gameObject.SetActive(false);
    }

    public void ShowDialog(string text, Vector2 position = default)
    {
        dialogText.text = text;
        dialogPanel.SetActive(true);
        dialogPanel.GetComponent<RectTransform>().position = position;
    }

    public void HideDialog()
    {
        dialogPanel.SetActive(false);
    }

    public void ShowMask(RectTransform rectObj)
    {
        // Implement mask display logic here
        float width = rectObj.sizeDelta.x;
        float height = rectObj.sizeDelta.y;
        Vector2 position = rectObj.position;
        Debug.Log($"Mask Position: {position}, Size: {width}x{height}");
        maskRect.sizeDelta = new Vector2(width, height);
        maskRect.position = position;
        Debug.Log($"Mask Rect: {maskRect.rect}, Position: {maskRect.position}");
        maskRect.gameObject.SetActive(true);

        var scaleX = width / 100f;
        var scaleY = height / 100f;
        var shape = maskEffect.shape;
        shape.scale = new Vector3(scaleX, scaleY, 1f);
    }
}
