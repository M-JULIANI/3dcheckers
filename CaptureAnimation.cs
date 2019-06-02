
using System.Collections;
using UnityEngine;

public class CaptureAnimation : MonoBehaviour
{
    public static bool EnableCapture = false;
    string folder = @"D:\_BARTLETT\MSC AC\Studio Term 1\3D GEN CHECKERS\Recording02";

    void Start()
    {
        Time.captureFramerate = 30;
    }

    void Update()
    {
        if (EnableCapture)
            StartCoroutine(Capture());
    }

    IEnumerator Capture()
    {
        for (int i = 0; i < 10000; i++)
     
        {
            if (i % 2 == 0)
            {
                yield return new WaitForEndOfFrame();
                string filename = $@"D:\_BARTLETT\MSC AC\Studio Term 1\3D GEN CHECKERS\Recording02\image_{Time.frameCount:00000}.png";
                ScreenCapture.CaptureScreenshot(filename, 1);
            }
        }
       
    }
}