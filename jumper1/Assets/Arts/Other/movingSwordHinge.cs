using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movingSwordHinge : MonoBehaviour
{
    public HingeJoint2D turningSword;
    public float maxRange = 2.0f;
    public float pushPower;
    // Start is called before the first frame update
    void Start()
    {
        
    }

  

    private void FixedUpdate()
    {
        float depth = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 center =
            new Vector3(Screen.width / 2, Screen.height / 2, depth);
        Vector3 mouse =
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth);

        // Transform to world space
        center = Camera.main.ScreenToWorldPoint(center);
        mouse = Camera.main.ScreenToWorldPoint(mouse);
        Vector3 mouseVec = Vector3.ClampMagnitude(mouse - center, maxRange);
      
       //urningSword.


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Untagged"))
        {
            turningSword.useMotor = false;
            Debug.Log("hit");
        }
    }


}
