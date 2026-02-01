using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

/// <summary>
/// 玩家核心脚本 - 2D俯视角主角（支持Spine骨骼动画）
/// 包含：移动、生命值、面具能力、状态机、Spine动画控制
/// </summary>
public class PlayerGo : MonoBehaviour
{
    #region 移动相关 (Movement)
    
    [Header("=== 移动设置 ===")]
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool movementEnabled = true;
    
    #endregion
    
    #region Spine动画系统 (Spine Animation)
    
    [Header("=== Spine动画 ===")]
    public SkeletonAnimation skeletonAnimation;     // Spine骨骼动画组件（从Body子物体获取）
    
    [Header("动画名称配置")]
    [SerializeField] private string idleAnimName = "idle";      // 待机动画名
    [SerializeField] private string walkAnimName = "walk";      // 移动动画名
    
    private string currentAnimName = "";            // 当前播放的动画名（防止重复设置）
    
    #endregion
    
    #region 生命值系统 (Health System)
    
    [Header("=== 生命值系统 ===")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public event Action<int, int> OnHealthChanged; // (current, max)
    
    [Header("无敌帧设置")]
    [SerializeField] private float invincibleDuration = 1f; // 无敌时间
    private bool isInvincible = false;
    private float invincibleTimer = 0f;
    
    #endregion
    
    #region 面具能力系统 (Mask Capabilities)
    
    [Header("=== 面具系统 ===")]
    public MaskType currentMask = MaskType.None;            // 当前装备的面具
    private HashSet<MaskType> unlockedMasks = new HashSet<MaskType>();
    
    [Header("=== 面具视觉 ===")]
    public SpriteRenderer maskRenderer;                     // 面具渲染器（FaceMask子物体）
    public Sprite qiongqiSprite;                            // 穷奇面具图片
    public Sprite tenggenSprite;                            // 腾根面具图片
    
    [Header("=== 腾根面具攻击 ===")]
    public GameObject rootProjectilePrefab;                 // 树根子弹预制体
    public float attackSpeed = 8f;                         // 树根飞行速度
    public int attackDamage = 1;                            // 树根伤害值
    public float attackCooldown = 0.5f;                     // 攻击冷却时间（秒）
    public Transform firePoint;                             // 发射点（可选，不设置则从玩家中心发射）
    
    private float nextAttackTime = 0f;                      // 下次可以攻击的时间
    
    /// <summary>
    /// 面具类型枚举
    /// </summary>
    public enum MaskType
    {
        None,       // 无面具
        Qiongqi,    // 穷奇面具（抗毒）
        Tenggen     // 腾根面具（攻击）
    }
    
    /// <summary>
    /// 是否能抵抗毒素（穷奇面具能力）
    /// </summary>
    public bool CanResistPoison => currentMask == MaskType.Qiongqi;
    
    #endregion
    
    #region 钥匙系统 (Key System)
    
    [Header("=== 钥匙系统 ===")]
    public bool hasDoorKey = false;  // 是否拥有门钥匙
    
    #endregion
    
    #region 状态机桩 (State Machine Stub)
    
    [Header("=== 状态机 ===")]
    private PlayerState currentState = PlayerState.Normal;
    
    /// <summary>
    /// 玩家状态枚举
    /// </summary>
    public enum PlayerState
    {
        Normal,     // 正常状态
        Attacking   // 攻击状态
    }
    
    #endregion
    
    #region Unity生命周期
    
    void Start()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        
        // 如果没有手动赋值，尝试从子物体获取SkeletonAnimation
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }
        
        // 初始化生命值
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 调试检查
        if (rb == null)
        {
            Debug.LogError("PlayerGo缺少Rigidbody2D组件！请添加并设置GravityScale=0");
        }
        
        if (skeletonAnimation == null)
        {
            Debug.LogWarning("PlayerGo未找到SkeletonAnimation组件！请确保Body子物体包含Spine动画");
        }
        else
        {
            // 初始播放待机动画
            PlayAnimation(idleAnimName, true);
        }
        
        // 初始化面具视觉（确保初始状态正确）
        UpdateMaskVisual();
        
        Debug.Log($"玩家初始化完成 - 生命值: {currentHealth}/{maxHealth}");
    }
    
    void Update()
    {
        // 获取输入
        GetInput();
        
        // 检测面具切换输入
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchMask();
        }
        
        // 检测腾根面具攻击输入（鼠标左键）
        // 必须满足：装备腾根面具 + 按下鼠标左键 + 冷却时间已过
        if (Input.GetMouseButtonDown(0) && 
            currentMask == MaskType.Tenggen && 
            Time.time >= nextAttackTime)
        {
            FireRootAttack();
            nextAttackTime = Time.time + attackCooldown; // 更新下次攻击时间
        }
        
        // 更新动画状态
        UpdateAnimation();
        
        // 更新无敌帧计时器
        UpdateInvincible();
    }
    
    void FixedUpdate()
    {
        // 物理移动
        Move();
    }
    
    #endregion
    
    #region 移动逻辑 (Movement Logic)
    
    /// <summary>
    /// 设置移动是否启用（对话等时可冻结）
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled)
        {
            moveInput = Vector2.zero;
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 获取WASD输入
    /// </summary>
    private void GetInput()
    {
        if (!movementEnabled)
        {
            moveInput = Vector2.zero;
            return;
        }
        // 只有在Normal状态下才能移动
        if (currentState != PlayerState.Normal)
        {
            moveInput = Vector2.zero;
            return;
        }
        
        // 使用GetAxisRaw获取精确输入（-1, 0, 1）
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D 或 左/右箭头
        float vertical = Input.GetAxisRaw("Vertical");     // W/S 或 上/下箭头
        
        // 归一化向量，防止斜向移动速度变快
        moveInput = new Vector2(horizontal, vertical).normalized;
        
        // 根据水平输入翻转角色朝向
        if (horizontal != 0)
        {
            Flip(horizontal);
        }
    }
    
    /// <summary>
    /// 使用Rigidbody2D执行移动
    /// </summary>
    private void Move()
    {
        if (!movementEnabled)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }
        if (rb == null) return;
        
        // 计算目标位置
        Vector2 targetPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        
        // 使用MovePosition进行物理移动（比直接修改velocity更平滑）
        rb.MovePosition(targetPosition);
    }
    
    /// <summary>
    /// 翻转角色朝向（Spine版本）
    /// </summary>
    /// <param name="direction">水平方向 (正数=右, 负数=左)</param>
    private void Flip(float direction)
    {
        if (skeletonAnimation == null) return;
        
        if (direction > 0)
        {
            // 朝右 - Spine的ScaleX设为1
            skeletonAnimation.skeleton.ScaleX = 1;
        }
        else if (direction < 0)
        {
            // 朝左 - Spine的ScaleX设为-1（翻转）
            skeletonAnimation.skeleton.ScaleX = -1;
        }
    }
    
    #endregion
    
    #region Spine动画逻辑 (Spine Animation Logic)
    
    /// <summary>
    /// 更新动画状态（根据移动状态）
    /// </summary>
    private void UpdateAnimation()
    {
        if (skeletonAnimation == null) return;
        
        // 判断是否在移动
        bool isMoving = moveInput.magnitude > 0.01f;
        
        if (isMoving)
        {
            // 移动时播放行走动画
            PlayAnimation(walkAnimName, true);
        }
        else
        {
            // 静止时播放待机动画
            PlayAnimation(idleAnimName, true);
        }
    }
    
    /// <summary>
    /// 播放Spine动画（防止重复设置）
    /// </summary>
    /// <param name="animName">动画名称</param>
    /// <param name="loop">是否循环</param>
    private void PlayAnimation(string animName, bool loop)
    {
        // 如果已经在播放相同动画，直接返回（避免动画卡死）
        if (currentAnimName == animName)
        {
            return;
        }
        
        // 设置新动画
        if (skeletonAnimation != null && skeletonAnimation.state != null)
        {
            skeletonAnimation.state.SetAnimation(0, animName, loop);
            currentAnimName = animName;
            Debug.Log($"播放动画: {animName}");
        }
    }
    
    #endregion
    
    #region 生命值逻辑 (Health Logic)
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage)
    {
        // 无敌帧期间不受伤
        if (isInvincible)
        {
            Debug.Log("无敌帧中，免疫伤害");
            return;
        }
        
        // 扣血
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"玩家受到 {damage} 点伤害，当前生命值: {currentHealth}/{maxHealth}");
        
        // 触发无敌帧
        StartInvincible();
        
        // 检查死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 回复生命值
    /// </summary>
    /// <param name="amount">回复量</param>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// 开始无敌帧
    /// </summary>
    private void StartInvincible()
    {
        isInvincible = true;
        invincibleTimer = invincibleDuration;
    }
    
    /// <summary>
    /// 更新无敌帧计时器
    /// </summary>
    private void UpdateInvincible()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            
            if (invincibleTimer <= 0)
            {
                isInvincible = false;
                Debug.Log("无敌帧结束");
            }
        }
    }
    
    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Die()
    {
        Debug.Log("玩家死亡！");
        
        // 禁用移动
        currentState = PlayerState.Attacking; // 临时用攻击状态阻止移动
        
        // TODO: 后续添加死亡动画、重生逻辑等
        
        // 显示游戏结束画面
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameEnd();
        }
    }
    
    #endregion
    
    #region 面具能力逻辑 (Mask Logic)
    
    /// <summary>
    /// 解锁面具
    /// </summary>
    /// <param name="mask">面具类型</param>
    public void UnlockMask(MaskType mask)
    {
        if (unlockedMasks.Add(mask))
        {
            Debug.Log($"解锁新面具: {mask}");
            
            // 根据面具类型触发特殊效果
            switch (mask)
            {
                case MaskType.Qiongqi:
                    Debug.Log("获得穷奇面具 - 现在可以抵抗毒素！");
                    break;
                case MaskType.Tenggen:
                    Debug.Log("获得腾根面具 - 现在可以使用攻击能力！");
                    break;
            }
        }
        else
        {
            Debug.Log($"已拥有面具: {mask}");
        }
    }
    
    /// <summary>
    /// 检查是否拥有指定面具
    /// </summary>
    /// <param name="mask">面具类型</param>
    /// <returns>是否拥有</returns>
    public bool HasMask(MaskType mask)
    {
        return unlockedMasks.Contains(mask);
    }
    
    /// <summary>
    /// 切换面具（按Q键）
    /// </summary>
    private void SwitchMask()
    {
        try
        {
            Debug.Log($"[{Time.frameCount}] 按下Q键，开始切换面具...");
            
            // 定义面具切换顺序：None -> Qiongqi -> Tenggen -> None
            MaskType[] maskOrder = new MaskType[]
            {
                MaskType.None,
                MaskType.Qiongqi,
                MaskType.Tenggen
            };
            
            Debug.Log("步骤1: 数组创建成功");
            
            // 手动查找当前面具在顺序中的索引
            int currentIndex = -1;
            for (int j = 0; j < maskOrder.Length; j++)
            {
                if (maskOrder[j] == currentMask)
                {
                    currentIndex = j;
                    break;
                }
            }
            
            Debug.Log($"步骤2: 切换前: 当前面具={currentMask}, 索引={currentIndex}");
            
            // 如果找不到当前面具（异常情况），从头开始
            if (currentIndex == -1)
            {
                Debug.LogWarning("当前面具未在列表中找到，重置为None");
                currentIndex = 0;
                currentMask = MaskType.None;
            }
            
            Debug.Log("步骤3: 开始循环查找");
            
            // 从下一个面具开始尝试，最多尝试所有面具一遍
            for (int i = 0; i < maskOrder.Length; i++)
            {
                // 计算下一个索引（循环）
                currentIndex = (currentIndex + 1) % maskOrder.Length;
                MaskType nextMask = maskOrder[currentIndex];
                
                Debug.Log($"尝试第{i+1}次: 索引={currentIndex}, 面具={nextMask}, 已解锁={HasMask(nextMask)}");
                
                // 检查是否可以切换到这个面具
                // None总是可用，其他面具需要检查是否已解锁
                if (nextMask == MaskType.None || HasMask(nextMask))
                {
                    // 切换成功
                    currentMask = nextMask;
                    
                    // 更新面具视觉
                    UpdateMaskVisual();
                    
                    // 打印当前面具
                    string maskName = GetMaskDisplayName(currentMask);
                    Debug.Log($"<color=cyan>✓ 切换成功！当前面具: {maskName}</color>");
                    
                    // TODO: 播放切换音效
                    // TODO: 播放切换动画/特效
                    
                    return;
                }
            }
            
            // 如果所有面具都不可用（理论上不会发生，因为None总是可用）
            Debug.LogWarning("没有可用的面具");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"切换面具时发生错误: {e.Message}\n{e.StackTrace}");
        }
    }
    
    /// <summary>
    /// 获取面具的显示名称（中文）
    /// </summary>
    /// <param name="mask">面具类型</param>
    /// <returns>中文名称</returns>
    private string GetMaskDisplayName(MaskType mask)
    {
        switch (mask)
        {
            case MaskType.None:
                return "无面具";
            case MaskType.Qiongqi:
                return "穷奇面具";
            case MaskType.Tenggen:
                return "腾根面具";
            default:
                return mask.ToString();
        }
    }
    
    /// <summary>
    /// 更新面具视觉显示
    /// </summary>
    private void UpdateMaskVisual()
    {
        // 检查maskRenderer是否存在
        if (maskRenderer == null)
        {
            return; // 没有面具渲染器，直接返回
        }
        
        // 根据当前面具类型设置对应的Sprite
        switch (currentMask)
        {
            case MaskType.None:
                // 无面具 - 隐藏面具
                maskRenderer.sprite = null;
                break;
                
            case MaskType.Qiongqi:
                // 穷奇面具
                maskRenderer.sprite = qiongqiSprite;
                break;
                
            case MaskType.Tenggen:
                // 腾根面具
                maskRenderer.sprite = tenggenSprite;
                break;
        }
    }
    
    #endregion
    
    #region 攻击逻辑桩 (Attack Stub)
    
    /// <summary>
    /// 攻击方法（预留）
    /// </summary>
    public void Attack()
    {
        // 检查是否拥有腾根面具
        if (!HasMask(MaskType.Tenggen))
        {
            Debug.Log("需要腾根面具才能攻击！");
            return;
        }
        
        // TODO: 后续实现攻击逻辑
        Debug.Log("执行攻击！（功能待实现）");
        
        // 切换到攻击状态（预留）
        // currentState = PlayerState.Attacking;
    }
    
    /// <summary>
    /// 发射树根攻击（腾根面具能力）
    /// </summary>
    private void FireRootAttack()
    {
        // 检查是否有树根子弹预制体
        if (rootProjectilePrefab == null)
        {
            Debug.LogError("未设置树根子弹预制体！请在Inspector中赋值 rootProjectilePrefab");
            return;
        }
        
        // 获取鼠标在世界空间的位置
        // 重要：需要设置正确的Z坐标，否则转换会出错
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.transform.position.z * -1; // 相机到玩家平面的距离
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        
        // 计算从玩家到鼠标的方向向量（归一化）
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        
        // 确定发射位置
        // 优先使用firePoint，否则使用玩家位置
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        
        // 调试：打印发射位置
        Debug.Log($"子弹生成位置: {spawnPosition} | 玩家位置: {transform.position} | 使用FirePoint: {firePoint != null}");
        
        // 生成树根子弹
        GameObject rootObj = Instantiate(rootProjectilePrefab, spawnPosition, Quaternion.identity);
        
        // 获取RootProjectile组件并初始化
        RootProjectile rootScript = rootObj.GetComponent<RootProjectile>();
        if (rootScript != null)
        {
            rootScript.Initialize(direction, attackSpeed, attackDamage);
        }
        else
        {
            Debug.LogError("树根子弹预制体上没有RootProjectile组件！");
            Destroy(rootObj);
        }
    }
    
    #endregion
    
    #region 调试辅助方法
    
    /// <summary>
    /// 测试受伤（仅用于调试）
    /// </summary>
    [ContextMenu("测试受伤-1点")]
    private void TestTakeDamage()
    {
        TakeDamage(1);
    }
    
    /// <summary>
    /// 测试解锁穷奇面具（仅用于调试）
    /// </summary>
    [ContextMenu("解锁穷奇面具")]
    private void TestUnlockQiongqi()
    {
        UnlockMask(MaskType.Qiongqi);
    }
    
    /// <summary>
    /// 测试解锁腾根面具（仅用于调试）
    /// </summary>
    [ContextMenu("解锁腾根面具")]
    private void TestUnlockTenggen()
    {
        UnlockMask(MaskType.Tenggen);
    }
    
    #endregion
}
