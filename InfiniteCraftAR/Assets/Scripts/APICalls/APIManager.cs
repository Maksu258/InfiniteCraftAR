using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.IO;
using System;
using Dummiesman;

public class Payload
{
    public string mode;
    public string prompt;

    public Payload() { }
}

[Serializable]
public class Wrapper
{
    public string[] items;
}

[Serializable]
public class generatedObject
{
    public int id;
    public string name;
    public string modelUrl;
    public string mtlUrl;
    public string pngUrl;
    public string createdAt;
    public string updatedAt;
}


public class TaskID
{
    public string result;

    public TaskID() { }
}

public class APIManager : MonoBehaviour
{

    public string apiUrl = "http://51.178.83.2/models/";
    private TaskID taskID = new TaskID { result ="0193da09-6ca8-7441-8f2d-2e6dec62f401" };
    private string imgPath = Application.dataPath + "/TestRessources/img.jpg"; // test image path

    private string WrapJson(string jsonArray)
    {
        return "{\"items\":" + jsonArray + "}";
    }


    public void AddTextureToObject(GameObject targetObject, string texturePath)
    {
        // Charger la texture depuis le chemin donn�
        Texture2D texture = LoadTexture(texturePath);

        if (texture != null)
        {
            // Assurez-vous que l'objet a un Renderer et r�cup�rez son mat�riel
            Renderer renderer = targetObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;

                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null)
                {
                    material.shader = shader;
                    Debug.Log("Shader modifi� pour Universal Render Pipeline/Lit");
                }
                else
                {
                    Debug.LogError("Le shader 'Universal Render Pipeline/Lit' n'a pas �t� trouv�.");
                    return;
                }

                material.SetTexture("_BaseMap", texture);
                Debug.Log("Texture appliqu�e au mat�riau");
            }
            else
            {
                Debug.LogWarning("L'objet n'a pas de Renderer.");
            }
        }
        else
        {
            Debug.LogError("La texture n'a pas pu �tre charg�e.");
        }
    }

    // Fonction pour charger la texture depuis un chemin
    private Texture2D LoadTexture(string path)
    {
        // Utiliser les m�thodes Unity pour charger la texture depuis un chemin de fichier
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2); // Dimensions temporaires avant de charger l'image
        texture.LoadImage(fileData); // Charge l'image � partir des donn�es en bytes
        Debug.Log("texture loaded");
        return texture;
    }

    public void analyzeImage(string imagePath)
    {
        StartCoroutine(postAnalyzeImage(apiUrl, imagePath));
    }

    public void generateFusionObject(string[] words)
    {
        StartCoroutine(generateFusionWord(apiUrl, words));
    }

    void Start()
    {
        /*var payload = new Payload
        {
            mode = "preview",
            prompt = "godzilla"
        };

        // Convertir l'objet en JSON
        string payloadJSON = JsonUtility.ToJson(payload);

        TaskID taskId = JsonUtility.FromJson<TaskID>(JsonUtility.ToJson(taskID));
        Debug.Log(taskId.result);*/


        // Generate with personalized words
        // string[] array = {"Vegeta","Son Goku"};
        // generateFusionObject(array);

        // Generate with a screenshot
        //analyzeImage(imgPath);
    }

    IEnumerator postAnalyzeImage(string url, string imagePath, string alreadyKnownObject = null)
    {
        // Configurer la requ�te
        
        Debug.Log(imagePath);
        UnityWebRequest request = new UnityWebRequest(url + "analyze-image", "POST");
        
        byte[] imageData = File.ReadAllBytes(imagePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageData, Path.GetFileName(imagePath), "image/jpeg");

        request.uploadHandler = new UploadHandlerRaw(form.data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", form.headers["Content-Type"]);
        Debug.Log("Size : " + form.data.Length);
        //Debug.Log("Payload : " + jsonPayload);
        //Debug.Log("En-t�tes : Content-Type: application/json, Authorization: Bearer " + apiKey);

        // Envoyer la requ�te
        yield return request.SendWebRequest();

        // Gestion des erreurs ou des succ�s
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request);
            Debug.LogError($"Erreur : {request.error}");
        }
        else
        {
            Debug.Log("POST upload: " + Encoding.UTF8.GetString(request.uploadHandler.data));
            Debug.Log($"R�ponse : {request.downloadHandler.text}");
            string[] stringArray = JsonUtility.FromJson<Wrapper>(WrapJson(request.downloadHandler.text)).items;
            if(alreadyKnownObject != null)
            {
                stringArray[1] = alreadyKnownObject;
                StartCoroutine(generateFusionWord(url, stringArray));
            }
            else
            {
                StartCoroutine(generateFusionWord(url, stringArray));
            }
        }
    }

  public  IEnumerator generateFusionWord(string url, string[] words)
    {
        // Configurer la requ�te
        UnityWebRequest request = new UnityWebRequest(url + "generate-fusion-word", "POST");

        WWWForm form = new WWWForm();
        form.AddField("word1", words[0]);
        form.AddField("word2", words[1]);


        request.uploadHandler = new UploadHandlerRaw(form.data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", form.headers["Content-Type"]);

        // Envoyer la requ�te
        yield return request.SendWebRequest();

        // Gestion des erreurs ou des succ�s
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request);
            Debug.LogError($"Erreur : {request.error}");
            Debug.Log($"R�ponse : {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log("POST upload: " + Encoding.UTF8.GetString(request.uploadHandler.data));
            Debug.Log($"R�ponse : {request.downloadHandler.text}");
            StartCoroutine(get3DObject(url, request.downloadHandler.text));
        }
    }

    IEnumerator get3DObject(string url, string word)
    {
        // Configurer la requ�te
        UnityWebRequest request = new UnityWebRequest(url + "get3d-object/" + word, "GET");

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.timeout = 10000000;

        // Envoyer la requ�te
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erreur lors du t�l�chargement : {request.error}");
            yield break;
        }
        if (request.responseCode == 504)
        {
            Debug.Log("Erreur 504 : Gateway Timeout");
            yield return StartCoroutine(get3DObject(url, word));
            yield break;
        }

        string data = request.downloadHandler.text;

        Debug.Log("get3DObject : " + data);

        string cleanedJson = data.Replace("\n", "").Replace("\t", "").Trim();

        generatedObject furniture = JsonUtility.FromJson<generatedObject>(cleanedJson);

        yield return StartCoroutine(DownloadObjFromUrlRequest(furniture.modelUrl, furniture.name, ".obj"));

        if (furniture.pngUrl == null || furniture.pngUrl == "")
        {
            StartCoroutine(getTexture(url, furniture.id));
        }
        else
        {
            Debug.Log("Object already have texture generated");
            yield return StartCoroutine(DownloadObjFromUrlRequest(furniture.pngUrl, furniture.name, ".png"));
            instantiate3DObj(Application.dataPath + "/Objects/" + furniture.name + ".obj", Application.dataPath + "/Objects/" + furniture.name + ".png");
        }
        
        /*
        string write_path = Application.dataPath + "/Objects/"+data+".obj";

        System.IO.File.WriteAllBytes(write_path, data);
        */

    }

    IEnumerator getTexture(string url, int id)
    {
        // Configurer la requ�te
        UnityWebRequest request = new UnityWebRequest(url + "get-texture/" + id, "GET");

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.timeout = 10000000;

        //Debug.Log("En-t�tes : Content-Type: application/json, Authorization: Bearer " + apiKey);

        // Envoyer la requ�te
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            if(request.error == "")
            Debug.LogError($"Erreur lors du t�l�chargement : {request.error}");
        }
        if (request.responseCode == 504)
        {
            Debug.Log("Erreur 504 : Gateway Timeout");
            yield return StartCoroutine(getTexture(url, id));
            yield break;
        }

        string data = request.downloadHandler.text;
        Debug.Log("getTexture : " + data);

        string cleanedJson = data.Replace("\n", "").Replace("\t", "").Trim();

        generatedObject furniture = JsonUtility.FromJson<generatedObject>(cleanedJson);

        if (furniture.pngUrl == null || furniture.pngUrl == "")
        {
            Debug.Log("Aucune texture n'a �t� g�n�r�e");
        }
        else
        {
            yield return StartCoroutine(DownloadObjFromUrlRequest(furniture.pngUrl, furniture.name, ".png"));
        }
        Debug.Log("Just before instatiate textured object");
        instantiate3DObj(Application.dataPath + "/Objects/" + furniture.name + ".obj", Application.dataPath + "/Objects/" + furniture.name + ".png");
    }

    IEnumerator DownloadObjFromUrlRequest(string url, string name, string extention)
    {
        // Configurer la requ�te
        UnityWebRequest request = new UnityWebRequest(url, "GET");

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //Debug.Log("URL : " + url + "/" + taskID);
        //Debug.Log("Payload : " + jsonPayload);
        //Debug.Log("En-t�tes : Content-Type: application/json, Authorization: Bearer " + apiKey);

        // Envoyer la requ�te
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erreur lors du t�l�chargement : {request.error}");
            yield break;
        }

        byte[] data = request.downloadHandler.data;

        string write_path = Application.dataPath + "/Objects/" + name + extention;

        File.WriteAllBytes(write_path, data);

        if (extention == ".obj" && GameObject.Find(name) == null)
        {
            instantiate3DObj(write_path);
        }
        Debug.Log(request.downloadHandler.text);
    }

    void instantiate3DObj(string objPath, string pngPath = null)
    {
        // V�rifier si l'objet existe d�j� dans la sc�ne
        string objectName = Path.GetFileNameWithoutExtension(objPath);
        GameObject existingObj = GameObject.Find(objectName);

        if (existingObj != null)
        {
            Debug.Log("L'objet existe d�j� dans la sc�ne : " + objectName);
            if(pngPath != null)
            {
                Debug.Log("Adding texture to existing object");
                AddTextureToObject(existingObj, pngPath);
            }
            return;
        }

        var loadedObj = new OBJLoader().Load(objPath);
        if (loadedObj != null)
        {
            Debug.Log(objPath);
            Debug.Log(pngPath);
            if(pngPath != null)
            {
                Debug.Log("Adding texture to object");
                AddTextureToObject(loadedObj, pngPath);
            }
            Debug.Log("Objet 3D instanci� : " + objectName);
        }
        else
        {
            Debug.LogError("Impossible de charger l'objet � partir du chemin sp�cifi�.");
        }
    }




}
