using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单管理：创建 UI 并处理按钮点击
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    private GameManager cachedGameManager;

    private void Start()
    {
        Debug.Log("[MainMenuManager] Start");

        // 确保核心管理器存在
        EnsureManagersExist();
        CreateUI();

        Debug.Log("[MainMenuManager] 初始化完成, GameManager.Instance = " + (GameManager.Instance != null));
    }

    /// <summary>
    /// 如果场景中没有 GameManager / AudioManager，自动创建
    /// </summary>
    private void EnsureManagersExist()
    {
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            Debug.Log("[MainMenuManager] 创建了 GameManager");
        }

        if (AudioManager.Instance == null)
        {
            GameObject am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
            Debug.Log("[MainMenuManager] 创建了 AudioManager");
        }

        cachedGameManager = GameManager.Instance;
    }

    /// <summary>
    /// 动态创建主菜单界面
    /// </summary>
    private void CreateUI()
    {
        // 确保 EventSystem 存在（UI 交互必需）
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log("[MainMenuManager] 创建了 EventSystem");
        }

        // === Canvas ===
        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // === 背景（关闭射线拦截） ===
        GameObject bgGO = new GameObject("Background", typeof(Image));
        Image bgImage = bgGO.GetComponent<Image>();
        bgImage.color = new Color(0.12f, 0.20f, 0.40f);
        bgImage.raycastTarget = false; // 不拦截点击
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.SetParent(canvasGO.transform, false);
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // === 标题（关闭射线拦截） ===
        GameObject titleGO = new GameObject("Title", typeof(Text));
        Text titleText = titleGO.GetComponent<Text>();
        titleText.text = "英语单词闯关";
        titleText.fontSize = 80;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.raycastTarget = false;

        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.SetParent(canvasGO.transform, false);
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.85f);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.anchoredPosition = Vector2.zero;

        // === 副标题（关闭射线拦截） ===
        GameObject subGO = new GameObject("Subtitle", typeof(Text));
        Text subText = subGO.GetComponent<Text>();
        subText.text = "Word Quest";
        subText.fontSize = 36;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.8f, 0.9f, 1.0f);
        subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subText.raycastTarget = false;

        RectTransform subRect = subGO.GetComponent<RectTransform>();
        subRect.SetParent(canvasGO.transform, false);
        subRect.anchorMin = new Vector2(0, 0.6f);
        subRect.anchorMax = new Vector2(1, 0.7f);
        subRect.sizeDelta = Vector2.zero;
        subRect.anchoredPosition = Vector2.zero;

        // === 按钮 ===
        CreateButton(canvasGO.transform, "开始游戏", new Vector2(0, -50), OnStartGame);
        CreateButton(canvasGO.transform, "退出游戏", new Vector2(0, -180), OnQuit);

        Debug.Log("[MainMenuManager] UI 创建完成");
    }

    /// <summary>
    /// 创建一个居中按钮
    /// </summary>
    private void CreateButton(Transform parent, string label, Vector2 offset, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject(label, typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);

        // 按钮外观
        Image btnImage = btnGO.GetComponent<Image>();
        btnImage.color = new Color(0.2f, 0.4f, 0.7f);
        btnImage.raycastTarget = true;

        // 按钮位置
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(300, 80);
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = offset;

        // 按钮文字
        GameObject textGO = new GameObject("Text", typeof(Text));
        textGO.transform.SetParent(btnGO.transform, false);

        Text btnText = textGO.GetComponent<Text>();
        btnText.text = label;
        btnText.fontSize = 40;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.raycastTarget = false;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // 设置 Button 组件
        Button btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = btnImage; // 明确指定目标图形
        btn.onClick.AddListener(onClick);

        // 按钮过渡颜色
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.8f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.6f);
        btn.colors = colors;
    }

    /// <summary>
    /// 开始游戏按钮
    /// </summary>
    public void OnStartGame()
    {
        Debug.Log("[MainMenuManager] 点击了开始游戏");

        if (GameManager.Instance != null)
        {
            Debug.Log("[MainMenuManager] GameManager 存在，开始游戏");
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("[MainMenuManager] GameManager.Instance 为空!");
        }
    }

    /// <summary>
    /// 退出游戏按钮
    /// </summary>
    public void OnQuit()
    {
        Debug.Log("[MainMenuManager] 点击了退出游戏");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}
