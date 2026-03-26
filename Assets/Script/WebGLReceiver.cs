using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 웹(JavaScript)에서 Unity로 메시지를 받는 스크립트
///
/// 사용법:
/// 1. 빈 GameObject 생성
/// 2. 이름을 "WebGLReceiver"로 변경 (중요!)
/// 3. 이 스크립트 부착
/// 4. (선택) messageText에 UI Text 연결
///
/// JavaScript에서 호출:
/// unityInstance.SendMessage('WebGLReceiver', 'ShowMessage', '안녕하세요!');
/// unityInstance.SendMessage('WebGLReceiver', 'LoadScene', 'Scene2');
/// unityInstance.SendMessage('WebGLReceiver', 'SetScore', 100);
/// </summary>
public class WebGLReceiver : MonoBehaviour
{
    [Header("UI 연결 (선택)")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLog = true;

    private void Awake()
    {
        // 씬 전환 시에도 유지 (필요한 경우)
        // DontDestroyOnLoad(gameObject);

        Log("WebGLReceiver 초기화 완료");
    }

    /// <summary>
    /// 웹에서 호출 - 텍스트 메시지 표시
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    public void ShowMessage(string message)
    {
        Log($"웹에서 받은 메시지: {message}");

        if (messageText != null)
        {
            messageText.text = message;
        }

        // 추가 처리가 필요하면 여기에 작성
        // 예: 이벤트 발생, UI 업데이트 등
    }

    /// <summary>
    /// 웹에서 호출 - 씬 이동
    /// </summary>
    /// <param name="sceneName">이동할 씬 이름</param>
    public void LoadScene(string sceneName)
    {
        Log($"씬 이동 요청: {sceneName}");

        // 씬이 Build Settings에 추가되어 있는지 확인 필요
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"씬 로드 실패: {sceneName}, 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 웹에서 호출 - 숫자(점수) 전달
    /// </summary>
    /// <param name="score">전달받은 점수</param>
    public void SetScore(int score)
    {
        Log($"점수 설정: {score}");

        // 점수 처리 로직 추가
        // 예: GameManager.Instance.SetScore(score);
    }

    /// <summary>
    /// 웹에서 호출 - float 값 전달
    /// </summary>
    /// <param name="value">전달받은 float 값</param>
    public void SetFloatValue(float value)
    {
        Log($"Float 값 설정: {value}");
    }

    /// <summary>
    /// 웹에서 호출 - 파라미터 없는 메서드
    /// </summary>
    public void OnWebButtonClick()
    {
        Log("웹 버튼 클릭됨!");

        // 버튼 클릭 시 처리할 로직
    }

    /// <summary>
    /// 웹에서 호출 - JSON 데이터 수신 (문자열로 받아서 파싱)
    /// </summary>
    /// <param name="jsonData">JSON 형식의 문자열</param>
    public void ReceiveJsonData(string jsonData)
    {
        Log($"JSON 데이터 수신: {jsonData}");

        // JSON 파싱 예시
        // var data = JsonUtility.FromJson<YourDataClass>(jsonData);
    }

    private void Log(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[WebGLReceiver] {message}");
        }
    }
}
