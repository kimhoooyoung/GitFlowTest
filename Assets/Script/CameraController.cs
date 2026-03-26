using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// 카메라 시점 전환 컨트롤러
///
/// 웹(JavaScript)에서 호출:
/// unityInstance.SendMessage('CameraController', 'MoveCamera', 0);
/// </summary>
public class CameraController : MonoBehaviour
{
    #region jslib 함수 연결

    [DllImport("__Internal")]
    private static extern void LogToConsole(string message);

    #endregion

    [Header("카메라 설정")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform[] cameraPoints;  // SubCamera들의 Transform

    [Header("전환 설정")]
    [SerializeField] private float transitionSpeed = 5f;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLog = true;

    private int targetIndex = -1;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Log("CameraController 초기화 완료");
    }

    /// <summary>
    /// 웹에서 호출 - 카메라를 지정된 시점으로 부드럽게 이동
    /// </summary>
    /// <param name="index">카메라 시점 인덱스 (0~3)</param>
    public void MoveCamera(int index)
    {
        Log($"MoveCamera 호출됨 - index: {index}, cameraPoints.Length: {cameraPoints?.Length ?? 0}, mainCamera: {(mainCamera != null ? "있음" : "없음")}");

        if (cameraPoints == null || cameraPoints.Length == 0)
        {
            Log("에러: cameraPoints 배열이 비어있습니다!");
            return;
        }

        if (index < 0 || index >= cameraPoints.Length)
        {
            Log($"잘못된 카메라 인덱스: {index} (유효 범위: 0~{cameraPoints.Length - 1})");
            return;
        }

        targetIndex = index;
        isTransitioning = true;
        Log($"카메라 이동 시작: 시점 {index}");
    }

    /// <summary>
    /// 즉시 카메라 이동 (부드러운 전환 없이)
    /// </summary>
    /// <param name="index">카메라 시점 인덱스</param>
    public void MoveCameraInstant(int index)
    {
        if (index < 0 || index >= cameraPoints.Length)
        {
            Log($"잘못된 카메라 인덱스: {index}");
            return;
        }

        Transform target = cameraPoints[index];
        mainCamera.transform.position = target.position;
        mainCamera.transform.rotation = target.rotation;
        isTransitioning = false;
        Log($"카메라 즉시 이동 완료: 시점 {index}");
    }

    private void Update()
    {
        if (!isTransitioning || targetIndex < 0)
            return;

        Transform target = cameraPoints[targetIndex];

        // Lerp로 부드러운 전환
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            target.position,
            transitionSpeed * Time.deltaTime);

        mainCamera.transform.rotation = Quaternion.Lerp(
            mainCamera.transform.rotation,
            target.rotation,
            transitionSpeed * Time.deltaTime);

        // 도착 판정
        if (Vector3.Distance(mainCamera.transform.position, target.position) < 0.01f)
        {
            mainCamera.transform.position = target.position;
            mainCamera.transform.rotation = target.rotation;
            isTransitioning = false;
            Log($"카메라 이동 완료: 시점 {targetIndex}");
        }
    }

    private void Log(string message)
    {
        if (!enableDebugLog)
            return;

        string formattedMessage = $"[CameraController] {message}";

#if UNITY_WEBGL && !UNITY_EDITOR
        LogToConsole(formattedMessage);
#else
        Debug.Log(formattedMessage);
#endif
    }

    #region 테스트용 메서드

    [ContextMenu("테스트 - 카메라 0으로 이동")]
    private void TestMoveToCamera0() => MoveCamera(0);

    [ContextMenu("테스트 - 카메라 1로 이동")]
    private void TestMoveToCamera1() => MoveCamera(1);

    [ContextMenu("테스트 - 카메라 2로 이동")]
    private void TestMoveToCamera2() => MoveCamera(2);

    [ContextMenu("테스트 - 카메라 3으로 이동")]
    private void TestMoveToCamera3() => MoveCamera(3);

    #endregion
}
