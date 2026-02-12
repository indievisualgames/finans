
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Points_Score : MonoBehaviour
{
    [SerializeField] TMP_Text xp;
    // [SerializeField] TMP_Text visit;
    [SerializeField] TMP_Text stars;
    // [SerializeField] TMP_Text view;

    [SerializeField] TMP_Text coins;





    // Start is called before the first frame update
    void Start()
    {
        xp.text = PointSystem.XP.ToString();
        //visit.text = PointSystem.Visit.ToString();
        stars.text = PointSystem.Stars.ToString();
        // view.text = PointSystem.View.ToString();
        coins.text = PointSystem.Coins.ToString();
        Debug.Log("Assigned Point System to HUD");
    }
    void Update()
    {
        xp.text = PointSystem.XP.ToString();
        //visit.text = PointSystem.Visit.ToString();
        stars.text = PointSystem.Stars.ToString();
        // view.text = PointSystem.View.ToString();
        coins.text = PointSystem.Coins.ToString();
    }
}