using UnityEngine;
using UnityEngine.Playables;
using Spine.Unity;

public class DirectSpineTimelineController : MonoBehaviour
{
    [Header("直接控制Spine动画")]
    public SkeletonAnimation skeletonAnimation;
    public string bookOpenAnimation = "book"; // PopupBook2Timeline中的动画名称
    public float animationDuration = 3f; // 动画持续时间
    
    private bool animationComplete = false;
    
    void Start()
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError("请拖入SkeletonAnimation组件!");
            return;
        }
        
        // 直接播放翻书动画
        PlayBookAnimation();
    }
    
    void PlayBookAnimation()
    {
        Debug.Log("🎬 直接播放翻书动画...");
        
        // 检查SkeletonAnimation状态
        if (skeletonAnimation.Skeleton == null)
        {
            Debug.LogWarning("Skeleton未初始化，尝试初始化...");
            skeletonAnimation.Initialize(true);
        }
        
        if (skeletonAnimation.AnimationState == null)
        {
            Debug.LogError("❌ AnimationState为null，无法播放动画");
            ForceComplete();
            return;
        }
        
        // 检查对象可见性
        var renderer = skeletonAnimation.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("❌ SkeletonAnimation没有Renderer组件!");
        }
        else
        {
            Debug.Log($"Renderer启用: {renderer.enabled}, 可见: {renderer.isVisible}");
            Debug.Log($"Position: {skeletonAnimation.transform.position}");
            Debug.Log($"Scale: {skeletonAnimation.transform.localScale}");
        }
        
        // 尝试播放指定的动画
        var animationState = skeletonAnimation.AnimationState;
        if (animationState != null)
        {
            var trackEntry = animationState.SetAnimation(0, bookOpenAnimation, false);
            if (trackEntry != null)
            {
                Debug.Log($"✅ 成功播放动画: {bookOpenAnimation}");
                Debug.Log($"动画时长: {trackEntry.Animation.Duration}秒");
                
                // 监听动画完成事件
                trackEntry.Complete += OnAnimationComplete;
                
                // 强制更新渲染
                skeletonAnimation.Update(0f);
                skeletonAnimation.LateUpdate();
            }
            else
            {
                Debug.LogWarning($"⚠️ 找不到动画 '{bookOpenAnimation}'，将使用第一个可用动画");
                TryPlayFirstAnimation();
            }
        }
        
        // 备用计时器，防止动画事件失效
        Invoke(nameof(ForceComplete), animationDuration);
    }
    
    void TryPlayFirstAnimation()
    {
        var skeletonData = skeletonAnimation.skeletonDataAsset.GetSkeletonData(false);
        if (skeletonData != null && skeletonData.Animations.Count > 0)
        {
            var firstAnim = skeletonData.Animations.Items[0].Name;
            Debug.Log($"播放第一个动画: {firstAnim}");
            
            var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, firstAnim, false);
            if (trackEntry != null)
            {
                trackEntry.Complete += OnAnimationComplete;
            }
        }
        else
        {
            Debug.LogError("❌ 没有找到任何动画!");
            ForceComplete();
        }
    }
    
    void OnAnimationComplete(Spine.TrackEntry trackEntry)
    {
        if (!animationComplete)
        {
            animationComplete = true;
            CancelInvoke(nameof(ForceComplete)); // 取消备用计时器
            
            Debug.Log("✅ 翻书动画播放完成!");
            EnableSlotClicking();
        }
    }
    
    void ForceComplete()
    {
        if (!animationComplete)
        {
            animationComplete = true;
            Debug.Log("⏰ 动画时间到，强制完成");
            EnableSlotClicking();
        }
    }
    
    void EnableSlotClicking()
    {
        // 启用PopupBookController的点击功能
        var popupController = GetComponent<PopupBookController>();
        if (popupController != null)
        {
            // 通过反射设置timelineComplete为true
            var field = typeof(PopupBookController).GetField("timelineComplete", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(popupController, true);
                Debug.Log("🎯 已启用PopupBookController的点击功能!");
            }
        }
        
        Debug.Log("🎯 翻书动画完成，现在可以点击各个区域了!");
    }
    
    [ContextMenu("手动完成动画")]
    public void ManualComplete()
    {
        ForceComplete();
    }
    
    [ContextMenu("重新播放动画")]
    public void ReplayAnimation()
    {
        animationComplete = false;
        PlayBookAnimation();
    }
}