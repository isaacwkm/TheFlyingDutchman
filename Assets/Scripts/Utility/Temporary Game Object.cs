using UnityEngine;

public class TemporaryGameObject : MonoBehaviour
{
    [SerializeField] public bool destroyAfterTimeout = false;
    [SerializeField] public float timeout = 10.0f;
    [SerializeField] public bool destroyAtKillPlane = false;
    [SerializeField] public float killPlane = -1000.0f;
    [SerializeField] public bool destroyWhenNotVisible = false;

    private float timer = 0.0f;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (
            (destroyAfterTimeout && timer >= timeout) ||
            (destroyAtKillPlane && transform.position.y <= killPlane)
        ) {
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        if (destroyWhenNotVisible)
        {
            Destroy(gameObject);
        }
    }
}
