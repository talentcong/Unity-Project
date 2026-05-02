using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// 闯关场景管理：动态创建 UI、显示题目、处理答题
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    [Header("答题配置")]
    public int scorePerQuestion = 10;   // 每题分值

    // UI 组件引用
    private Text wordText;
    private Button[] optionButtons;
    private Text[] optionTexts;
    private Text scoreText;
    private Text progressText;
    private GameObject correctPanel;
    private GameObject wrongPanel;
    private Text correctAnswerText;
    private GameObject feedbackOverlay;   // 反馈面板容器（含遮罩）
    private Button nextButton;            // 下一题按钮

    private void Start()
    {
        // 确保 EventSystem 存在
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // 如果 QuestionManager 不存在（测试时直接启动场景），自动创建
        if (QuestionManager.Instance == null)
        {
            GameObject qm = new GameObject("QuestionManager");
            qm.AddComponent<QuestionManager>();
            QuestionManager.Instance.GenerateRound(10);
        }

        CreateUI();
        ShowCurrentQuestion();
        UpdateInfo();
    }

    /// <summary>
    /// 动态创建闯关场景 UI
    /// </summary>
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

        // === 顶部信息栏 ===
        CreateTopBar(canvasGO.transform);

        // === 退出按钮 ===
        CreateExitButton(canvasGO.transform);

        // === 单词显示 ===
        wordText = CreateWordDisplay(canvasGO.transform);

        // === 选项按钮（2×2 网格） ===
        CreateOptionButtons(canvasGO.transform);

        // === 反馈面板 ===
        CreateFeedbackPanels(canvasGO.transform);

        // === 下一题按钮 ===
        CreateNextButton(canvasGO.transform);
    }

    private void CreateBackground(Transform parent)
    {
        GameObject bg = new GameObject("Background", typeof(Image));
        Image img = bg.GetComponent<Image>();
        img.color = new Color(0.12f, 0.20f, 0.40f);
        img.raycastTarget = false;
        RectTransform rt = bg.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
    }

    private void CreateTopBar(Transform parent)
    {
        // 分数
        GameObject scoreGO = new GameObject("ScoreText", typeof(Text));
        scoreText = scoreGO.GetComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleLeft;
        scoreText.raycastTarget = false;
        RectTransform scoreRT = scoreGO.GetComponent<RectTransform>();
        scoreRT.SetParent(parent, false);
        scoreRT.anchorMin = new Vector2(0, 0.92f);
        scoreRT.anchorMax = new Vector2(0.5f, 1);
        scoreRT.sizeDelta = Vector2.zero;
        scoreRT.anchoredPosition = new Vector2(120, 0);

        // 进度
        GameObject progGO = new GameObject("ProgressText", typeof(Text));
        progressText = progGO.GetComponent<Text>();
        progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        progressText.fontSize = 36;
        progressText.color = Color.white;
        progressText.alignment = TextAnchor.MiddleRight;
        progressText.raycastTarget = false;
        RectTransform progRT = progGO.GetComponent<RectTransform>();
        progRT.SetParent(parent, false);
        progRT.anchorMin = new Vector2(0.5f, 0.92f);
        progRT.anchorMax = new Vector2(1, 1);
        progRT.sizeDelta = Vector2.zero;
        progRT.anchoredPosition = new Vector2(-30, 0);
    }

    /// <summary>
    /// 创建游戏内退出按钮（返回主菜单）
    /// </summary>
    private void CreateExitButton(Transform parent)
    {
        GameObject btnGO = new GameObject("ExitButton", typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);

        Image img = btnGO.GetComponent<Image>();
        img.color = new Color(0.6f, 0.2f, 0.2f); // 红色
        img.raycastTarget = true;

        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(100, 45);
        btnRT.pivot = new Vector2(0, 1);           // 左上角为锚点
        btnRT.anchorMin = new Vector2(0, 1);
        btnRT.anchorMax = new Vector2(0, 1);
        btnRT.anchoredPosition = new Vector2(10, -10); // 距左上角 (10, 10)

        GameObject textGO = new GameObject("Text", typeof(Text));
        textGO.transform.SetParent(btnGO.transform, false);

        Text btnText = textGO.GetComponent<Text>();
        btnText.text = "退出";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 26;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.raycastTarget = false;

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        Button btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(OnExitClicked);
    }

    /// <summary>
    /// 退出按钮：返回主菜单
    /// </summary>
    private void OnExitClicked()
    {
        // 重置 QuestionManager 状态，下次游戏重新出题
        if (QuestionManager.Instance != null)
            QuestionManager.Instance.ResetRound();

        SceneManager.LoadScene("MainMenu");
    }

    private Text CreateWordDisplay(Transform parent)
    {
        GameObject wordGO = new GameObject("WordText", typeof(Text));
        Text text = wordGO.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 100;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        RectTransform rt = wordGO.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0, 0.55f);
        rt.anchorMax = new Vector2(1, 0.75f);
        rt.sizeDelta = Vector2.zero;
        return text;
    }

    private void CreateOptionButtons(Transform parent)
    {
        optionButtons = new Button[4];
        optionTexts = new Text[4];

        // 2×2 网格布局参数
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-230, 40),
            new Vector2(230, 40),
            new Vector2(-230, -80),
            new Vector2(230, -80)
        };

        for (int i = 0; i < 4; i++)
        {
            int index = i; // 闭包捕获
            GameObject btnGO = new GameObject($"Option{i}", typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);

            // 按钮外观
            Image img = btnGO.GetComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.7f);
            img.raycastTarget = true;

            // 按钮位置
            RectTransform btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(400, 100);
            btnRT.anchorMin = new Vector2(0.5f, 0.5f);
            btnRT.anchorMax = new Vector2(0.5f, 0.5f);
            btnRT.anchoredPosition = positions[i];

            // 按钮文字
            GameObject textGO = new GameObject("Text", typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);

            optionTexts[i] = textGO.GetComponent<Text>();
            optionTexts[i].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            optionTexts[i].fontSize = 40;
            optionTexts[i].alignment = TextAnchor.MiddleCenter;
            optionTexts[i].color = Color.white;
            optionTexts[i].raycastTarget = false;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            // Button 设置
            optionButtons[i] = btnGO.GetComponent<Button>();
            optionButtons[i].targetGraphic = img;
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));

            // 按钮颜色过渡
            ColorBlock colors = optionButtons[i].colors;
            colors.highlightedColor = new Color(0.3f, 0.5f, 0.8f);
            colors.pressedColor = new Color(0.1f, 0.3f, 0.6f);
            optionButtons[i].colors = colors;
        }
    }

    private void CreateFeedbackPanels(Transform parent)
    {
        // 反馈遮罩（半透明黑色，阻止与按钮交互）
        feedbackOverlay = new GameObject("FeedbackOverlay", typeof(Image));
        Image overlayImg = feedbackOverlay.GetComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.3f);
        overlayImg.raycastTarget = true;
        RectTransform overlayRT = feedbackOverlay.GetComponent<RectTransform>();
        overlayRT.SetParent(parent, false);
        overlayRT.anchorMin = new Vector2(0, 0);
        overlayRT.anchorMax = new Vector2(1, 1);
        overlayRT.sizeDelta = Vector2.zero;
        feedbackOverlay.SetActive(false);

        // 正确面板（在遮罩之上）
        correctPanel = CreateFeedbackPanel(feedbackOverlay.transform, "CorrectPanel", "✓ 正确！", new Color(0.1f, 0.7f, 0.2f, 0.9f));
        correctPanel.SetActive(false);

        // 错误面板（在遮罩之上）
        wrongPanel = CreateFeedbackPanel(feedbackOverlay.transform, "WrongPanel", "✗ 错误", new Color(0.7f, 0.1f, 0.2f, 0.9f), out correctAnswerText);
        wrongPanel.SetActive(false);
    }

    private GameObject CreateFeedbackPanel(Transform parent, string name, string message, Color bgColor)
    {
        GameObject panel = new GameObject(name, typeof(Image));
        Image img = panel.GetComponent<Image>();
        img.color = bgColor;
        img.raycastTarget = false;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.3f, 0.35f);
        rt.anchorMax = new Vector2(0.7f, 0.5f);
        rt.sizeDelta = Vector2.zero;

        GameObject msgGO = new GameObject("Message", typeof(Text));
        msgGO.transform.SetParent(panel.transform, false);
        Text msgText = msgGO.GetComponent<Text>();
        msgText.text = message;
        msgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgText.fontSize = 50;
        msgText.alignment = TextAnchor.MiddleCenter;
        msgText.color = Color.white;
        msgText.raycastTarget = false;
        RectTransform msgRT = msgGO.GetComponent<RectTransform>();
        msgRT.anchorMin = Vector2.zero;
        msgRT.anchorMax = Vector2.one;
        msgRT.sizeDelta = Vector2.zero;

        return panel;
    }

    private GameObject CreateFeedbackPanel(Transform parent, string name, string message, Color bgColor, out Text answerText)
    {
        GameObject panel = CreateFeedbackPanel(parent, name, message, bgColor);

        GameObject answerGO = new GameObject("CorrectAnswer", typeof(Text));
        answerGO.transform.SetParent(panel.transform, false);
        answerText = answerGO.GetComponent<Text>();
        answerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        answerText.fontSize = 36;
        answerText.alignment = TextAnchor.LowerCenter;
        answerText.color = Color.yellow;
        answerText.raycastTarget = false;
        RectTransform rt = answerGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.sizeDelta = Vector2.zero;

        return panel;
    }

    /// <summary>
    /// 创建"下一题"按钮（位于反馈面板下方）
    /// </summary>
    private void CreateNextButton(Transform parent)
    {
        GameObject btnGO = new GameObject("NextButton", typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);

        Image img = btnGO.GetComponent<Image>();
        img.color = new Color(0.3f, 0.6f, 0.3f); // 绿色
        img.raycastTarget = true;

        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(240, 70);
        btnRT.anchorMin = new Vector2(0.5f, 0.25f);
        btnRT.anchorMax = new Vector2(0.5f, 0.25f);
        btnRT.anchoredPosition = Vector2.zero;

        GameObject textGO = new GameObject("Text", typeof(Text));
        textGO.transform.SetParent(btnGO.transform, false);

        Text btnText = textGO.GetComponent<Text>();
        btnText.text = "下一题 →";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 36;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.raycastTarget = false;

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        nextButton = btnGO.GetComponent<Button>();
        nextButton.targetGraphic = img;
        nextButton.onClick.AddListener(OnNextClicked);

        ColorBlock colors = nextButton.colors;
        colors.highlightedColor = new Color(0.4f, 0.7f, 0.4f);
        colors.pressedColor = new Color(0.2f, 0.5f, 0.2f);
        nextButton.colors = colors;

        nextButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示当前题目
    /// </summary>
    private void ShowCurrentQuestion()
    {
        QuestionManager.QuestionData q = QuestionManager.Instance.GetCurrentQuestion();
        if (q == null) return;

        wordText.text = q.word;

        var options = QuestionManager.Instance.GetCurrentOptions();
        for (int i = 0; i < optionButtons.Length && i < options.Count; i++)
        {
            optionTexts[i].text = options[i];
            optionButtons[i].interactable = true;
        }

        // 隐藏反馈
        feedbackOverlay.SetActive(false);
        correctPanel.SetActive(false);
        wrongPanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // 播放单词发音
        AudioManager.Instance?.PlayWordVoice(q.word);
    }

    /// <summary>
    /// 更新分数和进度
    /// </summary>
    private void UpdateInfo()
    {
        int answered = QuestionManager.Instance.GetAnsweredCount();
        int total = QuestionManager.Instance.GetRoundTotal();
        scoreText.text = $"分数: {GameManager.Instance.currentScore}";
        progressText.text = $"{answered}/{total}";
    }

    /// <summary>
    /// 选项点击处理
    /// </summary>
    public void OnOptionClicked(int index)
    {
        // 禁用所有选项按钮
        foreach (Button btn in optionButtons)
            btn.interactable = false;

        // 判断答案
        string selected = optionTexts[index].text;
        bool isCorrect = QuestionManager.Instance.IsCorrectAnswer(selected);

        // 显示反馈遮罩
        feedbackOverlay.SetActive(true);

        if (isCorrect)
        {
            GameManager.Instance.AddScore(scorePerQuestion);
            correctPanel.SetActive(true);
            AudioManager.Instance?.PlayCorrectSFX();    // 答对音效
        }
        else
        {
            string correct = QuestionManager.Instance.GetCurrentQuestion().correctAnswer;
            correctAnswerText.text = $"正确答案: {correct}";
            wrongPanel.SetActive(true);
            AudioManager.Instance?.PlayWrongSFX();       // 答错音效
        }

        // 显示下一题按钮
        nextButton.gameObject.SetActive(true);
        UpdateInfo();
    }

    /// <summary>
    /// 点击"下一题"：前进到下一题或进入结算
    /// </summary>
    private void OnNextClicked()
    {
        feedbackOverlay.SetActive(false);
        correctPanel.SetActive(false);
        wrongPanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        if (QuestionManager.Instance.HasNextQuestion())
        {
            QuestionManager.Instance.MoveToNextQuestion();
            ShowCurrentQuestion();
            UpdateInfo();
        }
        else
        {
            GameManager.Instance.isGameOver = true;
            SceneManager.LoadScene("ResultScene");
        }
    }
}
