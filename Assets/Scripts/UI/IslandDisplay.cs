using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

// This script handles the display of the island name when the player enters a trigger zone
// It uses a coroutine to fade in the text, display it for a specified duration, and then fade it out
// Make sure to set the TextMeshProUGUI component in the inspector
public class IslandDisplay : MonoBehaviour
{
    public TextMeshProUGUI islandNameText; // Reference to the TextMeshProUGUI component for displaying the island name
    public float displayDuration = 2f; // Duration to display the island name
    public float fadeDuration = 1f; // Duration for the fade effect

    private Coroutine displayCoroutine; // Reference to the coroutine for displaying the island name

    public void ShowIslandName(string islandName)
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine); // Stop any existing coroutine
        }

        displayCoroutine = StartCoroutine(DisplayIslandNameCoroutine(islandName)); // Start the coroutine to display the island name
    }

    private IEnumerator DisplayIslandNameCoroutine(string islandName)
    {
        islandNameText.text = islandName; // Set the island name text
        islandNameText.color = new Color(islandNameText.color.r, islandNameText.color.g, islandNameText.color.b, 0); // Set initial alpha to 0

        // Fade in
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            islandNameText.color = new Color(islandNameText.color.r, islandNameText.color.g, islandNameText.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Wait for the display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            islandNameText.color = new Color(islandNameText.color.r, islandNameText.color.g, islandNameText.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
