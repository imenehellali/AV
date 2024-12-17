using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSetting : MonoBehaviour
{
    public Color _selectionColor;
    [SerializeField]
    private Image image;
    private Color _origColor;
    public string levelName;
    public float levelDuration=300f;
    public int levelOrder=0;
    public bool levelSelected=false;

    private void Start()
    {
        _origColor= image.color;
    }
    public void SetLevelDuration(int duration)
    {
        levelDuration = 60f * duration;
    }
    public void SetLevelOrder(int order)
    {
        levelOrder = order;
    }
    public void SelectLevel()
    {
        if (levelSelected)
        {
            levelSelected = !levelSelected;
            image.color = _origColor;
        }
        else
        {
            levelSelected = !levelSelected;
            image.color = _selectionColor;
        }

        
    } 
    

}
