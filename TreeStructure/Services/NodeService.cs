using Microsoft.EntityFrameworkCore;
using TreeStructure.Data;
using TreeStructure.Models;

namespace TreeStructure.Services;

public class NodeService
{
    private readonly NodesDbContext _context;
    
    public NodeService(NodesDbContext context)
    {
        _context = context;
    }
    
    public void AddNode(Node node)
    {
        _context.Nodes.Add(node);
        _context.SaveChanges();
    }
    
    public List<Node> GetChildren(int id)
    {
        return _context.Nodes
            .AsNoTracking()
            .Where(x => x.ParentId == id).ToList();
    }
    
    public bool HasChildren(int id)
    {
        return _context.Nodes
            .AsNoTracking()
            .Any(x => x.ParentId == id);
    }
    
    public Node GetNode(int id)
    {
        return _context.Nodes
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id);
    }
    
    public void RenameNode(int id, string name)
    {
        var oldNode = GetNode(id);
        var newNode = oldNode with { Name = name };

        _context.Nodes.Remove(oldNode);
        _context.Nodes.Add(newNode);
        _context.SaveChanges();
    }
    
    public void DeleteNodeWithChildren(Node? node)
    {
        if (node is null)
            return;

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
        _context.Nodes.RemoveRange(_context.Nodes);
        _context.SaveChanges();
    }
    
    public void Seed()
    {
        _context.Nodes.AddRange(new List<Node>
        {
            new ("Pineapple", 0, null),
            new ("Apple", 1, 0),
            new ("Banana", 2, 0),
            new ("Orange", 3, 0),
            new ("Peach", 4, 2),
            new ("Pear", 5, 2),
            
            new ("Lime", 6, null),
            new ("Lemon", 7, 6),
            new ("Grapefruit", 8, 6),
            new ("Mango", 9, 7)
        });
        _context.SaveChanges();
    }
    
    public List<Node> GetAllNodes()
    {
        return _context.Nodes
            .AsNoTracking()
            .ToList();
    }

    public List<Node> GetRootNodes()
    {
            return _context.Nodes
            .AsNoTracking()
            .Where(x => x.ParentId == null).ToList();
    }
}