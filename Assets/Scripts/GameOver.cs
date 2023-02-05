using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    private bool _canResetScene = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // deberia ser en main game pero no tenemos tiempo asi que let's gooooooo
        if (Input.GetKeyDown(KeyCode.Space) && _canResetScene)
        {
            //reload scene 
            Scene scene = SceneManager.GetActiveScene(); 
            SceneManager.LoadScene(scene.name);
        }
    }

    public void CanResetScene() => _canResetScene = true;
}
