using UnityEngine;

public class CinemTeleport : MonoBehaviour
{
    [SerializeField] private PlayerCharacterController playerController; // reference to player controller script
    public UsefulCommands commands;

    void Awake()
    {
        commands = SceneCore.commands;
    }

    public void CinematicTeleport(GameObject player, Transform reference)
    {
        // VFX go here
        commands.Teleport(player, reference);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
