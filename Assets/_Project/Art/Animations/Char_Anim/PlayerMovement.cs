using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;

    private Animator animator;
    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        // ✅ 移动（新版写法）
        rb.linearVelocity = new Vector3(move * speed, rb.linearVelocity.y, 0);

        // 动画
        animator.SetBool("isRunning", move != 0);

        // 朝向
        if (move > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (move < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        // 跳跃
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0);
            animator.SetTrigger("Jump");
        }
    }
}