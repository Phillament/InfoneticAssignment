using WorkflowEngine.Models;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Register WorkflowService
builder.Services.AddSingleton<WorkflowService>();

var app = builder.Build();

// Health check
app.MapGet("/", () => "Hello World!");

// POST /workflow - Add a workflow definition
app.MapPost("/workflow", (WorkflowDefinition def, WorkflowService service) =>
{
    if (service.AddDefinition(def, out var error))
        return Results.Ok(new { message = "Workflow definition added successfully." });

    return Results.BadRequest(new { error });
});

// POST /instance/{definitionId} - Start a new workflow instance
app.MapPost("/instance/{definitionId}", (string definitionId, WorkflowService service) =>
{
    var instance = service.StartInstance(definitionId);
    if (instance == null)
        return Results.BadRequest(new { error = "Workflow definition not found." });

    return Results.Ok(instance);
});

// GET /instance/{instanceId} - Get current state and history
app.MapGet("/instance/{instanceId}", (string instanceId, WorkflowService service) =>
{
    var instance = service.GetInstance(instanceId);
    if (instance == null)
        return Results.NotFound(new { error = "Workflow instance not found." });

    return Results.Ok(instance);
});

// POST /instance/{instanceId}/action/{actionId} - Execute action
app.MapPost("/instance/{instanceId}/action/{actionId}", (string instanceId, string actionId, WorkflowService service) =>
{
    if (service.ExecuteAction(instanceId, actionId, out var error))
        return Results.Ok(new { message = $"Action '{actionId}' executed successfully." });

    return Results.BadRequest(new { error });
});

app.Run();