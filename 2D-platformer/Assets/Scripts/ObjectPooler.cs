using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pooler mechanism. (Singleton)
/// </summary>
public class ObjectPooler : MonoBehaviour {
    /// <summary>
    /// Pool class that stores information about a certain pooling objects.
    /// </summary>
    [System.Serializable]
    public class Pool {
        /// <summary>
        /// object name.
        /// </summary>
        public string name;

        /// <summary>
        /// Object prefab to use.
        /// </summary>
        public GameObject prefab;

        /// <summary>
        /// Pool size.
        /// </summary>
        public int size;
    }

    /// <summary>
    /// List of pooling objects.
    /// </summary>
    [SerializeField] private List<Pool> pools;

    /// <summary>
    /// Pool dictionary to use in the scene.
    /// </summary>
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    /// <summary>
    /// Instance of this object.
    /// </summary>
    public static ObjectPooler Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools) {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++) {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.name, objectPool);
        }
    }

    /// <summary>
    /// Spawn from this pool to specified position and rotation.
    /// </summary>
    /// <param name="name">Name of the object to spawn.</param>
    /// <param name="position">Position to spawn.</param>
    /// <param name="rotation">Rotation to set of the spawned object.</param>
    /// <returns></returns>
    public GameObject SpawnFromPool(string name, Vector2 position, Quaternion rotation) {
        if (!poolDictionary.ContainsKey(name)) {
            Debug.LogWarning("Pool with name" + name + "doesn't exist");
            return null;
        }

        GameObject objectToSpwan = poolDictionary[name].Dequeue();

        objectToSpwan.SetActive(true);
        objectToSpwan.transform.position = position;
        objectToSpwan.transform.rotation = rotation;

        poolDictionary[name].Enqueue(objectToSpwan);

        return objectToSpwan;
    }
}
