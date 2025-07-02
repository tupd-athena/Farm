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
}
