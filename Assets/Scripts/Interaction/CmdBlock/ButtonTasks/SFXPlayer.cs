using System;
using UnityEngine;

public class SFXPlayer: ButtonTask
{
    [SerializeField] private ActionSound sound;
    [SerializeField] private int soundClipIndex = 0;
    [SerializeField] private bool playRandomClip = true;
    public override void DoTasks(GameObject player = null) {
        if (playRandomClip == true) sound.PlaySingleRandom();
        else sound.PlaySingleAtIndex(soundClipIndex);
    }
}
