using UnityEngine;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject Menu;
    private void Start()
    {
    Menu.SetActive(true);
    }
    public void MenuClose()
    {
    Menu.SetActive(false);
    }
}
