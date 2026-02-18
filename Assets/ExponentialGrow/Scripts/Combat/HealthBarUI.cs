using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[InlineEditor]
public class HealthBarUI : MonoBehaviour
{
    public RectTransform holder;

    public Image emptyHeart;//-> un solo prefab HeartUI que maneje 3 estados y animaciones
    public Image Heart;
    public Image HalfHeart;

    public void Start()
    {
        
    }
    public void Set(int hitpoints,int maxHitpoints)
    {
        ResetHealthBar();

        int maxHearts = maxHitpoints / 2;

        int fullHearts = hitpoints / 2;
        int halfHearts = hitpoints % 2;


        for (int i = 0; i < fullHearts; i++)
        {
            Instantiate(Heart, holder);
        }
        if (halfHearts == 1)
        {
            Instantiate(HalfHeart, holder);
        }
        int emptyHeartsCount = maxHearts - (fullHearts + halfHearts);

        for (int i = 0; i < emptyHeartsCount; i++)
        {
            Instantiate(emptyHeart, holder);
        }
    }
    public void ResetHealthBar()
    {
        foreach (Transform child in holder)
        {
            Destroy(child.gameObject);
        }
    }

}
