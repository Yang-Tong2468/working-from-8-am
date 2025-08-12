using UnityEngine;

public class TestSpineTransition : MonoBehaviour
{
    void Start()
    {
        Debug.Log("TestSpineTransition: 系统已启动，等待测试...");
    }
    
    void Update()
    {
        // 按T键测试过场动画
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestTransition();
        }
        
        // 按S键检查系统状态
        if (Input.GetKeyDown(KeyCode.S))
        {
            CheckSystemStatus();
        }
    }
    
    void TestTransition()
    {
        Debug.Log("开始测试过场动画...");
        
        if (SimpleSpineTransition.Instance != null)
        {
            // 测试加载当前场景
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SimpleSpineTransition.LoadScene(currentScene);
        }
        else
        {
            Debug.LogError("SimpleSpineTransition 实例未找到！请确保场景中有该组件。");
        }
    }
    
    void CheckSystemStatus()
    {
        Debug.Log("=== 系统状态检查 ===");
        
        SimpleSpineTransition transition = FindObjectOfType<SimpleSpineTransition>();
        if (transition != null)
        {
            Debug.Log("✅ SimpleSpineTransition 组件存在");
            
            if (transition.transitionSpineAsset != null)
            {
                Debug.Log($"✅ Spine资源已设置: {transition.transitionSpineAsset.name}");
            }
            else
            {
                Debug.Log("❌ 请拖入 Spine 动画资源到 transitionSpineAsset 字段");
            }
            
            Debug.Log($"过场状态: {(transition.IsTransitioning ? "进行中" : "空闲")}");
            Debug.Log($"最小过场时间: {transition.minTransitionTime}秒");
            Debug.Log($"暂停游戏: {transition.pauseGame}");
        }
        else
        {
            Debug.Log("❌ SimpleSpineTransition 组件未找到");
        }
        
        Debug.Log("=== 状态检查完成 ===");
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "按 T 键测试过场动画");
        GUI.Label(new Rect(10, 30, 300, 20), "按 S 键检查系统状态");
        
        if (GUI.Button(new Rect(10, 60, 150, 30), "测试过场动画"))
        {
            TestTransition();
        }
        
        if (GUI.Button(new Rect(10, 100, 150, 30), "检查系统状态"))
        {
            CheckSystemStatus();
        }
    }
}