using UnityEngine;

[CreateAssetMenu(fileName = "KeyCapSO", menuName = "Scriptable Objects/KeyCapSO")]
public class KeyCapSO : ScriptableObject
{
    [SerializeField] private KeyCapType keycode;

    [SerializeField] private GameObject prefab;



    public KeyCapType KeyCode => keycode;
    public GameObject Prefab => prefab;
}
