using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可交互的门脚本
/// 玩家进入感应区后按 E 键交互
/// 需要钥匙才能打开门
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DoorInteraction : MonoBehaviour
{
    [Header("=== 交互设置 ===")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;  // 交互按键
    
    [Header("=== 音效设置 ===")]
    [SerializeField] private AudioClip openSound;      // 开门音效（可选）
    [SerializeField] private AudioClip lockedSound;    // 锁住音效（可选）
    
    // 私有变量
    private bool isPlayerInZone = false;   // 记录主角是否在感应区
    private PlayerGo playerRef;            // 记录主角引用
    
    private void Start()
    {
        // 确保 Collider2D 设置为触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"DoorInteraction: {gameObject.name} 的 Collider2D 已自动设置为 Trigger");
        }
    }
    
    private void Update()
    {
        // 交互逻辑
        if (isPlayerInZone && Input.GetKeyDown(interactKey))
        {
            TryOpenDoor();
        }
    }
    
    /// <summary>
    /// 尝试打开门
    /// </summary>
    private void TryOpenDoor()
    {
        if (playerRef == null)
        {
            Debug.LogWarning("DoorInteraction: playerRef 为空！");
            return;
        }
        
        // 检查玩家是否有钥匙
        if (playerRef.hasDoorKey)
        {
            // 有钥匙 - 打开门
            Debug.Log("门打开了！");
            
            // 播放开门音效
            if (openSound != null)
            {
                AudioSource.PlayClipAtPoint(openSound, transform.position);
            }
            
            // 让门消失
            gameObject.SetActive(false);
        }
        else
        {
            // 没有钥匙 - 提示需要钥匙
            Debug.Log("需要钥匙！");
            
            // 播放锁住音效
            if (lockedSound != null)
            {
                AudioSource.PlayClipAtPoint(lockedSound, transform.position);
            }
        }
    }
    
    /// <summary>
    /// 玩家进入感应区
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            playerRef = other.GetComponent<PlayerGo>();
            
            if (playerRef == null)
            {
                Debug.LogWarning("DoorInteraction: 玩家对象上没有找到 PlayerGo 组件！");
            }
            else
            {
                Debug.Log($"玩家进入门的感应区，按 {interactKey} 键交互");
            }
        }
    }
    
    /// <summary>
    /// 玩家离开感应区
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            playerRef = null;
            Debug.Log("玩家离开门的感应区");
        }
    }
}
