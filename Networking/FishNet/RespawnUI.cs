using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishNet.Component.Spawning
{
    [AddComponentMenu("FishNet/Component/RespawnUI")]
    public class RespawnUI : NetworkBehaviour
    {
        #region Public.
        public event Action<NetworkObject> OnSpawned;
        #endregion

        #region Serialized.
        [SerializeField]
        public NetworkObject _playerPrefab;
        [SerializeField]
        private bool _addToDefaultScene = true;
        [SerializeField]
        private bool _spawnOnConnect = true;
        #endregion

        #region Private.
        private NetworkManager _networkManager;
        private List<Transform> _spawnPoints = new List<Transform>();
        #endregion

        private void Start()
        {
            InitializeOnce();
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
                _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }

        /// <summary>
        /// Initializes this script for use.
        /// </summary>
        private void InitializeOnce()
        {
            _networkManager = InstanceFinder.NetworkManager;
            if (_networkManager == null)
            {
                Debug.LogWarning($"RandomPlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
                return;
            }

            _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
            FindSpawnPointsWithTag("DefaultRespawn");
        }

        /// <summary>
        /// Finds and stores all spawn points with the specified tag.
        /// </summary>
        /// <param name="tag">Tag of the objects to be used as spawn points.</param>
        private void FindSpawnPointsWithTag(string tag)
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject spawn in spawns)
            {
                _spawnPoints.Add(spawn.transform);
            }
        }

        /// <summary>
        /// Called when a client loads initial scenes after connecting.
        /// </summary>
        private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
        {
            if (!asServer)
                return;
            if (!_spawnOnConnect)
                return;
            if (_playerPrefab == null)
            {
                Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
                return;
            }

            Vector3 position;
            Quaternion rotation;
            SetRandomSpawn(out position, out rotation);

            NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
            _networkManager.ServerManager.Spawn(nob, conn);

            if (_addToDefaultScene)
                _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            OnSpawned?.Invoke(nob);
        }

        /// <summary>
        /// Sets a random spawn position and rotation from the available spawn points.
        /// </summary>
        private void SetRandomSpawn(out Vector3 pos, out Quaternion rot)
        {
            if (_spawnPoints.Count > 0)
            {
                Transform randomSpawn = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)];
                pos = randomSpawn.position;
                rot = randomSpawn.rotation;
            }
            else
            {
                Debug.LogWarning("No spawn points available. Using default position.");
                pos = new Vector3(0, 0, 0);  // Default position if no spawn points are found
                rot = Quaternion.identity;
            }
        }

        /// <summary>
        /// Client-side method to request player spawn from the server.
        /// Call this from UI buttons or other client-side scripts.
        /// </summary>
        public void RequestSpawnPlayer()
        {
            if (!IsClient)
            {
                Debug.LogWarning("RequestSpawnPlayer can only be called from a client.");
                return;
            }

            RequestSpawnPlayerServerRpc();
        }

        /// <summary>
        /// Server-side method to spawn a player for the requesting client.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnPlayerServerRpc(NetworkConnection conn = null)
        {
            if (!IsServer)
                return;

            if (_playerPrefab == null)
            {
                Debug.LogWarning("Player prefab is empty and cannot be spawned.");
                return;
            }

            if (_networkManager == null)
            {
                Debug.LogWarning("NetworkManager is not initialized.");
                return;
            }

            // Use the connection that sent the request
            if (conn == null)
            {
                Debug.LogWarning("No valid connection available to spawn player.");
                return;
            }

            Vector3 position;
            Quaternion rotation;
            SetRandomSpawn(out position, out rotation);

            NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
            _networkManager.ServerManager.Spawn(nob, conn);

            if (_addToDefaultScene)
                _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            OnSpawned?.Invoke(nob);
        }

        /// <summary>
        /// Manually spawns a player for the specified connection.
        /// Can be called from a UI button or other scripts.
        /// </summary>
        /// <param name="connection">The connection to spawn the player for. If null, will spawn for the local connection.</param>
        public void SpawnPlayer()
        {
            NetworkConnection connection = null;
            if (_playerPrefab == null)
            {
                Debug.LogWarning("Player prefab is empty and cannot be spawned.");
                return;
            }

            if (_networkManager == null)
            {
                Debug.LogWarning("NetworkManager is not initialized.");
                return;
            }

            // If no connection is specified, use the local connection
            if (connection == null)
                connection = _networkManager.ClientManager.Connection;

            if (connection == null)
            {
                Debug.LogWarning("No valid connection available to spawn player.");
                return;
            }

            Vector3 position;
            Quaternion rotation;
            SetRandomSpawn(out position, out rotation);

            NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
            _networkManager.ServerManager.Spawn(nob, connection);

            if (_addToDefaultScene)
                _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            OnSpawned?.Invoke(nob);
        }
    }
}
