using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    public Transform cube;
	// Update is called once per frame
	void Update ()
    {
        cube.Rotate(Vector3.up * 30 * Time.deltaTime, Space.World);
	}

    public void PlayButtonPress()
    {
        SceneManager.LoadScene("MainScene");
    }
}
