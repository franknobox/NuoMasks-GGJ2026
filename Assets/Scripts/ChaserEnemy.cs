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
    
    private SpriteRenderer spriteRenderer;                  // 精灵渲染器（用于翻转）
    
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
        
        if (spriteRenderer == null)
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
        if (spriteRenderer == null) return;
        
        // 根据X轴方向翻转Sprite
        if (direction.x > 0.01f)
        {
            // 向右移动 - 不翻转
            spriteRenderer.flipX = false;
        }
        else if (direction.x < -0.01f)
        {
            // 向左移动 - 翻转
            spriteRenderer.flipX = true;
        }
        // 如果direction.x接近0，保持当前朝向不变
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
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
    }
    
    /// <summary>
    /// 红色闪烁协程
    /// </summary>
    private IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        
        yield return new WaitForSeconds(0.1f);
        
        spriteRenderer.color = originalColor;
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
