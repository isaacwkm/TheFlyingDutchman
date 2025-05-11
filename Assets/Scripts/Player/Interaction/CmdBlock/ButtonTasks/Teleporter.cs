using System;
using UnityEngine;

public class Teleporter: ButtonTask
{
    [SerializeField] private GameObject subjectObject; // reference to player object if teleporting the player
    [SerializeField] private Vector3 relativeCoordinates = Vector3.zero; // The offset from the referenceObject or scene origin
    [SerializeField] private Transform referenceObject; // The Transform of the gameobject to use as a reference (e.g., the player's ship)

    UsefulCommands usefulCommands;

    void OnEnable()
    {
        usefulCommands = SceneCore.commands;
    }

    public void Teleport()
    {
        usefulCommands.Teleport(subjectObject, referenceObject, relativeCoordinates);
    }
}
