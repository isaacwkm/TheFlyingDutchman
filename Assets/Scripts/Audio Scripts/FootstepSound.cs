using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    public AudioClip[] fallbackFootsteps; // Backup footsteps in case others fail
    public AudioClip[] grassFootsteps; // Footsteps for grass
    public AudioClip[] stoneFootsteps; // Footsteps for stone
    public AudioClip[] woodFootsteps;  // Footsteps for wood
    public AudioClip[] dirtFootsteps;  // Footsteps for dirt
    public AudioClip[] specialFootsteps1; // Footsteps for special surface 1
    public AudioClip[] specialFootsteps2; // Footsteps for special surface 2

    // Volume multipliers for each surface type (adjust these values based on the loudness of your recordings)
    public float fallbackVolume = 0.5f; // Volume for fallback footsteps
    public float grassVolume = 1.0f;  // Grass footstep volume (default)
    public float stoneVolume = 0.7f;  // Stone footstep volume (lower than grass)
    public float woodVolume = 0.8f;   // Wood footstep volume
    public float dirtVolume = 0.6f;   // Dirt footstep volume
    public float specialVolume1 = 1.2f; // Volume for special surface 1
    public float specialVolume2 = 1.1f; // Volume for special surface 2

    public Transform footTransform;    // Reference to the foot or footstep GameObject
    public float walkSpeedThreshold = 0.1f; // Speed threshold to trigger footstep sound

    private float footstepVolumeMultiplier = 0.03f; // Multiply all footstep sounds by this value to bring it to a background level.
    private AudioSource audioSource;
    private CharacterController characterController;
    private int lastFootstepIndex = -1;  // Track the last played footstep index
    private int repeatCount = 0;         // Count how many times the same footstep has been played consecutively
    private int lastWoodFootstepIndex = -1; // Track last played wood footstep index


    private void Start()
    {
        audioSource = footTransform.GetComponent<AudioSource>(); // Get AudioSource on foot
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        var pcc = characterController.GetComponent<PlayerCharacterController>();
        GameObject movementMedium = pcc?.GetMovementMedium();
        bool magnitudeSufficient = characterController.velocity.magnitude > walkSpeedThreshold;
        // Check if the character is walking and grounded (not in mid-air)
        if ((
            movementMedium && magnitudeSufficient
        ) || (characterController.isGrounded && (
            (pcc && pcc.AnyMovementInput()) ||
            (pcc && pcc.JustLanded()) ||
            (!pcc && magnitudeSufficient)
        ))) {
            PlayFootstepSound(movementMedium);
        }
    }

    private void PlayFootstepSound(GameObject movementMedium = null)
    {
        // If audio is already playing, don't play again until the clip finishes
        if (audioSource.isPlaying) return;

        AudioClip selectedFootstep = null;
        if (movementMedium) {
            selectedFootstep = GetFootstepSoundBasedOnSurface(movementMedium);
        } else {
            // Use a raycast to detect the ground material
            RaycastHit hit;
            if (Physics.Raycast(footTransform.position, Vector3.down, out hit, 1f))
            {
                selectedFootstep = GetFootstepSoundBasedOnSurface(hit.collider.gameObject);
            }
        }

        // If a valid sound is found, play it
        if (selectedFootstep != null)
        {
            audioSource.PlayOneShot(selectedFootstep);
        }
        else
        {
            // If no valid sound was found, fall back to a default sound
            Debug.LogWarning("No valid footstep sound found, falling back to default.");
            if (fallbackFootsteps != null && fallbackFootsteps.Length > 0)
            {
                audioSource.PlayOneShot(fallbackFootsteps[Random.Range(0, fallbackFootsteps.Length)]);
            }
            else
            {
                Debug.LogWarning("Fallback sounds are also missing! Playing nothing.");
            }
        }
    }

    private AudioClip GetFootstepSoundBasedOnSurface(GameObject surface)
    {
        // Get the surface material or tag
        AudioClip[] footstepArray = null;
        float volumeMultiplier = 1.0f; // Default volume multiplier

        // Floor types based on tags (add more floor types as needed)
        if (surface.CompareTag("Grass"))
        {
            footstepArray = grassFootsteps;
            volumeMultiplier = grassVolume;  // Set volume for grass
        }
        else if (surface.CompareTag("Stone"))
        {
            footstepArray = stoneFootsteps;
            volumeMultiplier = stoneVolume; // Set volume for stone
        }
        else if (surface.CompareTag("Wood"))
        {
            footstepArray = woodFootsteps;  // Wood footstep logic
            volumeMultiplier = woodVolume;  // Set volume for wood

            if (footstepArray != null && footstepArray.Length > 1)
            {
                lastWoodFootstepIndex = (lastWoodFootstepIndex + 1) % footstepArray.Length;
                audioSource.volume = volumeMultiplier * footstepVolumeMultiplier;
                return footstepArray[lastWoodFootstepIndex]; // Return the next clip in sequence
            }
        }
        else if (surface.CompareTag("Dirt"))
        {
            footstepArray = dirtFootsteps;  // Dirt footstep logic
            volumeMultiplier = dirtVolume;  // Set volume for dirt
        }
        else if (surface.CompareTag("Special1"))
        {
            footstepArray = specialFootsteps1;  // Special footstep 1 logic
            volumeMultiplier = specialVolume1; // Set volume for special 1
        }
        else if (surface.CompareTag("Special2"))
        {
            footstepArray = specialFootsteps2;  // Special footstep 2 logic
            volumeMultiplier = specialVolume2; // Set volume for special 2
        }
        else
        {
            // Default to fallback sound if no tag matches
            footstepArray = fallbackFootsteps;
            volumeMultiplier = fallbackVolume; // Set volume for fallback
        }

        // Check if the footstep array is empty to avoid out-of-bounds errors
        if (footstepArray == null || footstepArray.Length == 0)
        {
            Debug.LogWarning("Footstep array is empty for surface: " + surface.tag);
            return null; // Return null if no valid audio clips are available
        }

        // If only one sound is available, no need to repeat check
        if (footstepArray.Length == 1)
        {
            // Apply volume multiplier when playing the single sound
            audioSource.volume = volumeMultiplier * footstepVolumeMultiplier;
            return footstepArray[0]; // Always return the only sound in the array
        }

        // Select a random footstep sound, ensuring it doesn't repeat more than twice
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, footstepArray.Length); // Randomly pick an index
        } while (randomIndex == lastFootstepIndex && repeatCount >= 2); // Avoid repeating more than twice

        // Update the repeat count and last footstep index
        if (randomIndex == lastFootstepIndex)
        {
            repeatCount++;
        }
        else
        {
            repeatCount = 0; // Reset repeat count when a new sound is selected
        }

        // Update the last played footstep index
        lastFootstepIndex = randomIndex;

        // Apply the volume multiplier for this footstep type
        audioSource.volume = volumeMultiplier * footstepVolumeMultiplier;

        // Return the selected footstep sound
        return footstepArray[randomIndex];
    }
}
