using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour
{
    public GameObject CurrentTarget;
    public float direction = 0.0f;
    public float elevation = 0.0f;
    public float distance = 10.0f;

    public int mainMoveButton = 0;
    public int mainCancelButton = 1;

    public void Update()
    {
        ManageMouseEvents();
        if(!changingTarget)
            UpdateCameraPosition(CurrentTarget != null ? CurrentTarget.transform.position : Vector3.zero);
    }

    public void SetNewTarget(GameObject newTarget)
    {
        if (!changingTarget && CurrentTarget != newTarget)
            StartCoroutine(MoveToNewTarget(newTarget));
    }

    protected bool changingTarget;

    protected IEnumerator MoveToNewTarget(GameObject newTarget)
    {
        changingTarget = true;
        
        //On anime le changement de point de vue (lookAt vers la nouvelle cible) : on se considère comme "local caméra"
        Vector3 current = CurrentTarget != null ? CurrentTarget.transform.position : Vector3.zero;
        
        float animLength = 0.0f;
        while(animLength <= 1.0f)
        {
            Vector3 delta = newTarget.transform.position - current; //On recalcule le delta à chaque frame car il peut potentiellement bouger

            transform.LookAt(current + delta * animLength);
            animLength += Time.deltaTime;
            yield return null;
        }

        //On inscrit la modification en coordonnées polaires : on se considère comme "local cible"
        CurrentTarget = newTarget;

        Utils.CartesianToPolar(transform.position - CurrentTarget.transform.position, out direction, out elevation, out distance);
        direction *= Mathf.Rad2Deg;
        elevation *= Mathf.Rad2Deg;

        changingTarget = false;
    }

    protected bool dragging = false;
    protected Vector3 startDragPosition = Vector3.zero;

    protected float startDirection = 0.0f;
    protected float startElevation = 0.0f;

    protected void ManageMouseEvents()
    {
        if (changingTarget)
            return;

        if(dragging)
        {
            if (Input.GetMouseButtonUp(mainCancelButton))
            {
                dragging = false;
                direction = startDirection;
                elevation = startElevation;
            }
            else
            {
                Vector3 deltaMove = Input.mousePosition - startDragPosition;

                direction = startDirection + deltaMove.x * 0.2f;
                elevation = startElevation + deltaMove.y * 0.1f;

                if (Input.GetMouseButtonUp(mainMoveButton))
                    dragging = false;
            }
        }
        else
        {
            if(Input.GetMouseButtonDown(mainMoveButton))
            {
                dragging = true;
                startDragPosition = Input.mousePosition;
                startDirection = direction;
                startElevation = elevation;
            }
        }

        float scroll = Input.mouseScrollDelta.y;
        distance -= scroll * distance * 0.1f;
    }

    protected void UpdateCameraPosition(Vector3 target)
    {
        transform.position = 
            target +
            Utils.PolarToCartesian(direction * Mathf.Deg2Rad, elevation * Mathf.Deg2Rad, distance);

        transform.LookAt(target);
    }
}

