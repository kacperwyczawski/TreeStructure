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
    
    public Node GetNode(int id)
    {
        return _context.Nodes
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id);
    }
    
    public void UpdateNode(Node node)
    {
        _context.Nodes.Update(node);
        _context.SaveChanges();
    }
    
    public void DeleteNode(int id)
    {
        var node = _context.Nodes.FirstOrDefault(x => x.Id == id);
        
        if (node == null)
            return;
        
        _context.Nodes.Remove(node);
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