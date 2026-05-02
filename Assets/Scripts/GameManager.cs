using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏流程管理：分数、关卡状态、场景切换
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("游戏配置")]
    public int totalQuestionsPerRound = 10;  // 每轮题目数量
    public int passScore = 60;               // 及格分（百分比）

    [Header("当前状态")]
    public int currentScore = 0;             // 当前得分
    public int correctCount = 0;             // 答对次数
    public bool isGameOver = false;          // 是否结束

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 开始新游戏：重置分数、初始化题库、切到闯关场景
    /// </summary>
    public void StartGame()
    {
        currentScore = 0;
        correctCount = 0;
        isGameOver = false;

        // 确保 QuestionManager 存在并生成本轮题目
        if (QuestionManager.Instance == null)
        {
            GameObject qm = new GameObject("QuestionManager");
            qm.AddComponent<QuestionManager>();
        }
        else
        {
            // 重新生成时重置 QuestionManager 内部状态
            QuestionManager.Instance.ResetRound();
        }
        QuestionManager.Instance.GenerateRound(totalQuestionsPerRound);

        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// 答对时加分
    /// </summary>
    public void AddScore(int points)
    {
        currentScore += points;
        correctCount++;
    }

    /// <summary>
    /// 前进到下一题，或结束游戏进入结算
    /// </summary>
    public void NextQuestion()
    {
        if (QuestionManager.Instance == null) return;

        if (QuestionManager.Instance.HasNextQuestion())
        {
            QuestionManager.Instance.MoveToNextQuestion();
        }
        else
        {
            isGameOver = true;
            SceneManager.LoadScene("ResultScene");
        }
    }

    /// <summary>
    /// 选项被点击时由 UIManager 调用
    /// </summary>
    public void OnOptionSelected(int optionIndex)
    {
        // 将在后续步骤中实现：获取选项文字 → 判断对错 → 加分 → 反馈 → 下一题
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
