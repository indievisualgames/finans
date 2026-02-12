using System;
using UnityEngine;

public class DropzonePanelController : MonoBehaviour
{
    public event Action OnComplete;
    public DropZone[] dropZones; // Assign all dropzones in this panel in Inspector

    void Start()
    {
        // Optionally, auto-find dropzones if not assigned
        if (dropZones == null || dropZones.Length == 0)
            dropZones = GetComponentsInChildren<DropZone>();
        foreach (var dz in dropZones)
        {
            dz.OnPartPlaced.AddListener(OnDropzonePartPlaced);
        }
    }

    void OnDropzonePartPlaced(string partID, DraggablePart part, bool correct)
    {
        if (correct && AllDropzonesFilled())
        {
            OnComplete?.Invoke();
        }
    }

    bool AllDropzonesFilled()
    {
        foreach (var dz in dropZones)
        {
            if (dz == null) continue; // Skip destroyed or missing references
            if (dz.enabled) return false;
        }
        return true;
    }
} 