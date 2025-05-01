using UnityEngine;

public class TestCannonAnimator : MonoBehaviour
{
    [SerializeField] private Transform mainBody;
    [SerializeField] private Transform barrel;
    [SerializeField] private ProjectileLauncher launcher;

    // Update is called once per frame
    void Update()
    {
        mainBody.localRotation =
            Quaternion.Euler(0.0f, launcher.yaw, 0.0f);
        barrel.localRotation =
            Quaternion.Euler(launcher.pitch, 0.0f, 0.0f);
    }
}
