using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class WorkflowService
{
    private readonly Dictionary<string, WorkflowDefinition> _definitions = new();
    private readonly Dictionary<string, WorkflowInstance> _instances = new();

    public bool AddDefinition(WorkflowDefinition def, out string error)
    {
        error = "";

        if (_definitions.ContainsKey(def.Id))
        {
            error = "Definition with this ID already exists.";
            return false;
        }

        if (def.States.Count(s => s.IsInitial) != 1)
        {
            error = "Exactly one initial state is required.";
            return false;
        }

        var stateIds = def.States.Select(s => s.Id).ToHashSet();

        foreach (var a in def.Actions)
        {
            if (!stateIds.Contains(a.ToState) || a.FromStates.Any(s => !stateIds.Contains(s)))
            {
                error = $"Transition '{a.Id}' refers to invalid state(s).";
                return false;
            }
        }

        _definitions[def.Id] = def;
        return true;
    }

    public WorkflowDefinition? GetDefinition(string id) =>
        _definitions.TryGetValue(id, out var def) ? def : null;

    public WorkflowInstance? StartInstance(string definitionId)
    {
        if (!_definitions.TryGetValue(definitionId, out var def)) return null;

        var initialState = def.States.First(s => s.IsInitial);
        var instance = new WorkflowInstance
        {
            DefinitionId = def.Id,
            CurrentStateId = initialState.Id
        };
        _instances[instance.Id] = instance;
        return instance;
    }

    public WorkflowInstance? GetInstance(string id) =>
        _instances.TryGetValue(id, out var inst) ? inst : null;

    public bool ExecuteAction(string instanceId, string actionId, out string error)
    {
        error = "";
        if (!_instances.TryGetValue(instanceId, out var inst)) {
            error = "Instance not found.";
            return false;
        }

        var def = _definitions[inst.DefinitionId];
        var action = def.Actions.FirstOrDefault(a => a.Id == actionId);

        if (action == null) {
            error = "Action not found.";
            return false;
        }

        if (!action.Enabled) {
            error = "Action is disabled.";
            return false;
        }

        var currentState = def.States.First(s => s.Id == inst.CurrentStateId);
        if (currentState.IsFinal)
        {
            error = "Cannot execute action from a final state.";
            return false;
        }

        if (!action.FromStates.Contains(currentState.Id))
        {
            error = "Current state not allowed for this action.";
            return false;
        }

        inst.CurrentStateId = action.ToState;
        inst.History.Add((actionId, DateTime.UtcNow));
        return true;
    }
}
