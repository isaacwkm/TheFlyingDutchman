using UnityEngine;
using Needle.Console;
using Unity.VisualScripting;

public class FootstepSound : MonoBehaviour
{
    public AudioClip[] fallbackFootsteps; // Backup footsteps in case others fail
    public AudioClip[] grassFootsteps; // Footsteps for grass
    public AudioClip[] stoneFootsteps; // Footsteps for stone
    public AudioClip[] woodFootsteps;  // Footsteps for wood
    public AudioClip[] dirtFootsteps;  // Footsteps for dirt
    public AudioClip[] sandFootsteps;  // Footsteps for sand
    public AudioClip[] specialFootsteps1; // Footsteps for special surface 1
    public AudioClip[] specialFootsteps2; // Footsteps for special surface 2

    // Volume multipliers for each surface type (adjust these values based on the loudness of your recordings)
    public float fallbackVolume = 0.5f; // Volume for fallback footsteps
    public float grassVolume = 1.0f;  // Grass footstep volume (default)
    public float stoneVolume = 0.7f;  // Stone footstep volume (lower than grass)
    public float woodVolume = 0.8f;   // Wood footstep volume
    public float dirtVolume = 0.6f;   // Dirt footstep volume
    public float sandVolume = 1.0f;   // Sand footstep volume
    public float specialVolume1 = 1.2f; // Volume for special surface 1
    public float specialVolume2 = 1.1f; // Volume for special surface 2

    public Transform footTransform;    // Reference to the foot or footstep GameObject
    public float walkSpeedThreshold = 0.1f; // Speed threshold to trigger footstep sound

    private float footstepVolumeMultiplier = 0.03f; // Multiply all footstep sounds by this value to bring it to a background level.
    private AudioSource audioSource;
    private CharacterController characterController;
    private int lastFootstepIndex = -1;  // Track the last played footstep index
    private int repeatCount = 0;         // Count how many times the same footstep has been played consecutively
    private bool footstepsEnabled = true;

    private void OnEnable()
    {
        InputModeManager.Instance.OnInputModeSwitch += HandleInputModeSwitch;
    }

    private void OnDisable()
    {
        InputModeManager.Instance.OnInputModeSwitch -= HandleInputModeSwitch;
    }

    private void HandleInputModeSwitch()
    {
        if (InputModeManager.Instance.inputMode == InputModeManager.InputMode.Flying)
        {
            SetFootstepsAllowed(false); // Removes spurrious footsteps while sailing
        }
        else
        {
            SetFootstepsAllowed(true);
        }
    }
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
        ) || (characterController.isGrounded && ( // Refactoring may be needed here.
            (pcc && pcc.AnyMovementInput()) ||
            (pcc && pcc.JustLanded()) ||
            (!pcc && magnitudeSufficient)
        )))
        {
            if (footstepsEnabled == false) return;

            PlayFootstepSound(movementMedium);
        }
    }

    private void PlayFootstepSound(GameObject movementMedium = null)
    {
        // If audio is already playing, don't play again until the clip finishes
        if (audioSource.isPlaying) return;

        // Use a raycast to detect the ground material
        RaycastHit hit;
        AudioClip selectedFootstep = null;
        bool didItHit = Physics.Raycast(footTransform.position, Vector3.down, out hit, 1f); // Checks the floor underneath the player

        if (movementMedium != null)
        {
            selectedFootstep = GetFootstepSoundBasedOnSurface(movementMedium);
        }
        else
        {
            if (didItHit)
            {
                selectedFootstep = GetFootstepSoundBasedOnSurface(hit.collider.gameObject);
            }
        }

        // If a valid sound is found, play it
        if (selectedFootstep != null)
        {
            if (movementMedium != null)
            {
                D.Log($"Footstepped on {movementMedium.name}", movementMedium, "Aud");
            }
            else
            {
                if (hit.collider.gameObject)
                {
                    D.Log($"Footstepped on {hit.collider.gameObject.name}", hit.collider.gameObject, "Aud");
                }
                //D.Log($"Footstepped clip {selectedFootstep.name}", null, "Aud");
            }
            audioSource.PlayOneShot(selectedFootstep);

        }
        else if (hit.collider != null)
        {
            // If no valid sound was found, fall back to a default sound
            D.LogWarning("No valid footstep sound found, falling back to default.", hit.collider.gameObject, "Aud");
        }
    }

    // The most horrendous footstep script by me (Isaac). This was like the very first script I ever wrote in this project. Not sure why I was allowed to keep this.
    // //   +1 it works tho
    private AudioClip GetFootstepSoundBasedOnSurface(GameObject surface)
    {
        // Get the surface material or tag
        AudioClip[] footstepArray = null;
        float volumeMultiplier = 1.0f; // Default volume multiplier

        // Floor types based on tags (add more floor types as needed)
        if (surface.CompareTag("SilentFootstep"))
        {
            return null;
        }
        else if (surface.CompareTag("Grass"))
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

            // Commented out Jalen's implementation of alternating footsteps to try 3+ footsteps in line with other footsteps.
            // if (footstepArray != null && footstepArray.Length > 1)
            // {
            //     lastWoodFootstepIndex = (lastWoodFootstepIndex + 1) % footstepArray.Length;
            //     audioSource.volume = volumeMultiplier * footstepVolumeMultiplier;
            //     return footstepArray[lastWoodFootstepIndex]; // Return the next clip in sequence
            // }
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
        else if (surface.CompareTag("Sand"))
        {
            footstepArray = sandFootsteps;  // Sand footstep logic
            volumeMultiplier = sandVolume;  // Set volume for sand
        }
        else
        {
            // Default to fallback sound if no tag matches
            footstepArray = fallbackFootsteps;
            volumeMultiplier = fallbackVolume; // Set volume for fallback
        }
        ///////////////////////////////////////////////////////////////////

        // Check if the footstep array is empty to avoid out-of-bounds errors
        if (footstepArray == null || footstepArray.Length == 0)
        {
            Debug.LogWarning("Footstep array is empty for surface: " + surface.tag);
            return footstepArray[0]; // Return null if no valid audio clips are available
        }
        ///////////////////////////////////////////////////////////////////

        // If only one sound is available, no need to repeat check
        if (footstepArray.Length == 1)
        {
            // Apply volume multiplier when playing the single sound
            audioSource.volume = volumeMultiplier * footstepVolumeMultiplier;
            return footstepArray[0]; // Always return the only sound in the array
        }
        //////////////////////////////////////////////////////////////////

        // Select a random footstep sound, ensuring it doesn't repeat more than twice
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, footstepArray.Length); // Randomly pick an index
        } while (randomIndex == lastFootstepIndex && repeatCount >= 2); // Avoid repeating more than twice
        ///////////////////////////////////////////////////////////////////

        // Update the repeat count and last footstep index
        if (randomIndex == lastFootstepIndex)
        {
            repeatCount++;
        }
        else
        {
            repeatCount = 0; // Reset repeat count when a new sound is selected
        }
        ////////////////////////////////////////////////////////////////////

        // Update the last played footstep index
        lastFootstepIndex = randomIndex;

        // Apply the volume multiplier for this footstep type
        audioSource.volume = volumeMultiplier * footstepVolumeMultiplier;

        // Return the selected footstep sound
        return footstepArray[randomIndex];
    }

    public bool GetFootstepsAllowed()
    {
        return footstepsEnabled;
    }

    public void SetFootstepsAllowed(bool isActive)
    {
        footstepsEnabled = isActive;
    }


    // PerformanceTestLoop() - Read something interesting if you're having a boring day...
    //
    // Question: What if the footsteps rolled on the same footstep a million times? Would it create a noticable performance impact?
    //
    //
    // For perspective, if you had only two footstep sounds, the probability of rolling the same one 1 million times in a row would be:
    // (1/2)^1,000,000
    // That's a 1 in 10^301030 chance — a number so absurdly small that it’s beyond comprehension.
    // Even if you walked every second for the entire lifetime of the universe, you wouldn't hit that streak.
    //
    // With three footstep sounds, it gets even more ridiculous:
    // (1/3)^1,0000,000
    // Which is around 1 in 10^477122 — you’d literally have better odds of quantum tunneling through a wall than experiencing that in a game.
    //
    //
    // TEST RESULTS 2/14/2025 (happy valentines day!):
    // 1,000,000 footstep checks took on average 12-13 ms. May vary depending on machine used.
    //
    // Conclusion:
    // On average though, you would be looking at 2-3 repeats, maybe up to 10 as extreme outliers. That would lie around 10-100 nanoseconds.
    // Our worst-case scenario is 1,000,000 and it's at most 12 ms.
    private void PerformanceTestLoop()
    {
        D.LogError("PerformanceTestLoop should not be called if you value your computer's resources.", gameObject, "Any");
        int lastIndex = -1;
        int count = 0;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        while (count < 1_000_000) // ONE MILLION CHECKS!!!
        {
            int newIndex = Random.Range(0, 10);
            if (newIndex != lastIndex) lastIndex = newIndex;
            count++;
        }

        sw.Stop();
        Debug.Log($"Time taken: {sw.ElapsedMilliseconds} ms");
    }
}
