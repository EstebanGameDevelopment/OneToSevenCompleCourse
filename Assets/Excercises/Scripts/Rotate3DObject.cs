using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate3DObject : MonoBehaviour
{
    private float m_rotationSpeed = 25;
    public float MovementSpeed = 1;

    public Vector3 ShiftTarget = Vector3.zero;
    public float TimeToTarget = 5;

    private bool m_direction = true;

    private int m_state = 0;

    public float RotationSpeed
    {
        get { return m_rotationSpeed; }
        set {
            if ((value > 0) && (value < 90))
            {
                m_rotationSpeed = value;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // iTween.MoveTo(this.gameObject, iTween.Hash("position", this.transform.position + ShiftTarget, "easetype", iTween.EaseType.easeOutBack, "time", TimeToTarget));
    }

    // Update is called once per frame
    void Update()
    {
        // this.transform.Rotate(new Vector3(0, m_rotationSpeed * Time.deltaTime, 0));
        /*
         this.transform.position += new Vector3(0, MovementSpeed * Time.deltaTime, 0);
         */

        /*
        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            m_direction = !m_direction;
        }
        */

        /*
        if (Input.GetKey(KeyCode.Space) == true)
        {
            m_direction = true;
        }
        else
        {
            m_direction = false;
        }
        */

        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            m_state = ((m_state + 1) % 4);
        }

        switch (m_state)
        {
            case 0:
                this.transform.Rotate(new Vector3(0, m_rotationSpeed * Time.deltaTime, 0));
                break;
            case 1:
                this.transform.Rotate(new Vector3(0, -m_rotationSpeed * Time.deltaTime, 0));
                break;
            case 2:
                this.transform.Rotate(new Vector3(0, 4 * m_rotationSpeed * Time.deltaTime, 0));
                break;
            case 3:
                this.transform.Rotate(new Vector3(4 * m_rotationSpeed * Time.deltaTime, 0, 0));
                break;
        }

        /*
        if (m_direction == true)
        {
            this.transform.Rotate(new Vector3(0, RotationSpeed * Time.deltaTime, 0));
        }
        else
        {
            this.transform.Rotate(new Vector3(0, -RotationSpeed * Time.deltaTime, 0));
        }
        */
    }
}
