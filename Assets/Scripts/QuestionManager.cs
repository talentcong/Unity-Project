using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 题库管理：加载题库、随机出题、答案判断
/// </summary>
public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance { get; private set; }

    [System.Serializable]
    public class QuestionData
    {
        public string word;          // 英文单词
        public string correctAnswer; // 中文释义（正确答案）
        public string[] wrongAnswers;// 错误选项
        public string imageName;     // 对应的图片文件名（可选）
    }

    [System.Serializable]
    private class QuestionList
    {
        public List<QuestionData> questions;
    }

    private List<QuestionData> allQuestions;   // 完整题库
    private List<QuestionData> currentRound;   // 本轮抽取的题目
    private int currentRoundIndex = 0;         // 当前题目在 currentRound 中的索引

    private void Awake()
    {
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

    private void Start()
    {
        LoadQuestions();
    }

    /// <summary>
    /// 从 Resources/Questions.json 加载题库
    /// </summary>
    private void LoadQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Questions");
        if (jsonFile == null)
        {
            Debug.LogError("题库文件丢失: Resources/Questions.json");
            return;
        }

        QuestionList questionList = JsonUtility.FromJson<QuestionList>(jsonFile.text);
        allQuestions = questionList.questions;
        Debug.Log($"题库加载完成，共 {allQuestions.Count} 题");
    }

    /// <summary>
    /// 从题库中随机抽取一组题目
    /// </summary>
    public void GenerateRound(int count)
    {
        // 如果 Start() 还没运行（动态创建时），手动加载题库
        if (allQuestions == null || allQuestions.Count == 0)
        {
            LoadQuestions();
        }

        if (allQuestions == null || allQuestions.Count == 0)
        {
            Debug.LogError("题库为空，无法生成题目");
            return;
        }

        // 打乱题库后取前 count 道
        var shuffled = allQuestions.OrderBy(q => Random.value).ToList();
        currentRound = shuffled.Take(Mathf.Min(count, allQuestions.Count)).ToList();
        currentRoundIndex = 0;
    }

    /// <summary>
    /// 重置本轮状态，供重新开始游戏时调用
    /// </summary>
    public void ResetRound()
    {
        currentRound = null;
        currentRoundIndex = 0;
    }

    /// <summary>
    /// 获取当前题目
    /// </summary>
    public QuestionData GetCurrentQuestion()
    {
        if (currentRound == null || currentRoundIndex >= currentRound.Count)
            return null;

        return currentRound[currentRoundIndex];
    }

    /// <summary>
    /// 获取当前题目的选项列表（已打乱）
    /// </summary>
    public List<string> GetCurrentOptions()
    {
        QuestionData q = GetCurrentQuestion();
        if (q == null) return new List<string>();

        List<string> options = new List<string>(q.wrongAnswers);
        options.Add(q.correctAnswer);
        // 打乱选项顺序
        options = options.OrderBy(o => Random.value).ToList();
        return options;
    }

    /// <summary>
    /// 验证答案是否正确
    /// </summary>
    public bool IsCorrectAnswer(string selectedAnswer)
    {
        QuestionData q = GetCurrentQuestion();
        return q != null && selectedAnswer == q.correctAnswer;
    }

    /// <summary>
    /// 前进到下一题
    /// </summary>
    public void MoveToNextQuestion()
    {
        currentRoundIndex++;
    }

    /// <summary>
    /// 判断是否还有下一题
    /// </summary>
    public bool HasNextQuestion()
    {
        return currentRound != null && currentRoundIndex < currentRound.Count - 1;
    }

    /// <summary>
    /// 已答完的题数
    /// </summary>
    public int GetAnsweredCount()
    {
        return currentRoundIndex + 1;
    }

    /// <summary>
    /// 本轮总题数
    /// </summary>
    public int GetRoundTotal()
    {
        return currentRound != null ? currentRound.Count : 0;
    }
}
