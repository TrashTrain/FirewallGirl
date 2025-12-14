using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public bool PlayerTurn = false;
    static public GameManager Instance = null;

    public int enemyCount = 0;

   // private bool checkTurn;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        PlayerTurn = true;
        //checkTurn = PlayerTurn;
    }

    public void OnTrunButtonClick()
    {
        // true -> PlayerTurn으로 바꾸기
        if (PlayerTurn)
        {
            Debug.Log("턴 넘기기 성공");
            PlayerTurn = !PlayerTurn;
            SequenceTurn.instance.SetResetSequenceCheck();
        }
    }

    // 게임 오버 -> 스테이지 씬 
    public void GameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(3f);
        SceneLoader.LoadStageScene();
    }

    // 게임 클리어 -> 카드 증감 씬
    public void GameClear()
    {
        StartCoroutine(GameClearSequence());
    }

    private IEnumerator GameClearSequence()
    {
        yield return new WaitForSeconds(0.1f);
        SceneLoader.LoadUpDownCardScene();
    }
}

// 바이러스 SO 데이터 모음
public class VirusSOData
{
    public List<VirusObjectSO> virusObjectList;

}
