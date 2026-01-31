using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private bool movementEnabled = true;
    private Vector2 lastDirection = Vector2.down; // 记录最后朝向（默认向下）
    private string currentAnimation = ""; // 当前播放的动画名

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 在子对象中查找Animator组件
        animator = GetComponentInChildren<Animator>();
        
        // 调试检查
        if (rb == null)
        {
            Debug.LogError("Player缺少Rigidbody2D组件！");
        }
        if (animator == null)
        {
            Debug.LogError("Player或其子对象缺少Animator组件！");
        }
        else
        {
            Debug.Log("Animator找到了，位于: " + animator.gameObject.name);
        }
    }

    /// <summary>
    /// 设置移动是否启用（对话等时可冻结）
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled)
        {
            moveInput = Vector2.zero;
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }

    void Update()
    {
        if (!movementEnabled)
        {
            moveInput = Vector2.zero;
            UpdateAnimation();
            return;
        }
        // 获取WASD输入
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;

        moveInput = new Vector2(horizontal, vertical).normalized;
        
        // 更新动画
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (!movementEnabled)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }
        // 使用Rigidbody2D移动
        rb.velocity = moveInput * moveSpeed;
    }
    
    void UpdateAnimation()
    {
        // 判断是否在移动
        bool isMoving = moveInput.magnitude > 0;
        
        if (isMoving)
        {
            // 记录当前方向
            lastDirection = moveInput;
            
            // 判断主要方向（四方向）
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                // 左右移动优先
                if (moveInput.x > 0)
                {
                    PlayAnimation("walkright");
                }
                else
                {
                    PlayAnimation("walkleft");
                }
            }
            else
            {
                // 上下移动
                if (moveInput.y > 0)
                {
                    PlayAnimation("walkup");
                }
                else
                {
                    PlayAnimation("walkdown");
                }
            }
        }
        else
        {
            // 静止时播放idle动画
            PlayAnimation("idledown");
        }
    }
    
    void PlayAnimation(string animationName)
    {
        if (animator != null && currentAnimation != animationName)
        {
            animator.Play(animationName);
            currentAnimation = animationName;
        }
    }
}
