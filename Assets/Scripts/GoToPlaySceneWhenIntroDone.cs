using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPlaySceneWhenIntroDone : MonoBehaviour
{
    public int nextScene;
    public VideoPlayer videoplayer;

    private void Update()
    {
        videoplayer.loopPointReached += ChangeScene;
            
    }

    void ChangeScene(VideoPlayer vp)
    {
        Debug.Log("Done with film");
        SceneManager.LoadScene("PlayScene");
    }
}
