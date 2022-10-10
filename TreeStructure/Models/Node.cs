namespace TreeStructure.Models;

public class Node
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ParentId { get; set; }

    public override string ToString() => $"{Name} #{Id} with parent #{ParentId}";
}