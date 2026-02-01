using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 树根弹幕脚本
/// 腾根面具的攻击弹幕，可以对敌人造成伤害
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RootProjectile : MonoBehaviour
{
    #region 变量定义
    
    private Rigidbody2D rb;                 // 刚体组件
    private Vector2 direction;              // 飞行方向
    private float speed;                    // 飞行速度
    private int damage;                     // 伤害值
    
    private bool isInitialized = false;     // 是否已初始化
    
    #endregion
    
    #region Unity生命周期
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 配置刚体
        if (rb != null)
        {
            rb.gravityScale = 0f;           // 不受重力影响
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // 修复：重置所有子物体的本地位置为(0,0,0)
        // 防止Spine动画子物体有偏移导致视觉位置不对
        foreach (Transform child in transform)
        {
            if (child.localPosition != Vector3.zero)
            {
                Debug.Log($"重置子物体 [{child.name}] 位置从 {child.localPosition} 到 (0,0,0)");
                child.localPosition = Vector3.zero;
            }
        }
    }
    
    void FixedUpdate()
    {
        // 只有初始化后才移动
        if (!isInitialized || rb == null)
        {
            return;
        }
        
        // 使用MovePosition移动子弹
        Vector2 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
    
    #endregion
    
    #region 初始化
    
    /// <summary>
    /// 初始化树根弹幕
    /// </summary>
    /// <param name="dir">飞行方向</param>
    /// <param name="spd">飞行速度</param>
    /// <param name="dmg">伤害值</param>
    public void Initialize(Vector2 dir, float spd, int dmg)
    {
        // 存储参数
        direction = dir.normalized;     // 归一化方向向量
        speed = spd;
        damage = dmg;
        
        // 计算旋转角度，让子弹的X轴朝向飞行方向
        // Atan2返回弧度，需要转换为角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 标记为已初始化
        isInitialized = true;
        
        // 2秒后自动销毁，防止子弹飞到天涯海角
        Destroy(gameObject, 2f);
    }
    
    #endregion
    
    #region 碰撞检测
    
    /// <summary>
    /// 触发碰撞检测
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 情况1：碰到敌人
        if (other.CompareTag("Enemy"))
        {
            // 获取敌人的EnemyBase组件
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            
            if (enemy != null)
            {
                // 对敌人造成伤害
                enemy.TakeDamage(damage);
                Debug.Log($"<color=orange>树根命中敌人 [{other.gameObject.name}]，造成 {damage} 点伤害！</color>");
            }
            else
            {
                Debug.LogWarning($"敌人 [{other.gameObject.name}] 没有EnemyBase组件！");
            }
            
            // 销毁子弹
            Destroy(gameObject);
            return;
        }
        
        // 情况2：碰到墙壁/障碍物
        // 优先使用Layer检测，然后使用名称检测（不依赖Tag）
        string objName = other.gameObject.name.ToLower();
        
        // 检查Layer是否为Wall
        bool isWallLayer = other.gameObject.layer == LayerMask.NameToLayer("Wall");
        
        // 检查名称是否包含墙壁/障碍物关键字
        bool isWallByName = objName.Contains("wall") || 
                            objName.Contains("obstacle") || 
                            objName.Contains("ground") || 
                            objName.Contains("tilemap");
        
        if (isWallLayer || isWallByName)
        {
            Debug.Log($"树根撞到障碍物 [{other.gameObject.name}]，销毁");
            Destroy(gameObject);
            return;
        }
    }
    
    #endregion
    
    #region 编辑器可视化
    
    /// <summary>
    /// 在Scene视图中绘制子弹飞行方向
    /// </summary>
    void OnDrawGizmos()
    {
        if (isInitialized)
        {
            // 绘制飞行方向
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * 2f));
            
            // 绘制速度指示器
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
    
    #endregion
}
