using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public SceneName _sceneName;
    
    public void LoadScene()
    {
        SceneManager.LoadScene(_sceneName.ToString());
    }
    public enum SceneName
    {
        TitleScene,
        MainScene,
        GameOverScene
    }
}
