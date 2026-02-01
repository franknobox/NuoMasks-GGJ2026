using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人抽象基类 - 2D俯视角游戏
/// 提供通用的生命值、移动、攻击和碰撞伤害逻辑
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    #region 数据管理 (Data Management)
    
    [Header("=== 敌人属性 ===")]
    [SerializeField] protected int maxHealth = 3;          // 最大生命值
    [SerializeField] protected float moveSpeed = 2f;       // 移动速度
    [SerializeField] protected int attackDamage = 1;       // 攻击伤害
    
    [Header("=== Boss 设置 ===")]
    public bool isBoss = false;                            // 是否是Boss（勾选后死亡时触发游戏结束）
    
    protected int currentHealth;                           // 当前生命值
    
    #endregion
    
    #region 目标锁定 (Target Tracking)
    
    [Header("=== 目标追踪 ===")]
    protected Transform playerTransform;                   // 玩家Transform缓存
    
    #endregion
    
    #region 组件引用
    
    protected Rigidbody2D rb;                              // 刚体组件
    
    #endregion
    
    #region Unity生命周期
    
    protected virtual void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        
        // 通过Tag查找玩家并缓存Transform
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            Debug.Log($"{gameObject.name} 找到玩家目标");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 未找到Player标签的对象！请确保玩家对象Tag设置为'Player'");
        }
    }
    
    protected virtual void Update()
    {
        // 调用子类的攻击逻辑
        PerformAttack();
    }
    
    protected virtual void FixedUpdate()
    {
        // 调用子类的移动逻辑
        PerformMovement();
    }
    
    #endregion
    
    #region 生命周期管理 (Health Management)
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="amount">伤害值</param>
    public void TakeDamage(int amount)
    {
        // 扣血
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} 受到 {amount} 点伤害，剩余生命值: {currentHealth}/{maxHealth}");
        
        // 触发受击回调（子类可重写添加特效）
        OnTakeDamage();
        
        // 检测死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 受击回调 - 虚方法，子类可重写添加受击特效
    /// 例如：变色、震动、播放音效等
    /// </summary>
    protected virtual void OnTakeDamage()
    {
        // 默认为空，子类可重写
        // 示例：StartCoroutine(FlashRed());
    }
    
    /// <summary>
    /// 死亡处理
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} 死亡");
        
        // TODO: 子类可重写添加死亡特效、掉落物品等
        
        // 检查是否是Boss
        if (isBoss)
        {
            // Boss死亡 - 触发游戏结束
            Debug.Log($"Boss {gameObject.name} 被击败！游戏结束");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowGameEnd();
            }
            
            // 销毁Boss对象
            Destroy(gameObject);
        }
        else
        {
            // 普通敌人 - 直接销毁
            Destroy(gameObject);
        }
    }
    
    #endregion
    
    #region 行为接口 (Behavior Interface)
    
    /// <summary>
    /// 执行移动逻辑 - 虚方法，子类重写实现具体移动行为
    /// 在 FixedUpdate 中调用
    /// </summary>
    protected virtual void PerformMovement()
    {
        // 默认为空
        // 子类示例：
        // - 追踪玩家移动
        // - 巡逻移动
        // - 随机移动
    }
    
    /// <summary>
    /// 执行攻击逻辑 - 虚方法，子类重写实现具体攻击行为
    /// 在 Update 中调用
    /// </summary>
    protected virtual void PerformAttack()
    {
        // 默认为空
        // 子类示例：
        // - 检测攻击距离
        // - 发射子弹
        // - 近战攻击判定
    }
    
    #endregion
    
    #region 碰撞伤害 (Collision Damage)
    
    /// <summary>
    /// 碰撞检测 - 碰到玩家时造成伤害
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查是否碰到玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            // 尝试获取玩家的TakeDamage方法
            PlayerGo player = collision.gameObject.GetComponent<PlayerGo>();
            
            if (player != null)
            {
                // 对玩家造成伤害
                player.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} 碰撞到玩家，造成 {attackDamage} 点伤害");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} 碰到Player标签对象，但未找到PlayerGo组件");
            }
        }
    }
    
    #endregion
    
    #region 辅助方法 (Helper Methods)
    
    /// <summary>
    /// 获取与玩家的距离
    /// </summary>
    /// <returns>距离值，如果玩家不存在返回float.MaxValue</returns>
    protected float GetDistanceToPlayer()
    {
        if (playerTransform != null)
        {
            return Vector2.Distance(transform.position, playerTransform.position);
        }
        return float.MaxValue;
    }
    
    /// <summary>
    /// 获取指向玩家的方向向量（已归一化）
    /// </summary>
    /// <returns>方向向量</returns>
    protected Vector2 GetDirectionToPlayer()
    {
        if (playerTransform != null)
        {
            return (playerTransform.position - transform.position).normalized;
        }
        return Vector2.zero;
    }
    
    #endregion
    
    #region 调试辅助
    
    /// <summary>
    /// 测试受伤（仅用于调试）
    /// </summary>
    [ContextMenu("测试受伤-1点")]
    private void TestTakeDamage()
    {
        TakeDamage(1);
    }
    
    #endregion
}
