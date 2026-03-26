/*
 * WebGLBridge.jslib
 *
 * Unity C#에서 웹(JavaScript)으로 통신하기 위한 브릿지 파일
 *
 * 사용법:
 * 1. 이 파일을 Assets/Plugins/WebGL/ 폴더에 배치
 * 2. C#에서 [DllImport("__Internal")] 속성으로 함수 연결
 * 3. WebGL 빌드 시 자동으로 포함됨
 *
 * 웹에서 메시지 수신 설정:
 * window.onUnityMessage = function(message) {
 *     console.log('Unity에서 받은 메시지:', message);
 * };
 */

mergeInto(LibraryManager.library, {

    /**
     * Unity에서 웹으로 메시지 전송
     * 웹에서 window.onUnityMessage 콜백으로 수신
     *
     * @param {number} message - UTF8 인코딩된 문자열 포인터
     */
    SendToWeb: function(message) {
        // UTF8 포인터를 JavaScript 문자열로 변환
        var msg = UTF8ToString(message);

        console.log("[Unity → Web] 메시지:", msg);

        // 웹 페이지에 정의된 콜백 함수 호출
        if (typeof window.onUnityMessage === 'function') {
            window.onUnityMessage(msg);
        } else {
            console.warn("window.onUnityMessage 콜백이 정의되지 않았습니다.");
        }
    },

    /**
     * 웹 브라우저에 Alert 창 표시
     *
     * @param {number} message - UTF8 인코딩된 문자열 포인터
     */
    ShowAlert: function(message) {
        var msg = UTF8ToString(message);
        alert(msg);
    },

    /**
     * 웹 브라우저 콘솔에 로그 출력
     *
     * @param {number} message - UTF8 인코딩된 문자열 포인터
     */
    LogToConsole: function(message) {
        var msg = UTF8ToString(message);
        console.log("[Unity]", msg);
    },

    /**
     * 웹 브라우저 콘솔에 경고 출력
     *
     * @param {number} message - UTF8 인코딩된 문자열 포인터
     */
    WarnToConsole: function(message) {
        var msg = UTF8ToString(message);
        console.warn("[Unity]", msg);
    },

    /**
     * 웹 브라우저 콘솔에 에러 출력
     *
     * @param {number} message - UTF8 인코딩된 문자열 포인터
     */
    ErrorToConsole: function(message) {
        var msg = UTF8ToString(message);
        console.error("[Unity]", msg);
    },

    /**
     * 웹 페이지의 특정 요소에 텍스트 설정
     *
     * @param {number} elementId - 요소 ID (UTF8 포인터)
     * @param {number} text - 설정할 텍스트 (UTF8 포인터)
     */
    SetElementText: function(elementId, text) {
        var id = UTF8ToString(elementId);
        var txt = UTF8ToString(text);

        var element = document.getElementById(id);
        if (element) {
            element.innerText = txt;
        } else {
            console.warn("요소를 찾을 수 없습니다:", id);
        }
    },

    /**
     * 웹 페이지의 특정 요소 표시/숨김
     *
     * @param {number} elementId - 요소 ID (UTF8 포인터)
     * @param {number} visible - 표시 여부 (0 또는 1)
     */
    SetElementVisible: function(elementId, visible) {
        var id = UTF8ToString(elementId);

        var element = document.getElementById(id);
        if (element) {
            element.style.display = visible ? 'block' : 'none';
        } else {
            console.warn("요소를 찾을 수 없습니다:", id);
        }
    },

    /**
     * 웹 페이지 URL 이동
     *
     * @param {number} url - 이동할 URL (UTF8 포인터)
     */
    NavigateToUrl: function(url) {
        var urlStr = UTF8ToString(url);
        window.location.href = urlStr;
    },

    /**
     * 새 탭에서 URL 열기
     *
     * @param {number} url - 열 URL (UTF8 포인터)
     */
    OpenUrlInNewTab: function(url) {
        var urlStr = UTF8ToString(url);
        window.open(urlStr, '_blank');
    },

    /**
     * 로컬 스토리지에 데이터 저장
     *
     * @param {number} key - 키 (UTF8 포인터)
     * @param {number} value - 값 (UTF8 포인터)
     */
    SaveToLocalStorage: function(key, value) {
        var k = UTF8ToString(key);
        var v = UTF8ToString(value);

        try {
            localStorage.setItem(k, v);
        } catch (e) {
            console.error("LocalStorage 저장 실패:", e);
        }
    },

    /**
     * 로컬 스토리지에서 데이터 불러오기
     * 주의: 문자열 반환을 위해 별도 처리 필요
     *
     * @param {number} key - 키 (UTF8 포인터)
     * @returns {number} - 문자열 포인터 (없으면 빈 문자열)
     */
    LoadFromLocalStorage: function(key) {
        var k = UTF8ToString(key);

        try {
            var value = localStorage.getItem(k) || "";
            var bufferSize = lengthBytesUTF8(value) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(value, buffer, bufferSize);
            return buffer;
        } catch (e) {
            console.error("LocalStorage 불러오기 실패:", e);
            return 0;
        }
    }
});
