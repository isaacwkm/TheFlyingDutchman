using UnityEngine;

public class TestCannonAI : MonoBehaviour
{
    [SerializeField] ProjectileLauncher launcher;

    // Update is called once per frame
    void Update()
    {
        launcher.Aim(SceneCore.playerCharacter);
        if (launcher.aimTargetInLineOfFire) launcher.Launch();
    }
}
