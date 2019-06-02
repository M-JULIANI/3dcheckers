using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class SimpleGrid
{

    public class Cell
    {
        //This class largely borrows from the Unity sprint early in the semester
        //I added the property parent 'target'
        SimpleGrid _grid;
        public Vector3Int Position { get; set; }
        public Vector3 WorldPosition { get; set; }
        public GameObject DisplayCell { get; set; }
        public string Target { get; set; }
        public bool IsActive { get; set; }

        //This functionality was added by you in order to link this class with SimpleGrid class
        public Cell(SimpleGrid grid)
        {
            _grid = grid;
        }

    }
}