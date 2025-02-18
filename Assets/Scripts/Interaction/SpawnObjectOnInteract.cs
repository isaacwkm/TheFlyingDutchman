using UnityEngine;
using System;

public class SpawnObjectOnInteract : MonoBehaviour
{
    [SerializeField] public Interactable interactTarget;
    [SerializeField] public GameObject objectToSpawn;
    [SerializeField] public Vector3 position;
    [SerializeField] public Quaternion rotation;
    [SerializeField] public Transform parent;
    [SerializeField] public PrefabInstanceInitializer initializer;
    [SerializeField] public bool repeatable;
    private Action<GameObject> spawnAction;
    public event Action<GameObject> OnSpawn;

    void OnEnable()
    {
        spawnAction = _ => HandleSpawnAction();
        interactTarget.OnInteract += spawnAction;
    }

    void OnDisable()
    {
        interactTarget.OnInteract -= spawnAction;
    }

    void HandleSpawnAction()
    {
        GameObject spawned;
        if (parent)
        {
            spawned = Instantiate(objectToSpawn, position, rotation, parent);
        }
        else
        {
            spawned = Instantiate(objectToSpawn, position, rotation);
        }
        initializer?.Initialize(spawned);
        OnSpawn?.Invoke(spawned);
        if (!repeatable)
        {
            Destroy(GetComponent<SpawnObjectOnInteract>());
                // equivalent to Destroy(this) but more explicit
        }
    }
}
