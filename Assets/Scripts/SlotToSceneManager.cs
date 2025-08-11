using UnityEngine;
using Spine.Unity;
using UnityEngine.SceneManagement;

public class SlotToSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class SlotSceneMapping
    {
        public string slotName;        // slot的名称
        public string targetScene;     // 对应跳转的场景名
        public GameObject followerObj; // BoundingBoxFollower的GameObject
    }

    public SkeletonAnimation skeletonAnimation;
    public SlotSceneMapping[] slotMappings = new SlotSceneMapping[]
    {
        new SlotSceneMapping { slotName = "图书馆", targetScene = "Library" },
        new SlotSceneMapping { slotName = "食堂", targetScene = "Restaurant" },
        new SlotSceneMapping { slotName = "宿舍", targetScene = "Apartment" },
        new SlotSceneMapping { slotName = "学院", targetScene = "Gym" },
        new SlotSceneMapping { slotName = "体育馆", targetScene = "Campus" }
    };

    void Start()
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError("SkeletonAnimation not assigned!");
            return;
        }

        foreach (var mapping in slotMappings)
        {
            // 创建子GameObject
            var clickableObj = new GameObject(mapping.slotName + "_Clickable");
            clickableObj.transform.SetParent(transform);
            clickableObj.transform.localPosition = Vector3.zero;

            // 添加BoxCollider2D而不是BoundingBoxFollower
            var collider = clickableObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            
            // 根据slot的位置和大小设置碰撞器
            var slot = skeletonAnimation.Skeleton.FindSlot(mapping.slotName);
            if (slot != null && slot.Attachment != null)
            {
                // 获取slot的大致范围
                var bounds = GetSlotBounds(slot);
                collider.size = bounds.size;
                collider.offset = bounds.center;
            }

            // 添加点击处理脚本
            var clickHandler = clickableObj.AddComponent<SlotClickHandler>();
            clickHandler.targetSceneName = mapping.targetScene;

            mapping.followerObj = clickableObj;
        }
    }

    private Bounds GetSlotBounds(Spine.Slot slot)
    {
        // 根据slot的attachment类型获取大致范围
        var attachment = slot.Attachment;
        if (attachment is Spine.RegionAttachment region)
        {
            float width = region.Width;
            float height = region.Height;
            return new Bounds(Vector3.zero, new Vector3(width, height, 1f));
        }
        // 如果无法获取精确大小，使用默认值
        return new Bounds(Vector3.zero, new Vector3(100f, 100f, 1f));
    }
}

// 处理单个slot的点击
public class SlotClickHandler : MonoBehaviour
{
    public string targetSceneName;

    void OnMouseDown()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"Clicked on {gameObject.name}, loading scene: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
    }

    // 可选：添加鼠标悬停效果
    void OnMouseEnter()
    {
        // TODO: 在这里添加悬停效果，比如改变颜色或显示提示
        Debug.Log($"Mouse entered {gameObject.name}");
    }

    void OnMouseExit()
    {
        // TODO: 在这里清除悬停效果
        Debug.Log($"Mouse exited {gameObject.name}");
    }
}