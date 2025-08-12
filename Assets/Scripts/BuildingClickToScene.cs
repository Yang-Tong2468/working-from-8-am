using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingClickToScene : MonoBehaviour
{
    // 设置要跳转的场景名
    public string targetSceneName;

    void OnMouseDown()
    {
        // 鼠标点击时跳转场景
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SimpleSpineTransition.LoadScene(targetSceneName);
        }
    }
}