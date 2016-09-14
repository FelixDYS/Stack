using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StackManager : MonoBehaviour
{
    public Color32[] gameColors;
    public Material stackMat;
    public Text scoreText;
    public GameObject Menu;

    private const float MOVIE_SIZE = 5f;
    private const float BOUNDS_SIZE = 3.5f;
    private const float STACK_MOVING_SPEED = 5f;
    private const float ERROR_MARGIN = 0.1f;
    private const float STACK_BOUNDS_GAIN = 0.25f;
    private const int COMBO_START_GAIN = 5;

    private GameObject[] stacks;
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

    private int scoreCount = 0;
    private int stackIndex;
    private int combo;

    private float tileTransition = 0f;
    private float tileSpeed = 2.5f;
    private float secondaryPosition;
    private float dirMovie = MOVIE_SIZE;
    private float timer = 0;
    private float movingSpeed = 1f;

    private bool isMovingOnX = true;
    private bool isGameOver = false;
    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;
    // Use this for initialization
    void Start ()
    {
        Init();
    }

    private void Init()
    {
        stacks = new GameObject[transform.childCount];
        int colorIndex = UnityEngine.Random.Range(0, stacks.Length);
        for (int i = 0; i < transform.childCount; i++)
        {
            stacks[i] = transform.GetChild(i).gameObject;
            ColorMesh(stacks[i].GetComponent<MeshFilter>().mesh, colorIndex + i);
        }
        stackIndex = transform.childCount - 1;
    }

    public void RePlayButtonPress()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void MenuButtonPress()
    {
        SceneManager.LoadScene("MenuScene");
    }

    // Update is called once per frame
    void Update ()
    {
        if (isGameOver)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                timer = 0;
                scoreCount++;
                if (scoreCount % 5 == 0 && movingSpeed < 1.8f)
                {
                    movingSpeed += 0.05f;
                }
                scoreText.text = scoreCount.ToString();
            }
            else
            {
                EndGame();
            }
        } 

        MoveTile();

        transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);
	}

    private void MoveTile()
    {
        if (Mathf.Abs(tileTransition) == MOVIE_SIZE)
        {
            dirMovie = -dirMovie;
            timer = 0;
        }

        timer += Time.deltaTime * movingSpeed;
        tileTransition = Mathf.Lerp(dirMovie, -dirMovie, timer);

        if (isMovingOnX)
        {
            stacks[stackIndex].transform.localPosition = new Vector3(tileTransition, scoreCount, secondaryPosition);
        }
        else
        {
            stacks[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, tileTransition);
        }
    }

    private void EndGame()
    {
        isGameOver = true;
        Debug.Log("Game Over");
        stacks[stackIndex].AddComponent<Rigidbody>();
        Menu.SetActive(isGameOver);
    }

    private void SpawnTile()
    {
        lastTilePosition = stacks[stackIndex].transform.localPosition;
        stackIndex--;
        if (stackIndex < 0)
        {
            stackIndex = transform.childCount - 1;
        }
        desiredPosition = (Vector3.down) * scoreCount;

        if (isMovingOnX)
        {
            stacks[stackIndex].transform.localPosition = new Vector3(-dirMovie, scoreCount, 0);
        }
        else
        {
            stacks[stackIndex].transform.localPosition = new Vector3(0, scoreCount, -dirMovie);
        }
        stacks[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        ColorMesh(stacks[stackIndex].GetComponent<MeshFilter>().mesh, scoreCount);  
    }

    private void ColorMesh(Mesh mesh, int scoreIndex)
    {
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];
        float f = Mathf.Sin(scoreIndex * 0.25f);

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Lerp(gameColors, scoreIndex);
        }

        mesh.colors = colors;
    }

    private Color32 Lerp(Color32[] colors, int t)
    {
        int index = t % colors.Length;
        if (index >= colors.Length - 1)
        {
            index = 0;
        }
        return Color.Lerp(colors[index], colors[index + 1], 0.33f); 
    }

    private void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>().mass = 4f;

        go.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(go.GetComponent<MeshFilter>().mesh, scoreCount);
    }
    private bool PlaceTile()
    {
        Transform t = stacks[stackIndex].transform;
        if (isMovingOnX)
        {
            float deltaX = lastTilePosition.x - t.position.x;
            if (Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x <= 0)
                {
                    return false;
                }

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                CreateRubble(new Vector3((t.position.x > 0)
                    ? (t.position.x + (t.localScale.x / 2))
                    : (t.position.x - (t.localScale.x / 2)), t.position.y, t.position.z)
                    , new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z));
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);

            }
            else
            {
                combo++;
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.x += STACK_BOUNDS_GAIN;
                    if (stackBounds.x > BOUNDS_SIZE)
                    {
                        stackBounds.x = BOUNDS_SIZE;
                    }
                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
                }
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
                Debug.Log("X");
            }
        }
        else
        {
            float deltaZ = lastTilePosition.z - t.position.z;
            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
            {
                combo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);
                if (stackBounds.y <= 0)
                {
                    return false;
                }

                float middle = lastTilePosition.z + t.localPosition.z / 2;
                CreateRubble(new Vector3(t.position.x, t.position.y,
                    (t.position.z > 0)
                    ? (t.position.z + (t.localScale.z / 2))
                    : (t.position.z - (t.localScale.z / 2)))
                    , new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ)));
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
            }
            else
            {

                combo++;
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.y += STACK_BOUNDS_GAIN;
                    if (stackBounds.y > BOUNDS_SIZE)
                    {
                        stackBounds.y = BOUNDS_SIZE;
                    }
                    float middle = lastTilePosition.x + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
                }
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
                Debug.Log("Z");
            }
        }
        secondaryPosition = (isMovingOnX) ? t.localPosition.x : t.localPosition.z;
        isMovingOnX = !isMovingOnX;
        return true;
    }
}
