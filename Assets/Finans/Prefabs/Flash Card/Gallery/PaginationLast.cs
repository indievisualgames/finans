using System.Collections;
using System.Collections.Generic;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

public class PaginationLast : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PagedRect _pagedRect = transform.parent.parent.GetComponent<PagedRect>();
        Debug.Log($"I found the parent named {transform.parent.parent.name}");
        GetComponent<Button>().onClick.AddListener(delegate () { _pagedRect.ShowLastPage(); });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
