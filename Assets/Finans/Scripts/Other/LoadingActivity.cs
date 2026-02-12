using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadingActivity : MonoBehaviour
{
    //.wp-block-code{border:0;padding:0;-webkit-text-size-adjust:100%;text-size-adjust:100%}.wp-block-code>span{display:block;overflow:auto}.shcb-language{border:0;clip:rect(1px,1px,1px,1px);-webkit-clip-path:inset(50%);clip-path:inset(50%);height:1px;margin:-1px;overflow:hidden;padding:0;position:absolute;width:1px;word-wrap:normal;word-break:normal}.hljs{box-sizing:border-box}.hljs.shcb-code-table{display:table;width:100%}.hljs.shcb-code-table>.shcb-loc{color:inherit;display:table-row;width:100%}.hljs.shcb-code-table .shcb-loc>span{display:table-cell}.wp-block-code code.hljs:not(.shcb-wrap-lines){white-space:pre}.wp-block-code code.hljs.shcb-wrap-lines{white-space:pre-wrap}.hljs.shcb-line-numbers{border-spacing:0;counter-reset:line}.hljs.shcb-line-numbers>.shcb-loc{counter-increment:line}.hljs.shcb-line-numbers .shcb-loc>span{padding-left:.75em}.hljs.shcb-line-numbers .shcb-loc:before{border-right:1px solid #ddd;content:counter(line);display:table-cell;padding:0 .75em;text-align:right;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;white-space:nowrap;width:1%}using System.Collections;
    private RectTransform rectComponent;
    private float rotateSpeed = 200f;
    public void LoadLevel(string levelName)
    {
        //StartCoroutine(LoadSceneAsync(levelName));
    }
    IEnumerator LoadSceneAsync(string levelName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(levelName);
        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / .9f);
            Debug.Log(op.progress);
            yield return null;
        }
    }




    private void Start()
    {
        rectComponent = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
