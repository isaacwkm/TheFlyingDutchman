using UnityEngine;

public class PlayTimeMetricTimer : MonoBehaviour
{
    [SerializeField] private string key = "play time";
    void Update()
    {
        Metrics.Set(key, Metrics.Get(key) + Time.deltaTime);
    }
}
