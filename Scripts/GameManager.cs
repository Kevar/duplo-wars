using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    protected class MagnetData
    {
        public Brick Target;

        public Vector3 TargetAnchor;
        public Vector3 SelfAnchor;

        public MagnetData(Brick target, Vector3 targetAnchor, Vector3 selfAnchor)
        {
            Target = target;
            TargetAnchor = targetAnchor;
            SelfAnchor = selfAnchor;
        }
    }

    protected List<Brick> my_BrickList = null;
    protected Brick my_SelectedBrick = null;

    protected Camera my_Camera;

    protected bool my_MouseDown;
    protected Vector3 my_MousePreviousPosition;

    protected bool my_MovingBrick = false;
    protected Plane my_BrickMovePlane = new Plane();
    protected Vector3 my_BrickDeltaPosition = Vector3.zero;
    protected Brick my_ConnectedBrickCandidate = null;

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
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                DisconnectSelectedBrick();

            if (my_MovingBrick)
            {
                Ray r = my_Camera.ScreenPointToRay(Input.mousePosition);
                float distance;

                if (my_BrickMovePlane.Raycast(r, out distance))
                    my_SelectedBrick.transform.position = r.GetPoint(distance) + my_BrickDeltaPosition;

                //On gère le magnétisme sur ancres
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    //On récupère les paires d'ancre top/bottom qui sont visuellement (par rapport à la caméra) assez proches
                    List<MagnetData> anchorPairsInThreshold = new List<MagnetData>();

                    float threshold = 20;
                    
                    foreach(Brick target in my_BrickList)
                    {
                        if(target != my_SelectedBrick && !my_SelectedBrick.ConnectedBrickMap.ContainsKey(target))
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
                                        anchorPairsInThreshold.Add(new MagnetData(target, worldBottomAnchor, worldTopAnchor));
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
                                        anchorPairsInThreshold.Add(new MagnetData(target, worldTopAnchor, worldBottomAnchor));
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
                        Brick targetBestBrick = null;

                        float bestScreenDistance = float.MaxValue;

                        foreach(MagnetData anchorPair in anchorPairsInThreshold)
                        {
                            float screenDistance1 = my_Camera.WorldToScreenPoint(anchorPair.SelfAnchor).z;
                            float screenDistance2 = my_Camera.WorldToScreenPoint(anchorPair.TargetAnchor).z;

                            float screenDistance = Mathf.Min(screenDistance1, screenDistance2);

                            if(bestScreenDistance > screenDistance)
                            {
                                bestScreenDistance = screenDistance;

                                selectedBestAnchor = anchorPair.SelfAnchor;
                                targetBestAnchor = anchorPair.TargetAnchor;
                                targetBestBrick = anchorPair.Target;
                            }
                        }

                        //On déplace la brique sélectionnée pour correspondre à l'ancrage : ça revient à faire correspondre son ancre avec celle de l'ancre cible
                        Vector3 selectedBrickGlobalDeltaWithSelfAnchor = selectedBestAnchor - my_SelectedBrick.transform.position;
                        my_SelectedBrick.transform.position = targetBestAnchor - selectedBrickGlobalDeltaWithSelfAnchor;

                        my_ConnectedBrickCandidate = targetBestBrick;
                    }
                    else
                    {
                        //Il n'y a pas de brique à potentiellement connecter
                        my_ConnectedBrickCandidate = null;
                    }
                }
                else
                {
                    //On "lâche" le magnétisme, on ne considère donc plus qu'il y a connexion avec une autre brique
                    my_ConnectedBrickCandidate = null;
                }

                //On gère le mécanisme de connexion
                my_SelectedBrick.ApplyConnexionConstraints();
            }

            my_MousePreviousPosition = Input.mousePosition;
        }
    }

    private void ManageMouseUp()
    {
        my_MouseDown = false;
        my_MovingBrick = false;

        if (my_ConnectedBrickCandidate != null)
        {
            my_SelectedBrick.ConnectedBrickMap.Add(my_ConnectedBrickCandidate, my_ConnectedBrickCandidate.transform.position - my_SelectedBrick.transform.position);
            my_ConnectedBrickCandidate.ConnectedBrickMap.Add(my_SelectedBrick, my_SelectedBrick.transform.position - my_ConnectedBrickCandidate.transform.position);

            my_ConnectedBrickCandidate = null;
        }

    }

    private void DisconnectSelectedBrick()
    {
        if (my_SelectedBrick != null)
        {
            foreach (Brick cb in new List<Brick>(my_SelectedBrick.ConnectedBrickMap.Keys))
            {
                my_SelectedBrick.ConnectedBrickMap.Remove(cb);
                cb.ConnectedBrickMap.Remove(my_SelectedBrick);
            }
        }
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
