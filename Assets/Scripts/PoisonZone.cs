using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 毒液区域脚本
/// 持续对进入区域的玩家造成伤害
/// 穷奇面具可以免疫毒液伤害
/// </summary>
public class PoisonZone : MonoBehaviour
{
    #region 参数配置
    
    [Header("=== 伤害设置 ===")]
    [SerializeField] private int damageAmount = 1;              // 每次伤害值
    [SerializeField] private float damageInterval = 1.0f;       // 伤害间隔（秒）
    
    private float nextDamageTime = 0f;                          // 下次可以造成伤害的时间点
    
    [Header("=== 视觉反馈（可选）===")]
    [SerializeField] private Color poisonColor = new Color(0.5f, 1f, 0.3f, 0.3f);  // 毒液区域颜色（用于Gizmos）
    
    #endregion
    
    #region Unity生命周期
    
    void Start()
    {
        // 确保该物体有Collider2D并且Is Trigger已勾选
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"PoisonZone [{gameObject.name}] 缺少Collider2D组件！");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"PoisonZone [{gameObject.name}] 的Collider2D未设置为Trigger！已自动设置。");
            col.isTrigger = true;
        }
    }
    
    #endregion
    
    #region 触发检测
    
    /// <summary>
    /// 当玩家持续停留在毒液区域时触发
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    void OnTriggerStay2D(Collider2D other)
    {
        // 第一步：检查是否是玩家脚部碰撞器
        // 只检测BoxCollider2D（脚部），忽略CapsuleCollider2D（全身）
        if (!other.CompareTag("Player") || !(other is BoxCollider2D))
        {
            return; // 不是玩家或不是脚部碰撞器，忽略
        }
        
        // 第二步：获取玩家组件
        PlayerGo player = other.GetComponent<PlayerGo>();
        if (player == null)
        {
            Debug.LogWarning("检测到Player标签，但未找到PlayerGo组件！");
            return;
        }
        
        // 第三步：检查面具免疫
        // 如果玩家装备了穷奇面具，直接免疫毒液伤害
        if (player.currentMask == PlayerGo.MaskType.Qiongqi)
        {
            // 穷奇面具免疫毒液
            return;
        }
        
        // 第四步：伤害间隔判定
        // 只有达到伤害间隔时间才能造成伤害
        if (Time.time >= nextDamageTime)
        {
            // 造成伤害
            player.TakeDamage(damageAmount);
            
            // 更新下次伤害时间
            nextDamageTime = Time.time + damageInterval;
            
            // 调试信息
            Debug.Log($"<color=green>毒液伤害！玩家受到 {damageAmount} 点毒素伤害</color>");
        }
    }
    
    /// <summary>
    /// 当玩家进入毒液区域时触发（用于提示）
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 只检测脚部碰撞器（BoxCollider2D）
        if (other.CompareTag("Player") && other is BoxCollider2D)
        {
            PlayerGo player = other.GetComponent<PlayerGo>();
            if (player != null)
            {
                // 检查是否有穷奇面具
                if (player.currentMask == PlayerGo.MaskType.Qiongqi)
                {
                    Debug.Log("<color=cyan>穷奇面具生效！免疫毒液伤害</color>");
                }
                else
                {
                    Debug.Log("<color=yellow>警告：进入毒液区域！装备穷奇面具可免疫伤害</color>");
                }
                
                // 重置伤害计时器，确保进入时立即可以造成伤害
                nextDamageTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 当玩家离开毒液区域时触发
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    void OnTriggerExit2D(Collider2D other)
    {
        // 只检测脚部碰撞器（BoxCollider2D）
        if (other.CompareTag("Player") && other is BoxCollider2D)
        {
            Debug.Log("离开毒液区域");
        }
    }
    
    #endregion
    
    #region 编辑器可视化
    
    /// <summary>
    /// 在Scene视图中绘制毒液区域范围
    /// </summary>
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // 绘制毒液区域
            Gizmos.color = poisonColor;
            
            if (col is BoxCollider2D box)
            {
                // 矩形区域
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                // 圆形区域
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
    }
    
    /// <summary>
    /// 选中时绘制更明显的边框
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // 绘制边框
            Gizmos.color = Color.green;
            
            if (col is BoxCollider2D box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
    }
    
    #endregion
}
