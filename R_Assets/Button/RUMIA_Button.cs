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

		//===Load the path we built into a sprite and add as child
		var sprite = new Sprite2D();
		sprite.Texture = ResourceLoader.Load<Texture2D>(texturePath);
		AddChild(sprite);
	}
}

//===The actual button, as a Tool script because it runs also in the editor
[Tool] public partial class RUMIA_Button : Control{
	//===Defines a signal in the editor that we can then connect to something
	[Signal] public delegate void PressedEventHandler();
	//=MAN==How to connect the button to something:
	// - In the editor, got to the Node menu and connect Pressed to the script of choice
	// - Copy the name of the function from the editor and put it in the class of choice
	// - Make it a private void (I don't think anything else will be usefull ever but maybe)
	// - Put some code in it 
	// - Done

	//===Button characteristics
	[Export] public int R_Height {get; set;} = 0;
	[Export] public int R_Width {get; set;} = 0;
	[Export] public string R_Text {get; set;} = "";
	[Export] public bool R_disabled {get; set;} = false;

	//===Save the current values so we can check for changes
	private int _lastHeight;
	private int _lastWidth;
	private string _lastText;

	private const int tileSize = 16;//Tiles are ALWAYS 16px square

	//===Defines how long we'll wait between auto-refreshes
	private double _checkTimer = 0.0;
	private const double CheckInterval = 0.15;

	//===Runs on init
	public override void _Ready(){
		//===We'll set the values on load and cause a rebuild
		_lastWidth = R_Width;
		_lastHeight = R_Height;
		_lastText = R_Text;
		RebuildVisuals();
	}

	//===Runs every frame (non physics)
	public override void _Process(double delta){
		//=NTS=The following checks if we're running in the editor, else exits
		if (!Engine.IsEditorHint())
			return;

		//===The following exits if the timer hasn't yet met the refresh interval
		_checkTimer += delta;
		if (_checkTimer < CheckInterval)
			return;
		_checkTimer = 0.0;//===If it has we reset it and proceed to the next part

		//===We check if anything changed, if yes then update variables and rebuild
		if (R_Width != _lastWidth || R_Height != _lastHeight || R_Text != _lastText){
			_lastWidth = R_Width;
			_lastHeight = R_Height;
			_lastText = R_Text;
			RebuildVisuals();
		}
	}

	//===This defines what happens upon rebuild
	public void RebuildVisuals(){
		//===We nuke all current children in order to rebuild fresh
		foreach(var child in GetChildren()){
			if(child is Tile tile){
				tile.QueueFree();
			}
			if(child is Button button){
				button.QueueFree();
			}
		}

		//===This constructs the button by creating tiles and adding as children
		for(int y = 0; y < R_Height + 2; y++){
			for(int x = 0; x < R_Width + 2; x++){
				var tile = new Tile{//Create a tile and set it's type
					xType = (x == 0) ? 0 : (x == R_Width + 1) ? 2 : 1,
					yType = (y == 0) ? 0 : (y == R_Height + 1) ? 2 : 1,
				};

				AddChild(tile);
				tile.Position = new Vector2(x * tileSize + 8, y * tileSize + 8);
				tile.Setup();
			}
		}
		
		//===Creates the actual clickable button
		var clickSurface = new Button{
			Text = R_Text,
			Position = new Vector2(0, 0),
			Size = new Vector2((R_Width + 2) * tileSize, (R_Height + 2) * tileSize),
			MouseFilter = MouseFilterEnum.Stop,
			FocusMode = FocusModeEnum.None,
			Flat = true, //Makes it invisible, no default skin
		};

		//===Get the font ready
		var font = ResourceLoader.Load<FontFile>("res://R_Assets/R_font.ttf");

		//===Create a theme for the font
		var theme = new Theme();
		theme.SetFont("font", "Button", font);
		theme.SetFontSize("font_size", "Button", 24);
		theme.SetColor("font_color", "Button", new Color(0, 0, 0));

		//===Put the theme on the button
		clickSurface.Theme = theme;

		//===Connect the Button pressed signal to our custom signal
		clickSurface.Pressed += () => EmitSignal(SignalName.Pressed);

		//===Add the button to the Rumia button
		AddChild(clickSurface);
	}
}
