using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _pauseMenu.SetActive(!_pauseMenu.activeSelf);
            
            // Set Cursor Lock State
            Cursor.lockState = _pauseMenu.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
