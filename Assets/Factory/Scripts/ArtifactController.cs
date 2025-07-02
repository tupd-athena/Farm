using System.Collections;
using System.Collections.Generic;
using Factory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArtifactController : MonoBehaviour, IPointerClickHandler
{
    public ArtifactData artifactData;
    public Image artifactImage;
    public TMP_Text artifactNameText;
    public TMP_Text artifactDescriptionText;
    public Image artifactRarityImage;

    public void Initialize(ArtifactData data)
    {
        artifactData.CopyFrom(data);
        UpdateUI();
        Debug.Log("Artifact initialized: " + artifactData.artifactName);
    }

    private void UpdateUI()
    {
        if (artifactImage != null)
        {
            // Assuming you have a method to load the image based on artifactData.artifactId
            var sprite = LoadArtifactImage(artifactData.artifactId);
            if (sprite != null)
            {
                artifactImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning("Artifact image not found for ID: " + artifactData.artifactId);
            }
        }

        if (artifactNameText != null)
        {
            artifactNameText.text = artifactData.artifactName;
        }

        if (artifactDescriptionText != null)
        {
            artifactDescriptionText.text = artifactData.artifactDescription;
        }
        artifactRarityImage.sprite = GameManager.Instance.GetArtifactRaritySprite(
            artifactData.artifactType
        );
    }

    private Sprite LoadArtifactImage(string artifactId)
    {
        return Resources.Load<Sprite>("Sprites/Artifacts/" + artifactId); // Replace with actual sprite loading logic
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.ActiveArtifact(artifactData);
        GameManager.Instance.homeUI.HideArtifactPopup();
    }
}
