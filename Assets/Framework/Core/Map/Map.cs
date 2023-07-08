using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public enum Tile
{
    None = 0, // (default)
    Floor = 1,
    Wall = 2,
    Door = 3,
    Hall = 4
}

public class Floor
{
    private Tile[,] _tiles;
    private Tilemap _tileMap;
    
    public Floor(int width, int height, GameObject floorRef)
    {
        _tiles = new Tile[width, height];
        _tileMap = GameObject.Instantiate(floorRef, GameMan.inst.transform).GetComponent<Tilemap>();
    }

    public class BSPNode
    {
        public RectInt bounds { private set; get; }
        public BSPNode a { private set; get; }
        public BSPNode b { private set; get; }

        public BSPNode(int minX, int minY, int width, int height)
        {
            bounds = new RectInt(minX, minY, width, height);   
        }
        public int GetArea() => bounds.size.x * bounds.size.y;
        public void Divide()
        {
            if (bounds.width > bounds.height)
            {
                int aWidth = (int)(Random.Range(0.25f, 0.75f) * bounds.width);
                a = new BSPNode(bounds.xMin, bounds.yMin, aWidth, bounds.height);
                b = new BSPNode(bounds.xMin + aWidth, bounds.yMin, bounds.width - aWidth, bounds.height);
            }
            else
            {
                int aHeight = (int)(Random.Range(0.25f, 0.75f) * bounds.height);
                a = new BSPNode(bounds.xMin, bounds.yMin, bounds.width, aHeight);
                b = new BSPNode(bounds.xMin, bounds.yMin + aHeight, bounds.width, bounds.height - aHeight);
            }
        }
    }

    public void GenerateRooms(FloorStats stats)
    {
        BSPNode root = new BSPNode(0, 0, _tiles.GetLength(0), _tiles.GetLength(1));
        Queue<BSPNode> nodes = new Queue<BSPNode>();
        nodes.Enqueue(root);

        while (nodes.Count > 0)
        {
            BSPNode current = nodes.Dequeue();
            if (current.GetArea() > stats.roomSize)
            {
                current.Divide();
                nodes.Enqueue(current.a);
                nodes.Enqueue(current.b);
            }
            else
            {
                // Valid room
                
                // visualization
                BoxCollider2D col = GameMan.inst.gameObject.AddComponent<BoxCollider2D>();
                col.offset = current.bounds.center;
                col.size = current.bounds.size;

                // Rooms
                int w = (current.bounds.size.x + 2) / 2;
                int h = (current.bounds.size.y + 2) / 2;
                int roomWidth = Random.Range(w, Math.Max(current.bounds.size.x - 1, w));
                int roomHeight = Random.Range(h, Math.Max(current.bounds.size.y - 1, h));

                int roomOffsetX = Random.Range(1, current.bounds.size.x - roomWidth + 1);
                int roomOffsetY = Random.Range(1, current.bounds.size.y - roomHeight + 1);

                for (int x = 0; x < roomWidth; ++x)
                {
                    for (int y = 0; y < roomHeight; ++y)
                    {
                        int newX = current.bounds.xMin + x + roomOffsetX;
                        int newY = current.bounds.yMin + y + roomOffsetY;
                        _tiles[newX, newY] = Tile.Floor;
                        _tileMap.SetTile(new Vector3Int(newX, newY), stats.floorTile);
                    }
                }
            }
        }
        
        // Halls now
        nodes.Enqueue(root);
        while (nodes.Count > 0)
        {
            BSPNode current = nodes.Dequeue();
            if (current.a == null) continue;
            
            if (current.bounds.width > current.bounds.height)
            {
                for (int x = (int)current.a.bounds.center.x; x < current.b.bounds.center.x; ++x)
                {
                    int tileY = (int)current.bounds.center.y;
                    if (_tiles[x, tileY] == (int)Tile.None)
                    {
                        _tiles[x, tileY] = Tile.Hall;
                        _tileMap.SetTile(new Vector3Int(x, tileY), stats.wallTile);
                    }
                }
            }
            else
            {
                for (int y = (int)current.a.bounds.center.y; y < current.b.bounds.center.y; ++y)
                {
                    int tileX = (int)current.bounds.center.x;
                    if (_tiles[tileX, y] == (int)Tile.None)
                    {
                        _tiles[tileX, y] = Tile.Hall;
                        _tileMap.SetTile(new Vector3Int(tileX, y), stats.wallTile);
                    }
                }
            }
            nodes.Enqueue(current.a);
            nodes.Enqueue(current.b);
        }
    }
}

public class Map
{
    private List<Floor> _floors = new(5);
    
    public Map()
    {
    }
    
    public void AddFloor(FloorStats stats)
    {
        // base floor
        Floor newFloor = new Floor(stats.floorSize, stats.floorSize, stats.floorTilemap);
        newFloor.GenerateRooms(stats);
        _floors.Add(newFloor);
    }
}
