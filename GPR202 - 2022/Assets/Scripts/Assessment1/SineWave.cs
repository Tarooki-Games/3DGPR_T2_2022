//from video at https://youtu.be/6C1NPy321Nk

using UnityEngine;
 
public class SineWave : MonoBehaviour
{
    public LineRenderer myLineRenderer;
    public int points;
    public Vector2 xLimits = new Vector2(0,1);
    public float amplitude = 1;
    public float frequency = 1;
    public float movementSpeed = 1;
    [Range(0,2*Mathf.PI)]
    public float radians;
    void Start()
    {
        myLineRenderer = GetComponent<LineRenderer>();
    }
    
    void Draw()
    {
        float xStart = xLimits.x;
        float Tau = 2 * Mathf.PI;
        float xFinish = xLimits.y;
 
        myLineRenderer.positionCount = points;
        for(int currentPoint = 0; currentPoint < points; currentPoint++)
        {
            float progress = (float)currentPoint / (points - 1);
            float x = Mathf.Lerp(xStart, xFinish, progress);
            float y = amplitude * Mathf.Sin((Tau * frequency * x) + (Time.timeSinceLevelLoad * movementSpeed + Time.deltaTime));
            myLineRenderer.SetPosition(currentPoint, new Vector3(x, y, 0));
            // Debug.Log($"{x}   ,   {y}");
        }
    }
 
    void Update()
    {
        Draw();
    }
}