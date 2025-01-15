using UnityEngine;
using System.IO;
using Dummiesman;

public static class Utils
{
    public static float distanceFromCamera = 5f;
    // Use this function to instantiate 3DObjects in the scene
    public static void instantiate3DObj(string objPath, string pngPath = null, GameObject camera = null)
    {
        // V�rifier si l'objet existe d�j� dans la sc�ne
        string objectName = Path.GetFileNameWithoutExtension(objPath);
        GameObject existingObj = GameObject.Find(objectName);
        GameObject dummyObj = GameObject.Find(objectName +"Dummy");

        if (existingObj != null)
        {
            Debug.Log("L'objet existe déja dans la scène : " + objectName);
            if (pngPath != null)
            {
                Debug.Log("Adding texture to existing object");
                AddTextureToObject(existingObj, pngPath);
            }
            return;
        }

        if(dummyObj != null)
        {
            GameObject.Destroy(dummyObj);
        }

        var loadedObj = new OBJLoader().Load(objPath);
        if (loadedObj != null)
        {
            Debug.Log(objPath);
            Debug.Log(pngPath);
            if(camera != null)
            {
                PositionObjectInFrontOfCamera(loadedObj, camera);
            }
            AddTextureToObject(loadedObj, pngPath);
            Debug.Log("Objet 3D instancié : " + objectName);
        }
        else
        {
            Debug.LogError("Impossible de charger l'objet à partir du chemin sp�cifi�.");
        }
    }

    public static void InstantiateCylinderWithText(string name, GameObject camera = null)
    {
        // Vérifier si l'objet existe déjà dans la scène avec ce nom
        GameObject existingObj = GameObject.Find(name + "Dummy");
        if (existingObj != null)
        {
            Debug.Log("L'objet existe déjà dans la scène : " + name);
            return;
        }

        // Créer le cylindre
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name + "Dummy";

        // Positionner le cylindre devant la caméra
        if (camera != null)
        {
            PositionObjectInFrontOfCamera(cylinder, camera);
        }

        // Créer un objet texte 3D (TextMesh) pour afficher le texte au-dessus du cylindre
        GameObject textObject = new GameObject("TextObject");
        textObject.transform.parent = cylinder.transform;  // Assigner le texte comme enfant du cylindre

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = name;
        textMesh.fontSize = 10;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textObject.transform.localPosition = new Vector3(0, 2f, 0);  // Positionner le texte au-dessus du cylindre

        // Optionnel : Vous pouvez modifier d'autres propriétés du texte, comme la couleur ou la police
        textMesh.color = Color.black;

        Debug.Log("Dummyu object created : " + name);
    }



    private static void PositionObjectInFrontOfCamera(GameObject obj, GameObject camera)
    {
        // Calculer la position de l'objet devant la caméra en utilisant sa direction
        Vector3 positionInFront = camera.transform.position + camera.transform.forward * distanceFromCamera;
        Debug.Log(positionInFront);
        // Assigner la position calculée à l'objet
        obj.transform.position = positionInFront;
    }

    public static void AddTextureToObject(GameObject targetObject, string texturePath = null)
    {
        // Charger la texture depuis le chemin donné
        Texture2D texture = null;
        if (texturePath != null)
        {
            texture = LoadTexture(texturePath);
        }
     
        // Assurez-vous que l'objet a un Renderer et récupérez son matériau
        Renderer renderer = targetObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                material.shader = shader;
                Debug.Log("Shader modifié pour Universal Render Pipeline/Lit");
            }
            else
            {
                Debug.LogError("Le shader 'Universal Render Pipeline/Lit' n'a pas été trouvé.");
                return;
            }

            if (texture != null)
            {
                material.SetTexture("_BaseMap", texture);
                Debug.Log("Texture appliquée au matériau");
            }
            else
            {
                Debug.LogWarning("La texture est null, mais le shader a été changé.");
            }
        }
        else
        {
            Debug.LogWarning("L'objet n'a pas de Renderer.");
        }
    }

    private static Texture2D LoadTexture(string path)
    {
        // Utiliser les m�thodes Unity pour charger la texture depuis un chemin de fichier
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2); // Dimensions temporaires avant de charger l'image
        texture.LoadImage(fileData); // Charge l'image � partir des donn�es en bytes
        Debug.Log("texture loaded");
        return texture;
    }
}
