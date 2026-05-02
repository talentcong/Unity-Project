using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// 结算场景管理：显示分数、星级评价、重新开始/返回主菜单
/// </summary>
public class ResultSceneManager : MonoBehaviour
{
    private void Start()
    {
        // 确保 EventSystem 存在
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        CreateUI();
    }

    private void CreateUI()
    {
        // === Canvas ===
        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // === 背景 ===
        CreateBackground(canvasGO.transform);

        // 计算分数数据
        int score = GameManager.Instance != null ? GameManager.Instance.currentScore : 0;
        int correct = GameManager.Instance != null ? GameManager.Instance.correctCount : 0;
        int total = GameManager.Instance != null ? GameManager.Instance.totalQuestionsPerRound : 10;
        int maxScore = total * 10;
        float percent = maxScore > 0 ? (float)score / maxScore * 100f : 0f;

        // === 标题 ===
        CreateTitle(canvasGO.transform);

        // === 分数显示 ===
        CreateScoreDisplay(canvasGO.transform, score, correct, total);

        // === 星级 ===
        CreateStars(canvasGO.transform, percent);

        // === 评级文字 ===
        CreateRatingText(canvasGO.transform, percent);

        // === 按钮 ===
        CreateButton(canvasGO.transform, "再来一次", new Vector2(0, -150), OnRestart);
        CreateButton(canvasGO.transform, "返回主菜单", new Vector2(0, -280), OnMainMenu);
    }

    private void CreateBackground(Transform parent)
    {
        GameObject bg = new GameObject("Background", typeof(Image));
        Image img = bg.GetComponent<Image>();
        UIStyler.ApplyVerticalGradient(img,
            new Color(0.10f, 0.15f, 0.35f),
            new Color(0.25f, 0.40f, 0.65f));
        RectTransform rt = bg.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
    }

    private void CreateTitle(Transform parent)
    {
        GameObject titleGO = new GameObject("Title", typeof(Text));
        Text text = titleGO.GetComponent<Text>();
        text.text = "闯关结束";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 72;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;

        RectTransform rt = titleGO.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0, 0.78f);
        rt.anchorMax = new Vector2(1, 0.9f);
        rt.sizeDelta = Vector2.zero;
    }

    private void CreateScoreDisplay(Transform parent, int score, int correct, int total)
    {
        // 分数
        GameObject scoreGO = new GameObject("ScoreText", typeof(Text));
        Text scoreText = scoreGO.GetComponent<Text>();
        scoreText.text = $"得分: {score}";
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 56;
        scoreText.alignment = TextAnchor.MiddleCenter;
        scoreText.color = Color.white;
        scoreText.raycastTarget = false;

        RectTransform scoreRT = scoreGO.GetComponent<RectTransform>();
        scoreRT.SetParent(parent, false);
        scoreRT.anchorMin = new Vector2(0, 0.62f);
        scoreRT.anchorMax = new Vector2(1, 0.72f);
        scoreRT.sizeDelta = Vector2.zero;

        // 答对题数
        GameObject detailGO = new GameObject("DetailText", typeof(Text));
        Text detailText = detailGO.GetComponent<Text>();
        detailText.text = $"答对 {correct}/{total} 题";
        detailText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        detailText.fontSize = 36;
        detailText.alignment = TextAnchor.MiddleCenter;
        detailText.color = new Color(0.8f, 0.9f, 1.0f);
        detailText.raycastTarget = false;

        RectTransform detailRT = detailGO.GetComponent<RectTransform>();
        detailRT.SetParent(parent, false);
        detailRT.anchorMin = new Vector2(0, 0.55f);
        detailRT.anchorMax = new Vector2(1, 0.62f);
        detailRT.sizeDelta = Vector2.zero;
    }

    private void CreateStars(Transform parent, float percent)
    {
        int starCount = percent >= 90f ? 3 : (percent >= 60f ? 2 : 1);
        string stars = "";
        for (int i = 0; i < starCount; i++) stars += "⭐";
        if (starCount == 0) stars = "💫";

        GameObject starsGO = new GameObject("Stars", typeof(Text));
        Text starsText = starsGO.GetComponent<Text>();
        starsText.text = stars;
        starsText.fontSize = 80;
        starsText.alignment = TextAnchor.MiddleCenter;
        starsText.raycastTarget = false;

        RectTransform rt = starsGO.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0, 0.42f);
        rt.anchorMax = new Vector2(1, 0.52f);
        rt.sizeDelta = Vector2.zero;
    }

    private void CreateRatingText(Transform parent, float percent)
    {
        string rating;
        Color ratingColor;

        if (percent >= 90f)
        {
            rating = "优秀！";
            ratingColor = Color.yellow;
        }
        else if (percent >= 60f)
        {
            rating = "及格";
            ratingColor = Color.green;
        }
        else
        {
            rating = "继续努力";
            ratingColor = Color.red;
        }

        GameObject ratingGO = new GameObject("RatingText", typeof(Text));
        Text ratingText = ratingGO.GetComponent<Text>();
        ratingText.text = rating;
        ratingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ratingText.fontSize = 48;
        ratingText.alignment = TextAnchor.MiddleCenter;
        ratingText.color = ratingColor;
        ratingText.raycastTarget = false;

        RectTransform rt = ratingGO.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0, 0.32f);
        rt.anchorMax = new Vector2(1, 0.42f);
        rt.sizeDelta = Vector2.zero;
    }

    private void CreateButton(Transform parent, string label, Vector2 offset, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject(label, typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);

        Image btnImage = btnGO.GetComponent<Image>();
        Color btnColor = label == "再来一次"
            ? new Color(0.20f, 0.45f, 0.75f)
            : new Color(0.45f, 0.30f, 0.20f);
        UIStyler.StyleButton(btnImage, btnColor);

        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(300, 80);
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = offset;

        GameObject textGO = new GameObject("Text", typeof(Text));
        textGO.transform.SetParent(btnGO.transform, false);

        Text btnText = textGO.GetComponent<Text>();
        btnText.text = label;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 40;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.raycastTarget = false;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Button btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(onClick);

        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.8f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.6f);
        btn.colors = colors;
    }

    /// <summary>
    /// 再来一次
    /// </summary>
    public void OnRestart()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void OnMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }
}
