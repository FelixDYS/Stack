using UnityEngine;
using System.Collections;
using System;

public class StackManager : MonoBehaviour
{

    public Color32[] gameColors;
    public Material stackMat;

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

    private bool isMovingOnX = true;
    private bool isGameOver = false;
    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;
    // Use this for initialization
    void Start ()
    {
        stacks = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            stacks[i] = transform.GetChild(i).gameObject;
            ColorMesh(stacks[i].GetComponent<MeshFilter>().mesh);
        }
        stackIndex = transform.childCount - 1;
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
                scoreCount++;
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
        tileTransition += Time.deltaTime * tileSpeed;
        if (isMovingOnX)
        {
            stacks[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition);
        }
        else
        {
            stacks[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * BOUNDS_SIZE);
        }
    }

    private void EndGame()
    {
        isGameOver = true;
        Debug.Log("Game Over");
        stacks[stackIndex].AddComponent<Rigidbody>();
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
        stacks[stackIndex].transform.localPosition = new Vector3(0, scoreCount, 0);
        stacks[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        ColorMesh(stacks[stackIndex].GetComponent<MeshFilter>().mesh);  
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];
        float f = Mathf.Sin(scoreCount * 0.25f);

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Lerp(gameColors, scoreCount);
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
        ColorMesh(go.GetComponent<MeshFilter>().mesh);
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
