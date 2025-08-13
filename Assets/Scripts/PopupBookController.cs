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
            Debug.Log($"Timeline长度: {timelineDirector.duration}");
            
            if (timelineDirector.playableAsset == null)
            {
                Debug.LogError("❌ Timeline没有Playable Asset! 请将PopupBook2Timeline.playable拖入PlayableDirector");
                timelineComplete = true;
                SetupSlotClickHandlers();
                yield break;
            }
            
            // 检查绑定
            var outputs = timelineDirector.playableAsset.outputs;
            bool hasBindings = false;
            foreach (var output in outputs)
            {
                var binding = timelineDirector.GetGenericBinding(output.sourceObject);
                if (binding != null)
                {
                    Debug.Log($"✅ Track '{output.streamName}' → {binding.name}");
                    hasBindings = true;
                }
                else
                {
                    Debug.LogError($"❌ Track '{output.streamName}' 未绑定!");
                }
            }
            
            if (!hasBindings)
            {
                Debug.LogError("❌ 没有绑定任何Track到SkeletonAnimation!");
                Debug.Log("解决方法: 打开Timeline窗口，将SkeletonAnimation拖到每个Track左侧");
            }
            
            // 强制从头开始播放
            timelineDirector.time = 0;
            timelineDirector.Play();
            Debug.Log("🎬 Timeline开始播放...");
            
            // 等待一帧确保播放开始
            yield return null;
            
            // 检查Timeline是否真的在播放
            if (timelineDirector.state != PlayState.Playing)
            {
                Debug.LogWarning($"⚠️ Timeline没有开始播放! 当前状态: {timelineDirector.state}");
                Debug.Log("Timeline可能长度为0或立即完成，直接启用点击");
            }
            else
            {
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
        Debug.Log("外部触发Timeline完成");
    }

    // 调试：列出所有slots
    [ContextMenu("列出所有Slots")]
    void ListAllSlots()
    {
        if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
        {
            Debug.LogError("SkeletonAnimation未正确初始化");
            return;
        }

        Debug.Log("=== 所有可用的Slots ===");
        var slots = skeletonAnimation.Skeleton.Slots;
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots.Items[i];
            Debug.Log($"Slot {i}: {slot.Data.Name} (Attachment: {slot.Attachment?.Name ?? "无"})");
        }
    }
}

// 简化的点击处理器
public class SimpleSlotClickHandler : MonoBehaviour
{
    private PopupBookController.SlotSceneMapping mapping;
    private PopupBookController controller;

    public void Setup(PopupBookController.SlotSceneMapping slotMapping, PopupBookController ctrl)
    {
        mapping = slotMapping;
        controller = ctrl;
    }

    void OnMouseDown()
    {
        if (controller != null && mapping != null)
        {
            controller.OnSlotClicked(mapping);
        }
    }

    void OnMouseEnter()
    {
        Debug.Log($"鼠标进入: {mapping?.slotName}");
    }
}