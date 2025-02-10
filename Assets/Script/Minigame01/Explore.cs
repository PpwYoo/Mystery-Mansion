using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explore : MonoBehaviour
{
    public int questionIndex;
    public GameObject magnifyingGlassIcon;

    public void Onclick()
    {
        FindObjectOfType<Fingerprint>().ShowQuestion(questionIndex);
        magnifyingGlassIcon.SetActive(false);
    }
}
