using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Athena.Common.UI;
using DG.Tweening;
using Factory;
using UnityEngine;

public class ScreenTrigger : MonoBehaviour
{
    public List<GameObject> _particles;
    private Camera _uiCamera;
    private RectTransform _canvasRect;

    [Header("Fish Knockback Settings")]
    public float knockbackRadius = 2f;
    public LayerMask fishLayerMask = -1; // Default to all layers

    // For debugging - shows the knockback radius in the scene view
    private Vector3 lastHandPosition;
    private bool showDebugRadius = false;

    private void Start()
    {
        _uiCamera = UIManager.Instance.CameraUI;
        if (_particles.Count > 0)
        {
            _canvasRect = _particles[0].transform.parent.GetComponent<RectTransform>();
        }
    }

    public GameObject GetObject()
    {
        return _particles.FirstOrDefault(x => x.activeSelf == false);
    }

    void Update()
    {
        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleInputPosition(touch.position);
            }
        }
        // Handle mouse input for editor testing
        else if (Input.GetMouseButtonDown(0))
        {
            HandleInputPosition(Input.mousePosition);
        }
    }

    private void HandleInputPosition(Vector2 screenPosition)
    {
        if (_canvasRect == null || _uiCamera == null)
            return;

        // Convert screen position to world position
        Vector3 worldPosition = _uiCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, _uiCamera.nearClipPlane)
        );

        // Convert world position to local position in canvas
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPosition,
            _uiCamera,
            out localPoint
        );

        // Check for nearby fish and apply knockback
        CheckAndKnockbackNearbyFish(worldPosition);

        SpawnParticle(localPoint);
    }

    public void SpawnParticle(Vector2 localPosition)
    {
        var particle = GetObject();
        if (particle == null)
            return;

        // Validate position is within screen bounds
        if (localPosition.y < Screen.height / 2f)
        {
            RectTransform particleRect = particle.GetComponent<RectTransform>();
            particleRect.localPosition = localPosition;
            particle.SetActive(true);

            AudioManager.Instance.PlaySound("Tap");

            DOVirtual.DelayedCall(
                1f,
                () =>
                {
                    particle.SetActive(false);
                }
            );
        }
    }

    private void CheckAndKnockbackNearbyFish(Vector3 handWorldPosition)
    {
        // Store for debug visualization
        lastHandPosition = handWorldPosition;
        showDebugRadius = true;

        // Only check for fish if FishManager exists and has fish
        if (FishManager.Instance == null || FishManager.Instance._fishes == null)
            return;

        // Create a temporary GameObject to represent the hand position for knockback calculation
        GameObject handPosition = new GameObject("TempHandPosition");
        handPosition.transform.position = handWorldPosition;

        try
        {
            // Use Physics2D.OverlapCircleAll to find all fish within knockback radius
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
                handWorldPosition,
                knockbackRadius,
                fishLayerMask
            );

            int fishKnockedBack = 0;

            foreach (Collider2D collider in nearbyColliders)
            {
                // Check if the collider has a FishController component
                ItemController itemController = collider.GetComponent<ItemController>();
                if (itemController != null)
                {
                    // itemController.ApplyKnockback(handPosition.transform);
                }
            }

            if (fishKnockedBack > 0)
            {
                Debug.Log($"Knocked back {fishKnockedBack} fish at position {handWorldPosition}");
            }
        }
        finally
        {
            // Clean up the temporary GameObject
            DestroyImmediate(handPosition);

            // Hide debug radius after a short delay
            DOVirtual.DelayedCall(0.5f, () => showDebugRadius = false);
        }
    }

    private void OnDrawGizmos()
    {
        if (showDebugRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastHandPosition, knockbackRadius);
        }
    }
}
