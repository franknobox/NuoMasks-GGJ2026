using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 毒液子弹 - Boss发射的远程攻击弹药
/// </summary>
public class AcidProjectile : MonoBehaviour
{
    #region 配置参数
    
    [Header("=== 子弹设置 ===")]
    [SerializeField] private float projectileSpeed = 8f;    // 子弹速度
    [SerializeField] private float lifeTime = 4f;           // 存活时间（秒）
    
    #endregion
    
    #region 运行时数据
    
    private Rigidbody2D rb;                                 // 刚体组件
    private Vector2 direction;                              // 飞行方向
    private int damage;                                     // 伤害值
    private bool isInitialized = false;                     // 是否已初始化
    
    #endregion
    
    #region Unity生命周期
    
    void Start()
    {
        // 获取刚体组件
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError($"{gameObject.name} 缺少Rigidbody2D组件！");
            Destroy(gameObject);
            return;
        }
        
        // 设置自动销毁（防止子弹飞到天涯海角）
        Destroy(gameObject, lifeTime);
        
        Debug.Log($"毒液子弹生成，{lifeTime}秒后自动销毁");
    }
    
    #endregion
    
    #region 初始化方法
    
    /// <summary>
    /// 初始化子弹（由Boss调用）
    /// </summary>
    /// <param name="dir">飞行方向（已归一化）</param>
    /// <param name="dmg">伤害值</param>
    public void Initialize(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;
        isInitialized = true;
        
        // 如果刚体已经获取到，立即施加速度
        if (rb != null)
        {
            ApplyVelocity();
        }
        else
        {
            // 否则等Start中获取刚体后再施加
            StartCoroutine(DelayedApplyVelocity());
        }
        
        Debug.Log($"毒液子弹初始化：方向={direction}, 伤害={damage}");
    }
    
    /// <summary>
    /// 延迟施加速度（确保刚体已获取）
    /// </summary>
    private IEnumerator DelayedApplyVelocity()
    {
        // 等待一帧，确保Start已执行
        yield return null;
        
        if (rb != null)
        {
            ApplyVelocity();
        }
    }
    
    /// <summary>
    /// 施加速度
    /// </summary>
    private void ApplyVelocity()
    {
        rb.velocity = direction * projectileSpeed;
        
        // 可选：旋转子弹朝向飞行方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    #endregion
    
    #region 碰撞检测
    
    /// <summary>
    /// 触发器碰撞检测
    /// </summary>
    /// <param name="collision">碰撞对象</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 忽略与Enemy的碰撞
        if (collision.CompareTag("Enemy"))
        {
            return;
        }
        
        // 碰到玩家
        if (collision.CompareTag("Player"))
        {
            // 尝试获取玩家脚本并造成伤害
            PlayerGo player = collision.GetComponent<PlayerGo>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"毒液命中玩家，造成 {damage} 点伤害");
            }
            
            // 销毁子弹
            Destroy(gameObject);
            return;
        }
        
        // 碰到墙壁或障碍物（使用安全的检查方式）
        // 方法1：检查图层
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Debug.Log("毒液撞墙销毁（图层检测）");
            Destroy(gameObject);
            return;
        }
        
        
        // 其他情况：忽略道具、收集品等（不销毁子弹）
        // 只有明确需要销毁的情况才销毁
    }
    
    #endregion
    
    #region 调试辅助
    
    /// <summary>
    /// 在Scene视图中绘制子弹方向
    /// </summary>
    private void OnDrawGizmos()
    {
        if (isInitialized)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * 2f));
        }
    }
    
    #endregion
}
