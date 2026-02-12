using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GalleryCard : MonoBehaviour
{
    [SerializeField] public Image CardImg;


    public string Title
    {
        get { return CardTitle.text; }
        set { CardTitle.text = value; }
    }
    public string SubTitle
    {
        get { return CardSubTitle.text; }
        set { CardSubTitle.text = value; }
    }
    public string Description
    {
        get { return CardDescription.text; }
        set { CardDescription.text = value; }
    }

    [SerializeField] TMP_Text CardTitle;
    [SerializeField] TMP_Text CardSubTitle;
    [SerializeField] TMP_Text CardDescription;

    void Start()
    {

    }

    void Update()
    {

    }
}
