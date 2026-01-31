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
    
    [Header("无敌帧设置")]
    [SerializeField] private float invincibleDuration = 1f; // 无敌时间
    private bool isInvincible = false;
    private float invincibleTimer = 0f;
    
    #endregion
    
    #region 面具能力系统 (Mask Capabilities)
    
    [Header("=== 面具系统 ===")]
    private HashSet<MaskType> unlockedMasks = new HashSet<MaskType>();
    
    /// <summary>
    /// 面具类型枚举
    /// </summary>
    public enum MaskType
    {
        None,       // 无面具
        Qiongqi,    // 穷奇面具（抗毒）
        Tenggen     // 藤根面具（攻击）
    }
    
    /// <summary>
    /// 是否能抵抗毒素（穷奇面具能力）
    /// </summary>
    public bool CanResistPoison => HasMask(MaskType.Qiongqi);
    
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
        
        Debug.Log($"玩家初始化完成 - 生命值: {currentHealth}/{maxHealth}");
    }
    
    void Update()
    {
        // 获取输入
        GetInput();
        
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
    /// 获取WASD输入
    /// </summary>
    private void GetInput()
    {
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
        
        // TODO: 后续添加死亡动画、重生逻辑等
        // 暂时禁用移动
        currentState = PlayerState.Attacking; // 临时用攻击状态阻止移动
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
                    Debug.Log("获得藤根面具 - 现在可以使用攻击能力！");
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
    
    #endregion
    
    #region 攻击逻辑桩 (Attack Stub)
    
    /// <summary>
    /// 攻击方法（预留）
    /// </summary>
    public void Attack()
    {
        // 检查是否拥有藤根面具
        if (!HasMask(MaskType.Tenggen))
        {
            Debug.Log("需要藤根面具才能攻击！");
            return;
        }
        
        // TODO: 后续实现攻击逻辑
        Debug.Log("执行攻击！（功能待实现）");
        
        // 切换到攻击状态（预留）
        // currentState = PlayerState.Attacking;
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
    /// 测试解锁藤根面具（仅用于调试）
    /// </summary>
    [ContextMenu("解锁藤根面具")]
    private void TestUnlockTenggen()
    {
        UnlockMask(MaskType.Tenggen);
    }
    
    #endregion
}
