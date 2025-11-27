using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] RawInputManager inputManager;
    void Start()
    {
        
    }

    void Update()
    {
        Debug.Log(inputManager.Move());
    }
}
