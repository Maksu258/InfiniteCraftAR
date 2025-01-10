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

    private string apiUrl = "http://51.178.83.2/models/";
    private TaskID taskID = new TaskID { result ="0193da09-6ca8-7441-8f2d-2e6dec62f401" };
    private string imgPath = Application.dataPath + "/TestRessources/img.jpg";

    private string urlObj = "";
    private string apiKey = "";

    private string WrapJson(string jsonArray)
    {
        return "{\"items\":" + jsonArray + "}";
    }


    void Start()
    {
        var payload = new Payload
        {
            mode = "preview",
            prompt = "godzilla"
        };

        // Convertir l'objet en JSON
        string payloadJSON = JsonUtility.ToJson(payload);

        TaskID taskId = JsonUtility.FromJson<TaskID>(JsonUtility.ToJson(taskID));
        Debug.Log(taskId.result);

        //StartCoroutine(postAnalyzeImage(apiUrl, imgPath));

        string[] array = { "computer", "glass" };
        StartCoroutine(generateFusionWord(apiUrl, array));
    }

    IEnumerator postAnalyzeImage(string url, string imagePath, string alreadyKnownObject = null)
    {
        // Configurer la requête
        UnityWebRequest request = new UnityWebRequest(url + "analyze-image", "POST");
        
        byte[] imageData = File.ReadAllBytes(imagePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageData, Path.GetFileName(imagePath), "image/jpeg");

        request.uploadHandler = new UploadHandlerRaw(form.data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", form.headers["Content-Type"]);
        Debug.Log("Size : " + form.data.Length);
        //Debug.Log("Payload : " + jsonPayload);
        //Debug.Log("En-têtes : Content-Type: application/json, Authorization: Bearer " + apiKey);

        // Envoyer la requête
        yield return request.SendWebRequest();

        // Gestion des erreurs ou des succès
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request);
            Debug.LogError($"Erreur : {request.error}");
        }
        else
        {
            Debug.Log("POST upload: " + Encoding.UTF8.GetString(request.uploadHandler.data));
            Debug.Log($"Réponse : {request.downloadHandler.text}");
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

    IEnumerator generateFusionWord(string url, string[] words)
    {
        // Configurer la requête
        UnityWebRequest request = new UnityWebRequest(url + "generate-fusion-word", "POST");

        WWWForm form = new WWWForm();
        form.AddField("word1", words[0]);
        form.AddField("word2", words[1]);


        request.uploadHandler = new UploadHandlerRaw(form.data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", form.headers["Content-Type"]);

        // Envoyer la requête
        yield return request.SendWebRequest();

        // Gestion des erreurs ou des succès
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request);
            Debug.LogError($"Erreur : {request.error}");
            Debug.Log($"Réponse : {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log("POST upload: " + Encoding.UTF8.GetString(request.uploadHandler.data));
            Debug.Log($"Réponse : {request.downloadHandler.text}");
            StartCoroutine(get3DObject(url, request.downloadHandler.text));
        }
    }

    IEnumerator get3DObject(string url, string word)
    {
        // Configurer la requête
        UnityWebRequest request = new UnityWebRequest(url + "get3d-object/" + word, "GET");

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        //Debug.Log("En-têtes : Content-Type: application/json, Authorization: Bearer " + apiKey);

        // Envoyer la requête
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erreur lors du téléchargement : {request.error}");
            yield break;
        }

        string data = request.downloadHandler.text;

        Debug.Log("get3DObject : " + data);

        string cleanedJson = data.Replace("\n", "").Replace("\t", "").Trim();

        generatedObject furniture = JsonUtility.FromJson<generatedObject>(cleanedJson);

        StartCoroutine(DownloadObjFromUrlRequest(furniture.modelUrl, furniture.name, ".obj"));
        if(furniture.mtlUrl == null || furniture.mtlUrl == "" || furniture.pngUrl == null || furniture.pngUrl == "")
        {
            StartCoroutine(getTexture(url, furniture.id));
        }
        else
        {
            StartCoroutine(DownloadObjFromUrlRequest(furniture.modelUrl, furniture.name, ".mtl"));
            StartCoroutine(DownloadObjFromUrlRequest(furniture.modelUrl, furniture.name, ".jpg"));
            instantiate3DObj(Application.dataPath + "/Objects/" + furniture.name + ".obj");
        }
        
        /*
        string write_path = Application.dataPath + "/Objects/"+data+".obj";

        System.IO.File.WriteAllBytes(write_path, data);
        */

    }

    IEnumerator getTexture(string url, int id)
    {
        // Configurer la requête
        UnityWebRequest request = new UnityWebRequest(url + "get-texture/" + id, "GET");

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        request.timeout = 1000000;

        //Debug.Log("En-têtes : Content-Type: application/json, Authorization: Bearer " + apiKey);

        // Envoyer la requête
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erreur lors du téléchargement : {request.error}");
            yield break;
        }

        string data = request.downloadHandler.text;
        Debug.Log("getTexture : " + data);

        string cleanedJson = data.Replace("\n", "").Replace("\t", "").Trim();

        generatedObject furniture = JsonUtility.FromJson<generatedObject>(cleanedJson);
        
        if (furniture.mtlUrl == null || furniture.mtlUrl == "")
        {
            Debug.Log("Aucun .mtl n'a été généré");
        }
        else
        {
            StartCoroutine(DownloadObjFromUrlRequest(furniture.modelUrl, furniture.name, ".mtl"));
        }
        if (furniture.pngUrl == null || furniture.pngUrl == "")
        {
            Debug.Log("Aucune texture n'a été générée");
        }
        else
        {
            StartCoroutine(DownloadObjFromUrlRequest(furniture.modelUrl, furniture.name, ".jpg"));
        }


    }

    IEnumerator DownloadObjFromUrlRequest(string url, string name, string extention)
    {
        // Configurer la requête
        UnityWebRequest request = new UnityWebRequest(url, "GET");

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //Debug.Log("URL : " + url + "/" + taskID);
        //Debug.Log("Payload : " + jsonPayload);
        //Debug.Log("En-têtes : Content-Type: application/json, Authorization: Bearer " + apiKey);

        // Envoyer la requête
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erreur lors du téléchargement : {request.error}");
            yield break;
        }

        byte[] data = request.downloadHandler.data;

        string write_path = Application.dataPath + "/Objects/" + name + extention;

        File.WriteAllBytes(write_path, data);

        if(extention == ".obj")
        {
            instantiate3DObj(write_path);
        }
        Debug.Log(request.downloadHandler.text);

    }

    void instantiate3DObj(string objPath, string mtlPath = null)
    {
        // Vérifier si l'objet existe déjà dans la scène
        string objectName = Path.GetFileNameWithoutExtension(objPath);
        GameObject existingObj = GameObject.Find(objectName);

        if (existingObj != null)
        {
            Debug.Log("L'objet existe déjà dans la scène : " + objectName);
            Destroy(existingObj);
        }

        var loadedObj = new OBJLoader().Load(objPath, mtlPath);
        if (loadedObj != null)
        {
            Debug.Log("Objet 3D instancié : " + objectName);
        }
        else
        {
            Debug.LogError("Impossible de charger l'objet à partir du chemin spécifié.");
        }
    }




}
