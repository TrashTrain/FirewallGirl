using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class ChatMgr : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject ChatGameObject;
    public GameObject NameGameObject;
    public Image chatBox;
    public Image nameBox;
    public TextMeshProUGUI chatText;
    public TextMeshProUGUI nameText;

    [Header("CSV Reader")]
    public string csv_FileName;

    [Header("NextScene")]
    public string nextScene;

    int ScriptCount = 0;
    List<Dictionary<string, object>> chatData;

    void Start()
    {
        chatData = CSVReader.Read(csv_FileName);
        nameText.text = chatData[ScriptCount].ContainsKey("Name_Character") ? chatData[ScriptCount]["Name_Character"].ToString() : "";
        chatText.text = chatData[ScriptCount].ContainsKey("Dialog") ? chatData[ScriptCount]["Dialog"].ToString() : "";

        if (chatData == null || chatData.Count == 0)
        {
            Debug.LogError("CSV 파일을 읽을 수 없습니다");
            return;
        }

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void DisplayNextSentence()
    {

        // 대화 데이터가 더 이상 없는경우 EndDialogue
        if (ScriptCount >= chatData.Count)
        {
            EndDialogue();
            return;
        }

        var row = chatData[ScriptCount];

        nameText.text = row["Name_Character"].ToString();
        chatText.text = row["Dialog"].ToString();

        // 캐릭터이름이 null인 경우
        if (row["Name_Character"] == null)
        {
            nameText.text = "";

        }

        ScriptCount++;
    }

    void EndDialogue()
    {
        ChatGameObject.SetActive(false);
        NameGameObject.SetActive(false);
        SceneManager.LoadScene(nextScene);
        Debug.Log("대화 종료");
    }
}