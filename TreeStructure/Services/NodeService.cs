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
        // assign the node display index
        node.DisplayIndex = GetNextDisplayIndex(node.ParentId);
        
        _logger.LogInformation("Add node {Node} with assigned display index {DisplayIndex}",
            node, node.DisplayIndex);
        _context.Nodes.Add(node);
        _context.SaveChanges();
    }
    
    private int GetNextDisplayIndex(int? parentId)
    {
        var maxDisplayIndex = _context.Nodes
            .Where(n => n.ParentId == parentId)
            .Max(n => n.DisplayIndex);
        return maxDisplayIndex + 1;
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
        
        var node = _context.Nodes
            .AsNoTracking()
            .SingleOrDefault(x => x.Id == id);

        if (node is not null)
            return node;
        
        _logger.LogWarning("Node #{Id} not found", id);
        throw new Exception($"Node #{id} not found");

    }
    
    private Node GetNodeWithTracking(int id)
    {
        _logger.LogInformation("Get node #{Id} with tracking", id);
        
        var node = _context.Nodes
            .SingleOrDefault(x => x.Id == id);

        if (node is not null)
            return node;
        
        _logger.LogWarning("Node #{Id} not found", id);
        throw new Exception($"Node #{id} not found");

    }
    
    public void RenameNode(int id, string name)
    {
        _logger.LogInformation("Rename node #{Id} to {NewName}", id, name);
        
        var node = GetNodeWithTracking(id);

        node.Name = name;
        _context.SaveChanges();
    }
    
    public void ChangeParent(int id, int? newParentId)
    {
        _logger.LogInformation("Change parent of node #{Id} to #{NewParentId}", id, newParentId);
        
        if (id == newParentId)
        {
            _logger.LogWarning("Cannot change parent to itself");
            return;
        }

        var node = GetNodeWithTracking(id);

        if (node.ParentId == newParentId)
        {
            _logger.LogInformation("New parent id is the same as the current one");
            return;
        }

        node.ParentId = newParentId;
        _context.SaveChanges();
    }

    public void DeleteNodeRecursively(int id)
    {
        _logger.LogInformation("Delete node #{Id} with children", id);

        var node = GetNodeWithTracking(id);

        _context.Nodes.Remove(node);
        
        if (GetChildren(node.Id) is var children && children.Any())
        {
            foreach (var child in children)
            {
                DeleteNodeRecursively(child.Id);
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
        
        // layer 0 (roots)
        var pineapple = new Node { Name = "Pineapple", ParentId = null };
        _context.Nodes.Add(pineapple);
        var lime = new Node { Name = "Lime", ParentId = null };
        _context.Nodes.Add(lime);
        _context.SaveChanges();
        
        // layer 1
        var apple = new Node { Name = "Apple", ParentId = pineapple.Id };
        _context.Nodes.Add(apple);
        var banana = new Node { Name = "Banana", ParentId = pineapple.Id };
        _context.Nodes.Add(banana);
        var orange = new Node { Name = "Orange", ParentId = pineapple.Id };
        _context.Nodes.Add(orange);
        var cherry = new Node { Name = "Cherry", ParentId = pineapple.Id };
        _context.Nodes.Add(cherry);
        
        var lemon = new Node { Name = "Lemon", ParentId = lime.Id };
        _context.Nodes.Add(lemon);
        _context.SaveChanges();
        
        // layer 2
        var peach = new Node { Name = "Peach", ParentId = banana.Id };
        _context.Nodes.Add(peach);
        var pear = new Node { Name = "Pear", ParentId = banana.Id };
        _context.Nodes.Add(pear);
        
        var grapefruit = new Node { Name = "Grapefruit", ParentId = lemon.Id };
        _context.Nodes.Add(grapefruit);
        var mango = new Node { Name = "Mango", ParentId = lemon.Id };
        _context.Nodes.Add(mango);
        _context.SaveChanges();
        
        // layer 3
        var strawberry = new Node { Name = "Strawberry", ParentId = pear.Id };
        _context.Nodes.Add(strawberry);
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
    
    public string GetName(int id)
    {
        _logger.LogInformation("Get name of node #{Id}", id);
        return _context.Nodes
            .AsNoTracking()
            .Single(n => n.Id == id).Name;
    }
}