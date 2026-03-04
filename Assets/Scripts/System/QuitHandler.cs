using UnityEngine;

public class QuitHandler : MonoBehaviour
{
    public void ExitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Debug.Log("게임이 종료되었습니다.");
    }
}