using Godot;
using System;

//===This is the tiles with which we build the slider
public partial class SliderTile : Node2D{
	//===Set the tile type here
	public required int xType {get; set;}//0 = left, 2 = right

	//===The string that will be used to find the texture
	public string texturePath = "res://R_Assets/Slider/Textures/";

	public void Setup() {
		if(xType == 0) texturePath += "left.png";
		else if(xType == 1) texturePath += "middle.png";
		else texturePath += "right.png";

		//===Load the path we built into a sprite and add as child
		var sprite = new Sprite2D();
		sprite.Texture = ResourceLoader.Load<Texture2D>(texturePath);
		AddChild(sprite);
	}
}

//===The actual slider, as a Tool script because it runs also in the editor
[Tool] public partial class RUMIA_Slider : Control {
	//===Slider characteristics
	[Export] public int R_Width { get; set; } = 100;
	[Export] public bool R_disabled { get; set; } = false;
	[Export] public int R_Value = 0;

	//===Save the current values so we can check for changes
	private int _lastWidth;
	private int _lastValue;

	private const int tileSize = 4;//Tiles are ALWAYS 4px square

	private double _checkTimer = 0.0;
	private const double CheckInterval = 0.15;

	//===Runs on init
	public override void _Ready() {
		//===We'll set the values on load and cause a rebuild
		_lastWidth = R_Width;
		_lastValue = R_Value;
		RebuildVisuals();
	}

	//===Runs every frame (non physics)
	public override void _Process(double delta) {
		//=NTS=The following checks if we're running in the editor, else exits
		if(!Engine.IsEditorHint())
			return;

		//===The following exits if the timer hasn't yet met the refresh interval
		_checkTimer += delta;
		if(_checkTimer < CheckInterval)
			return;
		_checkTimer = 0.0;//===If it has we reset it and proceed to the next part

		//===We check if anything changed, if yes then update variables and rebuild
		if(R_Width != _lastWidth || R_Value != _lastValue) {
			_lastWidth = R_Width;
			_lastValue = R_Value;
			RebuildVisuals();
		}
	}

	//===This defines what happens upon rebuild
	public void RebuildVisuals() {
		//===We nuke all current children in order to rebuild fresh
		foreach(var child in GetChildren()) {
			if(child is Tile tile) tile.QueueFree();
		}

		for(int x = 0; x < R_Width; x += tileSize) {
			var tile = new SliderTile {// If the firts tile (x = 0) -> 0, if the last tile (width + 1) -> 2, else -> 1
				xType = (x == 0) ? 0 : (x == R_Width + 1) ? 2 : 1,
			};

			AddChild(tile);
			tile.Position = new Vector2(x * tileSize + 8, 0); // MAYBE NEEDS TO BE CHANGED FROM 0 TO SOMETHING ELSE
			tile.Setup();
		}
	}
}