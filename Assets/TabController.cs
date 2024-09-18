using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabController : MonoBehaviour
{
    public int CurrentTab;
    public GameObject ResourcesContent;
    public GameObject RecipesContent;
    public bool Open;
    public UnityEngine.UI.Button ResourceTabButton;
    public UnityEngine.UI.Button RecipesTabButton;
    Animator animator;

    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void CloseTab()
    {
        animator.SetBool("Resources", false);
        animator.SetBool("Recipes", false);
        //Add the code for animating the closing
        //Sidebar.transform.localPosition = Vector3.zero;
        Open = false;
        CurrentTab = 0;
    }

    public void OnTabClose()
    {
        RecipesContent.SetActive(false);
        ResourcesContent.SetActive(false);
    }

    public void OnResourcesOpen()
    {
        ResourcesContent.SetActive(true);
    }

    public void OnRecipesOpen()
    {
        RecipesContent.SetActive(true);
    }

    public void OpenResourceTab()
    {
        if (Open && CurrentTab == 1) CloseTab();
        else
        {
            if (Open) CloseTab();
            animator.SetBool("Resources", true);
            CurrentTab = 1;
            Open = true;
        }
    }

    public void OpenRecipesTab()
    {
        if (Open && CurrentTab == 2) CloseTab();
        else
        {
            if (Open) CloseTab();
            animator.SetBool("Recipes", true);
            CurrentTab = 2;
            Open = true;
        }
    }
}
