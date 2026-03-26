using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Unity에서 웹(JavaScript)으로 메시지를 보내는 스크립트
///
/// 사용법:
/// 1. 이 스크립트를 GameObject에 부착
/// 2. WebGL 빌드 후 웹에서 window.onUnityMessage 콜백 설정
///
/// 웹에서 메시지 수신:
/// window.onUnityMessage = function(message) {
///     console.log('Unity에서 받은 메시지:', message);
/// };
/// </summary>
public class WebGLSender : MonoBehaviour
{
    #region .jslib 함수 연결 (외부 JavaScript 함수)

    [DllImport("__Internal")]
    private static extern void SendToWeb(string message);

    [DllImport("__Internal")]
    private static extern void ShowAlert(string message);

    [DllImport("__Internal")]
    private static extern void LogToConsole(string message);

    #endregion

    [Header("디버그")]
    [SerializeField] private bool enableDebugLog = true;

    // 로테이팅 메시지용 카운터
    private int messageCounter = 0;

    /// <summary>
    /// 웹으로 메시지 전송
    /// 웹에서 window.onUnityMessage 콜백으로 수신
    /// </summary>
    /// <param name="message">전송할 메시지</param>
    public void SendMessageToWeb(string message)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SendToWeb(message);
        Log($"웹으로 메시지 전송: {message}");
#else
        Log($"[Editor 모드] 웹으로 전송할 메시지: {message}");
#endif
    }

    /// <summary>
    /// 웹 브라우저에 Alert 창 표시
    /// </summary>
    /// <param name="message">Alert에 표시할 메시지</param>
    public void ShowWebAlert(string message)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowAlert(message);
        Log($"웹 Alert 표시: {message}");
#else
        Log($"[Editor 모드] Alert 메시지: {message}");
#endif
    }

    /// <summary>
    /// 웹 브라우저 콘솔에 로그 출력
    /// </summary>
    /// <param name="message">콘솔에 출력할 메시지</param>
    public void LogToWebConsole(string message)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        LogToConsole(message);
#else
        Log($"[Editor 모드] 웹 콘솔 로그: {message}");
#endif
    }

    /// <summary>
    /// JSON 데이터를 웹으로 전송
    /// </summary>
    /// <typeparam name="T">직렬화할 데이터 타입</typeparam>
    /// <param name="data">전송할 데이터</param>
    public void SendJsonToWeb<T>(T data)
    {
        string json = JsonUtility.ToJson(data);
        SendMessageToWeb(json);
    }

    /// <summary>
    /// 게임 이벤트를 웹으로 전송 (이벤트명과 데이터)
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <param name="data">이벤트 데이터</param>
    public void SendGameEvent(string eventName, string data = "")
    {
        string message = string.IsNullOrEmpty(data)
            ? $"{{\"event\":\"{eventName}\"}}"
            : $"{{\"event\":\"{eventName}\",\"data\":\"{data}\"}}";

        SendMessageToWeb(message);
    }

    /// <summary>
    /// 클릭할 때마다 카운터가 증가하며 다른 메시지 전송
    /// </summary>
    public void SendRotatingMessage()
    {
        messageCounter++;
        SendMessageToWeb($"{messageCounter}번째 메세지입니다.");
    }

    private void Log(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[WebGLSender] {message}");
        }
    }

    #region 테스트용 메서드

    [ContextMenu("테스트 - 웹으로 메시지 전송")]
    private void TestSendMessage()
    {
        SendMessageToWeb("Unity에서 보낸 테스트 메시지입니다!");
    }

    [ContextMenu("테스트 - 웹 Alert 표시")]
    private void TestShowAlert()
    {
        ShowWebAlert("Unity에서 보낸 Alert입니다!");
    }

    [ContextMenu("테스트 - 게임 이벤트 전송")]
    private void TestSendGameEvent()
    {
        SendGameEvent("game_started", "level_1");
    }

    [ContextMenu("테스트 - 로테이팅 메시지 전송")]
    private void TestSendRotatingMessage()
    {
        SendRotatingMessage();
    }

    #endregion
}
