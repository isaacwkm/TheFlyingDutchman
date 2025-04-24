using UnityEngine;

public class SceneCore : MonoBehaviour
{
    [SerializeField] private PlayerCharacterController m_playerCharacter;
    [SerializeField] private Camera m_camera;
    [SerializeField] private AudioManager m_audioManager;
    [SerializeField] private ItemCatalog m_itemCatalog;
    [SerializeField] private PlayerShip m_ship;
    [SerializeField] private StoryManager m_storyManager;
    [SerializeField] private Canvas m_canvas;
    [SerializeField] private UIStack m_uiStack;
    [SerializeField] private MainMenu m_mainMenu;
    [SerializeField] private CutsceneEffectsPlayer m_cinematics;
    [SerializeField] private UsefulCommands m_commands;

    private static SceneCore cachedInstance = null;
    // and then replace instance property with
    private static SceneCore instance
    {
        get
        {
            if (!cachedInstance)
            {
                cachedInstance = GameObject.FindWithTag("SceneCore").GetComponent<SceneCore>();
            }
            return cachedInstance;
        }
    }

    public static PlayerCharacterController playerCharacter
    {
        get { return instance.m_playerCharacter; }
    }

    public static new Camera camera
    {
        get { return instance.m_camera; }
    }

    public static AudioManager audioManager
    {
        get { return instance.m_audioManager; }
    }

    public static ItemCatalog itemCatalog
    {
        get { return instance.m_itemCatalog; }
    }

    public static PlayerShip ship
    {
        get { return instance.m_ship; }
    }

    public static ResourceInteraction resourceInventory
    {
        get { return instance.m_ship.resourceInventory; }
    }

    public static StoryManager storyManager
    {
        get { return instance.m_storyManager; }
    }

    public static Canvas canvas
    {
        get { return instance.m_canvas; }
    }

    public static UIStack uiStack
    {
        get { return instance.m_uiStack; }
    }

    public static MainMenu MainMenu()
    {
        return uiStack.Call(instance.m_mainMenu);
    }

    public static CutsceneEffectsPlayer cinematics
    {
        get { return instance.m_cinematics; }
    }

    public static UsefulCommands commands
    {
        get { return instance.m_commands; }
    }

    void Start()
    {
        if (m_ship == null)
        {
            Debug.Log(
                "WARNING: SceneCore now holds a reference to the ship object, " +
                "but since the ship object is not a child of SceneCore, " +
                "that reference cannot be set in the SceneCore prefab, " +
                "and must be set per-scene. In the current scene, " +
                "it is not set, so, as a fallback, SceneCore will find it " +
                "with Object.FindAnyObjectByType."
            );
            m_ship = Object.FindAnyObjectByType<PlayerShip>();
        }
    }
}
