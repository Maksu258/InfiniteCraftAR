using Dummiesman;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Doesn't have to be run from start, can be used on any trigger. */
public class Startup : MonoBehaviour
{
    void Start()
    {
        string apiKey = "msy_SMflMajPPAvNehCEX11cZtsWQAVaY1YLUbBO";
        //file path
        /** Sample OBJ from 
        * https://www.turbosquid.com/3d-models/free-obj-model-ivysaur-pokemon-sample/1136333 
        * Model .mtl file needs editing to add the texture by default
        */
        string filePath = Application.dataPath + "/Objects/" + "Screen" + ".obj";
        // MTL should be linked in the OBJ file by default, 
        string mtlPath = Application.dataPath + "/Objects/" + "Screen" + ".mtl";

        var loadedObj = new OBJLoader().Load(filePath, mtlPath);

        //Instantiate(loadedObj);
    }

    void Update()
    {

    }
}