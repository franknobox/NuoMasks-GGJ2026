using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蒸汽陷阱脚本
/// 喷射高温蒸汽的破损管道，周期性开关
/// 造成物理伤害，无视面具
/// </summary>
public class SteamTrap : MonoBehaviour
{
    #region 组件引用
    
    [Header("=== 组件引用 ===")]
    public ParticleSystem steamParticles;       // 蒸汽粒子特效
    public Collider2D damageArea;               // 伤害区域碰撞器
    
    #endregion
    
    #region 参数配置
    
    [Header("=== 时间配置 ===")]
    public float activeTime = 2f;               // 喷射持续时间（秒）
    public float inactiveTime = 2f;             // 停止持续时间（秒）
    public float startDelay = 0f;               // 初始延迟（用于错峰喷射）
    
    [Header("=== 伤害配置 ===")]
    public int damage = 20;                     // 伤害值
    public float damageInterval = 0.5f;         // 伤害间隔（秒）
    
    private bool isActive = false;              // 当前是否处于喷射状态
    private float nextDamageTime = 0f;          // 下次可以造成伤害的时间
    
    #endregion
    
    #region Unity生命周期
    
    void Start()
    {
        // 初始化：确保开始时是关闭状态
        SetSteamState(false);
        
        // 启动蒸汽开关协程
        StartCoroutine(ToggleSteamRoutine());
        
        // 检查组件
        if (steamParticles == null)
        {
            Debug.LogWarning($"SteamTrap [{gameObject.name}] 未设置 steamParticles！");
        }
        
        if (damageArea == null)
        {
            Debug.LogWarning($"SteamTrap [{gameObject.name}] 未设置 damageArea！");
        }
        else if (!damageArea.isTrigger)
        {
            Debug.LogWarning($"SteamTrap [{gameObject.name}] 的 damageArea 未设置为 Trigger！已自动设置。");
            damageArea.isTrigger = true;
        }
    }
    
    #endregion
    
    #region 蒸汽开关逻辑
    
    /// <summary>
    /// 蒸汽开关协程
    /// 周期性地开启和关闭蒸汽
    /// </summary>
    private IEnumerator ToggleSteamRoutine()
    {
        // 初始延迟（只在第一次生效）
        if (startDelay > 0)
        {
            Debug.Log($"<color=yellow>蒸汽陷阱 [{gameObject.name}] 初始延迟 {startDelay} 秒</color>");
            yield return new WaitForSeconds(startDelay);
        }
        
        // 无限循环
        while (true)
        {
            // 阶段1：关闭状态
            SetSteamState(false);
            Debug.Log($"<color=cyan>蒸汽陷阱 [{gameObject.name}] 将保持关闭 {inactiveTime} 秒</color>");
            yield return new WaitForSeconds(inactiveTime);
            
            // 阶段2：开启状态
            SetSteamState(true);
            Debug.Log($"<color=red>蒸汽陷阱 [{gameObject.name}] 将保持开启 {activeTime} 秒</color>");
            yield return new WaitForSeconds(activeTime);
        }
    }
    
    /// <summary>
    /// 设置蒸汽状态
    /// </summary>
    /// <param name="active">是否激活</param>
    private void SetSteamState(bool active)
    {
        isActive = active;
        
        // 控制粒子特效
        if (steamParticles != null)
        {
            if (active)
            {
                // 启用粒子系统GameObject
                steamParticles.gameObject.SetActive(true);
                
                // 播放粒子
                steamParticles.Play();
                
                Debug.Log($"<color=green>粒子系统已启动 - isPlaying: {steamParticles.isPlaying}, isEmitting: {steamParticles.isEmitting}</color>");
            }
            else
            {
                // 方法1：立即停止并清除所有粒子
                steamParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                
                // 方法2：强制清除所有粒子（确保立即消失）
                steamParticles.Clear(true);
                
                // 方法3：禁用粒子系统GameObject（最强力）
                steamParticles.gameObject.SetActive(false);
                
                Debug.Log($"<color=red>粒子系统已停止 - isPlaying: {steamParticles.isPlaying}, isEmitting: {steamParticles.isEmitting}, particleCount: {steamParticles.particleCount}</color>");
            }
        }
        else
        {
            Debug.LogWarning($"<color=yellow>steamParticles 为 null！无法控制粒子特效</color>");
        }
        
        // 控制伤害区域
        if (damageArea != null)
        {
            damageArea.enabled = active;
        }
        
        // 调试信息
        Debug.Log($"<color=cyan>蒸汽陷阱 [{gameObject.name}] {(active ? "开启" : "关闭")}</color>");
    }
    
    #endregion
    
    #region 伤害判定
    
    /// <summary>
    /// 当玩家持续停留在蒸汽区域时触发
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    void OnTriggerStay2D(Collider2D other)
    {
        // 只在蒸汽激活时造成伤害
        if (!isActive)
        {
            return;
        }
        
        // 检查是否是玩家
        if (!other.CompareTag("Player"))
        {
            return;
        }
        
        // 获取玩家组件
        PlayerGo player = other.GetComponent<PlayerGo>();
        if (player == null)
        {
            Debug.LogWarning("检测到Player标签，但未找到PlayerGo组件！");
            return;
        }
        
        // 伤害间隔判定
        if (Time.time >= nextDamageTime)
        {
            // 造成伤害（高温物理伤害，无视面具）
            player.TakeDamage(damage);
            
            // 更新下次伤害时间
            nextDamageTime = Time.time + damageInterval;
            
            // 调试信息
            Debug.Log($"<color=red>蒸汽伤害！玩家受到 {damage} 点高温伤害</color>");
        }
    }
    
    /// <summary>
    /// 当玩家进入蒸汽区域时触发
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            Debug.Log("<color=yellow>警告：进入高温蒸汽区域！</color>");
            
            // 重置伤害计时器，确保进入时立即可以造成伤害
            nextDamageTime = Time.time;
        }
    }
    
    /// <summary>
    /// 当玩家离开蒸汽区域时触发
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("离开蒸汽区域");
        }
    }
    
    #endregion
    
    #region 编辑器可视化
    
    /// <summary>
    /// 在Scene视图中绘制蒸汽区域范围
    /// </summary>
    void OnDrawGizmos()
    {
        if (damageArea != null)
        {
            // 根据激活状态选择颜色
            Gizmos.color = isActive ? new Color(1f, 0.3f, 0.3f, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
            
            if (damageArea is BoxCollider2D box)
            {
                // 矩形区域
                Gizmos.matrix = damageArea.transform.localToWorldMatrix;
                Gizmos.DrawCube(box.offset, box.size);
            }
            else if (damageArea is CircleCollider2D circle)
            {
                // 圆形区域
                Gizmos.DrawSphere(damageArea.transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
    }
    
    /// <summary>
    /// 选中时绘制更明显的边框
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (damageArea != null)
        {
            // 绘制边框
            Gizmos.color = isActive ? Color.red : Color.gray;
            
            if (damageArea is BoxCollider2D box)
            {
                Gizmos.matrix = damageArea.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (damageArea is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(damageArea.transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
    }
    
    #endregion
}
