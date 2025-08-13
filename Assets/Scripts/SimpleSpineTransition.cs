using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Spine.Unity;
using System.Collections;

public class SimpleSpineTransition : MonoBehaviour
{
    [Header("Spine 过场动画设置")]
    [Tooltip("拖入你的 Spine 动画资源")]
    public SkeletonDataAsset transitionSpineAsset;
    
    [Tooltip("动画名称（如果不填，使用第一个动画）")]
    public string animationName = "";
    
    [Tooltip("是否循环播放")]
    public bool loopAnimation = true;
    
    [Header("过场设置")]
    [Tooltip("最小过场时间（秒）")]
    public float minTransitionTime = 3f;
    
    [Tooltip("过场时暂停游戏")]
    public bool pauseGame = true;
    
    [Header("动画显示设置")]
    [Tooltip("动画缩放大小")]
    public Vector3 animationScale = Vector3.one;
    
    [Tooltip("动画位置偏移")]
    public Vector2 animationOffset = Vector2.zero;
    
    [Tooltip("是否适应屏幕大小")]
    public bool fitToScreen = false;
    
    [Tooltip("适应屏幕时的缩放系数")]
    [Range(0.1f, 2f)]
    public float screenFitScale = 1f;
    
    [Header("事件")]
    public UnityEvent onTransitionStart;
    public UnityEvent onTransitionEnd;

    private static SimpleSpineTransition instance;
    private GameObject transitionCanvas;
    private SkeletonGraphic spineGraphic;
    private bool isTransitioning = false;

    public static SimpleSpineTransition Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SimpleSpineTransition>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SimpleSpineTransition");
                    instance = go.AddComponent<SimpleSpineTransition>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupTransitionCanvas();
    }

    void SetupTransitionCanvas()
    {
        if (transitionCanvas != null) return;

        // 创建Canvas
        transitionCanvas = new GameObject("TransitionCanvas");
        Canvas canvas = transitionCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        // 添加CanvasScaler
        CanvasScaler scaler = transitionCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        DontDestroyOnLoad(transitionCanvas);
        transitionCanvas.SetActive(false);
    }

    void CreateSpineGraphic()
    {
        if (transitionSpineAsset == null)
        {
            Debug.LogError("请先拖入 Spine 动画资源到 transitionSpineAsset！");
            return;
        }

        // 清理旧的
        if (spineGraphic != null)
        {
            DestroyImmediate(spineGraphic.gameObject);
        }

        // 创建新的Spine对象
        GameObject spineGO = new GameObject("TransitionSpine");
        spineGO.transform.SetParent(transitionCanvas.transform, false);
        
        spineGraphic = spineGO.AddComponent<SkeletonGraphic>();
        spineGraphic.skeletonDataAsset = transitionSpineAsset;
        spineGraphic.Initialize(true);
        
        // 设置位置到屏幕中心
        RectTransform rectTransform = spineGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = animationOffset;
        rectTransform.localScale = animationScale;

        // 确保动画能播放
        spineGraphic.timeScale = 1f;
        
        // 获取动画名称
        string finalAnimName = animationName;
        if (string.IsNullOrEmpty(finalAnimName))
        {
            var skeletonData = transitionSpineAsset.GetSkeletonData(true);
            if (skeletonData != null && skeletonData.Animations.Count > 0)
            {
                finalAnimName = skeletonData.Animations.Items[0].Name;
                Debug.Log($"使用第一个动画: {finalAnimName}");
            }
        }

        // 播放动画
        if (!string.IsNullOrEmpty(finalAnimName))
        {
            spineGraphic.AnimationState.SetAnimation(0, finalAnimName, loopAnimation);
            Debug.Log($"开始播放过场动画: {finalAnimName}");
        }
        else
        {
            Debug.LogError("无法找到可播放的动画!");
        }
    }

    Vector3 CalculateScreenFitScale()
    {
        if (transitionSpineAsset == null) return Vector3.one;

        // 获取屏幕尺寸
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        // 获取Canvas的参考分辨率
        CanvasScaler scaler = transitionCanvas.GetComponent<CanvasScaler>();
        Vector2 referenceResolution = scaler.referenceResolution;
        
        // 计算适应比例
        float widthScale = referenceResolution.x / 1920f;  // 基于1920基准
        float heightScale = referenceResolution.y / 1080f; // 基于1080基准
        
        float scale = Mathf.Min(widthScale, heightScale);
        return Vector3.one * scale;
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;
        onTransitionStart.Invoke();

        // 验证场景名称
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ 场景名称为空，无法加载场景!");
            isTransitioning = false;
            yield break;
        }

        Debug.Log($"🎬 开始加载场景: {sceneName}");

        // 设置Canvas和动画
        SetupTransitionCanvas();
        CreateSpineGraphic();
        
        if (transitionCanvas != null)
        {
            transitionCanvas.SetActive(true);
        }

        if (pauseGame)
        {
            Time.timeScale = 0f;
        }

        // 开始异步加载场景
        float startTime = Time.realtimeSinceStartup;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // 检查场景加载是否成功开始
        if (asyncLoad == null)
        {
            Debug.LogError($"❌ 无法加载场景: {sceneName} - 场景可能不存在或未添加到Build Settings");
            
            // 清理并结束
            if (transitionCanvas != null)
            {
                transitionCanvas.SetActive(false);
            }
            if (pauseGame)
            {
                Time.timeScale = 1f;
            }
            isTransitioning = false;
            yield break;
        }
        
        asyncLoad.allowSceneActivation = false;

        // 等待场景加载完成且满足最小时间
        while (!asyncLoad.isDone)
        {
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            
            if (asyncLoad.progress >= 0.9f && elapsedTime >= minTransitionTime)
            {
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }

        // 结束过场
        if (transitionCanvas != null)
        {
            transitionCanvas.SetActive(false);
        }

        if (pauseGame)
        {
            Time.timeScale = 1f;
        }

        onTransitionEnd.Invoke();
        isTransitioning = false;
        
        Debug.Log($"✅ 场景加载完成: {sceneName}");
    }

    public static void LoadScene(string sceneName)
    {
        Instance.LoadSceneWithTransition(sceneName);
    }

    public static void LoadSceneWithCustomAnimation(string sceneName, SkeletonDataAsset customAnimation, string customAnimationName = "")
    {
        var instance = Instance;
        instance.transitionSpineAsset = customAnimation;
        if (!string.IsNullOrEmpty(customAnimationName))
        {
            instance.animationName = customAnimationName;
        }
        instance.LoadSceneWithTransition(sceneName);
    }

    public bool IsTransitioning => isTransitioning;

    void OnGUI()
    {
        if (transitionSpineAsset == null)
        {
            GUI.Label(new Rect(10, 10, 400, 20), "请拖入 Spine 动画资源到 SimpleSpineTransition 组件");
        }
        else
        {
            GUI.Label(new Rect(10, 10, 400, 20), $"Spine 资源: {transitionSpineAsset.name}");
        }
    }
}