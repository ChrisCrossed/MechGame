using UnityEngine;

public class c_Singleton : MonoBehaviour
{
    private static c_Singleton _instance;

    public static c_Singleton Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    private void OnDestroy()
    {
        if (this == _instance)
            _instance = null;
    }
}
