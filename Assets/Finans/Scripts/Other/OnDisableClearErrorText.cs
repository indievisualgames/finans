
using TMPro;
using UnityEngine;

public class OnDisableClearErrorText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void OnDisable()
    {
        GetComponent<TMP_Text>().text = "";
    }
}
