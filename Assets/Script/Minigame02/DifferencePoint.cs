using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifferencePoint : MonoBehaviour
{
    private Button button;
    private Image buttonImage;
    public Color clickedColor = Color.red;
    private bool isClicked = false;

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        SpotDifference spotDifference = FindObjectOfType<SpotDifference>();
        if (!spotDifference.IsGameActive() || isClicked) return;

        isClicked = true;
        buttonImage.color = clickedColor;

        Debug.Log("Point clicked!");
        spotDifference.OnDifferenceFound(button);
    }

    public void ResetPoint()
    {
        isClicked = false;
    }
}
