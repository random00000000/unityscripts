using UnityEngine;

/// <summary>
/// Sets a material on a specified index of a SkinnedMeshRenderer.
/// Useful for applying shirt materials to the body slot.
/// </summary>
public class MaterialSlotSetter : MonoBehaviour
{
    [Tooltip("Renderer whose material slot will be replaced")] 
    public SkinnedMeshRenderer targetRenderer;

    [Tooltip("Material to assign to the slot")]
    public Material slotMaterial;

    [Tooltip("Material index to replace")] 
    public int materialIndex = 1;

    private void Start()
    {
        ApplyMaterial();
    }

    /// <summary>
    /// Replace the material at the configured index.
    /// </summary>
    public void ApplyMaterial()
    {
        if (targetRenderer == null || slotMaterial == null)
            return;

        Material[] mats = targetRenderer.materials;
        if (materialIndex >= 0 && materialIndex < mats.Length)
        {
            mats[materialIndex] = slotMaterial;
            targetRenderer.materials = mats;
        }
        else
        {
            Debug.LogWarning($"MaterialSlotSetter: materialIndex {materialIndex} out of range");
        }
    }

    private void OnValidate()
    {
        ApplyMaterial();
    }
}
