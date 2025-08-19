using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressButton : MonoBehaviour
{
    public Animator[] animator;
    public GameObject pressButtonText;

    [Header("ButtonComponent")]
    public GameObject buttonPanel;
    public Image[] buttons;

    public void OnPressButtonClick()
    {
        for (int i = 0; i < animator.Length; i++)
        {
            animator[i].SetBool("isButton", true);
        }
        
        pressButtonText.SetActive(false);
        buttonPanel.SetActive(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            
        }
    }
}
