namespace TreeStructure.Models;

public class Node
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public List<Node> Childrens { get; set; }
}