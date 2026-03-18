using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        // 移动角色
        transform.Translate(Vector3.right * move * speed * Time.deltaTime);

        // 控制动画
        animator.SetBool("isRunning", move != 0);

        // 控制角色朝向
        if (move > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // 朝右
        }
        else if (move < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 朝左
        }
    }
}