using UnityEngine;
using UnityEngine.UI;
public enum SpriteType
{
    Full,
    Half
}
public class ArrowKeyUI : MonoBehaviour
{
    
    public Image visualArrow;
    public Image visualIconType;

    void Start()
    {
       // image = GetComponent<Image>();
    }
    public void Set(KeyCapType type,SpriteType spriteType,ComboType comboType = ComboType.None)
    {
        visualArrow.sprite = GameManagerUI.Instance.reourceUIDatabaseSO.GetKeyCapSprite(type);

        Sprite sprite = GameManagerUI.Instance.reourceUIDatabaseSO.GetComboTypeSprite(comboType);
        if (sprite == null)
            visualIconType.gameObject.SetActive(false);
        else
            visualIconType.sprite = sprite;

        Color color = visualArrow.color;
        switch (spriteType)
        {
            case SpriteType.Full:
                color.a = 1f;   
                break;
            case SpriteType.Half:
                color.a = 0.7f;
                break;
        }
        visualArrow.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
