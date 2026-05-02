using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI 管理：题目显示、按钮交互、反馈、分数更新
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("题目区域")]
    public Text wordText;               // 显示英文单词
    public Image questionImage;         // 题目配图（可选）

    [Header("选项按钮")]
    public Button[] optionButtons;      // 4 个选项按钮
    private Text[] optionTexts;         // 按钮上的文字

    [Header("信息显示")]
    public Text scoreText;              // 分数显示
    public Text progressText;           // 进度显示（如 3/10）

    [Header("反馈面板")]
    public GameObject correctPanel;     // 答对提示面板
    public GameObject wrongPanel;       // 答错提示面板
    public Text correctAnswerText;      // 答错时显示正确答案

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // 获取选项按钮上的文字组件
        optionTexts = new Text[optionButtons.Length];
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // 局部变量防止闭包问题
            optionTexts[i] = optionButtons[i].GetComponentInChildren<Text>();
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
        }

        // 默认隐藏反馈面板
        if (correctPanel != null) correctPanel.SetActive(false);
        if (wrongPanel != null) wrongPanel.SetActive(false);
    }

    /// <summary>
    /// 更新题目显示
    /// </summary>
    public void UpdateQuestion(string word, List<string> options)
    {
        if (wordText != null) wordText.text = word;

        for (int i = 0; i < optionButtons.Length && i < options.Count; i++)
        {
            if (optionTexts[i] != null)
                optionTexts[i].text = options[i];
            optionButtons[i].interactable = true;
        }
    }

    /// <summary>
    /// 更新分数显示
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"分数: {score}";
    }

    /// <summary>
    /// 更新进度显示
    /// </summary>
    public void UpdateProgress(int current, int total)
    {
        if (progressText != null)
            progressText.text = $"{current}/{total}";
    }

    /// <summary>
    /// 显示答对反馈
    /// </summary>
    public void ShowCorrectFeedback()
    {
        if (correctPanel != null)
        {
            correctPanel.SetActive(true);
            Invoke(nameof(HideCorrectFeedback), 1.0f);
        }
    }

    private void HideCorrectFeedback()
    {
        if (correctPanel != null) correctPanel.SetActive(false);
    }

    /// <summary>
    /// 显示答错反馈
    /// </summary>
    public void ShowWrongFeedback(string correctAnswer)
    {
        if (wrongPanel != null)
        {
            if (correctAnswerText != null)
                correctAnswerText.text = $"正确答案: {correctAnswer}";
            wrongPanel.SetActive(true);
            Invoke(nameof(HideWrongFeedback), 1.5f);
        }
    }

    private void HideWrongFeedback()
    {
        if (wrongPanel != null) wrongPanel.SetActive(false);
    }

    /// <summary>
    /// 选项点击处理
    /// </summary>
    private void OnOptionClicked(int index)
    {
        // 禁用所有按钮防止重复点击
        foreach (var btn in optionButtons)
            btn.interactable = false;

        // 由 GameManager 处理后续逻辑
        if (GameManager.Instance != null)
            GameManager.Instance.OnOptionSelected(index);
    }

    /// <summary>
    /// 隐藏反馈并启用按钮（在进入下一题时调用）
    /// </summary>
    public void ResetForNextQuestion()
    {
        if (correctPanel != null) correctPanel.SetActive(false);
        if (wrongPanel != null) wrongPanel.SetActive(false);
        foreach (var btn in optionButtons)
            btn.interactable = true;
    }
}
