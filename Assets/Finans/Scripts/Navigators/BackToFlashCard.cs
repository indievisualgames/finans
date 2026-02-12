using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToFlashCard : MonoBehaviour
{
    [SerializeField] GameObject gallery;
    [SerializeField] GameObject cards;
    bool waitTimeStarted = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (cards != null)
            {
                if (!waitTimeStarted)
                {
                    waitTimeStarted = true;
                    StartCoroutine(EnableCards());

                }
                return;
            }

        }




    }

    IEnumerator EnableCards()
    {
        yield return new WaitForSeconds(0.5f);
        gallery.SetActive(false);
        cards.SetActive(true);
        waitTimeStarted = false;
    }
}
