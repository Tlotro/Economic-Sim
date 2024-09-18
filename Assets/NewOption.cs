using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewOption : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_InputField input;

    [SerializeField]
    private TMPro.TMP_Dropdown dropdown;

    void Start()
    {
        input.onSubmit.AddListener(AddOption);
    }

    public void AddOption(string msg)
    {
        dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(msg));
        dropdown.value = dropdown.options.Count - 1;
        dropdown.RefreshShownValue();
        dropdown.Hide();
    }

    public void Click()
    {
        AddOption(input.text);
    }    
}
