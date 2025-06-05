using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

/// <summary>
/// Enables configured objects and components when the local client starts.
/// </summary>
public class EnableOnClientStart : NetworkBehaviour
{
    [Tooltip("GameObjects to enable on client start")]
    public List<GameObject> objectsToEnable = new List<GameObject>();

    [Tooltip("Components to enable on client start")]
    public List<MonoBehaviour> componentsToEnable = new List<MonoBehaviour>();

    /// <summary>
    /// Called when the local client begins.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (MonoBehaviour mb in componentsToEnable)
        {
            if (mb != null)
                mb.enabled = true;
        }
    }
}
