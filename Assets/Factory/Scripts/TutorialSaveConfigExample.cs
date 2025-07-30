using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example script showing how to configure which tutorial steps should save
/// </summary>
public class TutorialSaveConfigExample : MonoBehaviour
{
    void Start()
    {
        // Wait a frame to ensure TutorialManager is initialized
        StartCoroutine(ConfigureTutorialSave());
    }

    System.Collections.IEnumerator ConfigureTutorialSave()
    {
        yield return null; // Wait one frame

        if (TutorialManager.Instance != null)
        {
            // Example configurations:

            // Configuration 1: Only save important milestone steps
            var milestoneSteps = new List<int> { 0, 3, 7 }; // First step, middle checkpoint, final step
            TutorialManager.Instance.SetSaveableSteps(milestoneSteps);

            // Configuration 2: Save all steps except specific ones
            // TutorialManager.Instance.MakeAllStepsSaveable();
            // TutorialManager.Instance.RemoveSaveableStep(1); // Don't save step 1
            // TutorialManager.Instance.RemoveSaveableStep(2); // Don't save step 2

            // Configuration 3: Only save the final step
            // TutorialManager.Instance.ClearSaveableSteps();
            // TutorialManager.Instance.AddSaveableStep(7);

            // Configuration 4: Don't save any steps (for testing)
            // TutorialManager.Instance.ClearSaveableSteps();

            Debug.Log("Tutorial save configuration applied!");
        }
    }
}
