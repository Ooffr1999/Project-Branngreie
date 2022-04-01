using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameFunctions : MonoBehaviour
{
    public void TransferScene()
    {
        SceneManager.LoadScene(1);
    }
}
