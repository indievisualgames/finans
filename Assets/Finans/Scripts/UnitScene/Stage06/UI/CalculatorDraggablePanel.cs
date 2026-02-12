using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CalculatorDraggablePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[Header("References")]
	[SerializeField] private RectTransform targetWindow; // The root window/panel to move (e.g., CalculatorDraggable)
	[SerializeField] private Button toggleButton; // Button that minimizes/maximizes the window
	[SerializeField] private Button resetButton; // Button that resets window position and calculator
	[SerializeField] private RectTransform contentToToggle; // Content area hidden when minimized (keep header/handle visible)
	[SerializeField] private CalculatorController calculatorController; // optional link to reset calculator

	[Header("Behavior")]
	[SerializeField] private bool clampToCanvas = true;
	[SerializeField] private DragAxis dragAxis = DragAxis.HorizontalOnly;
	[SerializeField] private bool allowDragFromAnywhere = false; // if true, can attach to root and drag anywhere

	public enum DragAxis { Both, HorizontalOnly, VerticalOnly }

	private Canvas rootCanvas;
	private RectTransform canvasRect;
	private Vector2 initialAnchoredPos;
	private Vector2 defaultAnchoredPos;
	private bool isDragging;
	private bool isMinimized;

	private void Awake()
	{
		rootCanvas = GetComponentInParent<Canvas>();
		canvasRect = rootCanvas != null ? rootCanvas.transform as RectTransform : null;
		if (toggleButton != null)
		{
			toggleButton.onClick.AddListener(ToggleMinimize);
		}
		if (resetButton != null)
		{
			resetButton.onClick.AddListener(ResetAllUI);
		}
		if (targetWindow == null)
		{
			var rt = GetComponent<RectTransform>();
			targetWindow = rt != null ? rt : transform as RectTransform;
		}
		if (targetWindow != null)
		{
			defaultAnchoredPos = targetWindow.anchoredPosition;
		}
		if (calculatorController == null && targetWindow != null)
		{
			calculatorController = targetWindow.GetComponentInChildren<CalculatorController>(true);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (targetWindow == null || rootCanvas == null) return;
		isDragging = true;
		initialAnchoredPos = targetWindow.anchoredPosition;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!isDragging || targetWindow == null || rootCanvas == null) return;
		// Use delta for smooth, jitter-free movement
		var scaledDelta = eventData.delta / (rootCanvas.scaleFactor == 0 ? 1f : rootCanvas.scaleFactor);
		Vector2 current = targetWindow.anchoredPosition;
		Vector2 next = current;
		switch (dragAxis)
		{
			case DragAxis.HorizontalOnly:
				next.x += scaledDelta.x;
				break;
			case DragAxis.VerticalOnly:
				next.y += scaledDelta.y;
				break;
			default:
				next += scaledDelta;
				break;
		}
		next = ClampToCanvasIfNeeded(next);
		targetWindow.anchoredPosition = next;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;
	}

	public void ToggleMinimize()
	{
		if (contentToToggle == null) return;
		isMinimized = !isMinimized;
		contentToToggle.gameObject.SetActive(!isMinimized);
	}

	public void Minimize()
	{
		if (contentToToggle == null) return;
		isMinimized = true;
		contentToToggle.gameObject.SetActive(false);
	}

	public void Maximize()
	{
		if (contentToToggle == null) return;
		isMinimized = false;
		contentToToggle.gameObject.SetActive(true);
	}

	public void ResetAllUI()
	{
		if (targetWindow != null)
		{
			targetWindow.anchoredPosition = ClampToCanvasIfNeeded(defaultAnchoredPos);
		}
		Maximize();
		if (calculatorController != null)
		{
			calculatorController.Model.ResetAll();
		}
	}

	private Vector2 ClampToCanvasIfNeeded(Vector2 desiredAnchoredPos)
	{
		if (!clampToCanvas || canvasRect == null || targetWindow == null) return desiredAnchoredPos;

		Vector3[] canvasCorners = new Vector3[4];
		Vector3[] windowCorners = new Vector3[4];
		canvasRect.GetWorldCorners(canvasCorners);
		targetWindow.GetWorldCorners(windowCorners);

		Vector2 size = new Vector2(windowCorners[2].x - windowCorners[0].x, windowCorners[2].y - windowCorners[0].y);
		float scale = rootCanvas.scaleFactor == 0 ? 1f : rootCanvas.scaleFactor;
		Vector2 halfSize = size / (2f * scale);

		// Canvas rect in local space
		Rect canvasLocalRect = new Rect(
			canvasRect.rect.xMin + halfSize.x,
			canvasRect.rect.yMin + halfSize.y,
			canvasRect.rect.width - halfSize.x * 2f,
			canvasRect.rect.height - halfSize.y * 2f
		);

		float x = Mathf.Clamp(desiredAnchoredPos.x, canvasLocalRect.xMin, canvasLocalRect.xMax);
		float y = desiredAnchoredPos.y;
		if (dragAxis != DragAxis.HorizontalOnly)
		{
			y = Mathf.Clamp(desiredAnchoredPos.y, canvasLocalRect.yMin, canvasLocalRect.yMax);
		}
		return new Vector2(x, y);
	}
}


