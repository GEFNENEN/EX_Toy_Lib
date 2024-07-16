using UnityEngine;
using UnityEngine.UI;

public class Directory : MonoBehaviour
{
    public Toggle toggleRotation;
    public GameObject prefab;
    
    private static Directory _instance;

    public static Directory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("Directory").AddComponent<Directory>();
            }
            return _instance;
        }
    }
}
