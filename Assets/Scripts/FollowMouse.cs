using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FollowMouse : MonoBehaviour
{
    public Rigidbody2D rb2d;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pointPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pointPos.z = 0.0f;

        rb2d.MovePosition(pointPos);
    }

    private void Update()
    {

    }

}
