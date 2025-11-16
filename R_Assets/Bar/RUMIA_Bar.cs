using Godot;
using System;

//===Tile class, same as before, builds the individual pieces of the bar
public partial class BarTile : Node2D
{
    //===Tile type: 0=left/top, 1=center, 2=right/bottom
    public required int xType { get; set; }
    public required int yType { get; set; }

    //===Base texture path
    public string texturePath = "res://R_Assets/Bar/Textures/";

    //===Setup builds the texture string and adds a Sprite2D child
    public void Setup()
    {
        //===Determine y part of the texture
        if (yType == 0)
            texturePath += "top";
        else if (yType == 1)
            texturePath += "centre";
        else
            texturePath += "bottom";

        //===Determine x part of the texture
        if (xType == 0)
            texturePath += "_left.png";
        else if (xType == 1)
            texturePath += ".png";
        else
            texturePath += "_right.png";

        //===Create sprite and load texture
        var sprite = new Sprite2D();
        sprite.Texture = ResourceLoader.Load<Texture2D>(texturePath);
        AddChild(sprite);
    }
}

//===The actual bar class
[Tool]
public partial class RUMIA_Bar : CanvasLayer
{
    //===Tiles along main axis (width for horizontal, height for vertical)
    [Export] public int BarWidth { get; set; } = 10;
    //===Tiles along cross axis (height for horizontal, width for vertical)
    [Export] public int BarHeight { get; set; } = 1;

    //===Orientation of the bar
    public enum OrientationType { Horizontal, Vertical }
    [Export] public OrientationType Orientation { get; set; } = OrientationType.Horizontal;

    //===Alignment along main axis
    public enum AlignmentType { Start, Center, End }
    [Export] public AlignmentType Alignment { get; set; } = AlignmentType.Start;

    //===Vertical/Horizontal position on screen
    public enum PositionType { Top, Center, Bottom }
    [Export] public PositionType Position { get; set; } = PositionType.Bottom;

    //===Tile size in pixels
    private const int TileSize = 16;

    //===Editor tracking variables for auto-refresh
    private int _lastWidth, _lastHeight;
    private OrientationType _lastOrientation;
    private AlignmentType _lastAlignment;
    private PositionType _lastPosition;

    //===Editor refresh timer
    private double _checkTimer = 0.0;
    private const double CheckInterval = 0.2;

    //===Runs when the node is initialized
    public override void _Ready()
    {
        //===Save current orientation for editor flip detection
        _lastOrientation = Orientation;

        //===Build bar immediately
        RebuildBar();
        SaveLastValues();
    }

    //===Runs every frame in editor for auto-refresh
    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return; //===Only run in editor

        //===If orientation changed, swap width/height in inspector
        if (_lastOrientation != Orientation)
        {
            int temp = BarWidth;
            BarWidth = BarHeight;
            BarHeight = temp;

            _lastOrientation = Orientation;

            RebuildBar();
            return; //===Done, skip other checks
        }

        //===Editor auto-refresh timer
        _checkTimer += delta;
        if (_checkTimer < CheckInterval) return;
        _checkTimer = 0.0;

        //===Check if any property changed, rebuild if yes
        if (PropertiesChanged())
        {
            SaveLastValues();
            RebuildBar();
        }
    }

    //===Save current property values for change detection
    private void SaveLastValues()
    {
        _lastWidth = BarWidth;
        _lastHeight = BarHeight;
        _lastOrientation = Orientation;
        _lastAlignment = Alignment;
        _lastPosition = Position;
    }

    //===Check if any property changed since last save
    private bool PropertiesChanged()
    {
        return _lastWidth != BarWidth ||
               _lastHeight != BarHeight ||
               _lastOrientation != Orientation ||
               _lastAlignment != Alignment ||
               _lastPosition != Position;
    }

    //===Builds the bar visually
    private void RebuildBar()
    {
        //===Remove old tiles and container
        foreach (var child in GetChildren())
            if (child is BarTile || child is Control)
                child.QueueFree();

        //===Determine main and cross tiles depending on orientation
        int mainTiles = Orientation == OrientationType.Horizontal ? BarWidth : BarHeight;
        int crossTiles = Orientation == OrientationType.Horizontal ? BarHeight : BarWidth;

        int totalWidth = Orientation == OrientationType.Horizontal ? mainTiles + 2 : crossTiles + 2;
        int totalHeight = Orientation == OrientationType.Horizontal ? crossTiles + 2 : mainTiles + 2;

        //===Container for all tiles
        var container = new Control();
        AddChild(container);
        container.Position = Vector2.Zero;
        container.Size = new Vector2(totalWidth * TileSize, totalHeight * TileSize);

        //===Get viewport size for screen-relative positioning
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;
        Vector2 basePos = Vector2.Zero;

        //===Calculate screen position based on orientation
        if (Orientation == OrientationType.Horizontal)
        {
            // Vertical position
            basePos.Y = Position switch
            {
                PositionType.Top => 0,
                PositionType.Center => screenSize.Y / 2 - container.Size.Y / 2,
                _ => screenSize.Y - container.Size.Y
            };
            // Horizontal alignment
            basePos.X = Alignment switch
            {
                AlignmentType.Start => 0,
                AlignmentType.Center => screenSize.X / 2 - container.Size.X / 2,
                _ => screenSize.X - container.Size.X
            };
        }
        else // Vertical
        {
            // Horizontal position
            basePos.X = Position switch
            {
                PositionType.Top => 0,
                PositionType.Center => screenSize.X / 2 - container.Size.X / 2,
                _ => screenSize.X - container.Size.X
            };
            // Vertical alignment
            basePos.Y = Alignment switch
            {
                AlignmentType.Start => 0,
                AlignmentType.Center => screenSize.Y / 2 - container.Size.Y / 2,
                _ => screenSize.Y - container.Size.Y
            };
        }

        container.Position = basePos;

        //===Build each tile
        for (int y = 0; y < totalHeight; y++)
        {
            for (int x = 0; x < totalWidth; x++)
            {
                var tile = new BarTile
                {
                    xType = (x == 0) ? 0 : (x == totalWidth - 1) ? 2 : 1,
                    yType = (y == 0) ? 0 : (y == totalHeight - 1) ? 2 : 1
                };
                container.AddChild(tile);
                tile.Position = new Vector2(x * TileSize, y * TileSize);
                tile.Setup();
            }
        }
    }
}
