using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camerarotate : MonoBehaviour {
    public float speed;
    private double startx;
    private double starty;

    public float zoomspeed;


    // Update is called once per frame
    void LateUpdate () {
        if (Input.GetMouseButtonDown(1))
        {
            startx = (Input.mousePosition.x / Screen.width) - 0.5;
            starty = (Input.mousePosition.y / Screen.height) - 0.5;
        }
        if (Input.GetMouseButton(1))
        {
            double changex = ((Input.mousePosition.x / Screen.width) - 0.5 - startx) * 100;
            double changey = ((Input.mousePosition.y / Screen.height) - 0.5 - starty) * 100;

            transform.RotateAround(new Vector3(150/2, 0, 150/2), Vector3.up, (float)(changex * speed) * Time.deltaTime);
            transform.RotateAround(new Vector3(150/2, 0, 150/2), transform.right, (float)(changey * speed) * Time.deltaTime * -1);
        }

        float zoom = Input.GetAxis("Mouse ScrollWheel") * zoomspeed;
        transform.Translate(Vector3.forward * zoom * Time.deltaTime, Space.Self);
    }
}
