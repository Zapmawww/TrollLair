using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float scrollSpeed = .25f; // Basic speed
    public GameMap gameMap;

    private Camera cam;
    private Vector3 lastPosition;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Modifier
        float speedFactor = 1f;
        if (Input.GetKey(KeyCode.LeftControl))
            speedFactor = 3f;
        if (Input.GetKey(KeyCode.LeftShift))
            speedFactor = .3f;

        // Input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        // Exponential Speed
        float sFactor =
            scroll
            * speedFactor
            * scrollSpeed
            * cam.orthographicSize;

        //limit
        float maxSize;
        if (Screen.height > Screen.width)// vertical
        {
            maxSize = (float)gameMap.width / 2 * Screen.height / Screen.width;
        }
        else// horizontal
        {
            maxSize = (float)gameMap.height / 2;
        }
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - sFactor, 4, maxSize);

        // right button drag
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastPosition;
            var factor = 2 * cam.orthographicSize / Screen.height;
            transform.position += new Vector3(-delta.x * factor, -delta.y * factor, 0);

        }
        //limit
        var test = transform.position;
        var maxY = gameMap.height / 2 - cam.orthographicSize;
        var minY = -gameMap.height / 2 + cam.orthographicSize;
        var maxX = gameMap.width / 2;
        var minX = -gameMap.width / 2;
        if (test.x > maxX) test.x = maxX;
        if (test.x < minX) test.x = minX;
        if (test.y > maxY) test.y = maxY;
        if (test.y < minY) test.y = minY;
        transform.position = test;

        //update
        lastPosition = Input.mousePosition;
    }
}