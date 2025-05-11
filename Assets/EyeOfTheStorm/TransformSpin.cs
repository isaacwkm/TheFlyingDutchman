using UnityEngine;

public class TransformSpin : MonoBehaviour
{
    public float rotationSpeed = 90f; // degrees per second

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Spin();
    }

    void Spin()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

}
