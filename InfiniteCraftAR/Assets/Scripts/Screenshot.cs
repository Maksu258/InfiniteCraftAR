using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureCam : MonoBehaviour
{
    public Camera eyedropperCamera;

    private Texture2D eyedropperTexture;
    private Rect eyeRect;
    private bool firstGetPixel = true;
    private RenderTexture eyedropperRenderTexture;

    public void Trigger1()
    {
        StartCoroutine(TriggerTimeX());
    }

    public IEnumerator TriggerTimeX()
    {
        yield return new WaitForSeconds(2);
        StartCoroutine(GetPixel(true));
        yield return new WaitForSeconds(1);
        StartCoroutine(GetPixel(true));
    }

    public IEnumerator GetPixel(bool saveImage)
    {
        yield return new WaitForEndOfFrame();

        if (firstGetPixel)
        {
            firstGetPixel = false;

            eyedropperTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
            eyeRect = new Rect(0, 0, Screen.width, Screen.height);

            eyedropperRenderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            eyedropperRenderTexture.Create();

            eyedropperCamera.targetTexture = eyedropperRenderTexture;
            eyedropperCamera.enabled = false;
        }

        RenderTexture currentRT = RenderTexture.active;
        try
        {
            RenderTexture.active = eyedropperCamera.targetTexture;
            eyedropperCamera.backgroundColor = eyedropperCamera.backgroundColor;
            eyedropperCamera.transform.position = eyedropperCamera.transform.position;
            eyedropperCamera.transform.rotation = eyedropperCamera.transform.rotation;
            eyedropperCamera.Render();
            eyedropperTexture.ReadPixels(new Rect(0, 0, eyedropperCamera.targetTexture.width, eyedropperCamera.targetTexture.height), 0, 0);
        }
        finally
        {
            RenderTexture.active = currentRT;
        }

        string name = "Screenshot_EpicApp" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

        NativeGallery.SaveImageToGallery(eyedropperTexture, "Myapp pictures", name);
    }

}