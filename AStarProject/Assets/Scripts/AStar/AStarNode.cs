using UnityEngine;
using System.Collections;
using Pathing;
using System;
using System.Collections.Generic;

public class AStarNode : IAStarNode
{
    private GameObject hex;
    private Vector2 position;
    private float cost;
    private IEnumerable<IAStarNode> neighbours;

    public GameObject Hex
    {
        get { return hex; }
        set { hex = value; }
    }
    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

    public float Cost
    {
        get { return cost; }
        set { cost = value; }
    }

    public IEnumerable<IAStarNode> Neighbours
    {
        get { return neighbours; }
        set { neighbours = value; }
    }

    public override string ToString()
    {
        return "Position: " + Position.x + " , " + Position.y +  "   Cost: " + Cost;
    }

    public float CostTo(IAStarNode neighbour)
    {
        AStarNode aStarNode = (AStarNode)neighbour;
        return aStarNode.Cost;
    }

    // Using Manhattan distance
    public float EstimatedCostTo(IAStarNode goal)
    {
        AStarNode aStarNodeGoal = (AStarNode)goal;
        float dx = aStarNodeGoal.Position.x - this.Position.x;
        float dy = aStarNodeGoal.Position.y - this.Position.y;

        if (Math.Sign(dx) == Math.Sign(dy))
            return Math.Abs(dx + dy);
        else
            return Math.Max(Math.Abs(dx), Math.Abs(dy));
    }

}
