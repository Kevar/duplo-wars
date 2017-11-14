using UnityEngine;
using System.Collections;

public class CameraTargetBehaviour : MonoBehaviour
{
    protected CameraBehaviour Cam;
    protected bool my_FirstClicked = false;
    protected float my_FirstClickTime = 0.0f;

	private void Start ()
    {
        Cam = FindObjectOfType<CameraBehaviour>();
	}

    private void OnMouseDown()
    {
        if (!my_FirstClicked)
        {
            my_FirstClicked = true;
            my_FirstClickTime = Time.time;
        }
        else
        {
            if (Time.time - my_FirstClickTime < 0.5f)
            {
                if (Cam != null)
                    Cam.SetNewTarget(gameObject);
            }

            my_FirstClicked = false;
        }
    }
}
