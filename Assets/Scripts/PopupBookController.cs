using UnityEngine;
using UnityEngine.Playables;
using Spine.Unity;
using System.Collections;

public class PopupBookController : MonoBehaviour
{
    [System.Serializable]
    public class SlotSceneMapping
    {
        public string slotName;
        public string targetScene;
        public SkeletonDataAsset transitionAnimation;
        public string animationName = "";
    }

    [Header("基础设置")]
    public SkeletonAnimation skeletonAnimation;
    public PlayableDirector timelineDirector;
    
    [Header("Slot配置")]
    public SlotSceneMapping[] slotMappings;
    
    [Header("默认过场动画")]
    public SkeletonDataAsset defaultTransitionAnimation;

    private bool timelineComplete = false;

    void Start()
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError("请拖入SkeletonAnimation组件!");
            return;
        }

        // 直接启动Timeline并等待完成
        StartCoroutine(WaitForTimelineAndSetupSlots());
    }

    IEnumerator WaitForTimelineAndSetupSlots()
    {
        Debug.Log("=== PopupBook Timeline 诊断 ===");
        
        if (timelineDirector != null)
        {
            // 详细检查Timeline状态
            Debug.Log($"Timeline Director: {timelineDirector.name}");
            Debug.Log($"Playable Asset: {timelineDirector.playableAsset?.name ?? "未分配"}");
            Debug.Log($"Timeline状态: {timelineDirector.state}");

            if (timelineDirector.playableAsset != null)
            {
                // 确保Timeline从头开始播放
                timelineDirector.time = 0;
                timelineDirector.Play();
                
                Debug.Log("✅ Timeline开始播放...");

                // 监控播放状态，最多等待30秒
                float maxWaitTime = 30f;
                float waitTime = 0f;
                float logInterval = 0f;
                
                while (timelineDirector.state == PlayState.Playing && waitTime < maxWaitTime)
                {
                    waitTime += Time.deltaTime;
                    logInterval += Time.deltaTime;
                    
                    if (logInterval > 1f) // 每秒打印一次状态
                    {
                        Debug.Log($"Timeline播放中... 时间: {timelineDirector.time:F2}/{timelineDirector.duration:F2}");
                        logInterval = 0f;
                    }
                    yield return null;
                }
                
                if (waitTime >= maxWaitTime)
                {
                    Debug.LogWarning("⚠️ Timeline播放超时，强制完成");
                }
            }
            
            Debug.Log($"✅ Timeline播放完成! 最终状态: {timelineDirector.state}, 时间: {timelineDirector.time:F2}");
        }
        else
        {
            Debug.LogWarning("⚠️ 没有Timeline Director，直接启用点击");
        }

        timelineComplete = true;
        SetupSlotClickHandlers();
        Debug.Log("🎯 Slots已启用，可以点击了!");
    }

    void SetupSlotClickHandlers()
    {
        // 为每个配置的slot创建简单的点击检测
        foreach (var mapping in slotMappings)
        {
            var clickObj = new GameObject($"Click_{mapping.slotName}");
            clickObj.transform.SetParent(transform);
            clickObj.transform.localPosition = Vector3.zero;
            
            // 添加大范围碰撞器进行点击检测
            var collider = clickObj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(300, 300); // 大范围检测
            collider.isTrigger = true;
            
            var handler = clickObj.AddComponent<SimpleSlotClickHandler>();
            handler.Setup(mapping, this);
            
            Debug.Log($"已创建点击处理器: {mapping.slotName}");
        }
    }

    public void OnSlotClicked(SlotSceneMapping mapping)
    {
        if (!timelineComplete)
        {
            Debug.Log("请等待翻书动画完成!");
            return;
        }

        Debug.Log($"点击了slot: {mapping.slotName}, 加载场景: {mapping.targetScene}");

        // 检查场景名称是否有效
        if (string.IsNullOrEmpty(mapping.targetScene))
        {
            Debug.LogError($"❌ Slot '{mapping.slotName}' 的目标场景名称为空! 请在Inspector中配置targetScene字段");
            return;
        }

        // 选择过场动画
        var transitionAnim = mapping.transitionAnimation != null ? 
            mapping.transitionAnimation : defaultTransitionAnimation;

        if (transitionAnim != null)
        {
            // 使用过场动画加载场景
            var transition = SimpleSpineTransition.Instance;
            transition.transitionSpineAsset = transitionAnim;
            if (!string.IsNullOrEmpty(mapping.animationName))
            {
                transition.animationName = mapping.animationName;
            }
            transition.LoadSceneWithTransition(mapping.targetScene);
        }
        else
        {
            // 直接跳转场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(mapping.targetScene);
        }
    }

    // 调试：手动完成Timeline
    [ContextMenu("手动完成Timeline")]
    void ForceCompleteTimeline()
    {
        timelineComplete = true;
        SetupSlotClickHandlers();
        Debug.Log("已手动完成Timeline并启用点击");
    }
    
    // 外部调用接口
    public void CompleteTimelineExternally()
    {
        timelineComplete = true;
        SetupSlotClickHandlers();
        Debug.Log("已启用PopupBookController的点击功能!");
    }

    // 检查slot映射配置
    [ContextMenu("检查Slot映射配置")]
    void CheckSlotMappings()
    {
        Debug.Log("=== 检查Slot映射配置 ===");
        
        if (slotMappings == null || slotMappings.Length == 0)
        {
            Debug.LogWarning("❌ 没有配置任何Slot映射!");
            return;
        }

        for (int i = 0; i < slotMappings.Length; i++)
        {
            var mapping = slotMappings[i];
            Debug.Log($"Slot {i + 1}:");
            Debug.Log($"  - slotName: '{mapping.slotName}' {(string.IsNullOrEmpty(mapping.slotName) ? "❌ 空" : "✅")}");
            Debug.Log($"  - targetScene: '{mapping.targetScene}' {(string.IsNullOrEmpty(mapping.targetScene) ? "❌ 空" : "✅")}");
            Debug.Log($"  - transitionAnimation: {(mapping.transitionAnimation != null ? mapping.transitionAnimation.name : "未设置")}");
            Debug.Log($"  - animationName: '{mapping.animationName}'");
        }

        if (defaultTransitionAnimation != null)
        {
            Debug.Log($"✅ 默认过场动画: {defaultTransitionAnimation.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ 未设置默认过场动画");
        }

        Debug.Log("=== 配置检查完成 ===");
    }

    // 创建默认slot映射
    [ContextMenu("创建默认Slot映射")]
    void CreateDefaultSlotMappings()
    {
        slotMappings = new SlotSceneMapping[]
        {
            new SlotSceneMapping { slotName = "school", targetScene = "School" },
            new SlotSceneMapping { slotName = "shop", targetScene = "Shop" },
            new SlotSceneMapping { slotName = "fengdi", targetScene = "Restaurant" },
            new SlotSceneMapping { slotName = "gym", targetScene = "Gym" },
            new SlotSceneMapping { slotName = "library", targetScene = "Library" },
            new SlotSceneMapping { slotName = "restroom", targetScene = "Restroom" }
        };
        
        Debug.Log("✅ 已创建默认Slot映射配置");
    }
}

// 简单的点击处理器
public class SimpleSlotClickHandler : MonoBehaviour
{
    private PopupBookController.SlotSceneMapping mapping;
    private PopupBookController controller;

    public void Setup(PopupBookController.SlotSceneMapping mapping, PopupBookController controller)
    {
        this.mapping = mapping;
        this.controller = controller;
    }

    void OnMouseDown()
    {
        if (controller != null && mapping != null)
        {
            controller.OnSlotClicked(mapping);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Mouse entered {mapping?.slotName ?? "unknown"}_Clickable");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"Mouse exited {mapping?.slotName ?? "unknown"}_Clickable");
    }
}