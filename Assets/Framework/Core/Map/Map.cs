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

public class Room
{
    public RectInt bounds;

    public Room(RectInt bounds)
    {
        this.bounds = bounds;
    }
}

public class Floor
{
    class TileData
    {
        public float heat;
        public float lastHeated;

        public TileData()
        {
            heat = 0f;
            lastHeated = 20f;
        }
    }
    
    private Tile[,] _tiles;
    private TileData[,] _tileData;
    private Tilemap _tileMap;
    private List<Room> rooms = new(20);
    private FloorStats stats;
    
    public Floor(int width, int height, GameObject floorRef, FloorStats stats)
    {
        _tiles = new Tile[width, height];
        _tileData = new TileData[width, height];
        for (int i = 0; i < _tileData.GetLength(0); ++i)
        {
            for (int j = 0; j < _tileData.GetLength(1); ++j)
            {
                _tileData[i, j] = new TileData();
            }
        }
        
        _tileMap = GameObject.Instantiate(floorRef, GameMan.inst.transform).GetComponent<Tilemap>();
        this.stats = stats;
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
    public void GenerateRooms()
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
                
                /* visualization
                BoxCollider2D col = GameMan.inst.gameObject.AddComponent<BoxCollider2D>();
                col.offset = current.bounds.center;
                col.size = current.bounds.size;
                //*/
                
                // Rooms
                int w = (current.bounds.size.x + 2) / 2;
                int h = (current.bounds.size.y + 2) / 2;
                int roomWidth = Random.Range(w, Math.Max(current.bounds.size.x - 1, w));
                int roomHeight = Random.Range(h, Math.Max(current.bounds.size.y - 1, h));

                int roomOffsetX = Random.Range(1, current.bounds.size.x - roomWidth + 1);
                int roomOffsetY = Random.Range(1, current.bounds.size.y - roomHeight + 1);
                
                // add to rooms list
                rooms.Add(new Room(new RectInt(current.bounds.x + roomOffsetX, current.bounds.y + roomOffsetY, roomWidth, roomHeight)));

                for (int x = 0; x < roomWidth; ++x)
                {
                    for (int y = 0; y < roomHeight; ++y)
                    {
                        int newX = current.bounds.x + x + roomOffsetX;
                        int newY = current.bounds.y + y + roomOffsetY;
                        _tiles[newX, newY] = Tile.Floor;
                        var pos = new Vector3Int(newX, newY);
                        _tileMap.SetTile(pos, stats.floorTile);
                        _tileMap.SetTileFlags(pos, TileFlags.None);
                        //_tileMap.SetTransformMatrix();
                        //_tileMap.SetColor(pos, new Color(v, v, v, 1f));
                        //t.color = new Color(Random.value, Random.value, Random.value, 1);
                        //t.transform = Matrix4x4.Translate(Random.value * Vector2.up);
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
        
        // Mark tiles left as "outside"
        for (int x = -1; x <= _tiles.GetLength(0); ++x)
        {
            for (int y = -1; y <= _tiles.GetLength(1); ++y)
            {
                if ((x < 0 || x >= _tiles.GetLength(0) || y < 0 || y >= _tiles.GetLength(1)) || _tiles[x, y] == Tile.None)
                {
                    _tileMap.SetTile(new Vector3Int(x, y), stats.outsideTile);
                }
            }
        }
    }
    public Room GetRoom(Vector2Int pos)
    {
        for (int i = 0; i < rooms.Count; ++i)
        {
            if (rooms[i].bounds.Contains(pos)) return rooms[i];
        }
        return null;
    }
    
    // * Closest in terms of CENTER
    public Room GetClosestRoom(Vector2 pos)
    {
        float closestDist = float.MaxValue;
        Room closestRoom = null;
        
        for (int i = 0; i < rooms.Count; ++i)
        {
            float x = pos.x - rooms[i].bounds.center.x;
            float y = pos.y - rooms[i].bounds.center.y;
            float d = x * x + y * y;
            if (d < closestDist)
            {
                closestDist = d;
                closestRoom = rooms[i];
            }
        }

        return closestRoom;
    }

    public void UpdateTile(Vector3Int pos)
    {
        _tileMap.SetColor(pos, stats.heatGradient.Evaluate(_tileData[pos.x, pos.y].heat));
    }
    
    public void HeatTile(Vector3Int pos, float delta)
    {
        _tileData[pos.x, pos.y].heat = Mathf.MoveTowards(_tileData[pos.x, pos.y].heat, 1f, delta);
        _tileData[pos.x, pos.y].lastHeated = Time.time;
        UpdateTile(pos);
    }

    public void Update(float delta)
    {
        for (int x = 0; x < _tiles.GetLength(0); ++x)
        {
            for (int y = 0; y < _tiles.GetLength(1); ++y)
            {
                if (_tileData[x, y].heat > 0.001f)
                {
                    _tileData[x, y].heat = Mathf.MoveTowards(_tileData[x, y].heat, 0f, delta * 0.01f);
                    Vector3Int pos = new Vector3Int(x, y);
                    _tileMap.SetTransformMatrix(pos,
                        Matrix4x4.Translate(Vector3.up * stats.tileBounceAnim.Evaluate(Time.time - _tileData[x, y].lastHeated)));
                    UpdateTile(pos);
                }
            }
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
        Floor newFloor = new Floor(stats.floorSize, stats.floorSize, stats.floorTilemap, stats);
        newFloor.GenerateRooms();
        _floors.Add(newFloor);
    }

    public Floor GetFloor(int level) => _floors[level];
    public void Update(float delta)
    {
        for (int i = 0; i < _floors.Count; ++i)
        {
            _floors[i].Update(delta);
        }
    }
}
