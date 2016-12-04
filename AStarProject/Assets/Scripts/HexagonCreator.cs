using UnityEngine;
using System.Collections;
using Pathing;
using System.Collections.Generic;

public class HexagonCreator : MonoBehaviour
{

    const string grassTexture = "grass";
    const string desertTexture = "desert";
    const string moutainTexture = "mountain";
    const string forestTexture = "forest";
    const string waterTexture = "water";

    const int grassCost = 1;
    const int desertCost = 5;
    const int moutainCost = 10;
    const int forestCost = 3;
    const int waterCost = int.MaxValue;


    // Contains all the textures that will be used
    public Texture[] textures;
    private Renderer rend;
    // Contains all the node of HexGrid
    private List<IAStarNode> allNodes;

    // the selected start and end tiles and the shortest path between them
    private IAStarNode start;
    private IAStarNode goal;
    private List<IAStarNode> path;

    //following public variable is used to store the hex model prefab;
    //instantiate it by dragging the prefab on this variable using unity editor
    public GameObject Hex;
    //next two variables can also be instantiated using unity editor
    public int gridWidthInHexes = 8;
    public int gridHeightInHexes = 8;

    //Hexagon tile width and height in game world
    private float hexWidth;
    private float hexHeight;

    //Method to initialise Hexagon width and height
    void setSizes()
    {
        //renderer component attached to the Hex prefab is used to get the current width and height
        hexWidth = Hex.GetComponent<Renderer>().bounds.size.x;
        hexHeight = Hex.GetComponent<Renderer>().bounds.size.z;
    }

    //Method to calculate the position of the first hexagon tile
    //The center of the hex grid is (0,0,0)
    Vector3 calcInitPos()
    {
        Vector3 initPos;
        //the initial position will be in the left upper corner
        initPos = new Vector3(-hexWidth * gridWidthInHexes / 2f + hexWidth / 2, 0,
            gridHeightInHexes / 2f * hexHeight - hexHeight / 2);

        return initPos;
    }

    //method used to convert hex grid coordinates to game world coordinates
    public Vector3 calcWorldCoord(Vector2 gridPos)
    {
        float yDistance = 14;
        float zDistance = -3;

        //Position of the first hex tile
        Vector3 initPos = calcInitPos();
        //Every second row is offset by half of the tile width
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = hexWidth / 2;

        float x = initPos.x + offset + gridPos.x * hexWidth;
        //Every new line is offset in z direction by 3/4 of the hexagon height
        float y = transform.position.z + yDistance - gridPos.y * hexHeight * 0.75f;
        return new Vector3(x, y, zDistance);
    }

    //Finally the method which initialises and positions all the tiles
    void createGrid()
    {
        //Game object which is the parent of all the hex tiles
        GameObject hexGridGO = new GameObject("HexGrid");
        System.Random rnd = new System.Random();
        int i = 0;

        for (float y = 0; y < gridHeightInHexes; y++)
        {
            for (float x = 0; x < gridWidthInHexes; x++)
            {
                //GameObject assigned to Hex public variable is cloned
                GameObject hex = (GameObject)Instantiate(Hex);
                //Current position in grid
                Vector2 gridPos = new Vector2(x, y);
                hex.transform.position = calcWorldCoord(gridPos);
                hex.transform.parent = hexGridGO.transform;

                // All the Hex will face the main camera
                Vector3 directionToFace = transform.position - hex.transform.position;
                hex.transform.rotation = Quaternion.LookRotation(directionToFace, new Vector3(1, 0, 0)) * Quaternion.Euler(90, 0, 0);
                hex.transform.Rotate(0, 90, 0);
                hex.AddComponent<BoxCollider>();
                // Naming each Hexagone
                hex.name = "hex" + i++;

                // Giving random Texture de each Hexgaone
                int numTexture = rnd.Next(0, textures.Length);
                rend = hex.GetComponent<Renderer>();
                rend.enabled = true;
                rend.material.mainTexture = textures[numTexture];

                allNodes.Add(new AStarNode() { Hex = hex, Position = new Vector2(x, y), Cost = CostOfNode(textures[numTexture].name) });
            }
        }
    }

    // Calculate the cost of the given Texture's name
    private int CostOfNode(string textureName)
    {
        switch (textureName)
        {
            case grassTexture:
                return grassCost;
            case desertTexture:
                return desertCost;
            case moutainTexture:
                return moutainCost;
            case forestTexture:
                return forestCost;
            case waterTexture:
                return waterCost;
            default:
                return int.MaxValue;
        }
    }

    // Returning the list of neighbours for a given Node (Hexagon)
    private List<IAStarNode> myNeighbours(AStarNode node)
    {
        List<IAStarNode> neighbours = new List<IAStarNode>();
        int x = (int)node.Position.x;
        int y = (int)node.Position.y;

        if ((node.Position.x - 1) >= 0)
        {
            neighbours.Add(allNodes[(x - 1 + y * gridWidthInHexes)]);
        }
        if ((node.Position.x + 1) < gridWidthInHexes)
        {
            neighbours.Add(allNodes[(x + 1 + y * gridWidthInHexes)]);
        }
        if ((node.Position.y - 1) >= 0)
        {
            neighbours.Add(allNodes[(x + (y - 1) * gridWidthInHexes)]);
            if (((x + 1) < gridWidthInHexes) && ((y % 2) != 0))
            {
                neighbours.Add(allNodes[(x + 1 + (y - 1) * gridWidthInHexes)]);
            }
            else if (((x - 1) >= 0) && ((y % 2) == 0))
            {
                neighbours.Add(allNodes[(x - 1 + (y - 1) * gridWidthInHexes)]);
            }
        }
        if ((y + 1) < gridHeightInHexes)
        {
            neighbours.Add(allNodes[(x + (y + 1) * gridWidthInHexes)]);
            if (((x + 1) < gridWidthInHexes) && ((y % 2) != 0))
            {
                neighbours.Add(allNodes[(x + 1 + (y + 1) * gridWidthInHexes)]);
            }
            else if (((x - 1) >= 0) && ((y % 2) == 0))
            {
                neighbours.Add(allNodes[(x - 1 + (y + 1) * gridWidthInHexes)]);
            }
        }

        return neighbours;
    }

    // Cleaning the old path colors from HexGrid
    private void deleteOldPathColors()
    {
        if (path != null)
        {
            foreach (IAStarNode node in path)
            {
                rend = ((AStarNode)node).Hex.GetComponent<Renderer>();
                rend.material.color = Color.white;
            }
        }
    }

    //The grid will be generated on game start
    void Start()
    {
        allNodes = new List<IAStarNode>();
        setSizes();
        createGrid();

        // Determine the neighbours for each Hexagon
        foreach (IAStarNode node in allNodes)
        {
            ((AStarNode)node).Neighbours = myNeighbours((AStarNode)node);
        }
    }

    void Update()
    {
        // if left button pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Ray emerging from the main camera
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                int numHex = 0;
                // Taking the numbers of "Hex12" => 12
                if (int.TryParse(hit.collider.gameObject.name.Substring(3, hit.collider.gameObject.name.Length - 3), out numHex))
                {
                    if ((start == null) && (((AStarNode)allNodes[numHex]).Cost != int.MaxValue))
                    {
                        deleteOldPathColors();
                        start = allNodes[numHex];
                        // Giving start a green color
                        rend = ((AStarNode)allNodes[numHex]).Hex.GetComponent<Renderer>();
                        rend.material.color = Color.green;

                    }
                    else if ((goal == null) && (((AStarNode)allNodes[numHex]).Cost != int.MaxValue))
                    {
                        goal = allNodes[numHex];
                        // Giving goal a green color
                        rend = ((AStarNode)allNodes[numHex]).Hex.GetComponent<Renderer>();
                        rend.material.color = Color.green;

                        path = (List<IAStarNode>)AStar.GetPath(start, goal);

                        // Giving the path a red color
                        foreach (IAStarNode node in path)
                        {
                            if ((path.IndexOf(node) != 0) && (path.IndexOf(node) != path.Count - 1))
                            {
                                rend = ((AStarNode)node).Hex.GetComponent<Renderer>();
                                rend.material.color = Color.red;
                            }
                        }

                        // Reinit Start and Goal for the next Use
                        start = null;
                        goal = null;

                    }

                }
            }
        }
    }
}
