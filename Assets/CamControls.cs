using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CamControls : MonoBehaviour
{
    public static CamControls instance;

    public Vector3 world_mdouse_pos;
    public bool enable_mouse_movement;

    [SerializeField]
    private TargetCircleFade circle;
    [SerializeField]
    private TMPro.TMP_Text postext;

    private Camera cam;
    private int camspeed;
    private float camsize;
    private float camsizemult;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        world_mdouse_pos = cam.ScreenToWorldPoint(Input.mousePosition);
        camsize = cam.orthographicSize;
        camsizemult = 1;
        if (instance == null)
            instance = this;
        camspeed = 1;
        enable_mouse_movement = true;
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            camsizemult = Mathf.Clamp(camsizemult-Input.mouseScrollDelta.y*0.01f*camspeed, 0.316f, 3.16f);
            cam.orthographicSize = camsize * camsizemult * camsizemult;
            circle.Appear(0.25f);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
            camspeed = 2;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            camspeed = 1;

        if (enable_mouse_movement)
        {
            if (Input.GetMouseButton(2))
            {
                circle.Appear(0.25f);
                transform.position = Vector3.Lerp(transform.position, cam.ScreenToWorldPoint(Input.mousePosition), camspeed * Time.deltaTime * 6);
                postext.text = $"X:{transform.position.x:F2}\nY:{transform.position.y:F2}";
            }
            else
            {
                if (Input.GetMouseButton(1))
                {
                    transform.position += (world_mdouse_pos - cam.ScreenToWorldPoint(Input.mousePosition)) * camspeed;
                    postext.text = $"X:{transform.position.x:F2}\nY:{transform.position.y:F2}";
                }
            }
        }
        world_mdouse_pos = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    private void FixedUpdate()
    {
    }
}
