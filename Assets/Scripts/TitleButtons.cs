using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleButtons : MonoBehaviour
{
    [SerializeField] private bool isNewGame;

    public void LoadMain()
    {
        if (isNewGame && File.Exists(Application.persistentDataPath + "/SaveFile")) 
            File.Delete(Application.persistentDataPath + "/SaveFile");
        SceneManager.LoadScene("Main");
    }
}
