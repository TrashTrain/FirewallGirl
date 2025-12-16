using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    // 스테이지 씬 
    public static void LoadStageScene()
    {
        SceneManager.LoadScene("StageScene");
    }

    // 증감 카드 선택씬 
    public static void LoadUpDownCardScene()
    {
        SceneManager.LoadScene("UpDownSysScene");
    }

    // 배틀씬 
    public static void LoadBattleScene()
    {
        SceneManager.LoadScene("IntegratedScene"); 
    }
}
