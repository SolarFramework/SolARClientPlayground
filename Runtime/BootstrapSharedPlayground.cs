using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapSharedPlayground : MonoBehaviour
{
    public bool BootstrapOnStart = true;

    private string m_scene;
    private bool loaded = false;

    void Awake()
    {
        // Scene that contains all NetCode to communicate with the server
        m_scene = "Packages/com.b-com.shared-playground/Samples/Demo/DemoScene";    
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (BootstrapOnStart & !loaded)
        {
            LoadSharedPlayground();
        }    
    }

    public void LoadSharedPlayground()
    {
        SceneManager.LoadScene(m_scene, LoadSceneMode.Additive);
        loaded = true;
    }
}
