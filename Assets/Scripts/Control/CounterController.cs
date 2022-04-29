using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CounterController : MonoBehaviour
{
    public Text text;
    public bool percent;
    public int value;
    [Space()]
    public Renderer indicator;
    [ColorUsage(true, true)]
    public Color offEmissionColor;
    [ColorUsage(true, true)]
    public Color onEmissionColor;
    public float blinkThreshold;
    public float minBlinkSpeed;

    private bool indicatorIsOn;
    private float swapDelay;
    private MaterialPropertyBlock block;

    // Start is called before the first frame update
    void Start()
    {
        SetValue(value);

        if (indicator)
        {
            block = new MaterialPropertyBlock();

            indicatorIsOn = false;
            SetIndicatorEmission();
        }

    }

    void Update()
    {
        if (indicator)
        {
            if (value > 0 && value <= blinkThreshold)
            {
                if (swapDelay > 0)
                {
                    // Reduce delay
                    swapDelay -= Time.deltaTime;
                }
                else
                {
                    // Toggle indicator
                    indicatorIsOn = !indicatorIsOn;
                    Debug.Log("Toggling, " + indicatorIsOn);

                    SetIndicatorEmission();
                    // Set new delay
                    swapDelay = Mathf.Lerp(0, minBlinkSpeed, value / blinkThreshold);
                }
            }
            else if (value <= 0 && !indicatorIsOn)
            {
                indicatorIsOn = true;
                SetIndicatorEmission();
            }
            else if (value > blinkThreshold && indicatorIsOn)
            {
                indicatorIsOn = false;
                SetIndicatorEmission();
            }
        }
    }

    private void SetIndicatorEmission()
    {
        indicator.GetPropertyBlock(block);
        block.SetColor("_EmissionColor", indicatorIsOn ? onEmissionColor : offEmissionColor);
        indicator.SetPropertyBlock(block);
    }

    public void SetValue(int value)
    {
        if (value > 999)
            text.text = "999" + (percent ? "%" : "");
        else if (value > 99)
            text.text = value.ToString() + (percent ? "%" : "");
        else if (value > 9)
            text.text = "!" + value.ToString() + (percent ? "%" : "");
        else if (value >= 0)
            text.text = "!!" + value.ToString() + (percent ? "%" : "");
        else if (value > -10)
            text.text = "!" + value.ToString() + (percent ? "%" : "");
        else if (value > -100)
            text.text = value.ToString() + (percent ? "%" : "");
        else
            text.text = "-99" + (percent ? "%" : "");

        this.value = value;
    }
}
