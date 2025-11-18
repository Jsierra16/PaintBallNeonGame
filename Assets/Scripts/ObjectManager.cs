using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Singleton { get; private set; }

    [Header("Prefabs")]
    public GameObject[] networkedPrefabs; // Assign balls or other networked objects

    private List<GameObject> networkedObjects = new List<GameObject>();

    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Register an already spawned object (like PhysxBall)
    /// </summary>
    public void RegisterObject(GameObject obj)
    {
        if (!networkedObjects.Contains(obj))
            networkedObjects.Add(obj);
    }

    /// <summary>
    /// Remove an object from tracking
    /// </summary>
    public void UnregisterObject(GameObject obj)
    {
        if (networkedObjects.Contains(obj))
            networkedObjects.Remove(obj);
    }

    /// <summary>
    /// Get all tracked objects
    /// </summary>
    public IReadOnlyList<GameObject> GetAllObjects() => networkedObjects.AsReadOnly();

    /// <summary>
    /// Spawn a prefab by index in the inspector array
    /// </summary>
    public GameObject SpawnPrefab(int index, Vector3 position, Quaternion rotation, NetworkRunner runner)
    {
        if (index < 0 || index >= networkedPrefabs.Length) return null;
        if (runner == null) return null;

        GameObject prefab = networkedPrefabs[index];
        NetworkObject spawned = runner.Spawn(prefab, position, rotation);
        if (spawned != null)
        {
            RegisterObject(spawned.gameObject);
            return spawned.gameObject;
        }
        return null;
    }

    /// <summary>
    /// Spawn a prefab directly from reference
    /// </summary>
    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Quaternion rotation, NetworkRunner runner)
    {
        if (prefab == null || runner == null) return null;

        NetworkObject spawned = runner.Spawn(prefab, position, rotation);
        if (spawned != null)
        {
            RegisterObject(spawned.gameObject);
            return spawned.gameObject;
        }
        return null;
    }
}
