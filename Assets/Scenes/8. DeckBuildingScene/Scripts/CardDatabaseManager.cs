using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabaseManager : MonoBehaviour
{
    public static CardDatabaseManager instance;
    
    private Dictionary<int, CardObject> cardDictionary = new Dictionary<int, CardObject>();

    [Header("Current Run Data")]
    [Tooltip("다음 씬(배틀)에서 사용할 덱의 ID 리스트입니다.")]
    public List<int> currentDeckIds = new List<int>();

    [Header("Player Progress (Optional)")]
    [Tooltip("플레이어가 현재 보유 중인 카드 ID 리스트 (컬렉션 표시용)")]
    public List<int> ownedCardIds = new List<int>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitCardDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitCardDatabase()
    {
        // "Resources/Cards" 폴더 안의 모든 CardData 타입 에셋을 배열로 불러옴
        CardObject[] allCards = Resources.LoadAll<CardObject>("Cards");

        foreach (CardObject card in allCards)
        {
            // 카드 안에 설정된 고유 id를 키값으로 사전에 저장
            if (!cardDictionary.ContainsKey(card.cardIndex))
            {
                cardDictionary.Add(card.cardIndex, card);
            }
        }
        
        Debug.Log($"총 {cardDictionary.Count}장의 카드를 동적으로 불러왔습니다!");
    }
    
    public void SetCurrentDeck(List<int> ids)
    {
        currentDeckIds = new List<int>(ids); // 리스트 깊은 복사
        Debug.Log($"[Database] 현재 덱 저장 완료: {currentDeckIds.Count}장");
    }
    
    public List<int> GetCurrentDeck()
    {
        return currentDeckIds;
    }
    
    public CardObject GetCardById(int id)
    {
        if (cardDictionary.TryGetValue(id, out CardObject obj))
        {
            return obj;
        }
        Debug.LogWarning($"ID가 {id}인 카드를 찾을 수 없습니다.");
        return null;
    }

    public List<CardObject> GetAllCards()
    {
        return new List<CardObject>(cardDictionary.Values);
    }
    
    // 테스트용
    private void Start()
    {
        
    }
}
