# Tutorial Save Configuration System

## Overview
The TutorialManager now includes a configurable save system that allows you to choose which tutorial steps should save their progress to PlayerPrefs and which ones should not.

## Configuration in Inspector

### Saveable Steps List
In the Unity Inspector, you'll find a new section called **"Save Configuration"** with:

- **Saveable Steps**: A list of integers representing which tutorial step indices should save progress
- **Default Configuration**: `[0, 2, 4, 7]` - saves steps 0, 2, 4, and 7 only

### How to Configure:
1. Select the TutorialManager GameObject in your scene
2. In the Inspector, find the "Save Configuration" section
3. Modify the "Saveable Steps" list:
   - **Size**: Set how many steps you want to be saveable
   - **Element 0, 1, 2...**: Enter the step indices that should save

## Runtime Configuration

### Methods Available:

#### Check if a step saves:
```csharp
bool canSave = TutorialManager.Instance.ShouldSaveStep(3);
```

#### Add a step to saveable list:
```csharp
TutorialManager.Instance.AddSaveableStep(5);
```

#### Remove a step from saveable list:
```csharp
TutorialManager.Instance.RemoveSaveableStep(2);
```

#### Get current saveable steps:
```csharp
List<int> saveableSteps = TutorialManager.Instance.GetSaveableSteps();
```

#### Set entire saveable steps list:
```csharp
var newSaveableSteps = new List<int> { 0, 3, 6 };
TutorialManager.Instance.SetSaveableSteps(newSaveableSteps);
```

#### Make all steps saveable:
```csharp
TutorialManager.Instance.MakeAllStepsSaveable();
```

#### Clear all saveable steps (no saving):
```csharp
TutorialManager.Instance.ClearSaveableSteps();
```

## Use Cases

### 1. Milestone-Only Saving
Save only important checkpoints:
```csharp
// Only save at major milestones
var milestones = new List<int> { 0, 3, 7 };
TutorialManager.Instance.SetSaveableSteps(milestones);
```

### 2. No Saving (Testing)
Disable all saving for testing:
```csharp
TutorialManager.Instance.ClearSaveableSteps();
```

### 3. Save Everything Except Specific Steps
```csharp
TutorialManager.Instance.MakeAllStepsSaveable();
TutorialManager.Instance.RemoveSaveableStep(1); // Skip step 1
TutorialManager.Instance.RemoveSaveableStep(2); // Skip step 2
```

### 4. Dynamic Configuration
Change saveable steps based on game state:
```csharp
if (isNewPlayer)
{
    // New players: save more frequently
    TutorialManager.Instance.MakeAllStepsSaveable();
}
else
{
    // Experienced players: save only key points
    var keySteps = new List<int> { 0, 7 };
    TutorialManager.Instance.SetSaveableSteps(keySteps);
}
```

## Tutorial Step Indices Reference

| Step Index | Tutorial Step Name | Description |
|------------|-------------------|-------------|
| 0 | PlayButtonTutorialStep | Show play button |
| 1 | BuyGearTutorialStep | Buy gear tutorial |
| 2 | TreasuresButtonTutorialStep | Show treasures button |
| 3 | RerollTicketTutorialStep | Reroll ticket button |
| 4 | CardContainerTutorialStep | Card container interaction |
| 5 | InventoryButtonTutorialStep | Inventory button |
| 6 | InventoryItemTutorialStep | Inventory item interaction |
| 7 | LevelUpButtonTutorialStep | Level up button |

## Benefits

1. **Performance**: Reduce PlayerPrefs writes by only saving important steps
2. **Testing**: Easily disable saving for development/testing
3. **User Experience**: Save only at meaningful progress points
4. **Flexibility**: Configure different save patterns for different user types
5. **Debugging**: Clear logs show which steps save and which don't

## Console Output

The system provides clear logging:
- `"Tutorial step 3 saved to PlayerPrefs"` - When a step saves
- `"Tutorial step 1 not saved (not in saveable steps list)"` - When a step doesn't save
- `"Current saveable steps: [0, 2, 4, 7]"` - Shows current configuration
