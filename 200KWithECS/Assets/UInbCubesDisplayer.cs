using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UInbCubesDisplayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        m_text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayText(string _text)
    {
        m_text.SetText(_text);
    }

    private TMP_Text m_text;

}
