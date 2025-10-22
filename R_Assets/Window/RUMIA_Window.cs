using Godot;
using System;

//===This is the tile with whiwh we build the window
public partial class WindowTile : Node2D{
	//===Set the tile type here
	public required int xType { get; set; } // 0 = left, 2 = right
	public required int yType { get; set; } // 0 = top, 4 = bottom

	//===The string that will be used to find the texture
	public string texturePath = "res://R_Assets/Window/Textures/";

	//===Complete the texture path based on the tile type then add as child
	public void Setup()
	{
		// Build path from type
		if (yType == 0)
			texturePath += "bar_top";
		else if (yType == 1)
			texturePath += "bar_centre";
		else if (yType == 2)
			texturePath += "seam_centre";
		else if (yType == 3)
			texturePath += "window_centre";
		else
			texturePath += "window_bottom";

		if (xType == 0)
			texturePath += "_left.png";
		else if (xType == 1)
			texturePath += ".png";
		else
			texturePath += "_right.png";

		//===Load the path we built into a sprite and add as child
		var sprite = new Sprite2D();
		sprite.Texture = ResourceLoader.Load<Texture2D>(texturePath);
		AddChild(sprite);
	}
}

//===The actual Window, as a Tool script because it runs in the editor
[Tool] public partial class RUMIA_Window : Control{
	//===Window characteristics
	[Export] public int R_Height { get; set; } = 4;
	[Export] public int R_Width { get; set; } = 6;
	[Export] public int R_Winbar_Height { get; set; } = 1;
	[Export] public string R_Title { get; set; } = "Window Title";

	//===Save the current values so we can check for changes
	private int _lastHeight;
	private int _lastWidth;
	private string _lastTitle;

	private const int tileSize = 16;//Tiles are ALWAYS 16px square

	//===Defines how long we'll wait between auto-refreshes
	private double _checkTimer = 0.0;
	private const double CheckInterval = 0.15;

	//===Runs on init
	public override void _Ready(){
		//===We'll set the values on load and cause a rebuild
		_lastWidth = R_Width;
		_lastHeight = R_Height;
		RebuildVisuals();
	}

	//===Runs every frame (non-physics)
	public override void _Process(double delta){
		//===Check if running in editor, else exit
		if (!Engine.IsEditorHint())
			return;

		//===Check if the timer has met the refresh timer, else exit
		_checkTimer += delta;
		if (_checkTimer < CheckInterval)
			return;
		_checkTimer = 0.0;//If met then reset and proceed

		//===We check if anything changed, if yes then update variables and rebuild
		if (R_Width != _lastWidth || R_Height != _lastHeight || R_Title != _lastTitle){
			_lastWidth = R_Width;
			_lastHeight = R_Height;
			_lastTitle = R_Title;
			RebuildVisuals();
		}
	}

	//===Defines what happens on rebuild
	public void RebuildVisuals(){
		//===Nuke all current children in order to rebuild fresh
		foreach (var child in GetChildren()){
			if (child is WindowTile tile)
				tile.QueueFree();
			if (child is Label label)
				label.QueueFree();
		}

		//===This constructs the button by creating tiles and adding as children
		for (int y = 0; y < R_Height + 4; y++){
			for (int x = 0; x < R_Width + 2; x++){
				var tile = new WindowTile{//Create a Tile then set it's type
					xType = (x == 0) ? 0 : (x == R_Width + 1) ? 2 : 1,
					yType = (y == 0) ? 0
						: (y < R_Winbar_Height + 1) ? 1
						: (y == R_Winbar_Height + 1) ? 2
						: (y < R_Height + 3) ? 3 : 4,
				};

				AddChild(tile);
				tile.Position = new Vector2(x * tileSize + 8, y * tileSize + 8);
				tile.Setup();
			}
		}

		//===Add Title Lable (Centred)
		var titleLabel = new Label{
			Text = R_Title,
			Position = new Vector2(0, 5 + (8 * R_Winbar_Height)),//Makes the text always centred
			Size = new Vector2((R_Width + 2) * tileSize, 24),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};

		//===Get the font ready
		var font = ResourceLoader.Load<FontFile>("res://R_Assets/R_font.ttf");
		var theme = new Theme();

		//===Create a theme for the font
		theme.SetFont("font", "Label", font);
		theme.SetFontSize("font_size", "Label", 24);
		theme.SetColor("font_color", "Label", new Color(0, 0, 0));

		//===Apply the theme to the text
		titleLabel.Theme = theme;

		//===Add the text as a child
		AddChild(titleLabel);
	}
}
