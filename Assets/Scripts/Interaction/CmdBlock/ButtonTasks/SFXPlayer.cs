using System;
using UnityEngine;
using UnityEngine.LightTransport.PostProcessing;

public class SFXPlayer: ButtonTask
{
    [SerializeField] private SingleSoundComponent sound;
    [SerializeField] private int soundClipIndex = 0;
    [SerializeField] private bool playRandomClip = true;
    public override void DoTasks(GameObject player = null) {
        if (playRandomClip == true) sound.PlaySingleRandom();
        else sound.PlaySingleAtIndex(soundClipIndex);
    }
}
