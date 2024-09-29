using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelData : MonoBehaviour
{
    public static void LoadRandomLevel()
    {
        SceneManager.LoadScene(Random.Range(2,SceneManager.sceneCount));
    }
    
}
