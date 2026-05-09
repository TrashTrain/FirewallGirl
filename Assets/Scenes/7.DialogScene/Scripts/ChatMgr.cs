using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;
// using System;
using UnityEngine.SceneManagement;

public class ChatMgr : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject ChatGameObject;
    public GameObject NameGameObject;
    public Image bgImage;
    public Image characterImage;
    public TextMeshProUGUI chatText;
    public TextMeshProUGUI nameText;
    public Button actionButton;
    
    [Header("Audio")]
    public AudioSource sfxPlayer;   // 효과음 재생용 오디오 소스

    [Header("CSV Reader")]
    public string csv_FileName;
    
    public float typingSpeed = 0.05f; // 타이핑 속도
    public float bgFadeDuration = 0.5f; // 배경 전환 페이드 인/아웃 시간
    public float charFadeDuration = 0.5f;
    public float charMoveOffset = 100f;
    public Vector2 zoomFaceOffset = new Vector2(0, -150f);

    [Header("NextScene")]
    public string nextScene;

    private int scriptCount = 0;
    private List<Dictionary<string, object>> chatData;
    private bool isTyping = false;     // 현재 글자가 타이핑 중인지 확인
    private string currentFullText;    // 현재 줄의 전체 대사 저장
    private bool isWaitingForButton = false;

    private Vector2 originalBgPos;
    private Vector2 originalCharPos;
    private Vector3 originalCharScale;
    
    private string lastCharName = "";
    
    private Coroutine bgFadeCoroutine; // 현재 진행 중인 배경 페이드 코루틴 추적용
    private Coroutine zoomCoroutine;

    void Start()
    {
        chatData = CSVReader.Read(csv_FileName);

        if (chatData == null || chatData.Count == 0)
        {
            Debug.LogError("CSV 파일을 읽을 수 없습니다");
            return;
        }
        
        // 초기화
        if (bgImage != null)
        {
            originalBgPos = bgImage.rectTransform.anchoredPosition;
        }
        
        if (characterImage != null)
        {
            originalCharPos = characterImage.rectTransform.anchoredPosition;
            originalCharScale = characterImage.rectTransform.localScale;
            characterImage.gameObject.SetActive(false); // 처음엔 숨김
        }
        
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        ShowStep();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 1. 타이핑 중일 때 클릭하면 -> 타이핑 스킵하고 전체 문구 표시
                StopCoroutine("TypeText");
                chatText.text = currentFullText;
                isTyping = false;
            }
            else if (!isWaitingForButton)
            {
                // 타이핑이 끝났고, 특별한 버튼을 기다리는 상태가 아닐 때만 화면 클릭으로 넘어감
                scriptCount++;
                ShowStep();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    
    void ShowStep()
    {
        // 모든 대화가 끝났는지 체크
        if (scriptCount >= chatData.Count)
        {
            EndDialogue();
            return;
        }
        
        ResetZoomEffect();

        var row = chatData[scriptCount];

        // --- 1. 이름 설정 ---
        string nName = row.ContainsKey("Name_Character") ? row["Name_Character"].ToString() : "";
        if (string.IsNullOrEmpty(nName))
        {
            NameGameObject.SetActive(false);
        }
        else
        {
            NameGameObject.SetActive(true);
            nameText.text = nName;
        }
        
        // 2. 배경 이미지 교체
        string bgName = row.ContainsKey("Bg_Image") ? row["Bg_Image"].ToString() : "";
        Debug.Log(bgName);
        if (!string.IsNullOrEmpty(bgName))
        {
            // 만약 이미 페이드 효과가 진행 중이라면 중단 (빠르게 클릭해서 넘길 때 방지)
            if (bgFadeCoroutine != null) StopCoroutine(bgFadeCoroutine);

            if (bgName == "CLEAR")
            {
                bgFadeCoroutine = StartCoroutine(FadeBackground(null, true));
            }
            else
            {
                Sprite newBg = Resources.Load<Sprite>("Backgrounds/" + bgName);
                if (newBg != null)
                {
                    bgFadeCoroutine = StartCoroutine(FadeBackground(newBg, false));
                }
            }
        }

        // 3. 캐릭터 스프라이트 설정
        string spriteName = row.ContainsKey("Sprite_Character") ? row["Sprite_Character"].ToString() : "";
        HandleCharacterTransition(spriteName);
        
        // 4. 효과음 (SFX) 재생
        string sfxName = row.ContainsKey("SFX") ? row["SFX"].ToString() : "";
        if (!string.IsNullOrEmpty(sfxName) && sfxPlayer != null)
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/" + sfxName);
            if (clip != null) sfxPlayer.PlayOneShot(clip);
        }
        
        // 5. 특수 연출 (Effect) 처리
        string effect = row.ContainsKey("Effect") ? row["Effect"].ToString() : "";
        if (effect == "Shake") StartCoroutine(BgShake(0.5f, 10f));
        else if (effect == "ZoomIn") zoomCoroutine = StartCoroutine(ZoomInCharacter(2f, 2f)); // (시간, 줌 배율)

        // 6. 특정 버튼 대기 처리
        string requireBtn = row.ContainsKey("RequireButton") ? row["RequireButton"].ToString() : "";
        currentFullText = row.ContainsKey("Dialog") ? row["Dialog"].ToString() : "";
        
        if (requireBtn.ToUpper() == "TRUE" && actionButton != null)
        {
            isWaitingForButton = true;
            actionButton.gameObject.SetActive(true); // 알람 버튼 활성화
            ChatGameObject.SetActive(false);
        }
        else if (string.IsNullOrWhiteSpace(currentFullText))
        {
            isWaitingForButton = false;
            if (actionButton != null) actionButton.gameObject.SetActive(false);
            ChatGameObject.SetActive(false); // 대화창 숨김
        }
        else
        {
            isWaitingForButton = false;
            if (actionButton != null) actionButton.gameObject.SetActive(false);
            ChatGameObject.SetActive(true);
        }
        
        // 7. 대사 타이핑 시작
        if (ChatGameObject.activeSelf)
        {
            StopCoroutine("TypeText"); // 혹시 돌고 있을지 모를 코루틴 정지
            StartCoroutine("TypeText", currentFullText);
        }
    }
    
    // 캐릭터 스프라이트 전환 함수
    void HandleCharacterTransition(string newCharName)
    {
        if (newCharName == lastCharName) return;

        // 1. 사라지는 경우
        if (newCharName == "CLEAR" || string.IsNullOrEmpty(newCharName))
        {
            if (characterImage.gameObject.activeSelf) StartCoroutine(FadeOutCharacter());
            lastCharName = "";
            return;
        }

        Sprite newSprite = Resources.Load<Sprite>("Characters/" + newCharName);
        if (newSprite == null) return;

        // 2. 크로스 페이드 (Shadow -> Default)
        if (lastCharName == "AI_Shadow" && newCharName == "AI_Default")
        {
            StartCoroutine(CrossfadeSingleImage(newSprite));
        }
        // 3. 처음 등장
        else if (string.IsNullOrEmpty(lastCharName))
        {
            StartCoroutine(CharEntryEffect(newSprite));
        }
        // 4. 일반 교체 (기존 로직 그대로!)
        else
        {
            characterImage.sprite = newSprite;
            characterImage.gameObject.SetActive(true);
            characterImage.color = Color.white;
            characterImage.rectTransform.anchoredPosition = originalCharPos;
        }

        lastCharName = newCharName;
    }
    
    // 임시 잔상을 이용한 크로스 페이드
    IEnumerator CrossfadeSingleImage(Sprite newSprite)
    {
        // 1. 현재 이미지를 복제하여 '잔상(Afterimage)' 생성
        GameObject tempObj = new GameObject("TempAfterimage");
        tempObj.transform.SetParent(characterImage.transform.parent, false);
        tempObj.transform.SetSiblingIndex(characterImage.transform.GetSiblingIndex()); // 똑같은 깊이에 배치
        
        Image tempImg = tempObj.AddComponent<Image>();
        tempImg.sprite = characterImage.sprite; // 이전 이미지 복사
        tempImg.rectTransform.anchoredPosition = originalCharPos;
        tempImg.rectTransform.sizeDelta = characterImage.rectTransform.sizeDelta;
        tempImg.color = characterImage.color;

        // 2. 원본(characterImage)은 새 이미지로 교체하고 투명하게 만듦
        characterImage.sprite = newSprite;
        characterImage.color = new Color(1, 1, 1, 0);

        // 3. 잔상은 지우고, 원본은 나타나게 페이드!
        float elapsed = 0f;
        while (elapsed < charFadeDuration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / charFadeDuration;
            
            tempImg.color = new Color(1, 1, 1, 1 - p); // 잔상 페이드 아웃
            characterImage.color = new Color(1, 1, 1, p); // 새 이미지 페이드 인
            
            yield return null;
        }

        characterImage.color = Color.white;
        Destroy(tempObj); // 연출이 끝나면 잔상 오브젝트 파괴 (깔끔!)
    }

    // 등장 효과 (오른쪽에서 중앙으로)
    IEnumerator CharEntryEffect(Sprite newSprite)
    {
        characterImage.sprite = newSprite;
        characterImage.gameObject.SetActive(true);
        
        float elapsed = 0f;
        Vector2 startPos = originalCharPos + new Vector2(charMoveOffset, 0);

        while (elapsed < charFadeDuration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / charFadeDuration;
            characterImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalCharPos, p);
            characterImage.color = new Color(1, 1, 1, p);
            yield return null;
        }
        characterImage.rectTransform.anchoredPosition = originalCharPos;
    }

    // 퇴장 효과
    IEnumerator FadeOutCharacter()
    {
        float elapsed = 0f;
        while (elapsed < charFadeDuration)
        {
            elapsed += Time.deltaTime;
            characterImage.color = new Color(1, 1, 1, 1 - (elapsed / charFadeDuration));
            yield return null;
        }
        characterImage.gameObject.SetActive(false);
    }
    
    IEnumerator TypeText(string text)
    {
        isTyping = true;
        chatText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            chatText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
    
    // 배경 페이드 인/아웃 연출
    IEnumerator FadeBackground(Sprite newSprite, bool isClear)
    {
        Color c = bgImage.color;

        // 1. 현재 배경이 켜져 있다면 서서히 투명하게 (페이드 아웃)
        if (bgImage.gameObject.activeSelf && c.a > 0)
        {
            float elapsed = 0f;
            float startAlpha = c.a;
            while (elapsed < bgFadeDuration)
            {
                c.a = Mathf.Lerp(startAlpha, 0f, elapsed / bgFadeDuration);
                bgImage.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        c.a = 0f;
        bgImage.color = c;

        // CLEAR 명령이면 아예 끄고 종료
        if (isClear)
        {
            bgImage.gameObject.SetActive(false);
            yield break;
        }

        // 2. 새로운 이미지로 교체 후 활성화
        bgImage.sprite = newSprite;
        bgImage.gameObject.SetActive(true);

        // 3. 다시 서서히 불투명하게 (페이드 인)
        float fadeElapsed = 0f;
        while (fadeElapsed < bgFadeDuration)
        {
            c.a = Mathf.Lerp(0f, 1f, fadeElapsed / bgFadeDuration);
            bgImage.color = c;
            fadeElapsed += Time.deltaTime;
            yield return null;
        }

        c.a = 1f;
        bgImage.color = c;
    }
    
    // 화면 흔들림 연출
    IEnumerator BgShake(float duration, float magnitude)
    {
        Debug.Log("흔들기");
        float elapsed = 0.0f;
        RectTransform bgRect = bgImage.rectTransform;
        Debug.Log(bgRect.anchoredPosition);

        while (elapsed < duration)
        {
            // magnitude(픽셀) 단위만큼 랜덤한 x, y 오프셋 생성
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            // 배경 이미지를 오프셋만큼 이동
            bgRect.anchoredPosition = new Vector2(originalBgPos.x + x, originalBgPos.y + y);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 흔들림이 끝나면 원래 위치로 정확히 복구
        bgRect.anchoredPosition = originalBgPos;
    }
    
    // 캐릭터 클로즈업(줌인) 연출
    IEnumerator ZoomInCharacter(float targetScale, float duration)
    {
        Vector3 initialScale = characterImage.transform.localScale;
        Vector3 finalScale = initialScale * targetScale;
        // 위치(오프셋) 변수
        Vector2 initialPos = characterImage.rectTransform.anchoredPosition;
        Vector2 finalPos = originalCharPos + zoomFaceOffset;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            characterImage.transform.localScale = Vector3.Lerp(initialScale, finalScale, elapsed / duration);
            characterImage.rectTransform.anchoredPosition = Vector2.Lerp(initialPos, finalPos, elapsed / duration);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        characterImage.transform.localScale = finalScale;
        characterImage.rectTransform.anchoredPosition = finalPos;
    }
    
    IEnumerator ZoomInFace(float duration, float targetScaleMultiplier)
    {
        float elapsed = 0f;
        Vector3 startScale = characterImage.rectTransform.localScale;
        Vector3 targetScale = originalCharScale * targetScaleMultiplier;
        
        Vector2 startPos = characterImage.rectTransform.anchoredPosition;
        Vector2 targetPos = originalCharPos + zoomFaceOffset; // 설정한 Offset만큼 이동

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            
            // SmoothStep을 사용하여 시작과 끝이 부드러운 애니메이션 적용
            float t = Mathf.SmoothStep(0f, 1f, p); 

            characterImage.rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            characterImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        characterImage.rectTransform.localScale = targetScale;
        characterImage.rectTransform.anchoredPosition = targetPos;
    }
    
    // 줌인 상태를 원래대로 되돌리는 함수
    void ResetZoomEffect()
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }
        
        if (characterImage != null)
        {
            characterImage.rectTransform.localScale = originalCharScale;
            characterImage.rectTransform.anchoredPosition = originalCharPos;
        }
    }

    // 액션 버튼(알람 버튼)이 클릭되었을 때 실행
    public void OnActionButtonClicked()
    {
        // 버튼을 누르는 순간 대화창을 다시 켜고 다음 단계로 진행
        actionButton.gameObject.SetActive(false);
        isWaitingForButton = false;
        ChatGameObject.SetActive(true); // 대화창 다시 켜기
        
        scriptCount++;
        ShowStep();
    }

    void DisplayNextSentence()
    {

        // 대화 데이터가 더 이상 없는경우 EndDialogue
        if (scriptCount >= chatData.Count)
        {
            EndDialogue();
            return;
        }

        var row = chatData[scriptCount];

        nameText.text = row["Name_Character"].ToString();
        chatText.text = row["Dialog"].ToString();

        // 캐릭터이름이 null인 경우
        if (row["Name_Character"] == null)
        {
            nameText.text = "";

        }

        scriptCount++;
    }

    void EndDialogue()
    {
        // ChatGameObject.SetActive(false);
        // NameGameObject.SetActive(false);
        Debug.Log("대화 종료");
        SceneManager.LoadScene(nextScene);
    }
}