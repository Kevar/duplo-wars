using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BrickShapeDataModel
{
    public string Name;

    public List<Vector3> MaleAnchorList = new List<Vector3>();
    public List<Vector3> FemaleAnchorList = new List<Vector3>();

    public GameObject ShapePrefab;
}

[Serializable]
public class BrickDataModel
{
    //On met cette donnée ici plutôt que dans ShipDataModel ou SceneModel. Ce serait plus cohérent, mais nous obligerait à utiliser un dictionnaire, qui n'est pas
    //sérialisé de manière automatique par Unity. On pourrait développer une sérialisation custom mais je préfère rester simple pour le moment
    public Vector3 Position = Vector3.zero;

    public string ShapeModel = "";

    public Color MainColor = Color.white;
}

[Serializable]
public class ShipDataModel
{
    public Vector3 Position;

    //On ne sauvegarde pas d'information de connexion entre les briques : ce sera déduit de manière implicite par la proximité des ancres mâles et femelles
    public List<BrickDataModel> BrickList = new List<BrickDataModel>();
}

//On rend cette classe ScriptableObject car c'est une donnée qu'on va sérialiser explicitement sur le disque
[CreateAssetMenu(fileName = "newScene", menuName = "New DW Scene")]
public class SceneModel : ScriptableObject
{
    //En fait, on n'aura jamais de brique indépendante dans les données car n'importe quelle brique est la base d'un vaisseau en puissance
    public List<ShipDataModel> ShipList = new List<ShipDataModel>();
}

[CreateAssetMenu(fileName = "newBrickDataBase", menuName = "New DW Brick data base")]
public class BrickShapeDataBaseModel : ScriptableObject
{
    public List<BrickShapeDataModel> ShapeList = new List<BrickShapeDataModel>();
}

public class Model : MonoBehaviour
{
    public BrickShapeDataBaseModel ShapeDataBase;
    public SceneModel Scene;

    //Appelé à l'activation du MonoBehaviour ET dès qu'une variable de la classe est changé via l'inspector. Permet de mettre à jour la logique MVC
    private void OnValidate()
    {
        //Notifier le contrôleur que les données ont changé et donc qu'il faut reconstruire la vue
    }
}
