using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageShieldScript : MonoBehaviour
{
    [SerializeField] private GameObject magicPlane;

    public void BlockMagic()
    {
        StartCoroutine(BlockMagicRoutine());
    }

    private IEnumerator BlockMagicRoutine() 
    { 
        float timeElapsed = 0;

        var plane = Instantiate(magicPlane, transform.position, Quaternion.identity);
        plane.transform.position += new Vector3(0f, 0.2f, 0f);
        
        while (timeElapsed < 0.5f)
        {
            Renderer[] rs = plane.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
            {
                float x = Mathf.Lerp(0, 1, timeElapsed / 0.5f);
                r.material.SetFloat("_Fllipbook_Opacity", x);
                //Debug.Log(r.material.GetFloat("_Fllipbook_Opacity"));
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        timeElapsed = 0;

        while (timeElapsed < 0.5f)
        {
            Renderer[] rs = plane.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
            {
                float x = Mathf.Lerp(1, 0, timeElapsed / 0.5f);
                r.material.SetFloat("_Fllipbook_Opacity", x);
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(plane);
    }
}
