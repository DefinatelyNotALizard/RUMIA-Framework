using Godot;
using System;

//=MAN=: Make this an autoload then add:
//	R_SFXManager.Instance.StartSound("nun");
//to play the sound

public partial class R_SFXManager : Node
{
	private static string pathBase = "res://Assets/Audio/Sounds/";
	private const string extension = ".mp3";

	public static R_SFXManager Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public void StartSound(string name)
	{
		string fullPath = $"{pathBase}{name}{extension}";
		AudioStream sound = GD.Load<AudioStream>(fullPath);

		if (sound == null)
		{
			GD.PrintErr($"SFX: Could not load sound at path '{fullPath}'");
			return;
		}

		// Create and configure the AudioStreamPlayer
		AudioStreamPlayer speaker = new AudioStreamPlayer
		{
			Bus = "SFX",
			Stream = sound
		};

		AddChild(speaker);   // Add to the scene so it can play
		speaker.Play();      // Start playback
		speaker.Finished += () => speaker.QueueFree();  // Free when done
	}
}
