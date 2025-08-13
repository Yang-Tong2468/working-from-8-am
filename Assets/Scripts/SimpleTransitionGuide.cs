/*
 * 简化的 Spine 过场动画系统 - 使用说明
 * =====================================
 * 
 * 这是一个简洁的 Spine 过场动画解决方案。
 * 
 * 使用步骤：
 * 
 * 1. 在场景中创建一个 GameObject
 * 2. 添加 SimpleSpineTransition 组件
 * 3. 将你的 SkeletonDataAsset 拖到 "Transition Spine Asset" 字段
 * 4. 可选：填写动画名称（留空将使用第一个动画）
 * 5. 调整其他参数（循环、时间等）
 * 
 * 已集成到现有系统：
 * - SlotToSceneManager：点击图书馆slot -> 播放过场动画 -> 跳转Campus
 * - BuildingClickToScene：点击建筑 -> 播放过场动画 -> 跳转场景
 * 
 * 简单直接，开箱即用！
 */

using UnityEngine;

public class SimpleTransitionGuide : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 200, 500, 300));
        GUILayout.Label("=== Spine 过场动画设置指南 ===");
        GUILayout.Space(10);
        GUILayout.Label("1. 找到 SimpleSpineTransition 组件");
        GUILayout.Label("2. 拖入你的 SkeletonDataAsset 到 'Transition Spine Asset'");
        GUILayout.Label("3. 填写动画名称（可选，留空使用第一个动画）");
        GUILayout.Label("4. 点击图书馆slot测试效果");
        GUILayout.Space(10);
        GUILayout.Label("当前系统状态:");
        
        SimpleSpineTransition transition = FindObjectOfType<SimpleSpineTransition>();
        if (transition != null)
        {
            if (transition.transitionSpineAsset != null)
            {
                GUILayout.Label($"✅ Spine资源: {transition.transitionSpineAsset.name}");
            }
            else
            {
                GUILayout.Label("❌ 请拖入 Spine 动画资源");
            }
            
            GUILayout.Label($"状态: {(transition.IsTransitioning ? "过场中" : "就绪")}");
        }
        else
        {
            GUILayout.Label("❌ 找不到 SimpleSpineTransition 组件");
        }
        
        GUILayout.EndArea();
    }
}