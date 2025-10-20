using Godot;
using System;

public partial class WindowTile : Node2D
{
	public required int xType { get; set; } // 0 = left, 2 = right
	public required int yType { get; set; } // 0 = top, 4 = bottom

	public string texturePath = "res://R_Assets/Window/Textures/";

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

		// Load texture and create sprite
		var sprite = new Sprite2D();
		sprite.Texture = ResourceLoader.Load<Texture2D>(texturePath);
		AddChild(sprite);
	}
}


[Tool]
public partial class RUMIA_Window : Control
{
	[Export] public int R_Height { get; set; } = 4;
	[Export] public int R_Width { get; set; } = 6;
	[Export] public int R_Winbar_Height { get; set; } = 1;
	[Export] public string R_Title { get; set; } = "Window Title";

	private int _lastHeight;
	private int _lastWidth;
	private string _lastTitle;

	private const int tileSize = 16;
	private double _checkTimer = 0.0;
	private const double CheckInterval = 0.15;

	public override void _Ready()
	{
		_lastWidth = R_Width;
		_lastHeight = R_Height;
		RebuildVisuals();
	}

	public override void _Process(double delta)
	{
		if (!Engine.IsEditorHint())
			return;

		_checkTimer += delta;
		if (_checkTimer < CheckInterval)
			return;

		_checkTimer = 0.0;

		if (R_Width != _lastWidth || R_Height != _lastHeight || R_Title != _lastTitle)
		{
			_lastWidth = R_Width;
			_lastHeight = R_Height;
			_lastTitle = R_Title;
			RebuildVisuals();
		}
	}

	public void RebuildVisuals()
	{
		// Clean up previous children
		foreach (var child in GetChildren())
		{
			if (child is WindowTile tile)
				tile.QueueFree();
			if (child is Label label)
				label.QueueFree();
		}

		// Build tiles
		for (int y = 0; y < R_Height + 4; y++)
		{
			for (int x = 0; x < R_Width + 2; x++)
			{
				var tile = new WindowTile
				{
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

		Size = new Vector2((R_Width + 2) * tileSize, (R_Height + 4) * tileSize);

		// Add title label (centered)
		var titleLabel = new Label
		{
			Text = R_Title,
			Position = new Vector2(0, 0),
			Size = new Vector2((R_Width + 2) * tileSize, 24),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};

		var font = ResourceLoader.Load<FontFile>("res://R_Assets/BigBlueTerm437NerdFont-Regular.ttf");
		var theme = new Theme();
		theme.SetFont("font", "Label", font);
		theme.SetFontSize("font_size", "Label", 20);
		theme.SetColor("font_color", "Label", new Color(0, 0, 0));

		titleLabel.Theme = theme;
		AddChild(titleLabel);
	}
}
