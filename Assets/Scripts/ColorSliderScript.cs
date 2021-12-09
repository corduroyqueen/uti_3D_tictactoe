using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSliderScript : MonoBehaviour
{
    /// This script takes the background image of the color slider and spits out the color it's selecting
    private float textureWidth = 2462;
    private Slider colorSlider;
    public Color selectedColor;
    public Texture2D hueTexture;
    public TMPro.TextMeshProUGUI playerText, playerScoreText;

    void Start()
    {
        colorSlider = GetComponent<Slider>();
        colorSlider.value = Random.Range(0.00f, 1.00f);
    }

    void Update()
    {
        Color tempColor = hueTexture.GetPixel((int)(colorSlider.value * textureWidth), 220);
        selectedColor = new Color(tempColor.r, tempColor.g, tempColor.b, 1f);
        playerText.color = selectedColor;
        playerScoreText.color = selectedColor;
    }
}
