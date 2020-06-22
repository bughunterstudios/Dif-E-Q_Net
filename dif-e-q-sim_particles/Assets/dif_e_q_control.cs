using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dif_e_q_control : MonoBehaviour {
    public Gradient gradient;
    public float gradientoffset;

    [Header("Adjust the edge width of the grid.")]
    public int size;
    public float distance;

    public float C;
    [Header("This will change the friction on particles.")]
    public float friction;

    private ParticleSystem.Particle[] particlearray = null;
    private float[,] velocities;
    private float[,] heights;

    private ParticleSystem ps = null;

    private Plane plane;
    private Ray ray;
    private float dist;

    private bool clicked;
    private Vector2Int clickpos;
    private Vector3 screenPoint;
    private Vector3 offset;

    private LineRenderer lr;

    public Texture2D cursortex;

    // Use this for initialization
    void Start () {
        Cursor.SetCursor(cursortex, new Vector2(450, 190), CursorMode.Auto);
        lr = GetComponent<LineRenderer>();
        lr.positionCount = size * size;
        ps = GetComponent<ParticleSystem>();
        CreateField();
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //Keep track of time
        float dT = Time.deltaTime;

        //Set Accelerations
        float[,] accelerations = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                //Booleans to check edges
                bool le = x == 0;
                bool re = x == (size - 1);
                bool ue = y == 0;
                bool de = y == (size - 1);

                float height = heights[x, y];

                //Turnary operator (?) is used so that I don't try and pull from outside of heights boundaries. Just an alternative to an if statement
                float changex = re ? 0 : heights[x + 1, y];
                changex += le ? 0 : heights[x - 1, y];
                changex -= le || re ? height : 2 * height;

                float changey = de ? 0 : heights[x, y + 1];
                changey += ue ? 0 : heights[x, y - 1];
                changey -= ue || de ? height : 2 * height;

                accelerations[x, y] = (1 / C) * (changex + changey);
            }
        }

        //Check for clicked pos
        if (!clicked && Input.GetMouseButton(0))
        {
            //Only checks on a plane at Vector3.zero. Pretends all points are always on this plane.
            plane = new Plane(Vector3.up, Vector3.zero);
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButton(0) && plane.Raycast(ray, out dist))
            {
                clicked = true;
                Vector3 point = ray.GetPoint(dist);
                clickpos = new Vector2Int(Mathf.RoundToInt(point.x / distance), Mathf.RoundToInt(point.z / distance));
                screenPoint = Camera.main.WorldToScreenPoint(particlearray[(clickpos.x * size) + clickpos.y].position);
                offset = particlearray[(clickpos.x * size) + clickpos.y].position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            }
        }
        if (!Input.GetMouseButton(0))
            clicked = false;
        
        //Set Velocities and Heights
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                //If a point is clicked, we set it's height to the mouse position.
                if (clicked && clickpos == new Vector2Int(x, y))
                {
                    Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                    Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
                    heights[x, y] = cursorPosition.y;
                }
                else
                {
                    //Otherwise we need to calculate velocities based on acceleration.
                    velocities[x, y] += accelerations[x, y] * 0.03f;
                    velocities[x, y] *= friction;
                    if (Input.GetAxis("Pause") != 0)
                        velocities[x, y] = 0;
                    if (Input.GetAxis("Freeze") != 0)
                    {
                        velocities[x, y] = 0;
                        heights[x, y] *= 0.9f;
                    }
                    else
                        heights[x, y] += velocities[x, y] * 0.03f;
                }
                particlearray[(x * size) + y].position = new Vector3(x * distance, heights[x, y], y * distance);
                lr.SetPosition((x * size) + y, particlearray[(x * size) + y].position);
                particlearray[(x * size) + y].color = gradient.Evaluate(Mathf.Clamp((heights[x, y] + gradientoffset) / (2 * gradientoffset), 0f, 1f));
            }
        }
        ps.SetParticles(particlearray, size * size);
    }

    //Set up the particles
    private void CreateField()
    {
        //var emitparams = new ParticleSystem.EmitParams();
        //ps.Emit(emitparams, size * size);
        velocities = new float[size, size];
        heights = new float[size, size];

        particlearray = new ParticleSystem.Particle[size * size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                particlearray[(x * size) + y].position = new Vector3(x * distance, 0, y * distance);
                particlearray[(x * size) + y].velocity = Vector3.zero;
                particlearray[(x * size) + y].startSize = 1;
                particlearray[(x * size) + y].color = Color.white;
                velocities[x, y] = 0;
                heights[x, y] = 0;
            }
        }
        ps.SetParticles(particlearray, size * size);
    }
}
