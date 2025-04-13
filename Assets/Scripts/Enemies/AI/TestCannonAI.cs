using UnityEngine;

public class TestCannonAI : MonoBehaviour
{
    [SerializeField] ProjectileLauncher launcher;

    void Start()
    {
        launcher.Aim(SceneCore.playerCharacter);
    }

    // Update is called once per frame
    void Update()
    {
        if (launcher.aimTargetInLineOfFire) launcher.Launch();
    }
}
