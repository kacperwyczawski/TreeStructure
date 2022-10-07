using Microsoft.EntityFrameworkCore;
using TreeStructure.Data;
using TreeStructure.Models;

namespace TreeStructure.Services;

public class NodeService
{
    private readonly NodesDbContext _context;

    private readonly ILogger<NodeService> _logger;

    public NodeService(NodesDbContext context, ILogger<NodeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void AddNode(Node node)
    {
        _logger.LogInformation("Add node {Node}", node);
        _context.Nodes.Add(node);
        _context.SaveChanges();
    }

    public List<Node> GetChildren(int id)
    {
        _logger.LogInformation("Get children for node #{Id}", id);
        return _context.Nodes
            .AsNoTracking()
            .Where(x => x.ParentId == id).ToList();
    }

    public bool HasChildren(int id)
    {
        _logger.LogInformation("Check if node #{Id} has children", id);
        return _context.Nodes
            .AsNoTracking()
            .Any(x => x.ParentId == id);
    }
    
    public bool Exists(int id)
    {
        _logger.LogInformation("Check if node #{Id} exists", id);
        return _context.Nodes
            .AsNoTracking()
            .Any(x => x.Id == id);
    }

    public Node GetNode(int id)
    {
        _logger.LogInformation("Get node #{Id}", id);
        return _context.Nodes
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id);
    }

    public void RenameNode(int id, string name)
    {
        _logger.LogInformation("Rename node #{Id} to {Name}", id, name);
        var oldNode = GetNode(id);
        var newNode = oldNode with { Name = name };

        _context.Nodes.Remove(oldNode);
        _context.Nodes.Add(newNode);
        _context.SaveChanges();
    }

    public void ChangeParent(Node node, int? newParentId)
    {
        if (node.Id == newParentId)
        {
            _logger.LogWarning("Cannot change parent to itself");
            return;
        }
        
        if (node.ParentId == newParentId)
        {
            _logger.LogInformation("New parent id is the same as the current one");
            return;
        }
        
        _logger.LogInformation("Change parent of node {Node} form #{OldParentId} to #{NewParentId}",
            node, node.ParentId, newParentId);
        
        var newNode = node with { ParentId = newParentId };
        
        _context.Nodes.Remove(node);
        _context.Nodes.Add(newNode);
        _context.SaveChanges();
    }

    public void DeleteNodeWithChildren(Node? node)
    {
        if (node is null)
            return;

        _logger.LogInformation("Delete node {Node}", node);

        _context.Nodes.Remove(node);

        if (GetChildren((int)node.Id) is var children && children.Any())
        {
            foreach (var child in children)
            {
                DeleteNodeWithChildren(child);
            }
        }

        _context.SaveChanges();
    }

    public void DeleteAllNodes()
    {
        _logger.LogInformation("Delete all nodes");
        _context.Nodes.RemoveRange(_context.Nodes);
        _context.SaveChanges();
    }

    public void Seed()
    {
        _logger.LogInformation("Seed database");
        _context.Nodes.AddRange(new List<Node>
        {
            new("Pineapple", 0, null),
            new("Apple", 1, 0),
            new("Banana", 2, 0),
            new("Orange", 3, 0),
            new("Cherry", 12, 0),
            new("Peach", 4, 2),
            new("Pear", 5, 2),
            new("Strawberry", 6, 5),

            new("Lime", 7, null),
            new("Lemon", 8, 7),
            new("Grapefruit", 9, 8),
            new("Mango", 10, 8),
            new("Papaya", 11, 7),
        });
        _context.SaveChanges();
    }

    public List<Node> GetAllNodes()
    {
        _logger.LogInformation("Get all nodes");
        return _context.Nodes
            .AsNoTracking()
            .ToList();
    }

    public List<Node> GetRootNodes()
    {
        _logger.LogInformation("Get root nodes");
        return _context.Nodes
            .AsNoTracking()
            .Where(x => x.ParentId == null).ToList();
    }
}