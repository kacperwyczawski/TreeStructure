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
        Custom,
        CustomReversed
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
    
    public void MoveUp(int id)
    {
        _logger.LogInformation("Move node #{Id} up", id);
        var node = GetNodeWithTracking(id);
        var displayIndex = node.DisplayIndex;
        
        if (displayIndex == 0)
        {
            _logger.LogWarning("Node #{Id} is already at the top", id);
            return;
        }
        
        var siblings = GetSiblingNodes(node.Id);
        var nodeAbove = siblings.Single(n => n.DisplayIndex == displayIndex - 1);
        
        // swap display indexes
        node.DisplayIndex--;
        nodeAbove.DisplayIndex++;
        
        _context.SaveChanges();
    }
    
    public void MoveDown(int id)
    {
        _logger.LogInformation("Move node #{Id} down", id);
        var node = GetNodeWithTracking(id);
        var displayIndex = node.DisplayIndex;
        
        var siblings = GetSiblingNodes(node.Id);
        if (displayIndex == siblings.Count - 1)
        {
            _logger.LogWarning("Node #{Id} is already at the bottom", id);
            return;
        }
        
        var nodeBelow = siblings.Single(n => n.DisplayIndex == displayIndex + 1);
        
        // swap display indexes
        node.DisplayIndex++;
        nodeBelow.DisplayIndex--;
        
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
    
    public async Task<List<int>> GetChildrenIdsAsync(int id)
    {
        _logger.LogInformation("Get children for node #{Id}", id);
        return await _context.Nodes
            .AsNoTracking()
            .Where(n => n.ParentId == id)
            .Select(n => n.Id)
            .ToListAsync();
    }
    
    public List<int> GetChildrenIds(int id, Sort sort)
    {
        _logger.LogInformation("Get children for node #{Id} sorted by Sort.{Sort}", id, sort);

        var children = _context.Nodes
            .AsNoTracking()
            .Where(n => n.ParentId == id);
        
        var sortedChildren = sort switch
        {
            Sort.Ascending => children.OrderBy(n => n.Name),
            Sort.Descending => children.OrderByDescending(n => n.Name),
            Sort.Custom => children.OrderBy(n => n.DisplayIndex),
            Sort.CustomReversed => children.OrderByDescending(n => n.DisplayIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null)
        };
        
        return sortedChildren.Select(n => n.Id).ToList();
    }
    
    public List<Node> GetChildren(int id, Sort sort)
    {
        _logger.LogInformation("Sort children for node #{Id} by Sort.{Sort}", id, sort);
        var children = GetChildren(id);
        return sort switch
        {
            Sort.Ascending => children.OrderBy(x => x.Name).ToList(),
            Sort.Descending => children.OrderByDescending(x => x.Name).ToList(),
            Sort.Custom => children.OrderBy(x => x.DisplayIndex).ToList(),
            Sort.CustomReversed => children.OrderByDescending(x => x.DisplayIndex).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null)
        };
    }

    public List<Node> GetRootNodes()
    {
        _logger.LogInformation("Get root nodes");
        return _context.Nodes
            .Where(x => x.ParentId == null).ToList();
    }
    
    public List<Node> GetRootNodes(Sort sort)
    {
        var rootNodes = GetRootNodes();
        _logger.LogInformation("Sort root nodes by Sort.{Sort}", sort);
        return sort switch
        {
            Sort.Ascending => rootNodes.OrderBy(x => x.Name).ToList(),
            Sort.Descending => rootNodes.OrderByDescending(x => x.Name).ToList(),
            Sort.Custom => rootNodes.OrderBy(x => x.DisplayIndex).ToList(),
            Sort.CustomReversed => rootNodes.OrderByDescending(x => x.DisplayIndex).ToList(),
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

    public Result MoveToAnotherParent(int id, int? newParentId)
    {
        if (newParentId is null)
            _logger.LogInformation("Move node #{Id} to root", id);
        else
            _logger.LogInformation("Move node #{Id} to parent #{NewParentId}", id, newParentId);
        
        if (id == newParentId)
        {
            _logger.LogWarning("Cannot move to itself");
            return new Result(false, "Cannot move to itself");
        }
        
        var node = GetNodeWithTracking(id);
        
        if (node.ParentId == newParentId)
        {
            _logger.LogInformation("New parent id is the same as the current one");
            return new Result(false, "New parent id is the same as the current one");
        }
        
        // prevent moving to descendants
        // using stack and loop instead of recursion
        var stack = new Stack<int>();
        stack.Push(id);
        while (stack.Count > 0)
        {
            var nodeId = stack.Pop();
            var children = GetChildrenIds(nodeId, Sort.Custom);
            foreach (var childId in children)
            {
                if (childId == newParentId)
                {
                    _logger.LogWarning("Cannot move node #{Id} to its own descendant #{NewParentId}",
                        id, newParentId);
                    return new Result(false, $"Cannot move node #{id} to its own descendant #{newParentId}");
                }
                stack.Push(childId);
            }
        }

        node.ParentId = newParentId;

        _context.SaveChanges();
        
        return new Result(true, $"Successfully moved node #{id} to parent #{newParentId}");
    }

    public async Task DeleteNodeRecursivelyAsync(int id)
    {
        _logger.LogInformation("Delete node #{Id} with children", id);

        var node = await _context.Nodes.FindAsync(id);
        
        if (node is null)
        {
            _logger.LogWarning("Node #{Id} not found", id);
            return;
        }

        _context.Nodes.Remove(node);

        if (await GetChildrenIdsAsync(node.Id) is var children && children.Any())
        {
            foreach (var childId in children)
            {
                await DeleteNodeRecursivelyAsync(childId);
            }
        }

        await _context.SaveChangesAsync();
    }

    public void DeleteAllNodes()
    {
        _logger.LogInformation("Delete all nodes");
        _context.Nodes.RemoveRange(_context.Nodes);
        _context.SaveChanges();
    }

    public void Seed()
    {
        DeleteAllNodes();
        
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

    public string GetName(int id)
    {
        _logger.LogInformation("Get name of node #{Id}", id);
        return _context.Nodes
            .AsNoTracking()
            .Single(n => n.Id == id).Name;
    }
    
    public class Result
    {
        public Result(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}