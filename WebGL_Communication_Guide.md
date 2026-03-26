# Unity WebGL ↔ 웹 양방향 통신 가이드

> Unity WebGL 빌드와 웹 페이지 간의 양방향 통신을 구현하는 완벽 가이드

## 📋 목차
1. [개요](#1-개요)
2. [프로젝트 구조](#2-프로젝트-구조)
3. [Unity 측 설정](#3-unity-측-설정)
4. [웹 측 설정](#4-웹-측-설정)
5. [테스트 방법](#5-테스트-방법)
6. [문제 해결](#6-문제-해결)
7. [실전 활용 예시](#7-실전-활용-예시)
8. [API 레퍼런스](#8-api-레퍼런스)

---

## 1. 개요

### Unity WebGL 통신이란?
Unity로 만든 게임/앱을 웹 브라우저에서 실행할 때, **Unity 내부**와 **웹 페이지(JavaScript)** 간에 데이터를 주고받는 기술입니다.

### 🔄 양방향 통신 아키텍처

```
┌─────────────────────────────────────────────────────────────┐
│                        웹 브라우저                           │
│  ┌─────────────────────┐      ┌─────────────────────────┐   │
│  │     index.html      │      │     Unity WebGL         │   │
│  │   (JavaScript)      │      │      (C# 코드)          │   │
│  │                     │      │                         │   │
│  │  ┌───────────────┐  │      │  ┌─────────────────┐    │   │
│  │  │ 버튼 클릭     │──┼──────┼─▶│ WebGLReceiver   │    │   │
│  │  │ SendMessage() │  │      │  │ ShowMessage()   │    │   │
│  │  └───────────────┘  │      │  └─────────────────┘    │   │
│  │                     │      │                         │   │
│  │  ┌───────────────┐  │      │  ┌─────────────────┐    │   │
│  │  │onUnityMessage │◀─┼──────┼──│ WebGLSender     │    │   │
│  │  │ 콜백 함수     │  │      │  │ SendToWeb()     │    │   │
│  │  └───────────────┘  │      │  └─────────────────┘    │   │
│  └─────────────────────┘      └─────────────────────────┘   │
│          ▲                              │                   │
│          │         .jslib 브릿지        │                   │
│          └──────────────────────────────┘                   │
└─────────────────────────────────────────────────────────────┘
```

### 📦 주요 구성 요소

| 구성 요소 | 위치 | 역할 |
|----------|------|------|
| `WebGLBridge.jslib` | Assets/Plugins/ | Unity→Web 통신 브릿지 |
| `WebGLSender.cs` | Assets/Script/ | Unity에서 웹으로 메시지 송신 |
| `WebGLReceiver.cs` | Assets/Script/ | 웹에서 Unity로 메시지 수신 |
| `index.html` | 빌드 폴더 | 웹 페이지 설정 |

---

## 2. 프로젝트 구조

```
📁 Unity 프로젝트
├── 📁 Assets
│   ├── 📁 Plugins
│   │   └── 📄 WebGLBridge.jslib    ← Unity→Web 브릿지
│   ├── 📁 Script
│   │   ├── 📄 WebGLSender.cs       ← Unity→Web 송신
│   │   └── 📄 WebGLReceiver.cs     ← Web→Unity 수신
│   └── 📁 Scenes
│       └── 📄 Main.unity
│
📁 빌드 폴더 (WebGL 빌드 후)
├── 📁 Build
│   ├── 📄 [빌드명].data
│   ├── 📄 [빌드명].framework.js
│   ├── 📄 [빌드명].loader.js
│   └── 📄 [빌드명].wasm
├── 📁 TemplateData
└── 📄 index.html                   ← 수정 필요
```

---

## 3. Unity 측 설정

### 3.1 WebGLBridge.jslib 생성

> 📍 위치: `Assets/Plugins/` 폴더 내 어디든 가능

이 파일은 Unity C# 코드에서 JavaScript 함수를 호출할 수 있게 해주는 **브릿지** 역할을 합니다.

---

#### 📐 기본 구조

```javascript
mergeInto(LibraryManager.library, {
    Unity함수명: function(파라미터) {
        var 변환된값 = UTF8ToString(파라미터);
        window.웹함수명(변환된값);
    }
});
```

---

#### 💡 실제 코드 예시

```javascript
mergeInto(LibraryManager.library, {
    SendToWeb: function(message) {
        var msg = UTF8ToString(message);
        window.onUnityMessage(msg);
    }
});
```

**↓ 각 부분 설명**

| 코드 | 설명 | 어디서 정의? |
|------|------|-------------|
| `SendToWeb` | Unity C#에서 호출하는 함수 이름 | 이 jslib 파일에서 정의 → C#의 `[DllImport]`와 이름 일치해야 함 |
| `message` | Unity에서 전달하는 문자열 (포인터) | C#에서 `SendToWeb("안녕")` 호출 시 전달됨 |
| `UTF8ToString(message)` | Unity 문자열 포인터 → JS 문자열 변환 | Unity WebGL 내장 함수 (자동 제공) |
| `window.onUnityMessage` | 웹(HTML/React)에서 정의한 콜백 함수 | index.html 또는 React에서 `window.onUnityMessage = function(msg){...}` 로 정의 |

---

#### 🔗 흐름 정리

```
[Unity C#]                    [jslib]                      [웹 JavaScript]
    │                            │                              │
    │  SendToWeb("안녕")         │                              │
    │ ─────────────────────────▶ │                              │
    │                            │  var msg = UTF8ToString()    │
    │                            │  window.onUnityMessage(msg)  │
    │                            │ ─────────────────────────────▶│
    │                            │                              │  메시지 수신!
```

---

> ⚠️ **핵심 포인트**
> - `.jslib` 파일은 `Assets/Plugins/` 폴더 내 어디든 위치 가능
> - `UTF8ToString()` → Unity 문자열을 JS 문자열로 변환 (필수!)
> - `window.함수명` → 웹에서 미리 정의해둔 전역 함수 호출

---

### 3.2 WebGLSender.cs 작성

> 📍 위치: `Assets/Script/WebGLSender.cs`

Unity에서 웹으로 메시지를 보내는 스크립트입니다.

```csharp
using UnityEngine;
using System.Runtime.InteropServices;

public class WebGLSender : MonoBehaviour
{
    // .jslib 함수 연결
    [DllImport("__Internal")]
    private static extern void SendToWeb(string message);

    [DllImport("__Internal")]
    private static extern void ShowAlert(string message);

    // 웹으로 메시지 전송
    public void SendMessageToWeb(string message)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SendToWeb(message);
#else
        Debug.Log($"[Editor] 웹으로 전송할 메시지: {message}");
#endif
    }

    // Alert 표시
    public void ShowWebAlert(string message)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowAlert(message);
#else
        Debug.Log($"[Editor] Alert: {message}");
#endif
    }
}
```

> ⚠️ **핵심 포인트**
> - `[DllImport("__Internal")]`로 .jslib 함수를 연결합니다
> - `#if UNITY_WEBGL && !UNITY_EDITOR`로 에디터와 빌드 환경을 구분합니다

---

### 3.3 WebGLReceiver.cs 작성

> 📍 위치: `Assets/Script/WebGLReceiver.cs`

웹에서 Unity로 메시지를 받는 스크립트입니다.

```csharp
using UnityEngine;
using TMPro;

public class WebGLReceiver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    // 웹에서 호출 - 메시지 표시
    public void ShowMessage(string message)
    {
        Debug.Log($"웹에서 받은 메시지: {message}");

        if (messageText != null)
            messageText.text = message;
    }

    // 웹에서 호출 - 점수 설정
    public void SetScore(int score)
    {
        Debug.Log($"점수 설정: {score}");
    }

    // 웹에서 호출 - 버튼 클릭 알림
    public void OnWebButtonClick()
    {
        Debug.Log("웹 버튼 클릭됨!");
    }
}
```

> 🚨 **매우 중요!**
> - **GameObject 이름이 정확해야 합니다!**
> - 웹에서 `SendMessage('WebGLReceiver', ...)` 호출 시, 씬에 **"WebGLReceiver"** 이름의 GameObject가 있어야 합니다
> - 파라미터는 **string, int, float**만 가능합니다 (배열, 객체 불가)

---

### 3.4 씬 설정

#### Step 1: WebGLReceiver GameObject 생성
1. **Hierarchy** → 우클릭 → **Create Empty**
2. 이름을 **"WebGLReceiver"**로 변경 (⚠️ 정확히 일치해야 함!)
3. **WebGLReceiver.cs** 스크립트 부착

#### Step 2: WebGLSender 설정
1. 아무 GameObject에 **WebGLSender.cs** 부착
2. 또는 별도 빈 GameObject 생성 후 부착

#### Step 3: UI 버튼 연결 (Unity → Web 테스트용)
1. **Canvas** → **Button** 생성
2. Button의 **On Click()** 이벤트에:
   - WebGLSender 오브젝트 드래그
   - **WebGLSender → SendMessageToWeb(string)** 선택
   - 전송할 메시지 입력

---

### 3.5 빌드 설정

#### Build Settings
1. **File → Build Settings** 열기
2. **Platform**: WebGL 선택 → **Switch Platform**
3. 씬 추가 확인
4. **Build** 또는 **Build And Run** 클릭

#### Player Settings (권장)
- **Publishing Settings**:
  - Compression Format: **Gzip** (서버 지원 시)
  - Decompression Fallback: ✅ 체크

---

## 4. 웹 측 설정

### 4.1 unityInstance 전역화

Unity 빌드 시 생성된 `index.html`에서 `createUnityInstance` Promise의 결과를 전역 변수로 저장:

```javascript
.then((instance) => {
    window.unityInstance = instance;
    // ...
});
```

---

### 4.2 Unity → Web 수신 (콜백 등록)

jslib에서 `window.onUnityMessage(msg)`를 호출하므로, 웹에서 해당 함수 정의:

```javascript
window.onUnityMessage = function(message) {
    // message: Unity에서 전송한 문자열
    const data = JSON.parse(message); // JSON인 경우
};
```

---

### 4.3 Web → Unity 전송 (SendMessage)

```javascript
unityInstance.SendMessage('GameObjectName', 'MethodName', param);
```

| 파라미터 타입 | 지원 |
|-------------|------|
| string | ✅ |
| number (int/float) | ✅ |
| 없음 | ✅ |
| object/array | ❌ → `JSON.stringify()` 필요 |

---

## 5. 테스트 방법

### 5.1 로컬 서버 실행

WebGL 빌드는 **로컬 서버**에서 실행해야 합니다. (`file://`로는 동작 안 함)

#### Python
```bash
cd /빌드폴더/경로
python -m http.server 8080
```

#### Node.js
```bash
npx http-server -p 8080
```

#### VS Code Live Server
`index.html` 우클릭 → **Open with Live Server**

---

### 5.2 테스트 체크리스트

#### ✅ Web → Unity 테스트
1. 웹 페이지의 버튼 클릭
2. Unity 콘솔에 메시지 출력 확인

#### ✅ Unity → Web 테스트
1. Unity 내 버튼 클릭
2. 웹 페이지에 메시지 표시 확인
3. 브라우저 콘솔에 로그 출력 확인

#### 브라우저 콘솔에서 직접 테스트
```javascript
unityInstance.SendMessage('WebGLReceiver', 'ShowMessage', '테스트!');
```

---

## 6. 문제 해결

### ❌ "unityInstance is not defined"

**원인**: unityInstance가 전역화되지 않음

**해결**:
```javascript
.then((instance) => {
    window.unityInstance = instance;
});
```

---

### ❌ "Could not find object named: WebGLReceiver"

**원인**: 씬에 해당 이름의 GameObject가 없음

**해결**:
1. Hierarchy에서 GameObject 이름 확인
2. 이름이 **정확히 "WebGLReceiver"**인지 확인 (대소문자 구분)
3. 해당 GameObject에 스크립트가 부착되어 있는지 확인

---

### ❌ 메서드가 호출되지 않음

**확인사항**:
1. 메서드가 `public`인지 확인
2. 파라미터 타입이 **string, int, float** 중 하나인지 확인
3. 메서드 이름 철자 확인 (대소문자 구분)

---

## 7. 실전 활용 예시

### 예시 1: 게임 점수를 웹으로 전송

**Unity (C#)**:
```csharp
public void OnGameEnd(int finalScore)
{
    string json = $"{{\"event\":\"game_end\",\"score\":{finalScore}}}";
    webglSender.SendMessageToWeb(json);
}
```

**웹 (JavaScript)**:
```javascript
window.onUnityMessage = function(message) {
    var data = JSON.parse(message);
    if (data.event === "game_end") {
        // 서버로 점수 전송 등
    }
};
```

---

### 예시 2: 웹에서 게임 설정 변경

**웹 (JavaScript)**:
```javascript
function setDifficulty(level) {
    unityInstance.SendMessage('GameManager', 'SetDifficulty', level);
}
```

**Unity (C#)**:
```csharp
public class GameManager : MonoBehaviour
{
    public void SetDifficulty(string level)
    {
        switch (level)
        {
            case "easy": /* 설정 */ break;
            case "hard": /* 설정 */ break;
        }
    }
}
```

---

## 8. API 레퍼런스

### 8.1 Unity 제공 내장 헬퍼 함수 (jslib에서 사용)

> ⚠️ 이 함수들은 **Unity WebGL 빌드 시 자동으로 제공**되는 함수입니다.

| 함수 | 설명 | 사용 예시 |
|------|------|----------|
| `UTF8ToString(ptr)` | Unity 문자열 포인터 → JS 문자열 변환 | `var msg = UTF8ToString(message);` |
| `stringToUTF8(str, ptr, maxBytes)` | JS 문자열 → UTF8 메모리에 작성 | `stringToUTF8(jsString, buffer, bufferSize);` |
| `lengthBytesUTF8(str)` | UTF8 문자열의 바이트 길이 계산 | `var len = lengthBytesUTF8(str) + 1;` |
| `_malloc(size)` | 메모리 할당 (문자열 반환 시 필요) | `var buffer = _malloc(bufferSize);` |

> 📝 **참고**: 구버전 Unity에서는 `Pointer_stringify()` 사용, 최신 버전은 `UTF8ToString()` 권장

---

### 8.2 jslib 함수 (직접 정의)

> ⚠️ **jslib 함수는 고정된 목록이 아닙니다!**
> 아래는 **커스텀 함수** 예시입니다. 필요에 따라 자유롭게 추가/수정 가능합니다.

| 함수 (커스텀) | 설명 | C#에서 호출 |
|--------------|------|------------|
| `SendToWeb(message)` | 웹으로 메시지 전송 | `SendToWeb("안녕");` |
| `ShowAlert(message)` | 브라우저 Alert 표시 | `ShowAlert("알림!");` |
| `LogToConsole(message)` | 브라우저 콘솔 로그 | `LogToConsole("로그");` |
| `SetElementText(id, text)` | DOM 요소 텍스트 변경 | `SetElementText("score", "100");` |
| `SetElementVisible(id, visible)` | DOM 요소 표시/숨김 | `SetElementVisible("popup", 1);` |
| `SaveToLocalStorage(key, value)` | 로컬 스토리지 저장 | `SaveToLocalStorage("name", "홍길동");` |

---

### 8.3 JavaScript → Unity 통신 (SendMessage)

> 📖 [Unity 공식 문서](https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html)

#### 기본 문법
```javascript
unityInstance.SendMessage('게임오브젝트이름', '메서드이름', 파라미터);
```

#### 파라미터 타입 제한
| 타입 | 지원 | 예시 |
|------|------|------|
| 문자열 (string) | ✅ | `'안녕하세요'` |
| 숫자 (int/float) | ✅ | `100`, `3.14` |
| 파라미터 없음 | ✅ | 생략 가능 |
| 배열/객체 | ❌ | JSON 문자열로 변환 필요 |

---

### 8.4 Unity → JavaScript 통신 (콜백)

#### 웹에서 콜백 정의
```javascript
window.onUnityMessage = function(message) {
    // 처리 로직
};
```

#### Unity에서 호출 (C# → jslib → 웹)
```csharp
[DllImport("__Internal")]
private static extern void SendToWeb(string message);

public void SendMessageToWeb(string message) {
    SendToWeb(message);  // jslib의 SendToWeb 호출 → window.onUnityMessage 실행
}
```

---

### 8.5 공식 문서 참고 링크

| 문서 | 링크 |
|------|------|
| Unity WebGL JavaScript 상호작용 (한국어) | https://docs.unity3d.com/kr/current/Manual/webgl-interactingwithbrowserscripting.html |
| Unity WebGL JavaScript 상호작용 (영어) | https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html |
| Unity WebGL 메모리 | https://docs.unity3d.com/Manual/webgl-memory.html |

---

## 📝 작성 정보

- 작성일: 2026-03-06
- 테스트 환경: Chrome, Firefox, Safari
