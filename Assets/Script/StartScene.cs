using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScene : MonoBehaviour
{
    public AudioClip startSceneBGM;

    void Start()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ChangeBGM(startSceneBGM);
        }
    }
}
