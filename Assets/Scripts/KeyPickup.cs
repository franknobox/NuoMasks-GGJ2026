using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 钥匙拾取道具
/// 玩家触碰后获得钥匙，可以用来开门
/// 可配合 FloatingEffect 脚本实现漂浮效果
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    [Header("=== 拾取音效 ===")]
    [SerializeField] private AudioClip pickupSound;  // 拾取音效（可选）
    
    private void Start()
    {
        // 确保 Collider2D 设置为触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"KeyPickup: {gameObject.name} 的 Collider2D 已自动设置为 Trigger");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测是否是玩家
        if (other.CompareTag("Player"))
        {
            // 获取 PlayerGo 组件
            PlayerGo player = other.GetComponent<PlayerGo>();
            
            if (player != null)
            {
                // 设置玩家拥有钥匙
                player.hasDoorKey = true;
                
                // 打印日志
                Debug.Log("获得钥匙！");
                
                // 播放拾取音效（如果有 AudioManager）
                PlayPickupSound();
                
                // 销毁钥匙物体
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("KeyPickup: 玩家对象上没有找到 PlayerGo 组件！");
            }
        }
    }
    
    /// <summary>
    /// 播放拾取音效
    /// </summary>
    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            // 尝试查找 AudioManager（如果存在）
            // 方式1: 通过单例模式
            // AudioManager.Instance?.PlaySound(pickupSound);
            
            // 方式2: 使用 AudioSource.PlayClipAtPoint（临时音效）
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}
