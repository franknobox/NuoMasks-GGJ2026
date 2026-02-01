using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 面具拾取脚本 - 玩家触碰后解锁对应面具能力
/// </summary>
public class MaskPickup : MonoBehaviour
{
    #region 配置参数
    
    [Header("=== 面具设置 ===")]
    [Tooltip("选择这个拾取物代表哪个面具")]
    public PlayerGo.MaskType maskType = PlayerGo.MaskType.Qiongqi;
    
    [Header("=== 可选效果 ===")]
    [SerializeField] private AudioClip pickupSound;             // 拾取音效（可选）
    [SerializeField] private GameObject pickupEffect;           // 拾取特效（可选）
    
    #endregion
    
    #region Unity生命周期
    
    void Start()
    {
        // 验证面具类型
        if (maskType == PlayerGo.MaskType.None)
        {
            Debug.LogWarning($"{gameObject.name} 的面具类型设置为None，这可能不是预期的配置");
        }
    }
    
    #endregion
    
    #region 拾取逻辑
    
    /// <summary>
    /// 触发器检测 - 玩家碰到时拾取面具
    /// </summary>
    /// <param name="collision">碰撞对象</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查是否是玩家
        if (!collision.CompareTag("Player"))
        {
            return; // 不是玩家，忽略
        }
        
        // 尝试获取玩家脚本
        PlayerGo player = collision.GetComponent<PlayerGo>();
        
        if (player == null)
        {
            Debug.LogError($"{gameObject.name} 检测到Player标签，但未找到PlayerGo组件！");
            return;
        }
        
        // 解锁面具（带空检查）
        try
        {
            player.UnlockMask(maskType);
            
            // 打印获得信息
            string maskName = GetMaskDisplayName(maskType);
            Debug.Log($"<color=yellow>获得了 {maskName} 面具！</color>");
            
            // 播放拾取效果
            PlayPickupEffects(collision.transform.position);
            
            // 销毁拾取物
            Destroy(gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"解锁面具时发生错误: {e.Message}");
        }
    }
    
    #endregion
    
    #region 辅助方法
    
    /// <summary>
    /// 获取面具的显示名称（中文）
    /// </summary>
    /// <param name="type">面具类型</param>
    /// <returns>中文名称</returns>
    private string GetMaskDisplayName(PlayerGo.MaskType type)
    {
        switch (type)
        {
            case PlayerGo.MaskType.Qiongqi:
                return "穷奇";
            case PlayerGo.MaskType.Tenggen:
                return "腾根";
            case PlayerGo.MaskType.None:
                return "无";
            default:
                return type.ToString();
        }
    }
    
    /// <summary>
    /// 播放拾取效果（音效和特效）
    /// </summary>
    /// <param name="position">播放位置</param>
    private void PlayPickupEffects(Vector3 position)
    {
        // 播放音效
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, position);
        }
        
        // 生成特效
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, position, Quaternion.identity);
            Destroy(effect, 2f); // 2秒后销毁特效
        }
    }
    
    #endregion
    
    #region 调试辅助
    
    /// <summary>
    /// 在Scene视图中显示面具类型
    /// </summary>
    private void OnDrawGizmos()
    {
        // 根据面具类型显示不同颜色
        switch (maskType)
        {
            case PlayerGo.MaskType.Qiongqi:
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // 橙色
                break;
            case PlayerGo.MaskType.Tenggen:
                Gizmos.color = new Color(0f, 1f, 0.5f, 0.5f); // 绿色
                break;
            default:
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 灰色
                break;
        }
        
        // 绘制拾取范围
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    
    /// <summary>
    /// 在Scene视图中显示面具名称
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制面具类型文本（仅在选中时）
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1f,
            $"面具: {GetMaskDisplayName(maskType)}",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.yellow },
                fontSize = 14,
                fontStyle = FontStyle.Bold
            }
        );
        #endif
    }
    
    #endregion
}
