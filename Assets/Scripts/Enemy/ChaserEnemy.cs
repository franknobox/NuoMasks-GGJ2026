using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 追击型敌人 - 蛊虫
/// 会持续追踪玩家，保持流畅的追击手感
/// </summary>
public class ChaserEnemy : EnemyBase
{
    #region 追击设置
    
    [Header("=== 追击设置 ===")]
    [SerializeField] private float chaseRange = 10f;        // 追击范围（超过此距离停止追击）
    [SerializeField] private float stopDistance = 0.5f;     // 停止距离（避免推着玩家走）
    
    private bool isChasing = false;                         // 当前是否在追击状态
    
    #endregion
    
    #region 朝向控制
    
    [Header("=== 朝向设置 ===")]
    [SerializeField] private bool useTransformFlip = true;  // 使用Transform翻转（适合帧动画）
    
    private SpriteRenderer spriteRenderer;                  // 精灵渲染器（用于Sprite翻转）
    private Transform visualTransform;                      // 视觉对象Transform（用于帧动画翻转）
    
    #endregion
    
    #region Unity生命周期
    
    protected override void Start()
    {
        base.Start(); // 调用父类初始化
        
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // 如果当前对象没有，尝试从子对象获取
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // 如果使用Transform翻转，获取视觉对象的Transform
        if (useTransformFlip)
        {
            // 优先使用子对象（通常帧动画在子对象上）
            if (spriteRenderer != null)
            {
                visualTransform = spriteRenderer.transform;
            }
            else
            {
                // 如果没有SpriteRenderer，尝试查找名为"Visual"或"Sprite"的子对象
                Transform visual = transform.Find("Visual");
                if (visual == null) visual = transform.Find("Sprite");
                if (visual == null) visual = transform.Find("Animator");
                
                if (visual != null)
                {
                    visualTransform = visual;
                }
                else
                {
                    // 使用自身Transform
                    visualTransform = transform;
                }
            }
            
            Debug.Log($"{gameObject.name} 使用Transform翻转模式，目标对象: {visualTransform.name}");
        }
        else if (spriteRenderer == null)
        {
            Debug.LogWarning($"{gameObject.name} 未找到SpriteRenderer组件，无法翻转朝向");
        }
    }
    
    #endregion
    
    #region 移动逻辑 (Movement Logic)
    
    /// <summary>
    /// 执行追击移动
    /// </summary>
    protected override void PerformMovement()
    {
        // 检查玩家是否存在
        if (playerTransform == null || rb == null)
        {
            isChasing = false;
            return;
        }
        
        // 计算与玩家的距离
        float distanceToPlayer = GetDistanceToPlayer();
        
        // 判断是否在追击范围内
        if (distanceToPlayer > chaseRange)
        {
            // 超出追击范围，停止追击（性能优化）
            isChasing = false;
            return;
        }
        
        // 判断是否已经足够接近玩家
        if (distanceToPlayer <= stopDistance)
        {
            // 已经很近了，停止移动（避免推着玩家走）
            isChasing = false;
            return;
        }
        
        // 开始追击
        isChasing = true;
        
        // 计算指向玩家的方向（已归一化）
        Vector2 direction = GetDirectionToPlayer();
        
        // 使用MovePosition进行平滑移动
        Vector2 targetPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
        
        // 更新朝向
        UpdateFacing(direction);
    }
    
    #endregion
    
    #region 朝向控制 (Facing Control)
    
    /// <summary>
    /// 根据移动方向更新朝向
    /// </summary>
    /// <param name="direction">移动方向</param>
    private void UpdateFacing(Vector2 direction)
    {
        // 判断朝向（左或右）
        bool facingRight = direction.x > 0.01f;
        bool facingLeft = direction.x < -0.01f;
        
        // 如果X方向接近0，不改变朝向
        if (!facingRight && !facingLeft)
        {
            return;
        }
        
        // 根据模式选择翻转方式
        if (useTransformFlip)
        {
            // 方式1：使用Transform的Scale翻转（适合帧动画）
            if (visualTransform != null)
            {
                Vector3 scale = visualTransform.localScale;
                
                if (facingRight)
                {
                    // 向右 - Scale.x为正
                    scale.x = Mathf.Abs(scale.x);
                }
                else if (facingLeft)
                {
                    // 向左 - Scale.x为负
                    scale.x = -Mathf.Abs(scale.x);
                }
                
                visualTransform.localScale = scale;
            }
        }
        else
        {
            // 方式2：使用SpriteRenderer的FlipX翻转（适合单张Sprite）
            if (spriteRenderer != null)
            {
                if (facingRight)
                {
                    spriteRenderer.flipX = false;
                }
                else if (facingLeft)
                {
                    spriteRenderer.flipX = true;
                }
            }
        }
    }
    
    #endregion
    
    #region 受击特效 (Hit Effect)
    
    /// <summary>
    /// 受击时的视觉反馈 - 短暂变红
    /// </summary>
    protected override void OnTakeDamage()
    {
        base.OnTakeDamage();
        
        // 播放受击闪烁效果
        StartCoroutine(FlashRed());
    }
    
    /// <summary>
    /// 红色闪烁协程
    /// 支持多种渲染器：SpriteRenderer, Spine, Animator等
    /// </summary>
    private IEnumerator FlashRed()
    {
        // 获取所有可能的渲染器组件
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        
        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name} 没有找到SpriteRenderer组件，无法显示受击特效");
            yield break;
        }
        
        // 保存原始颜色
        Color[] originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
            // 变红
            spriteRenderers[i].color = Color.red;
        }
        
        // 等待0.1秒
        yield return new WaitForSeconds(0.1f);
        
        // 恢复原始颜色
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null) // 防止对象被销毁
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }
    
    #endregion
    
    #region 调试辅助 (Debug Helpers)
    
    /// <summary>
    /// 在Scene视图中绘制追击范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制追击范围（黄色圆圈）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // 绘制停止距离（红色圆圈）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        
        // 如果正在追击，绘制指向玩家的线
        if (Application.isPlaying && isChasing && playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
    
    #endregion
}
