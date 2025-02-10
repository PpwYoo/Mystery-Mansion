using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionResult : MonoBehaviour
{
    public int totalQuestions;
    public int correctAnswers;

    public bool IsMissionSuccess()
    {
        return correctAnswers >= totalQuestions / 2;
    }

    public void DisplayResult()
    {
        if (IsMissionSuccess())
        {
            Debug.Log("Mission Passed!");
        }
        else
        {
            Debug.Log("Mission Failed!");
        }
    }

}
