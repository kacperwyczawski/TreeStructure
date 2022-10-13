using Microsoft.AspNetCore.Components;
using MudBlazor;
using TreeStructure.Models;
using TreeStructure.Services;

namespace TreeStructure.Components;

public partial class TreeViewItem : ComponentBase
{
    [Inject] private NodeService NodeServiceInjected { get; set; } = default!;
    [Parameter] [EditorRequired] public int NodeId { get; set; }

    [Parameter] public EventCallback<bool> OnIsExpandedChanged { get; set; }

    [Parameter] public EventCallback OnNodesChanged { get; set; }

    [CascadingParameter] public NodeService.Sort Sort { get; set; }

    private bool CanMoveUp => NodeServiceInjected.GetNode(NodeId).DisplayIndex > 0;

    private bool CanMoveDown => NodeServiceInjected.GetNode(NodeId).DisplayIndex <
                                NodeServiceInjected.GetSiblingNodes(NodeId).Count - 1;

    private bool _isExpanded;

    private string ExpandCollapseIcon => NodeServiceInjected.HasChildren(NodeId)
        ? _isExpanded
            ? Icons.Material.Outlined.KeyboardArrowDown // if has children and is expanded
            : Icons.Material.Outlined.KeyboardArrowRight // if has children and is collapsed
        : Icons.Material.Outlined.KeyboardArrowRight; // if has no children

    private bool _isEditing;

    private bool _isMoving;

    private string? _newName;

    private Node? _newParent;

    private void Rename()
    {
        if (_newName == null)
            return;

        NodeServiceInjected.RenameNode(NodeId, _newName);
        _newName = null;
        _isEditing = false;
    }

    private async Task AddChildAsync()
    {
        var parameters = new DialogParameters { { "ParentId", NodeId } };

        var dialog = DialogService.Show<AddNodeDialog>("Add child", parameters);

        var result = await dialog.Result;

        if (!result.Cancelled)
            await OnNodesChanged.InvokeAsync();
    }

    private async Task DeleteNodeAsync()
    {
        // if the node has children, ask for confirmation
        if (NodeServiceInjected.HasChildren(NodeId))
        {
            var result = await DialogService.ShowMessageBox("Delete node",
                "This node has children. Are you sure you want to delete it?",
                yesText: "Delete", cancelText: "Cancel");

            // if the user clicked cancel, return
            if (result is null)
                return;
        }

        // if user didn't cancel or the node has no children, delete the node
        await NodeServiceInjected.DeleteNodeRecursivelyAsync(NodeId);

        await OnNodesChanged.InvokeAsync();
    }

    protected override bool ShouldRender()
    {
        return base.ShouldRender() && NodeServiceInjected.Exists(NodeId);
    }

    private void MoveUp()
    {
        Logger.LogInformation("Moving node {NodeId} up", NodeId);
        NodeServiceInjected.MoveUp(NodeId);
        OnNodesChanged.InvokeAsync();
    }

    private void MoveDown()
    {
        Logger.LogInformation("Moving node {NodeId} down", NodeId);
        NodeServiceInjected.MoveDown(NodeId);
        OnNodesChanged.InvokeAsync();
    }

    private async Task<IEnumerable<Node>> SearchNodes(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return NodeServiceInjected.GetAllNodes();

        return NodeServiceInjected
            .GetAllNodes()
            .Where(n => n.Name
                .Contains(searchText, StringComparison.InvariantCultureIgnoreCase));
    }

    private void MoveNode()
    {
        if (_newParent == null)
            return;

        NodeServiceInjected.MoveToAnotherParent(NodeId, _newParent.Id);
        
        // reset fields
        _newParent = null;
        _isMoving = false;
        
        // refresh the tree
        OnNodesChanged.InvokeAsync();
    }
}