using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadingText : MonoBehaviour
{
    // Start is called before the first frame update
    private Text textElement;
    void Start()
    {
        textElement = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(textElement.color.a != 0)
        {
            textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.g, Mathf.Lerp(textElement.color.a, 0.0f, .02f));
        }
    }
}
