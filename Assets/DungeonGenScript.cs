using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using TMPro;

[System.Serializable]
public class Link
{

    [SerializeField]
    public Rect[] link;
    public Link(Rect[] link)
    {
        this.link = link;
    }

}


public class DungeonGenScript : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap walls;
    public TileBase tile;
    public TileBase wallTile;
    public TileBase wallTile2;
    public TileBase hallwayTemp;
    public Rect[] roomDims;
    public Rect[] hallways;
    public List<Link> connections;
    public GameObject text;

    public Vector2Int GenerateDungeon(Vector2Int area, int roomAmt)
    {
        tilemap.ClearAllTiles();
        walls.ClearAllTiles();
        roomDims = GenerateRects(roomAmt,area);
        connections = new();
        hallways = new Rect[roomAmt*2];
        for (int i = 0; i < roomDims.Length; i++)
        {
            (Rect, Vector2) near = FindClosestRect(roomDims[i], roomDims);
            GenHallway(i, near.Item1, near.Item2, 0);
            if (CheckForProximity(hallways[i], roomDims, 0, new List<Rect> {near.Item1, roomDims[i]}))
            {
                hallways[i] = new Rect(0, 0, 0, 0);
            }
            (Rect, Vector2) near2 = FindClosestRect(roomDims[i], roomDims);
            GenHallway(i, near2.Item1, near2.Item2, roomAmt);
            if (CheckForProximity(hallways[i + roomAmt], roomDims, 0, new List<Rect> { near2.Item1, roomDims[i] }))
            {
                hallways[i + roomAmt] = new Rect(0, 0, 0, 0);
            }
        }
        BoxFill(walls, wallTile2, new Vector3Int(-area.x / 2 - 5, -area.y / 2 - 5), new Vector3Int(area.x / 2 + 5, area.y / 2 + 5));
        BoxFill(walls, wallTile, new Vector3Int(-area.x / 2, -area.y / 2), new Vector3Int(area.x / 2, area.y / 2));

        foreach (Rect rect in roomDims)
        {
            for (int x = (int)rect.x; x < rect.width + rect.x; x++)
            {
                for (int y = (int)rect.y; y < rect.height + rect.y; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                    walls.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
        for (int i = 0; i < hallways.Length; i++)
        {
            for (int x = (int)hallways[i].x; x < hallways[i].width + hallways[i].x; x++)
            {
                for (int y = (int)hallways[i].y; y < hallways[i].height + hallways[i].y; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                    walls.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
        tilemap.RefreshAllTiles();
        return new Vector2Int((int)roomDims[0].x+3, (int)roomDims[0].y+3);
    }
    public Rect[] GenerateRects(int rooms, Vector2Int area)
    {
        Rect[] rects = new Rect[rooms];
        for (int i = 0; i < rooms; i++)
        {
            // Generate a random Rect
            Rect newRect = GenerateRandomRect(area);

            // Check for overlap with previously generated Rects
            int attempts = 0;
            while (CheckForProximity(newRect, rects) && attempts < 5000)
            {
                newRect = GenerateRandomRect(area);
                attempts++;
            }

            // Add the non-overlapping Rect to the list
            rects[i] = newRect;
        }
        return rects;
    }

    Rect GenerateRandomRect(Vector2Int area)
    {
        // Calculate the maximum width and height based on one third of the area, rounded to an integer
        int maxAreaThirdWidth = Mathf.RoundToInt(area.x / 4f);
        int maxAreaThirdHeight = Mathf.RoundToInt(area.y / 4f);

        // Random width and height within the specified ranges
        int width = Mathf.RoundToInt(UnityEngine.Random.Range(5f, Mathf.Min(maxAreaThirdWidth, area.x / 2f)));
        int height = Mathf.RoundToInt(UnityEngine.Random.Range(5f, Mathf.Min(maxAreaThirdHeight, area.y / 2f)));

        // Random position within the area
        int x = Mathf.RoundToInt(UnityEngine.Random.Range(-area.x / 2f, area.x / 2f - width));
        int y = Mathf.RoundToInt(UnityEngine.Random.Range(-area.y / 2f, area.y / 2f - height));

        Rect randomRect = new(x, y, width, height);
        return randomRect;
    }

    bool CheckForProximity(Rect rectToCheck, Rect[] rects, int d = 3, List<Rect> ignore = null)
    {
        if (ignore == null)
        {
            ignore = new List<Rect> { };
        }
        foreach (Rect existingRect in rects)
        {
            // Calculate the distances between edges of rectToCheck and existingRect
            float distanceX = Mathf.Min(
                Mathf.Abs(rectToCheck.xMin - existingRect.xMax),
                Mathf.Abs(rectToCheck.xMax - existingRect.xMin)
            );

            float distanceY = Mathf.Min(
                Mathf.Abs(rectToCheck.yMin - existingRect.yMax),
                Mathf.Abs(rectToCheck.yMax - existingRect.yMin)
            );

            // Check if both distances are less than the proximity threshold
            if ((distanceX < d || distanceY < d || rectToCheck.Overlaps(existingRect)) && !ignore.Contains(existingRect))
            {
                return true; // Too close to an existing Rect
            }
        }
        return false; // No proximity found
    }
    (Rect, Vector2) FindClosestRect(Rect targetRect, Rect[] rects)
    {
        Rect closestRect = new();
        float minDistance = float.MaxValue;
        Vector2 direction = Vector2.zero;

        foreach (Rect rect in rects)
        {
            if (rect != targetRect && !GetConnections(targetRect).Contains(rect))
            {
                Vector2 centerToCenterDistance = new(
                rect.center.x - targetRect.center.x,
                rect.center.y - targetRect.center.y
                 );

                // Calculate the squared magnitude for efficiency (avoiding square root)
                float distanceSquared = centerToCenterDistance.sqrMagnitude;

                // Compare distances to find the closest rectangle
                if (distanceSquared < minDistance)
                {
                    minDistance = distanceSquared;
                    closestRect = rect;
                    direction = centerToCenterDistance.normalized;
                }
            }
            // Calculate the distance between centers of targetRect and rect
            
        }


            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);

            if (absX > absY)
            {
                direction =  direction.x > 0 ? Vector2.right : Vector2.left;
            }
            else if (absX < absY)
            {
                direction =  direction.y > 0 ? Vector2.up : Vector2.down;
            }

        return (closestRect, direction);
    }
    public void GenHallway(int i, Rect end, Vector2 dir, int add)
    {
        if (dir == Vector2.up || dir == Vector2.down)
        {

            float distanceY = Mathf.Min(
                Mathf.Abs(roomDims[i].yMin - end.yMax),
                Mathf.Abs(roomDims[i].yMax - end.yMin)
            );
            if (dir == Vector2.up)
            {
                hallways[i+add] = new Rect(roomDims[i].x, roomDims[i].yMax, 1, distanceY);
                AlignHallway(i + add, dir, roomDims[i], end);
            }
            else
            {
                hallways[i+add] = new Rect(roomDims[i].x, end.yMax, 1, distanceY);
                AlignHallway(i + add, dir, roomDims[i], end);
            }
            connections.Add(new Link( new Rect[] { roomDims[i], end }));
        }
        else if (dir == Vector2.right || dir == Vector2.left)
        {

            float distanceX = Mathf.Min(
                Mathf.Abs(roomDims[i].xMin - end.xMax),
                Mathf.Abs(roomDims[i].xMax - end.xMin)
            );
            if (dir == Vector2.right)
            {
                hallways[i+add] = new Rect(roomDims[i].xMax, roomDims[i].y, distanceX, 1);
                AlignHallway(i + add, dir, roomDims[i], end);
            }
            else
            {
                hallways[i+add] = new Rect(end.xMax, roomDims[i].y, distanceX, 1);
                AlignHallway(i + add, dir, roomDims[i], end);
            }
            connections.Add(new Link(new Rect[] { roomDims[i], end }));
        }
    }
    public List<Rect> GetConnections(Rect origin)
    {
        List<Rect> send = new();
        foreach (Link combo in connections)
        {
            if (combo.link[0] == origin)
            {
                send.Add(combo.link[1]);
            }
            else if (combo.link[1] == origin)
            {
                send.Add(combo.link[0]);
            }
        }
        return send;
    }
    public void AlignHallway(int i, Vector2 dir, Rect origin, Rect end)
    {

        if (dir == Vector2.up || dir == Vector2.down)
        {
            while (hallways[i].x < end.x + 1)
            {
                hallways[i].x += 1;
            }
            while (hallways[i].x < end.xMax - 2 && hallways[i].x < origin.x + 2)
            {
                hallways[i].x += 1;
            }
            Rect temp = new(hallways[i].x, hallways[i].y, 1, 1);
            if (!CheckForProximity(temp, roomDims, 0))
            {
                walls.SetTile(new Vector3Int((int)temp.x, (int)temp.y-1, 0), wallTile);
            }
            Rect temp2 = new(hallways[i].x, hallways[i].yMax, 1, 1);
            if (!CheckForProximity(temp2, roomDims, 0))
            {
                walls.SetTile(new Vector3Int((int)temp2.x, (int)temp2.y, 0), wallTile);
            }


        }
        if (dir == Vector2.left || dir == Vector2.right)
        {
            while (hallways[i].y < end.y + 1)
            {
                hallways[i].y += 1;
            }
            while (hallways[i].y < end.yMax - 2 && hallways[i].y < origin.y + 2)
            {
                hallways[i].y += 1;
            }
            Rect temp = new(hallways[i].x, hallways[i].y, 1, 1);
            if (!CheckForProximity(temp, roomDims, 0))
            {
                walls.SetTile(new Vector3Int((int)temp.x - 1, (int)temp.y, 0), wallTile);
            }
            Rect temp2 = new(hallways[i].xMax, hallways[i].y, 1, 1);
            if (!CheckForProximity(temp2, roomDims, 0))
            {
                walls.SetTile(new Vector3Int((int)temp2.x, (int)temp2.y, 0), wallTile);
            }


        }
    }

    public void BoxFill(Tilemap map, TileBase tile, Vector3Int start, Vector3Int end)
    {
        //Determine directions on X and Y axis
        var xDir = start.x < end.x ? 1 : -1;
        var yDir = start.y < end.y ? 1 : -1;
        //How many tiles on each axis?
        int xCols = 1 + Mathf.Abs(start.x - end.x);
        int yCols = 1 + Mathf.Abs(start.y - end.y);
        //Start painting
        for (var x = 0; x < xCols; x++)
        {
            for (var y = 0; y < yCols; y++)
            {
                var tilePos = start + new Vector3Int(x * xDir, y * yDir, 0);
                map.SetTile(tilePos, tile);
            }
        }
    }

}
