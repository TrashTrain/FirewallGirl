using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [Header("Configuration")]
    public int maxDeckSize = 5; // 최대 선택 가능한 카드 수
    public GameObject cardPrefab; // 카드 prefab
    public float collectionScale = 0.7f; // 컬렉션에 있을 때 카드 크기
    public float selectedScale = 0.5f; // 선택했을 때 카드 크기

    [Header("Containers")]
    public Transform collectionContainer; // 컬렉션 영역 (Grid Layout)
    public Transform selectedContainer;   // 선택 영역 (Horizontal Layout)
    
    // 현재 선택된 카드들을 추적하는 리스트
    private List<CardController> selectedCards = new List<CardController>();
    
    private string nextSceneName = "StageScene";
    
    private void Start()
    {
        List<CardObject> cardsToLoad = new List<CardObject>();
        
        // 1. 카드 데이터 가져오기
        if (CardDatabaseManager.instance != null)
        {
            cardsToLoad = CardDatabaseManager.instance.GetAllCards();
            Debug.Log($"[DeckManager] DB에서 {cardsToLoad.Count}장의 카드를 성공적으로 불러왔습니다.");
        }
        
        foreach (CardObject cardData in cardsToLoad)
        {
            if (cardData == null) continue;

            GameObject cardObj = Instantiate(cardPrefab, collectionContainer);
            cardObj.transform.localScale = Vector3.one * collectionScale;
            PlayerCard playerCard = cardObj.GetComponent<PlayerCard>();
            
            if (playerCard != null)
            {
                // ✅ [수정] 원본 SO를 그대로 쓰지 않고 복사본(Clone)을 생성하여 할당합니다.
                // 이렇게 하면 증강체로 값을 바꿔도 원본 파일이 손상되지 않습니다.
                CardObject clonedData = Instantiate(cardData);
                clonedData.name = cardData.name; // (Clone) 이름 제거 (선택사항)
                playerCard.cardData = clonedData;
                playerCard.posValue = cardData.positiveStatValue;
                playerCard.negValue = cardData.negativeStatValue;
                playerCard.cost = cardData.cost;
            }

            CardController cardCtrl = cardObj.GetComponent<CardController>();
            if (cardCtrl != null)
            {
                cardCtrl.currentMode = CardController.CardMode.DeckBuilding;
                cardCtrl.SetCollectionState(true);
            }
            
            UpdateCardVisuals(cardObj, cardData);
        }

        // // 2. 카드 생성 및 초기화
        // InitializeCollection(cardsToLoad);
    }
    
    void InitializeCollection(List<CardObject> dataList)
    {
        // 기존에 배치된 테스트용 카드가 있다면 삭제 (선택 사항)
        foreach (Transform child in collectionContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var data in dataList)
        {
            CreateCard(data);
        }
    }
    
    void CreateCard(CardObject data)
    {
        // 프리팹 생성
        GameObject newCardObj = Instantiate(cardPrefab, collectionContainer);
        newCardObj.transform.localScale = Vector3.one * collectionScale;
        
        // CardController 가져오기
        CardController cardCtrl = newCardObj.GetComponent<CardController>();

        if (cardCtrl != null)
        {
            // A. 덱 빌딩 모드로 설정 (중요: CardController 내부의 모드 전환)
            cardCtrl.SetupForDeckBuilding(this);
            
            // B. 데이터 연동 (PlayerCard 컴포넌트가 있다면 데이터 주입)
            PlayerCard playerCard = newCardObj.GetComponent<PlayerCard>();
            if (playerCard != null)
            {
                playerCard.cardData = data;
                // 필요하다면 playerCard.Setup(data) 같은 초기화 함수 호출
            }

            // C. UI 시각적 갱신 (이미지, 텍스트 등)
            UpdateCardVisuals(newCardObj, data);
        }
    }

    // 카드 프리팹 내부의 UI 요소들을 찾아 데이터를 넣어주는 함수
    void UpdateCardVisuals(GameObject cardObj, CardObject data)
    {
        // 1. 아이콘 이미지 설정 (이름으로 찾거나 태그로 찾기)
        // 프리팹 구조에 따라 경로 수정 필요 (예: "Front/Icon")
        // Image icon = cardObj.transform.Find("Icon")?.GetComponent<Image>(); 
        // if (icon == null) icon = cardObj.transform.Find("Front/Icon")?.GetComponent<Image>(); // 예시 경로
        // if (icon != null) icon.sprite = data.cardImage;
        //
        // // 2. 텍스트 설정 (TextMeshProUGUI 사용 가정)
        // // 프리팹 자식 오브젝트 이름이 "NameText", "CostText", "DescText"라고 가정
        // TextMeshProUGUI nameText = FindChild<TextMeshProUGUI>(cardObj.transform, "NameText");
        // if (nameText != null) nameText.text = data.cardName;
        //
        // TextMeshProUGUI costText = FindChild<TextMeshProUGUI>(cardObj.transform, "CostText");
        // if (costText != null) costText.text = data.cost.ToString();
        //
        // TextMeshProUGUI descText = FindChild<TextMeshProUGUI>(cardObj.transform, "DescText");
        // if (descText != null) descText.text = data.description;
        
        Transform closeBtn = FindChild<Transform>(cardObj.transform, "CloseBtn");
        if (closeBtn != null) closeBtn.gameObject.SetActive(false);
        
        Transform useBtn = FindChild<Transform>(cardObj.transform, "UseBtn");
        if (useBtn != null) useBtn.gameObject.SetActive(false);
        
        TextMeshProUGUI detailNameText = FindChild<TextMeshProUGUI>(cardObj.transform, "CardName/Text");
        if (detailNameText != null) detailNameText.text = data.cardName;
        
        Image detailIconImg = FindChild<Image>(cardObj.transform, "Content");
        if (detailIconImg != null) detailIconImg.sprite = data.cardImage;
        
        TextMeshProUGUI detailPosText = FindChild<TextMeshProUGUI>(cardObj.transform, "PositiveStat/Text");
        if (detailPosText != null) detailPosText.text = data.positiveStatValue.ToString("+#;-#;0");
        
        Debug.Log($"부정수치: {data.negativeStatValue}");
        TextMeshProUGUI detailNegText = FindChild<TextMeshProUGUI>(cardObj.transform, "NegativeStat/Text");
        if (detailNegText != null) detailNegText.text = data.negativeStatValue.ToString("+#;-#;0");

        TextMeshProUGUI detailCostText = FindChild<TextMeshProUGUI>(cardObj.transform, "Cost/CostText");
        if (detailCostText != null) detailCostText.text = data.cost.ToString();
        
        TextMeshProUGUI detailDescText = FindChild<TextMeshProUGUI>(cardObj.transform, "Description");
        if (detailDescText != null)
        {
            string dynamicDesc = data.description
                .Replace("{0}", data.positiveStatValue.ToString("+#;-#;0"))
                .Replace("{1}", data.negativeStatValue.ToString("+#;-#;0"));
            detailDescText.text = dynamicDesc;
        }
    }

    // CardController의 OnPointerClick에서 호출됨
    public void OnCardClicked(CardController card)
    {
        if (card.isClone)
        {
            // 이미 선택된 상태 -> 컬렉션으로 반환
            DeselectCard(card);
        }
        else
        {
            // 선택되지 않은 상태 -> 덱으로 추가
            SelectCard(card);
        }
    }

    void SelectCard(CardController card)
    {
        if (selectedCards.Count >= maxDeckSize) return;
        if (selectedCards.Contains(card)) return;
        
        // GameObject cloneObj = Instantiate(card.gameObject, selectedContainer);
        // cloneObj.name = card.name + "_Clone";
        //
        // card.SetCollectionState(false);
        //
        // CardController cloneController = cloneObj.GetComponent<CardController>();
        //
        // if (cloneController != null)
        // {
        //     cloneController.SetupForDeckBuilding(this);
        //     cloneController.isClone = true;
        //     cloneController.originalCard = card; 
        //     cloneController.SetCollectionState(true);
        //
        //     cloneController.SetScale(selectedScale);
        //     selectedCards.Add(cloneController);
        // }
        
        GameObject selectedObj = Instantiate(cardPrefab, selectedContainer);
        CardController selectedCtrl = selectedObj.GetComponent<CardController>();
        
        PlayerCard selectedPlayerCard = selectedObj.GetComponent<PlayerCard>();
        PlayerCard originalPlayerCard = card.GetComponent<PlayerCard>();

        if (selectedPlayerCard != null && originalPlayerCard != null)
        {
            // ✅ [수정] 선택된 영역에 생성될 때도 원본(이미 클론일 수 있음)을 다시 복제하여 할당합니다.
            CardObject selectedClone = Instantiate(originalPlayerCard.cardData);
            selectedClone.name = originalPlayerCard.cardData.name;
            selectedPlayerCard.cardData = selectedClone;
            
            UpdateCardVisuals(selectedObj, selectedClone);
        }

        if (selectedCtrl != null)
        {
            selectedCtrl.currentMode = CardController.CardMode.DeckBuilding;
            selectedCtrl.isSelected = true;
            selectedCtrl.isClone = true;
            selectedCtrl.originalCard = card;
            selectedCards.Add(selectedCtrl);
        }

        selectedObj.transform.localScale = Vector3.one * selectedScale;
        card.SetCollectionState(false);
    }

    void DeselectCard(CardController card)
    {
        // 1. 리스트에서 제거
        selectedCards.Remove(card);

        // 2. 원본 카드 다시 활성화 (색상 복구, 클릭 가능)
        if (card.originalCard != null)
        {
            card.originalCard.SetCollectionState(true);
        }

        // 3. 복제본 오브젝트 파괴
        Destroy(card.gameObject);
    }
    
    // 유틸리티: 자식 컴포넌트 이름으로 찾기 (재귀 아님, 직계 자식 위주)
    private T FindChild<T>(Transform parent, string name) where T : Component
    {
        Transform child = parent.Find(name);
        if (child != null) return child.GetComponent<T>();
        return null;
    }
    
    // public void OnConfirmClicked()
    // {
    //     // 1. 덱 최소 개수 조건 확인 (옵션)
    //     if (selectedCards.Count == 0)
    //     {
    //         Debug.LogWarning("카드를 한 장 이상 선택해야 합니다!");
    //         return;
    //     }
    //
    //     // 2. 선택된 카드들의 ID 추출
    //     List<int> selectedIds = new List<int>();
    //
    //     foreach (CardController cardCtrl in selectedCards)
    //     {
    //         CardObject cardData = cardCtrl.GetComponent<PlayerCard>().cardData;
    //         
    //         if (cardCtrl.originalCard != null && cardData != null)
    //         {
    //             selectedIds.Add(cardData.cardIndex);
    //         }
    //     }
    //
    //     // 3. DB 매니저에 현재 덱 정보 저장
    //     if (CardDatabaseManager.instance != null)
    //     {
    //         CardDatabaseManager.instance.SetCurrentDeck(selectedIds);
    //         Debug.Log($"덱 저장 완료: {selectedIds.Count}장");
    //         
    //         // 4. 씬 전환
    //         Debug.Log(nextSceneName);
    //         SceneManager.LoadScene(nextSceneName);
    //     }
    //     else
    //     {
    //         Debug.LogError("CardDatabaseManager가 없습니다! 씬 전환 불가.");
    //     }
    // }
    
    public void OnConfirmClicked()
    {
        if (selectedCards.Count == 0)
        {
            Debug.LogWarning("카드를 한 장 이상 선택해야 합니다!");
            return;
        }
        
        if (selectedCards.Count != maxDeckSize)
        {
            Debug.LogWarning($"카드를 정확히 {maxDeckSize}장 선택해야 합니다! (현재 선택된 카드: {selectedCards.Count}장)");
            return;
        }

        // ✅ 수정: ID(int)가 아니라 CardObject 리스트를 만듭니다.
        List<CardObject> finalDeck = new List<CardObject>();

        foreach (CardController cardCtrl in selectedCards)
        {
            CardObject cardData = cardCtrl.GetComponent<PlayerCard>().cardData;
        
            if (cardCtrl.originalCard != null && cardData != null)
            {
                finalDeck.Add(cardData);
            }
        }

        if (CardDatabaseManager.instance != null)
        {
            CardDatabaseManager.instance.SetCurrentDeck(finalDeck);
            Debug.Log($"덱 저장 완료: {finalDeck.Count}장");
        
            Debug.Log(nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("CardDatabaseManager가 없습니다! 씬 전환 불가.");
        }
    }
}
