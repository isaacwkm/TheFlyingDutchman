using UnityEngine;

public class SceneCore : MonoBehaviour
{
    [SerializeField] private PlayerCharacterController m_playerCharacter;
    [SerializeField] private Camera m_camera;
    [SerializeField] private AudioManager m_audioManager;
    [SerializeField] private ItemCatalog m_itemCatalog;
    [SerializeField] private FlyingVehicle m_ship;
    [SerializeField] private ResourceInteraction m_resourceInventory;
    [SerializeField] private StoryManager m_storyManager;
    [SerializeField] private Canvas m_canvas;
    [SerializeField] private UIStack m_uiStack;
    [SerializeField] private MainMenu m_mainMenu;

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

    public static FlyingVehicle ship
    {
        get { return instance.m_ship; }
    }

    public static ResourceInteraction resourceInventory
    {
        get { return instance.m_resourceInventory; }
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
}
