using Godot;
using System;
using System.Collections.Generic;


//===This is the tiles with which we build the button
public partial class Tile : Node2D{
	//===Set the tile type here
	public required int xType {get; set;}//0 = left, 2 = right
	public required int yType {get; set;}//0 = top, 2 = bottom

	//===The string that will be used to find the texture
	public string texturePath = "res://R_Assets/Button/Textures/";

	//===Completing the texture path based on the tile type, then add as child
	public void Setup(){
		if(yType == 0){
			texturePath += "top";
		}else if(yType == 1){
			texturePath += "centre";
		}else{
			texturePath += "bottom";
		}

		if(xType == 0){
			texturePath += "_left.png";
		}else if(xType == 1){
			texturePath += ".png";
		}else{
			texturePath += "_right.png";
		}

		var sprite = new Sprite2D();
		sprite.Texture = ResourceLoader.Load<Texture2D>(texturePath);
		AddChild(sprite);
	}
}

//===The actual button, as a Tool script because it runs also in the editor
[Tool] public partial class RUMIA_Button : Control{

	[Signal] public delegate void PressedEventHandler();


	[Export] public int R_Height {get; set;} = 0;
	[Export] public int R_Width {get; set;} = 0;
	[Export] public string R_Text {get; set;} = "";
	[Export] public bool R_disabled {get; set;} = false;

	private int _lastHeight;
	private int _lastWidth;
	private string _lastText;

	private const int tileSize = 16;//Tiles are ALWAYS 16px square

	private double _checkTimer = 0.0;
	private const double CheckInterval = 0.15;

	public override void _Ready()
	{
		// Build initially
		_lastWidth = R_Width;
		_lastHeight = R_Height;
		RebuildVisuals();
	}

	public override void _Process(double delta)
	{
		// Only in editor
		if (!Engine.IsEditorHint())
			return;

		_checkTimer += delta;
		if (_checkTimer < CheckInterval)
			return;

		_checkTimer = 0.0;

		if (R_Width != _lastWidth || R_Height != _lastHeight || R_Text != _lastText)
		{
			_lastWidth = R_Width;
			_lastHeight = R_Height;
			_lastText = R_Text;
			RebuildVisuals();
		}
	}

	public void RebuildVisuals(){
		_lastHeight = R_Height;
		_lastWidth = R_Width;

		//Disabled = R_disabled;:

		foreach(var child in GetChildren()){
			if(child is Tile tile){
				tile.QueueFree();
			}
			if(child is Button button){
				button.QueueFree();
			}
		}

		for(int y = 0; y < R_Height + 2; y++){
			for(int x = 0; x < R_Width + 2; x++){
				var tile = new Tile{
					xType = (x == 0) ? 0 : (x == R_Width + 1) ? 2 : 1,
					yType = (y == 0) ? 0 : (y == R_Height + 1) ? 2 : 1,
				};

				AddChild(tile);
				tile.Position = new Vector2(x * tileSize + 8, y * tileSize + 8);
				tile.Setup();
			}
		}
		
		var clickSurface = new Button
		{
			Text = R_Text,
			Position = new Vector2(0, 0),
			Size = new Vector2((R_Width + 2) * tileSize, (R_Height + 2) * tileSize),
			MouseFilter = MouseFilterEnum.Stop,
			FocusMode = FocusModeEnum.None,
			Flat = true, // <-- makes it invisible, no default skin


		};

		var font = new FontFile();
		font = ResourceLoader.Load<FontFile>("res://R_Assets/BigBlueTerm437NerdFont-Regular.ttf");

		var theme = new Theme();
		theme.SetFont("font", "Button", font);
		theme.SetFontSize("font_size", "Button", 24);
		theme.SetColor("font_color", "Button", new Color(0, 0, 0)); // red text


		clickSurface.Theme = theme;




		// Connect the Button pressed signal to our custom signal
		clickSurface.Pressed += () => EmitSignal(SignalName.Pressed);



		AddChild(clickSurface);
	}

	

}
