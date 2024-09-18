using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCircleFade : MonoBehaviour
{
    private SpriteRenderer rend;
    private float fadeTime;
    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        rend.color = new Color(1,1,1,0f);
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeTime > 0)
        {
            fadeTime -= Time.deltaTime;
        }
        else if (rend.color.a > 0f)
        {
            rend.color = new Color(1, 1, 1, Mathf.Clamp01(rend.color.a-3*Time.deltaTime));
        }
        else
            enabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay">delay in milliseconds before the circle starts to fade</param>
    public void Appear(float delay)
    {
        enabled = true;
        rend.color = new Color(1,1,1,1f);
        fadeTime = delay;
    }
}
