using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public partial class SimpleGrid /*: MonoBehaviour*/

{

    Cell[,,] cells;
    public StringWriter sw;
    public Vector3Int gridSize;

    public List<Cell> FinalPath;

    private Color team1Mat;
    private Color team2Mat;

    private Color cellMaterialInactive;

    List<Cell> Team1Targets;
    List<Cell> Team2Targets;
    GameObject soldier;
    Material clearMat;
    List<GameObject> T1DistLines;
    List<GameObject> T2DistLines;

    public float MaxDistance;
    List<GameObject> T1Proxy;
    List<GameObject> T2Proxy;

    int countT1;
    int countT2;
    int count;

    int DistT1;
    int DistT2;

    public SimpleGrid(GameObject Soldier, Vector3Int GridSize, float cellSize, Color Team1Mat, Color Team2Mat, Color MatInactive, Material ClearMat, string T1, string T2)
    {

        cells = new Cell[GridSize.x, GridSize.y, GridSize.z];
        Team1Targets = new List<Cell>();
        Team2Targets = new List<Cell>();
        gridSize = GridSize;
        team1Mat = Team1Mat;
        team2Mat = Team2Mat;
        cellMaterialInactive = MatInactive;
        clearMat = ClearMat;
        sw = new StringWriter();
        soldier = Soldier;
        countT1 = 0;
        countT2 = 0;
        count = 0;
        InitGrid(cellSize);

        MaxDistance = FindMaxDist();

        DistT1 = 0;
        DistT2 = 0;
    }

    //Initializes the grid
    public void InitGrid(float cellSize)
    {
        for (int z = 0; z < gridSize.z; z++)
            for (int y = 0; y < gridSize.y; y++)
                for (int x = 0; x < gridSize.x; x++)
                {
                    var index = new Vector3Int(x, y, z);

                    Vector3 pos = new Vector3(index.x, index.y, index.z) * cellSize;
                    GameObject cube = GameObject.Instantiate(soldier, pos, Quaternion.identity);
                    cube.layer = 11;
                    cube.transform.localPosition = pos;
                    cube.transform.localScale = new Vector3(0.9f, 0.9f, 0.95f);

                    var lowerBound = Mathf.RoundToInt(gridSize.x * 0.3f);
                    var upperBound = Mathf.RoundToInt(gridSize.x * 0.6f);

                    //Team 1 Initiate
                    if (pos.x > upperBound && pos.y > upperBound && pos.z > upperBound)
                    {
                        cube.GetComponent<MeshRenderer>().material.color = team1Mat;

                        var cell = new Cell(this)
                        {
                            Position = index,
                            WorldPosition = pos,
                            DisplayCell = cube,
                            IsActive = true,
                            Target = "T2"

                        };

                        this.cells[x, y, z] = cell;
                        Team2Targets.Add(cell);
                    }
                    //Team 2 Initiate
                    else if (pos.x < lowerBound && pos.y < lowerBound && pos.z < lowerBound)
                    {

                        cube.GetComponent<MeshRenderer>().material.color = team2Mat;

                        var cell = new Cell(this)
                        {
                            Position = index,
                            WorldPosition = pos,
                            DisplayCell = cube,
                            IsActive = true,
                            Target = "T1"
                        };

                        this.cells[x, y, z] = cell;
                        Team1Targets.Add(cell);
                    }

                    //Initiate inactive cells
                    else
                    {
                        cube.GetComponent<MeshRenderer>().material.color = cellMaterialInactive;
                        var meshRend = cube.GetComponent<MeshRenderer>();
                        meshRend.enabled = false;

                        var cell = new Cell(this)
                        {
                            Position = index,
                            WorldPosition = pos,
                            DisplayCell = cube,
                            IsActive = false,
                            Target = "none"
                        };
                        this.cells[x, y, z] = cell;
                    }
                }

        foreach (var vec in Team1Targets)
        {
            var pos = vec.WorldPosition;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.layer = 14;
            cube.transform.localPosition = pos;
            cube.GetComponent<MeshRenderer>().material = clearMat;
            cube.GetComponent<BoxCollider>();
            //cloneCollider = cube.GetComponent<BoxCollider>();
            // cloneCollider.enabled = false;
        }
        foreach (var vec in Team2Targets)
        {
            var pos = vec.WorldPosition;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.layer = 14;
            cube.transform.localPosition = pos;
            cube.GetComponent<MeshRenderer>().material = clearMat;
            // cube.GetComponent<BoxCollider>();
            // cloneCollider.enabled = false;
        }
    }


    public IEnumerator GamePlay(Material team1, Material team2)
    {

        //ienumerator delay interval
        float delay = 0.001f;
        //actual moves that have passed
        int cumulCount;
        //this decided who starts
        int decider = Random.Range(0, 2);

      //List used un the logic of selecting appropriate index for sequential moves
        List<string> tTargets = new List<string>();
        tTargets.Add("T1");
        tTargets.Add("T2");

        List<Cell> FinalPath = new List<Cell>();

        for (int f = 0; f < 500000; f++)
        {
            //Visualization lines (right side of screen) of distances remaining
            DestroyLines();
            T1DistLines = new List<GameObject>();
            T2DistLines = new List<GameObject>();

            //Visualization (right side of screen) of active soliders on each team
            DestroyProxies();
            T1Proxy = new List<GameObject>();
            T1Proxy = new List<GameObject>();

            //Random selection of 'index' of cell to operate on within array
            var x = Random.Range(0, gridSize.x);
            var y = Random.Range(0, gridSize.y);
            var z = Random.Range(0, gridSize.z);
            var activeCell = this.cells[x, y, z];

            //After the first move (which is decided randomly) the logic that follows ensures sequential turn-based play:
                if (countT1 == 1 && countT2 == 0) decider = 0;
                else if (countT2 == 1 && countT1 == 0) decider = 1;

            //actual number of moves that have passed
            cumulCount = countT1 + countT2 + decider;

            //Logic that controls turns
            if ((cumulCount) % 2 == decider)
            {
                if(activeCell.Target == tTargets[Mathf.Abs(decider - 1)])
                MakeMove(activeCell);
            }
            else if ((cumulCount) % 2 == Mathf.Abs(decider - 1))
            {
                if(activeCell.Target == tTargets[decider])
                MakeMove(activeCell);
            }

            //Draws game stats regardless of whether a move is made or not
            DistT1 = DrawDistLines("T2", Team1Targets, team1Mat, T1DistLines);
            DistT2 = DrawDistLines("T1", Team2Targets, team2Mat, T2DistLines);
            T1Proxy = ColorGOs("T2", Team1Targets, team1Mat);
            T2Proxy = ColorGOs("T1", Team2Targets, team2Mat);


            yield return new WaitForSeconds(delay);
        }
    }

    //Changes the state of the whole array depending on the move 'turn'
    public void MakeMove(Cell activeCell)
    {
        var neighbors = GetNeighbors(activeCell);
        Cell neighborCell = new Cell(this);

        int index = Random.Range(0, neighbors.Count);
        var selectedNeighbor = neighbors[index];

        //Ortho neighbors of active cell (for ladder logic)
        var orthoNeighbs1 = GetOrthoNeighbors(activeCell);

        //Ladder logic, incorporating ladder neighbors to neighbors
        List<Cell> jumpPathEnds = new List<Cell>();
        Cell lastStep = new Cell(this);
        for (int i = 0; i < orthoNeighbs1.Count; i++)
        {
            List<Cell> path = new List<Cell>();

            if (orthoNeighbs1[i].IsActive)
            {
                var orthoNeighbs2 = GetOrthoNeighbors(orthoNeighbs1[i]);
                for (int j = 0; j < orthoNeighbs2.Count; j++)
                {
                    if (orthoNeighbs2[j].IsActive == false && (orthoNeighbs2[j].WorldPosition != orthoNeighbs1[i].WorldPosition))
                    {
                        path.Add(orthoNeighbs2[j]);
                        // round 2
                        var orthoNeighbs3 = GetOrthoNeighbors(orthoNeighbs2[j]);
                        for (int k = 0; k < orthoNeighbs3.Count; k++)
                        {
                            if (orthoNeighbs3[k].IsActive)
                            {
                                var orthoNeighbs4 = GetOrthoNeighbors(orthoNeighbs3[k]);

                                for (int m = 0; m < orthoNeighbs4.Count; m++)
                                {
                                    if (orthoNeighbs4[m].IsActive == false && (orthoNeighbs3[k].WorldPosition != orthoNeighbs2[j].WorldPosition))
                                    {
                                        path.Add(orthoNeighbs4[m]);/////
                                        var orthoNeighbs5 = GetOrthoNeighbors(orthoNeighbs4[m]);
                                        for (int n = 0; n < orthoNeighbs5.Count; n++)
                                        {
                                            if (orthoNeighbs5[n].IsActive)
                                            {
                                                var orthoNeighbs6 = GetOrthoNeighbors(orthoNeighbs5[n]);

                                                for (int p = 0; p < orthoNeighbs6.Count; p++)
                                                {
                                                    if (orthoNeighbs6[p].IsActive == false && (orthoNeighbs5[n].WorldPosition != orthoNeighbs4[m].WorldPosition))
                                                    {
                                                        path.Add(orthoNeighbs6[p]);/////
                                                        var orthoNeighbs7 = GetOrthoNeighbors(orthoNeighbs6[p]);
                                                        for (int q = 0; q < orthoNeighbs7.Count; q++)
                                                        {
                                                            if (orthoNeighbs7[q].IsActive)
                                                            {
                                                                var orthoNeighbs8 = GetOrthoNeighbors(orthoNeighbs7[q]);

                                                                for (int r = 0; r < orthoNeighbs8.Count; r++)
                                                                {
                                                                    if (orthoNeighbs8[r].IsActive == false && (orthoNeighbs7[q].WorldPosition != orthoNeighbs6[p].WorldPosition))
                                                                    {
                                                                        path.Add(orthoNeighbs8[r]);/////
                                                                        var orthoNeighbs9 = GetOrthoNeighbors(orthoNeighbs8[r]);
                                                                        for (int s = 0; s < orthoNeighbs9.Count; s++)
                                                                        {
                                                                            if (orthoNeighbs9[s].IsActive)
                                                                            {
                                                                                var orthoNeighbs10 = GetOrthoNeighbors(orthoNeighbs9[s]);

                                                                                for (int t = 0; t < orthoNeighbs10.Count; t++)
                                                                                {
                                                                                    if (orthoNeighbs10[t].IsActive == false && (orthoNeighbs9[s].WorldPosition != orthoNeighbs8[r].WorldPosition))
                                                                                    {
                                                                                        path.Add(orthoNeighbs10[t]);/////
                                                                                        var orthoNeighbs11 = GetOrthoNeighbors(orthoNeighbs10[t]);
                                                                                        for (int u = 0; u < orthoNeighbs11.Count; u++)
                                                                                        {
                                                                                            if (orthoNeighbs11[u].IsActive)
                                                                                            {
                                                                                                var orthoNeighbs12 = GetOrthoNeighbors(orthoNeighbs11[u]);

                                                                                                for (int v = 0; v < orthoNeighbs12.Count; v++)
                                                                                                {
                                                                                                    if (orthoNeighbs12[v].IsActive == false && (orthoNeighbs11[u].WorldPosition != orthoNeighbs10[t].WorldPosition))
                                                                                                    {
                                                                                                        path.Add(orthoNeighbs10[t]);/////
                                                                                                    }
                                                                                                }

                                                                                            }
                                                                                            else continue;
                                                                                        }
                                                                                    }
                                                                                    else continue;
                                                                                }

                                                                            }
                                                                            else continue;
                                                                        }
                                                                    }
                                                                    else continue;
                                                                }
                                                            }
                                                            else continue;
                                                        }
                                                    }
                                                    else continue;
                                                }
                                            }
                                            else continue;
                                        }
                                    }
                                    else continue;
                                }
                            }
                            else continue;
                        }
                    }
                    else continue;
                }
            }
            if (path.Count >= 1)

            {
                lastStep = path[path.Count - 1];
                jumpPathEnds.Add(lastStep);
            }

            else continue;

        }

        neighbors.AddRange(jumpPathEnds);

        //Pick best neighbor 
        if (activeCell.Target == "T2")
        {
            List<NeighborInfo> neighborBucket = new List<NeighborInfo>();

            for (int i = 0; i < neighbors.Count; i++)
            {
                var xDelta = Mathf.Abs(neighbors[i].WorldPosition.x - Team1Targets[0].WorldPosition.x);
                var yDelta = Mathf.Abs(neighbors[i].WorldPosition.y - Team1Targets[0].WorldPosition.y);
                var zDelta = Mathf.Abs(neighbors[i].WorldPosition.z - Team1Targets[0].WorldPosition.z);

                var moveCost = Mathf.Abs((neighbors[i].WorldPosition - activeCell.WorldPosition).magnitude);
                var manhattan = xDelta + yDelta + zDelta;
                var cumulScore = moveCost + manhattan;

                neighborBucket.Add(new NeighborInfo((float)cumulScore, neighbors[i]));

            }
                neighborBucket = neighborBucket.OrderBy(m => m.getScore).ToList();
                var bestNeighbor = neighborBucket[Random.Range(0, 2)].getNeighbor;
                neighborCell = this.cells[bestNeighbor.Position.x, bestNeighbor.Position.y, bestNeighbor.Position.z];
        }

        //Pick best neighbor 
        else if (activeCell.Target == "T1")
        {
            List<NeighborInfo> neighborBucket = new List<NeighborInfo>();

            for (int i = 0; i < neighbors.Count; i++)
            {
                var xDelta = Mathf.Abs(neighbors[i].WorldPosition.x - Team2Targets[0].WorldPosition.x);
                var yDelta = Mathf.Abs(neighbors[i].WorldPosition.y - Team2Targets[0].WorldPosition.y);
                var zDelta = Mathf.Abs(neighbors[i].WorldPosition.z - Team2Targets[0].WorldPosition.z);

                var moveCost = Mathf.Abs((neighbors[i].WorldPosition - activeCell.WorldPosition).magnitude);
                var manhattan = xDelta + yDelta + zDelta;
                var cumulScore = moveCost + manhattan;

                neighborBucket.Add(new NeighborInfo((float)cumulScore, neighbors[i]));
            }
                neighborBucket = neighborBucket.OrderBy(m => m.getScore).ToList();

                var bestNeighbor = neighborBucket[Random.Range(0, 2)].getNeighbor;
                neighborCell = this.cells[bestNeighbor.Position.x, bestNeighbor.Position.y, bestNeighbor.Position.z];

        }

        //Change state logic
        if (activeCell.Target == "T2")
        {
            if (neighborCell.IsActive == true)
            {
                activeCell.IsActive = true;
                activeCell.DisplayCell.GetComponent<MeshRenderer>().enabled = true;
                activeCell.Target = "T2";

                if (activeCell.WorldPosition == Team1Targets[0].WorldPosition)
                {
                    Team1Targets.RemoveAt(0);
                    activeCell.DisplayCell.GetComponent<MeshRenderer>().material.color = Color.red;
                    activeCell.Target = "none";
                    activeCell.IsActive = true;
                }
            }

            else if (neighborCell.IsActive == false)
            {
                activeCell.IsActive = false;
                activeCell.DisplayCell.GetComponent<MeshRenderer>().material.color = cellMaterialInactive;
                activeCell.DisplayCell.GetComponent<MeshRenderer>().enabled = false;
                activeCell.Target = "none";

                //draw red line if it us a 'ladder jump', draw normal otherwise
                if ((activeCell.WorldPosition - neighborCell.WorldPosition).magnitude > 1.75)
                    DrawLine(activeCell.WorldPosition, neighborCell.WorldPosition, Color.red, 10);
                else
                    DrawLine(activeCell.WorldPosition, neighborCell.WorldPosition, team1Mat, 10);

                neighborCell.IsActive = true;
                neighborCell.DisplayCell.GetComponent<MeshRenderer>().material.color = team1Mat;
                neighborCell.DisplayCell.GetComponent<MeshRenderer>().enabled = true;
                neighborCell.Target = "T2";

                var strOutput = new MoveViz(activeCell, neighborCell, count);
                sw.WriteLine(strOutput.RetrieveMoveString());

                if (neighborCell.WorldPosition == Team1Targets[0].WorldPosition)
                {
                    Team1Targets.RemoveAt(0);
                    neighborCell.DisplayCell.GetComponent<MeshRenderer>().material.color = Color.red;
                    neighborCell.Target = "none";
                    neighborCell.IsActive = true;
                }
                countT1++;
            }
        }
        else if (activeCell.Target == "T1")
        {
            if (neighborCell.IsActive == true)
            {
                activeCell.IsActive = true;
                activeCell.DisplayCell.GetComponent<MeshRenderer>().enabled = true;
                activeCell.Target = "T1";

                if (activeCell.WorldPosition == Team2Targets[0].WorldPosition)
                {
                    Team2Targets.RemoveAt(0);
                    activeCell.DisplayCell.GetComponent<MeshRenderer>().material.color = Color.red;
                    activeCell.Target = "none";
                    activeCell.IsActive = true;
                }
            }
            else if (neighborCell.IsActive == false)
            {
                activeCell.IsActive = false;
                activeCell.DisplayCell.GetComponent<MeshRenderer>().enabled = false;
                activeCell.DisplayCell.GetComponent<MeshRenderer>().material.color = cellMaterialInactive;
                activeCell.Target = "none";

                //draw red line if it us a 'ladder jump', draw normal otherwise
                if ((activeCell.WorldPosition - neighborCell.WorldPosition).magnitude > 1.75)
                    DrawLine(activeCell.WorldPosition, neighborCell.WorldPosition, Color.red, 10);
                else
                    DrawLine(activeCell.WorldPosition, neighborCell.WorldPosition, team2Mat, 10);

                neighborCell.IsActive = true;
                neighborCell.DisplayCell.GetComponent<MeshRenderer>().material.color = team2Mat;
                neighborCell.DisplayCell.GetComponent<MeshRenderer>().enabled = true;
                neighborCell.Target = "T1";

                var strOutput = new MoveViz(activeCell, neighborCell, count);
                sw.WriteLine(strOutput.RetrieveMoveString());

                if (neighborCell.WorldPosition == Team2Targets[0].WorldPosition)
                {
                    Team2Targets.RemoveAt(0);
                    neighborCell.DisplayCell.GetComponent<MeshRenderer>().material.color = Color.red;
                    neighborCell.Target = "none";
                    neighborCell.IsActive = true;
                }
                countT2++;
            }
        }
        else
        {
            activeCell.IsActive = false;
            activeCell.DisplayCell.GetComponent<MeshRenderer>().material.color = cellMaterialInactive;
            activeCell.DisplayCell.GetComponent<MeshRenderer>().enabled = false;
            activeCell.Target = "none";
        }

        //sort targets from furthest to closest from opposing team
        Team1Targets = this.Team1Targets.OrderBy(m => (m.WorldPosition - new Vector3(0, 0, 0)).magnitude).ToList();
        Team2Targets = this.Team2Targets.OrderBy(m => (m.WorldPosition - new Vector3(gridSize.x, gridSize.y, gridSize.z)).magnitude).ToList();

        File.WriteAllText(@"C:\Users\MJ\Desktop\doc.txt", sw.ToString());

    }

    //Neighbor-finding function borrowed from Unity workshop early in the semester. I kept it as is becasue it finds all the neighbors I need (All 26 neighbors if you are in the center of a 3x3 grid neighborhood) 
    public List<Cell> GetNeighbors(Cell cell)
    {
        var neighbors = new List<Cell>();

        //treats edge of cube as proper boundary: no wrapping because 'position = 0' and 'gridsize - 1' are set to 0, so when at a boundary, you can only move away from the boundary.
        var lx = cell.Position.x == 0 ? 0 : -1;
        var ux = cell.Position.x == gridSize.x - 1 ? 0 : 1;
        var ly = cell.Position.y == 0 ? 0 : -1;
        var uy = cell.Position.y == gridSize.y - 1 ? 0 : 1;
        var lz = cell.Position.z == 0 ? 0 : -1;
        var uz = cell.Position.z == gridSize.z - 1 ? 0 : 1;

        for (int z = lz; z <= uz; z++)
            for (int y = ly; y <= uy; y++)
                for (int x = lx; x <= ux; x++)
                {
                    //ignores the cell if it's itself 
                    if (x == 0 && y == 0 && z == 0) continue;
                    var index = new Vector3Int(cell.Position.x + x, cell.Position.y + y, cell.Position.z + z);
                    neighbors.Add(cells[index.x, index.y, index.z]);
                }

        return neighbors;
    }

    //Ortho neighbors as required for 'ladder behavior"
    public List<Cell> GetOrthoNeighbors(Cell cell)
    {
        var neighbors = GetNeighbors(cell);
        List<Cell> orthoNeighbors = new List<Cell>();

        for (int i = 0; i < neighbors.Count; i++)
        {
            if ((Mathf.Abs((neighbors[i].WorldPosition.x + neighbors[i].WorldPosition.y + neighbors[i].WorldPosition.z) - (cell.WorldPosition.x + cell.WorldPosition.y + cell.WorldPosition.z)) == 1) && ((cell.WorldPosition.x == neighbors[i].WorldPosition.x && cell.WorldPosition.x == neighbors[i].WorldPosition.y) || (cell.WorldPosition.x == neighbors[i].WorldPosition.y && cell.WorldPosition.x == neighbors[i].WorldPosition.z) || (cell.WorldPosition.x == neighbors[i].WorldPosition.x && cell.WorldPosition.x == neighbors[i].WorldPosition.z)))
            {
                orthoNeighbors.Add(neighbors[i]);
            }
        }
        return orthoNeighbors;
    }

    //Retrieve manhattan distance between active soldier and target cell
    int GetManhattan(Cell Cell_A, Cell Cell_B)
    {
        float dX = Mathf.Abs(Cell_A.WorldPosition.x - Cell_B.WorldPosition.x);
        float dY = Mathf.Abs(Cell_A.WorldPosition.y - Cell_B.WorldPosition.y);
        float dZ = Mathf.Abs(Cell_A.WorldPosition.z - Cell_B.WorldPosition.z);

        return (int)(dX + dY + dZ);
    }

    //This function was found online. I modified it by adding a layer to which the GO should belong to
    //Draws the trajectory the GOs take
    void DrawLine(Vector3 start, Vector3 end, Color color, int layer)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.shadowCastingMode = new UnityEngine.Rendering.ShadowCastingMode();
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.04f;
        lr.endWidth = 0.04f;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        myLine.layer = layer;
    }

    //This function was found online. I modified it by adding a layer and a list to which the GO should belong to
    //Draws the 'cumulative distance remaining' graphic, the one which gets updated at every iteration
    void DrawAndAdd(Vector3 start, Vector3 end, Color color, int layer, List<GameObject> list)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.shadowCastingMode = new UnityEngine.Rendering.ShadowCastingMode();
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        myLine.layer = layer;

        list.Add(myLine);
    }

    //Cumulative distance to target 'destroy' function
    void DestroyLines()
    {
        List<GameObject> go = FindInActiveObjectByLayer(12);
        foreach (var thing in go)
            UnityEngine.GameObject.Destroy(thing);
    }

    //Function to identify inactive objects for their subsequent destruction(soldier GOs or distance lines corresponding to them)
    List<GameObject> FindInActiveObjectByLayer(int layer)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        List<GameObject> expiredLines = new List<GameObject>();
        for (int i = 0; i < objs.Length; i++)
        {
            // this logic is borrowed from online forum to identify objects within a certain layer
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].gameObject.layer == layer)
                {
                    expiredLines.Add(objs[i].gameObject);
                }
            }
        }
        return expiredLines;
    }

    //Color active GO proxies in a gradient from their original team color to white (white being closest to target)
    List<GameObject> ColorGOs(string teamTarget, List<Cell> targets, Color teamColor)
    {
        List<GameObject> proxy = new List<GameObject>();
        var team = getTeamCells(teamTarget);
        proxy.AddRange(team);

        foreach (var t in team)
        {
            float distanceApart = getSqrDistance(t.transform.position, targets[0].WorldPosition);

            float lerp = mapValue(distanceApart, 0, MaxDistance, 0f, 1f);

            Color lerpColor = Color.Lerp(Color.white, teamColor, lerp);
            t.GetComponent<Renderer>().material.color = lerpColor;
        }

        return proxy;
    }

    //Destroying inactive GO proxies(those which have arrived at their goal)
    void DestroyProxies()
    {
        List<GameObject> go = FindInActiveObjectByLayer(13);

        foreach (var thing in go)
            UnityEngine.GameObject.Destroy(thing);
    }

    List<GameObject> getTeamCells(string target)
    {
        List<GameObject> team = new List<GameObject>();

        foreach (var cell in cells)
        {
            if (cell.Target == target)
            {
                var pos = new Vector3(cell.WorldPosition.x, cell.WorldPosition.y, cell.WorldPosition.z);

                GameObject cube = GameObject.Instantiate(soldier, pos, Quaternion.identity);
                cube.layer = 13;

                cube.transform.localPosition = pos;
                cube.transform.localScale = new Vector3(0.9f, 0.9f, 0.95f);

                team.Add(cube);
        
            }
        }
        return team;
    }

    //Cumulative distance to target 'draw' function
    public int DrawDistLines(string TeamTarget, List<Cell> TeamTargetCells, Color TeamColor, List<GameObject> TeamList)
    {
        var dist = 0;
        foreach (var cell in cells)
        {
            if (cell.IsActive && cell.Target == TeamTarget)
            {
                if (TeamTargetCells.Count > 0)
                {
                    DrawAndAdd(cell.WorldPosition, TeamTargetCells[0].WorldPosition, TeamColor, 12, TeamList);
                    var distance = (int)(cell.WorldPosition - TeamTargetCells[0].WorldPosition).magnitude;
                    dist += distance;
                }
                else
                    continue;
            }
        }
        return dist;
    }

    //Max distance serves as the upper value of a domain used to color active GO proxies
    float FindMaxDist()
    {
        var diag1 = Mathf.Sqrt(Mathf.Pow(gridSize.x, 2) + Mathf.Pow(gridSize.y, 2));
        var diag2 = Mathf.Sqrt(Mathf.Pow(diag1, 2) + Mathf.Pow(gridSize.z, 2));

        return diag2;
    }

    //Euclidean distance between GO proxy and target used for coloring proxies
    float getSqrDistance(Vector3 v1, Vector3 v2)
    {
        return (v1 - v2).sqrMagnitude;
    }

    //Remapping function
    float mapValue(float mainValue, float inValueMin, float inValueMax, float outValueMin, float outValueMax)
    {
        return (mainValue - inValueMin) * (outValueMax - outValueMin) / (inValueMax - inValueMin) + outValueMin;
    }

    // Miscelaneous Functions to retrieve game statistics
    public Vector3 getCentroid()
    {
        return new Vector3(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);
    }
    public int getT1Count()
    {
        return countT1;
    }
    public int getT2Count()
    {
        return countT2;
    }
    public int getDistT1()
    {
        return DistT1;
    }
    public int getDistT2()
    {
        return DistT2;
    }
    public int getT2TargetRemaining()
    {
        return Team2Targets.Count;
    }
    public int getT1TargetRemaining()
    {
        return Team1Targets.Count;
    }
}

//Class containing neighbor and corresponding neighbor score, this is used to sort neighbors by their score
public class NeighborInfo
{
    float cumulScore;
    SimpleGrid.Cell neighbor;

    public float getScore
    {
        get { return cumulScore; }
    }

    public SimpleGrid.Cell getNeighbor
    {
        get { return neighbor; }
    }

    public NeighborInfo(float _score, SimpleGrid.Cell _neighbor)
    {
        cumulScore = _score;
        neighbor = _neighbor;
    }
}

//Class used to export CSV data in the form of: GO location pairs (to and from) and iteration(turn)
public class MoveViz
    {
        SimpleGrid.Cell Pt1;
        SimpleGrid.Cell Pt2;
        int Turn;

        public MoveViz(SimpleGrid.Cell pt1, SimpleGrid.Cell pt2, int turn)
        {
            Pt1 = pt1;
            Pt2 = pt2;
            Turn = turn;
        }

        public string RetrieveMoveString()
        {
            var tempString = Pt1.WorldPosition.ToString() + ": " + Pt2.WorldPosition.ToString() + ": " + Pt2.Target + ":" + Turn.ToString();
            return tempString;
        }
    }

//Class to contain pairs of cells, one corresponding to 'active' cell, the other corresponding to its 'preferred neighbor'
public class MoveInfo
{
    SimpleGrid.Cell prefNeighbor;
    SimpleGrid.Cell actingCell;

    public SimpleGrid.Cell getActingCell
    {
        get { return actingCell; }
    }

    public SimpleGrid.Cell getprefNeighbor
    {
        get { return prefNeighbor; }
    }

    public MoveInfo(SimpleGrid.Cell _actCell, SimpleGrid.Cell _prefNeighbor)
    {
        prefNeighbor = _prefNeighbor;
        actingCell = _actCell;
    }
}




