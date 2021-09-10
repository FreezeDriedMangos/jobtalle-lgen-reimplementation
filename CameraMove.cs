using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

        /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/
     
     
    float mainSpeed = 10.0f; //regular speed
    float shiftAdd = 5.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 20.0f; //Maximum speed when holdin gshift
    float camSens = 0.25f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;
    private bool cursorLocked = false; 

    void Update () {
        lastMouse = Input.mousePosition - lastMouse ;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0 );
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0);
        if (!cursorLocked) transform.eulerAngles = lastMouse;
        lastMouse =  Input.mousePosition;
        //Mouse  camera angle done.  
       
        //Keyboard commands
        float f = 0.0f;
        Vector3 p = GetBaseInput();
        if (Input.GetKey (KeyCode.CapsLock)){
            totalRun += Time.deltaTime;
            p  = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else{
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1, 1000);
            p = p * mainSpeed;
        }
       

        float dy = p.y;
        p = Quaternion.Euler(0, transform.eulerAngles.y, 0) * p;

        p = p * Time.deltaTime;
        f = transform.position.y;
        transform.position += p;
        transform.position = new Vector3(transform.position.x, f+p.y, transform.position.z); //.y = f;
        

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            cursorLocked = !cursorLocked;
            //Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None; 
        }
    }
     
    private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = Vector3.zero;
        if (Input.GetKey (KeyCode.W)){
            p_Velocity += new Vector3(0, 0 , 1);
        }
        if (Input.GetKey (KeyCode.S)){
            p_Velocity += new Vector3(0, 0 , -1);
        }
        if (Input.GetKey (KeyCode.A)){
            p_Velocity += new Vector3(-1, 0 , 0);
        }
        if (Input.GetKey (KeyCode.D)){
            p_Velocity += new Vector3(1, 0 , 0);
        }
        
        if (Input.GetKey (KeyCode.Space)){
            p_Velocity += new Vector3(0, 1 , 0);
        }
        if (Input.GetKey (KeyCode.LeftShift)){
            p_Velocity += new Vector3(0, -1, 0);
        }
        return p_Velocity;
    }

}
