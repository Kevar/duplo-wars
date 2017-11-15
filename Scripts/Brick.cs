using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Brick : MonoBehaviour
{
    public List<Vector3> TopAnchors = new List<Vector3>();
    public List<Vector3> BottomAnchors = new List<Vector3>();
    
    //protected Collider my_Collider;

    //protected Camera my_Camera;
    //protected Plane my_MovePlane = new Plane();
    //protected bool my_Moving = false;
    //protected Vector3 my_DeltaPosition = Vector3.zero;

    private void Start()
    {
        if (TopAnchors.Count == 0)
        {
            TopAnchors.Add(new Vector3(0.25f, 0.5f, 0.25f));
            TopAnchors.Add(new Vector3(0.25f, 0.5f, -0.25f));
            TopAnchors.Add(new Vector3(-0.25f, 0.5f, -0.25f));
            TopAnchors.Add(new Vector3(-0.25f, 0.5f, 0.25f));

        }
        if (BottomAnchors.Count == 0)
        {
            BottomAnchors.Add(new Vector3(0.25f, -0.5f, 0.25f));
            BottomAnchors.Add(new Vector3(0.25f, -0.5f, -0.25f));
            BottomAnchors.Add(new Vector3(-0.25f, -0.5f, -0.25f));
            BottomAnchors.Add(new Vector3(-0.25f, -0.5f, 0.25f));
        }
        //my_Camera = Camera.main;
        //my_Collider = GetComponent<Collider>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 top in TopAnchors)
            Gizmos.DrawSphere(transform.TransformPoint(top), 0.1f);

        Gizmos.color = Color.blue;
        foreach (Vector3 bottom in BottomAnchors)
            Gizmos.DrawCube(transform.TransformPoint(bottom), Vector3.one * 0.1f);
    }

    //private void OnMouseDown()
    //{
    //    Ray r = my_Camera.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit hit;

    //    if (my_Collider.Raycast(r, out hit, float.MaxValue))
    //    {
    //        my_Moving = true;

    //        my_DeltaPosition = transform.position - hit.point;
    //        my_MovePlane = new Plane(r.direction, hit.point);
    //    }
    //}

    //private void OnMouseUp()
    //{
    //    my_Moving = false;
    //}

    //private void FixedUpdate()
    //{
    //    if(my_Moving)
    //    {
    //        Ray r = my_Camera.ScreenPointToRay(Input.mousePosition);
    //        float distance;

    //        if(my_MovePlane.Raycast(r, out distance))
    //            transform.position = r.GetPoint(distance) + my_DeltaPosition;

    //        if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
    //        {
    //            Brick bestCandidate = null;
    //            float bestDistance = float.MaxValue;

    //            foreach(Brick candidate in FindObjectsOfType<Brick>())
    //            {
    //                if(candidate != this)
    //                {
    //                    Vector3 sPos = my_Camera.WorldToScreenPoint(candidate.transform.position);

    //                    sPos.z = 0.0f;  //Pour se "coller" à la caméra et ainsi être dans les mêmes conditions que la souris

    //                    float bDistance = (sPos - Input.mousePosition).magnitude;
    //                    if(bestDistance > bDistance)
    //                    {
    //                        bestDistance = bDistance;
    //                        bestCandidate = candidate;
    //                    }
    //                }
    //            }

    //            if (bestCandidate != null && bestDistance < 20)
    //                transform.position = bestCandidate.transform.position;
    //        }
    //    }
    //}
}
