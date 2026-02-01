using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 浮动效果 - 让道具上下浮动，增加视觉吸引力
/// 适用于：面具、钥匙、收集品等可交互物品
/// </summary>
public class FloatingEffect : MonoBehaviour
{
    #region 浮动参数
    
    [Header("=== 浮动设置 ===")]
    [SerializeField] private float amplitude = 0.5f;        // 浮动高度（上下移动的距离）
    [SerializeField] private float frequency = 1f;          // 浮动速度（频率）
    
    [Header("可选效果")]
    [SerializeField] private bool enableRotation = false;   // 是否启用旋转
    [SerializeField] private float rotationSpeed = 50f;     // 旋转速度（度/秒）
    
    #endregion
    
    #region 运行时数据
    
    private Vector3 initialPosition;                        // 初始位置（基准点）
    private float timeOffset;                               // 时间偏移（让多个物体不同步）
    
    #endregion
    
    #region Unity生命周期
    
    void Start()
    {
        // 记录初始位置作为基准点
        initialPosition = transform.position;
        
        // 随机时间偏移，让多个物体浮动不同步，更自然
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        
        Debug.Log($"{gameObject.name} 浮动效果已启动");
    }
    
    void Update()
    {
        // 执行浮动效果
        ApplyFloating();
        
        // 可选：旋转效果
        if (enableRotation)
        {
            ApplyRotation();
        }
    }
    
    #endregion
    
    #region 浮动逻辑
    
    /// <summary>
    /// 应用浮动效果
    /// </summary>
    private void ApplyFloating()
    {
        // 使用Sin函数计算Y轴偏移量
        // Time.time * frequency 控制浮动速度
        // amplitude 控制浮动高度
        float yOffset = Mathf.Sin((Time.time + timeOffset) * frequency) * amplitude;
        
        // 基于初始位置计算新位置
        Vector3 newPosition = initialPosition + new Vector3(0, yOffset, 0);
        
        // 应用新位置
        transform.position = newPosition;
    }
    
    /// <summary>
    /// 应用旋转效果（可选）
    /// </summary>
    private void ApplyRotation()
    {
        // 绕Y轴缓慢旋转
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
    
    #endregion
    
    #region 调试辅助
    
    /// <summary>
    /// 在Scene视图中绘制浮动范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 如果还没运行，使用当前位置作为基准
        Vector3 basePos = Application.isPlaying ? initialPosition : transform.position;
        
        // 绘制浮动范围（黄色线条）
        Gizmos.color = Color.yellow;
        
        // 最高点
        Vector3 topPos = basePos + new Vector3(0, amplitude, 0);
        Gizmos.DrawWireSphere(topPos, 0.1f);
        
        // 最低点
        Vector3 bottomPos = basePos - new Vector3(0, amplitude, 0);
        Gizmos.DrawWireSphere(bottomPos, 0.1f);
        
        // 连线
        Gizmos.DrawLine(topPos, bottomPos);
        
        // 基准点（绿色）
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(basePos, 0.15f);
    }
    
    #endregion
}
