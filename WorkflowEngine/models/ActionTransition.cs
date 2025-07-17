namespace WorkflowEngine.Models;

public class ActionTransition
{
    public string Id { get; set; } = default!;
    public bool Enabled { get; set; } = true;
    public List<string> FromStates { get; set; } = new();
    public string ToState { get; set; } = default!;
}
