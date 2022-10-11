using Microsoft.EntityFrameworkCore;
using TreeStructure.Data;
using TreeStructure.Models;

namespace TreeStructure.Services;

public class NodeService
{
    private readonly NodesDbContext _context;

    private readonly ILogger<NodeService> _logger;

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

    public NodeService(NodesDbContext context, ILogger<NodeService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public enum Sort
    {
        Ascending,
        Descending,
        Custom
    }

    public void AddNode(Node node)
    {
        _logger.LogInformation("Add node {Node}", node);
        _context.Nodes.Add(node);
        
        _context.SaveChanges();
        
        // assign the node display index
        var displayIndex = GetSiblingNodes(node.Id).Count - 1;
        _logger.LogInformation("Assign display index {DisplayIndex} to node #{NodeId}",
            displayIndex, node.Id);
        node.DisplayIndex = displayIndex;
        
        // save again
        _context.SaveChanges();
    }

    public List<Node> GetSiblingNodes(int id)
    {
        var node = GetNode(id);

        return node.ParentId is null
            ? GetRootNodes()
            : _context.Nodes.Where(n => n.ParentId == node.ParentId).ToList();
    }

    public List<Node> GetChildren(int id)
    {
        _logger.LogInformation("Get children for node #{Id}", id);
        return _context.Nodes
            .AsNoTracking()
            .Where(x => x.ParentId == id).ToList();
    }
    
    public List<Node> GetChildren(int id, Sort sort)
    {
        var children = GetChildren(id);
        _logger.LogInformation("Sort children for node #{Id} by Sort.{Sort}", id, sort);
        return sort switch
        {
            Sort.Ascending => children.OrderBy(x => x.Name).ToList(),
            Sort.Descending => children.OrderByDescending(x => x.Name).ToList(),
            Sort.Custom => children.OrderBy(x => x.DisplayIndex).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null)
        };
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
        AddNode(pineapple);
        var lime = new Node { Name = "Lime", ParentId = null };
        AddNode(lime);
        
        // layer 1
        var apple = new Node { Name = "Apple", ParentId = pineapple.Id };
        AddNode(apple);
        var banana = new Node { Name = "Banana", ParentId = pineapple.Id };
        AddNode(banana);
        var orange = new Node { Name = "Orange", ParentId = pineapple.Id };
        AddNode(orange);
        var cherry = new Node { Name = "Cherry", ParentId = pineapple.Id };
        AddNode(cherry);

        var lemon = new Node { Name = "Lemon", ParentId = lime.Id };
        AddNode(lemon);
        var avocado = new Node { Name = "Avocado", ParentId = lime.Id };
        AddNode(avocado);
        var kiwi = new Node { Name = "Kiwi", ParentId = lime.Id };
        AddNode(kiwi);

        // layer 2
        var peach = new Node { Name = "Peach", ParentId = banana.Id };
        AddNode(peach);
        var pear = new Node { Name = "Pear", ParentId = banana.Id };
        AddNode(pear);
        var plum = new Node { Name = "Plum", ParentId = banana.Id };
        AddNode(plum);
        
        var grapefruit = new Node { Name = "Grapefruit", ParentId = lemon.Id };
        AddNode(grapefruit);
        var mango = new Node { Name = "Mango", ParentId = lemon.Id };
        AddNode(mango);

        // layer 3
        var strawberry = new Node { Name = "Strawberry", ParentId = pear.Id };
        AddNode(strawberry);
        var watermelon = new Node { Name = "Watermelon", ParentId = pear.Id };
        AddNode(watermelon);

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