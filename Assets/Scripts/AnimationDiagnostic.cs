using UnityEngine;
using Spine.Unity;

public class AnimationDiagnostic : MonoBehaviour
{
    [Header("诊断工具")]
    public SkeletonAnimation skeletonAnimation;
    
    void Start()
    {
        DiagnoseAnimation();
    }
    
    [ContextMenu("诊断动画问题")]
    public void DiagnoseAnimation()
    {
        Debug.Log("========== 开始诊断动画播放问题 ==========");
        
        // 1. 检查时间缩放
        Debug.Log($"当前 Time.timeScale: {Time.timeScale}");
        if (Time.timeScale == 0f)
        {
            Debug.LogWarning("⚠️ Time.timeScale为0，这会阻止动画播放!");
            Debug.Log("修复方案: 设置 Time.timeScale = 1f");
        }
        
        // 2. 检查SkeletonAnimation组件
        if (skeletonAnimation == null)
        {
            skeletonAnimation = FindObjectOfType<SkeletonAnimation>();
            if (skeletonAnimation == null)
            {
                Debug.LogError("❌ 场景中没有找到SkeletonAnimation组件!");
                return;
            }
        }
        
        // 3. 检查基础设置
        Debug.Log($"✅ SkeletonAnimation: {skeletonAnimation.name}");
        Debug.Log($"GameObject激活: {skeletonAnimation.gameObject.activeInHierarchy}");
        Debug.Log($"组件启用: {skeletonAnimation.enabled}");
        Debug.Log($"timeScale: {skeletonAnimation.timeScale}");
        
        // 4. 检查数据资源
        if (skeletonAnimation.skeletonDataAsset == null)
        {
            Debug.LogError("❌ SkeletonDataAsset未分配!");
            return;
        }
        
        Debug.Log($"✅ 数据资源: {skeletonAnimation.skeletonDataAsset.name}");
        
        // 5. 检查Skeleton初始化
        if (skeletonAnimation.Skeleton == null)
        {
            Debug.Log("Skeleton未初始化，尝试初始化...");
            skeletonAnimation.Initialize(true);
        }
        
        if (skeletonAnimation.Skeleton == null)
        {
            Debug.LogError("❌ Skeleton初始化失败!");
            return;
        }
        
        // 6. 列出所有可用动画
        var skeletonData = skeletonAnimation.skeletonDataAsset.GetSkeletonData(false);
        if (skeletonData != null)
        {
            Debug.Log($"可用动画数量: {skeletonData.Animations.Count}");
            for (int i = 0; i < skeletonData.Animations.Count; i++)
            {
                var anim = skeletonData.Animations.Items[i];
                Debug.Log($"动画 {i}: '{anim.Name}' (时长: {anim.Duration:F2}秒)");
            }
        }
        
        // 7. 检查当前动画状态
        if (skeletonAnimation.AnimationState != null)
        {
            var tracks = skeletonAnimation.AnimationState.Tracks;
            Debug.Log($"当前动画轨道数量: {tracks.Count}");
            
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks.Items[i];
                if (track != null)
                {
                    Debug.Log($"轨道 {i}: 动画='{track.Animation?.Name}', 时间={track.TrackTime:F2}, 时间缩放={track.TimeScale}");
                }
            }
        }
        
        // 8. 尝试播放第一个动画
        if (skeletonData != null && skeletonData.Animations.Count > 0)
        {
            var firstAnim = skeletonData.Animations.Items[0];
            Debug.Log($"🎬 尝试播放第一个动画: {firstAnim.Name}");
            
            var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, firstAnim.Name, true);
            if (trackEntry != null)
            {
                Debug.Log("✅ 动画播放命令已发送");
                
                // 强制更新
                skeletonAnimation.Update(0f);
                skeletonAnimation.LateUpdate();
            }
        }
        
        Debug.Log("========== 诊断完成 ==========");
    }
    
    [ContextMenu("修复Time.timeScale")]
    public void FixTimeScale()
    {
        Time.timeScale = 1f;
        Debug.Log("已修复Time.timeScale为1");
    }
    
    [ContextMenu("强制播放动画")]
    public void ForcePlayAnimation()
    {
        if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
        {
            var skeletonData = skeletonAnimation.skeletonDataAsset.GetSkeletonData(false);
            if (skeletonData != null && skeletonData.Animations.Count > 0)
            {
                var anim = skeletonData.Animations.Items[0];
                skeletonAnimation.AnimationState.SetAnimation(0, anim.Name, true);
                Debug.Log($"强制播放动画: {anim.Name}");
            }
        }
    }
}