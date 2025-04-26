using UnityEngine;

public class TaskExecutor : MonoBehaviour
{
    private ButtonTask[] commands;

    private void Awake()
    {
        commands = GetComponents<ButtonTask>();
    }

    public void ExecuteAllCommands(GameObject player = null)
    {
        foreach (var command in commands)
        {
            command.DoTasks(player);
        }
    }

    public void ExecuteCommand(int index, GameObject player = null)
    {
        if (index >= 0 && index < commands.Length)
        {
            commands[index].DoTasks(player);
        }
        else
        {
            Debug.LogError("Invalid command index!");
        }
    }
}
