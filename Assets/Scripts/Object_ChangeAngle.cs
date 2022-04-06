using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Object_ChangeAngle : MonoBehaviour
{
    [Space(10)]
    public Sprite[] objectSprites;

    [Space(10)]
    public SpriteRenderer spriteRenderer;
    Camera cam;

    private void OnEnable()
    {
        int rand = Random.Range(0, objectSprites.Length);

        spriteRenderer.sprite = objectSprites[rand];
    }

    /*
    void getAngle()
    {
        if (Mathf.Abs(transform.position.x - cam.transform.position.x) < changePerspectiveOffset)
            spriteRenderer.sprite = _forward;
        else if (cam.transform.position.x < transform.position.x)
            spriteRenderer.sprite = _left;
        else spriteRenderer.sprite = _right;
    }*/
}
