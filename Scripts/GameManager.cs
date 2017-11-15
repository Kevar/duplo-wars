using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    protected List<Brick> my_BrickList = null;
    protected Brick my_SelectedBrick = null;

    protected Camera my_Camera;

    protected bool my_MouseDown;
    protected Vector3 my_MousePreviousPosition;

    protected bool my_MovingBrick = false;
    protected Plane my_BrickMovePlane = new Plane();
    protected Vector3 my_BrickDeltaPosition = Vector3.zero;

    private void Awake()
    {
        my_Camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            ManageMouseDown();
        if (Input.GetMouseButtonUp(0))
            ManageMouseUp();
    }

    private void FixedUpdate()
    {
        ManageMouseMove();
    }

    private void ManageMouseDown()
    {
        my_MouseDown = true;
        my_MousePreviousPosition = Input.mousePosition;

        Ray r = my_Camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            Brick candidate = hit.collider.GetComponent<Brick>();

            if (candidate != null)
            {
                SelectBrick(candidate);

                my_MovingBrick = true;
                //On en profite pour mettre à jour la liste des briques dans la scène, au cas où il y aurait eu des créations/suppressions
                my_BrickList = new List<Brick>(FindObjectsOfType<Brick>());

                my_BrickDeltaPosition = my_SelectedBrick.transform.position - hit.point;
                my_BrickMovePlane = new Plane(my_Camera.transform.forward, hit.point);
            }
            else
                UnselectBrick();
        }
        else
            UnselectBrick();
    }

    private void ManageMouseMove()
    {
        if(my_MouseDown)
        {
            if(my_MovingBrick)
            {
                Ray r = my_Camera.ScreenPointToRay(Input.mousePosition);
                float distance;

                if (my_BrickMovePlane.Raycast(r, out distance))
                    my_SelectedBrick.transform.position = r.GetPoint(distance) + my_BrickDeltaPosition;

                //On gère le magnétisme sur ancres
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    //On récupère les paires d'ancre top/bottom qui sont visuellement (par rapport à la caméra) assez proches
                    List<KeyValuePair<Vector3, Vector3>> anchorPairsInThreshold = new List<KeyValuePair<Vector3, Vector3>>();

                    float threshold = 20;
                    
                    foreach(Brick target in my_BrickList)
                    {
                        if(target != my_SelectedBrick)
                        {
                            //On teste les ancres supérieures de la brique sélectionnée avec les ancres inférieures de la brique cible
                            foreach(Vector3 topAnchor in my_SelectedBrick.TopAnchors)
                            {
                                Vector3 worldTopAnchor = my_SelectedBrick.transform.TransformPoint(topAnchor);

                                foreach (Vector3 bottomAnchor in target.BottomAnchors)
                                {
                                    Vector3 worldBottomAnchor = target.transform.TransformPoint(bottomAnchor);

                                    Vector3 topScreenPosition = my_Camera.WorldToScreenPoint(worldTopAnchor);
                                    Vector3 bottomScreenPosition = my_Camera.WorldToScreenPoint(worldBottomAnchor);

                                    //On "écrase" la position contre l'écran
                                    topScreenPosition.z = 0.0f;
                                    bottomScreenPosition.z = 0.0f;

                                    if((topScreenPosition - bottomScreenPosition).magnitude < threshold)
                                        anchorPairsInThreshold.Add(new KeyValuePair<Vector3, Vector3>(worldTopAnchor, worldBottomAnchor));
                                }
                            }

                            //On teste les ancres inférieures de la brique sélectionnée avec les ancres supérieures de la brique cible
                            foreach (Vector3 bottomAnchor in my_SelectedBrick.BottomAnchors)
                            {
                                Vector3 worldBottomAnchor = my_SelectedBrick.transform.TransformPoint(bottomAnchor);

                                foreach (Vector3 topAnchor in target.TopAnchors)
                                {
                                    Vector3 worldTopAnchor = target.transform.TransformPoint(topAnchor);

                                    Vector3 bottomScreenPosition = my_Camera.WorldToScreenPoint(worldBottomAnchor);
                                    Vector3 topScreenPosition = my_Camera.WorldToScreenPoint(worldTopAnchor);

                                    //On "écrase" la position contre l'écran
                                    bottomScreenPosition.z = 0.0f;
                                    topScreenPosition.z = 0.0f;

                                    if ((bottomScreenPosition - topScreenPosition).magnitude < threshold)
                                        anchorPairsInThreshold.Add(new KeyValuePair<Vector3, Vector3>(worldBottomAnchor, worldTopAnchor));
                                }
                            }
                        }
                    }

                    //Si on possède des paires d'ancre, on choisit celle possédant la position la plus proche de la caméra
                    if (anchorPairsInThreshold.Count > 0)
                    {
                        //Valeurs par défaut qui donne un résultat "neutre" si aucune paire n'est trouvée
                        Vector3 selectedBestAnchor = my_SelectedBrick.transform.position;
                        Vector3 targetBestAnchor = my_SelectedBrick.transform.position;

                        float bestScreenDistance = float.MaxValue;

                        foreach(KeyValuePair<Vector3, Vector3> anchorPair in anchorPairsInThreshold)
                        {
                            float screenDistance1 = my_Camera.WorldToScreenPoint(anchorPair.Key).z;
                            float screenDistance2 = my_Camera.WorldToScreenPoint(anchorPair.Value).z;

                            float screenDistance = Mathf.Min(screenDistance1, screenDistance2);

                            if(bestScreenDistance > screenDistance)
                            {
                                bestScreenDistance = screenDistance;

                                selectedBestAnchor = anchorPair.Key;    //Par construction de la liste de paires, la première valeur correspond à l'ancre de la brique sélectionnée
                                targetBestAnchor = anchorPair.Value;
                            }
                        }

                        //On déplace la brique sélectionnée pour correspondre à l'ancrage : ça revient à faire correspondre son ancre avec celle de l'ancre cible
                        Vector3 selectedBrickGlobalDeltaWithSelfAnchor = selectedBestAnchor - my_SelectedBrick.transform.position;
                        my_SelectedBrick.transform.position = targetBestAnchor - selectedBrickGlobalDeltaWithSelfAnchor;
                    }
                }
            }

            my_MousePreviousPosition = Input.mousePosition;
        }
    }

    private void ManageMouseUp()
    {
        my_MouseDown = false;
        my_MovingBrick = false;
    }

    private void SelectBrick(Brick b)
    {
        if (b == my_SelectedBrick)
            return;

        UnselectBrick();

        if(b != null)
            my_SelectedBrick = b;
    }

    private void UnselectBrick()
    {
        my_SelectedBrick = null;
    }

    private void OnDrawGizmos()
    {
        if(my_SelectedBrick != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(my_SelectedBrick.transform.position, Vector3.one * 1.1f);
        }
    }
}
