using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蛊Boss - 远程攻击型敌人
/// 保持距离并发射毒液子弹攻击玩家
/// </summary>
public class GuBoss : EnemyBase
{
    #region 攻击配置 (Attack Configuration)
    
    [Header("=== 攻击设置 ===")]
    [SerializeField] private float attackRange = 8f;            // 攻击距离
    [SerializeField] private GameObject projectilePrefab;       // 毒液子弹预制体
    [SerializeField] private Transform firePoint;               // 子弹发射点
    [SerializeField] private float attackCooldown = 2f;         // 攻击冷却时间
    
    [Header("=== 检测设置 ===")]
    [SerializeField] private float detectionRange = 15f;        // 玩家检测距离（超过此距离Boss不激活）
    [SerializeField] private bool alwaysActive = false;         // 是否始终激活（不受距离限制）
    
    private float lastAttackTime = -999f;                       // 上次攻击时间
    private bool isActive = false;                              // Boss是否已激活
    
    #endregion
    
    #region 朝向控制 (Facing Control)
    
    private SpriteRenderer spriteRenderer;                      // 精灵渲染器
    
    #endregion
    
    #region Unity生命周期
    
    protected override void Start()
    {
        base.Start(); // 调用父类初始化
        
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"{gameObject.name} 未找到SpriteRenderer组件，无法翻转朝向");
        }
        
        // 检查必需组件
        if (projectilePrefab == null)
        {
            Debug.LogError($"{gameObject.name} 缺少子弹预制体！请在Inspector中设置Projectile Prefab");
        }
        
        if (firePoint == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少发射点！将使用自身位置作为发射点");
            firePoint = transform;
        }
    }
    
    #endregion
    
    #region 移动逻辑 (Movement Logic)
    
    /// <summary>
    /// 执行移动逻辑 - 保持在攻击距离内
    /// </summary>
    protected override void PerformMovement()
    {
        // 检查玩家是否存在
        if (playerTransform == null || rb == null)
        {
            return;
        }
        
        // 计算与玩家的距离
        float distanceToPlayer = GetDistanceToPlayer();
        
        // 检测玩家距离，决定是否激活Boss
        if (!alwaysActive)
        {
            if (distanceToPlayer <= detectionRange)
            {
                if (!isActive)
                {
                    isActive = true;
                    Debug.Log($"{gameObject.name} 检测到玩家，Boss激活！");
                }
            }
            else
            {
                // 玩家超出检测范围，Boss不激活
                isActive = false;
                rb.velocity = Vector2.zero;
                return;
            }
        }
        else
        {
            isActive = true; // 始终激活模式
        }
        
        // 始终面朝玩家
        FacePlayer();
        
        // 根据距离决定移动策略
        if (distanceToPlayer > attackRange)
        {
            // 距离太远，向玩家靠近
            Vector2 direction = GetDirectionToPlayer();
            Vector2 targetPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
        else
        {
            // 在攻击范围内，停止移动（保持原地）
            rb.velocity = Vector2.zero;
        }
    }
    
    #endregion
    
    #region 攻击逻辑 (Attack Logic)
    
    /// <summary>
    /// 执行攻击逻辑 - 发射毒液子弹
    /// </summary>
    protected override void PerformAttack()
    {
        // 检查玩家是否存在
        if (playerTransform == null)
        {
            return;
        }
        
        // 如果Boss未激活，不攻击
        if (!isActive)
        {
            return;
        }
        
        // 计算与玩家的距离
        float distanceToPlayer = GetDistanceToPlayer();
        
        // 检查是否在攻击范围内
        if (distanceToPlayer > attackRange)
        {
            return; // 超出射程，不攻击
        }
        
        // 检查冷却时间
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Shoot();
            lastAttackTime = Time.time;
        }
    }
    
    /// <summary>
    /// 发射子弹
    /// </summary>
    private void Shoot()
    {
        // 检查必需组件
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning($"{gameObject.name} 无法发射子弹：缺少预制体或发射点");
            return;
        }
        
        // 生成子弹
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // 计算指向玩家的方向
        Vector2 direction = GetDirectionToPlayer();
        
        // 尝试调用子弹的Initialize方法（假设子弹脚本有此方法）
        var projectileScript = projectile.GetComponent<MonoBehaviour>();
        if (projectileScript != null)
        {
            // 使用反射调用Initialize方法（如果存在）
            var initMethod = projectileScript.GetType().GetMethod("Initialize");
            if (initMethod != null)
            {
                initMethod.Invoke(projectileScript, new object[] { direction, attackDamage });
            }
            else
            {
                Debug.LogWarning($"子弹预制体 {projectilePrefab.name} 没有Initialize方法");
            }
        }
        
        Debug.Log($"{gameObject.name} 发射毒液子弹！");
    }
    
    #endregion
    
    #region 朝向控制 (Facing Control)
    
    /// <summary>
    /// 面朝玩家
    /// </summary>
    private void FacePlayer()
    {
        if (spriteRenderer == null || playerTransform == null)
        {
            return;
        }
        
        // 计算玩家相对位置
        float directionX = playerTransform.position.x - transform.position.x;
        
        // 根据X轴方向翻转Sprite
        if (directionX > 0.01f)
        {
            // 玩家在右边 - 不翻转
            spriteRenderer.flipX = false;
        }
        else if (directionX < -0.01f)
        {
            // 玩家在左边 - 翻转
            spriteRenderer.flipX = true;
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
        
        yield return new WaitForSeconds(0.15f);
        
        spriteRenderer.color = originalColor;
    }
    
    #endregion
    
    #region 调试可视化 (Debug Visualization)
    
    /// <summary>
    /// 在Scene视图中绘制攻击范围和检测范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制检测范围（蓝色圆圈）
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f); // 半透明蓝色
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制攻击范围（红色圆圈）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 如果有发射点，绘制发射点位置
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            
            // 绘制从Boss到发射点的连线
            Gizmos.DrawLine(transform.position, firePoint.position);
        }
        
        // 如果正在运行且玩家存在，绘制指向玩家的线
        if (Application.isPlaying && playerTransform != null)
        {
            float distance = GetDistanceToPlayer();
            
            // 根据距离和激活状态改变颜色
            if (!isActive && !alwaysActive)
            {
                Gizmos.color = Color.gray;  // 未激活
            }
            else if (distance <= attackRange)
            {
                Gizmos.color = Color.green; // 在射程内
            }
            else
            {
                Gizmos.color = Color.yellow; // 激活但超出射程
            }
            
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
    
    #endregion
}
