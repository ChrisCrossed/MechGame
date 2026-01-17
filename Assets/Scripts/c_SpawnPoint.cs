using UnityEngine;

public class c_SpawnPoint : MonoBehaviour
{
    [SerializeField] int PlayerNumber;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i < 3; i++)
            gameObject.transform.Find("Cube_" + i).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
