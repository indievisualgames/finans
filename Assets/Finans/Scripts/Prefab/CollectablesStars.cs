using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CollectablesStars : MonoBehaviour
{
    public float speed = 1f;
    bool moveCoin;
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("toStars");

        moveCoin = true;
        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
            moveCoin = true;
            Destroy(gameObject, 2f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (moveCoin)
        {
            transform.position = Vector3.Lerp(transform.position, target.transform.position, speed * Time.deltaTime);
        }
    }
}
