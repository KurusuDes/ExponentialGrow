using DG.Tweening;
using UnityEngine;

[RequireComponent (typeof(Animator))]
public class AttackDirectionVisualController : MonoBehaviour
{
    private Animator animator;


    private void Awake()
    {
        animator = GetComponent<Animator>();    
    }
    public void Set(KeyCapType cap)
    {
        switch (cap)
        {
            case KeyCapType.Up:
                animator.Play("ArrowUpAnim");
                break;
            case KeyCapType.Down:
                animator.Play("ArrowDownAnim");
                break;
            case KeyCapType.Left:
                animator.Play("ArrowLeftAnim");
                break;
            case KeyCapType.Right:
                animator.Play("ArrowRightAnim");
                break;
            case KeyCapType.Item:
                animator.Play("Interact");
                break;
            default:
                break;
        }

        float duration = animator.GetCurrentAnimatorStateInfo(0).length;

        Destroy(gameObject, duration);
    }
    void Start()
    {
        
    }

}
