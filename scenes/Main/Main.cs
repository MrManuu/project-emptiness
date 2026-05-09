#nullable enable
using Godot;

namespace ProjectEmptiness.Scenes;

public partial class Main : Node
{
    private static readonly PackedScene GalaxyMapScene =
        GD.Load<PackedScene>("res://scenes/GalaxyMap/GalaxyMap.tscn");

    private Node? _currentScene;

    public override void _Ready()
    {
        LoadScene(GalaxyMapScene);
    }

    public void LoadScene(PackedScene scene)
    {
        _currentScene?.QueueFree();
        _currentScene = scene.Instantiate();
        AddChild(_currentScene);
    }
}
