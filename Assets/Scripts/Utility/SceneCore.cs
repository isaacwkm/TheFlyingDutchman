using UnityEngine;

public class SceneCore : MonoBehaviour
{
    [SerializeField] private PlayerCharacterController m_playerCharacter;
    [SerializeField] private Camera m_camera;
    [SerializeField] private AudioManager m_audioManager;
    [SerializeField] private ItemCatalog m_itemCatalog;
    [SerializeField] private Canvas m_canvas;

    private static SceneCore instance
    {
        get { return GameObject.FindWithTag("SceneCore").GetComponent<SceneCore>(); }
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

    public static Canvas canvas
    {
        get { return instance.m_canvas; }
    }
}
