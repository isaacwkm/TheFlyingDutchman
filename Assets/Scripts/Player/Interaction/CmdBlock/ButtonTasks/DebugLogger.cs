using System;
using UnityEngine;

public class DebugLogger: ButtonTask
{
    [SerializeField] private string message = "Enter your debug log message here";
    private string messageSuffix;
    public override void DoTasks(GameObject player = null) {
        messageSuffix = $" (cmdBlock Task executed by player: {player.name}) ";
        Debug.Log(message + messageSuffix);
    }
    
}