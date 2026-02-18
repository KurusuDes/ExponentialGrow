using UnityEngine;

public class GameManagerUI : MonoBehaviour
{
    public static GameManagerUI Instance;
    public ReourceUIDatabaseSO reourceUIDatabaseSO;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
