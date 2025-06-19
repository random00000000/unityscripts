using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

/// <summary>
/// Displays a respawn UI for the local player and requests
/// respawn from the server when the button is pressed.
/// </summary>
public class RespawnUI : NetworkBehaviour
{
    [Tooltip("Panel shown when waiting to respawn")]
    public GameObject respawnPanel;

    [Tooltip("Transform used as the respawn point")]
    public Transform respawnPoint;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner && respawnPanel != null)
            respawnPanel.SetActive(false);
    }

    /// <summary>
    /// Show the respawn panel. Call this when the player dies.
    /// </summary>
    public void Show()
    {
        if (IsOwner && respawnPanel != null)
            respawnPanel.SetActive(true);
    }

    /// <summary>
    /// Hide the respawn panel.
    /// </summary>
    private void Hide()
    {
        if (IsOwner && respawnPanel != null)
            respawnPanel.SetActive(false);
    }

    /// <summary>
    /// Called by the UI button to respawn the player.
    /// </summary>
    public void RespawnButton()
    {
        if (IsOwner)
            RequestRespawnServerRpc();
    }

    [ServerRpc]
    private void RequestRespawnServerRpc(NetworkConnection conn = null)
    {
        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        RespawnTargetRpc(conn);
    }

    [TargetRpc]
    private void RespawnTargetRpc(NetworkConnection conn)
    {
        Hide();
    }
}
