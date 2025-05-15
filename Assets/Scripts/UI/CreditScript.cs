using UnityEngine;

public class CreditScript : MonoBehaviour
{
    public float scrollSpeed = 40f; // Speed of the scrolling text

    private RectTransform rect_transform; // Reference to the RectTransform component
    private Vector2 startPosition; // Starting position of the text

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rect_transform = GetComponent<RectTransform>();
        startPosition = rect_transform.anchoredPosition; // Store the initial position of the text
    }

    // Update is called once per frame
    void Update()
    {
        rect_transform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime); // Move the text upwards
    }
    
    public void ResetCreditsPosition()
    {
        rect_transform.anchoredPosition = startPosition; // Reset the text position to the starting point
    }
}