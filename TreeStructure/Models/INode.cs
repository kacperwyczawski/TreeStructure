namespace TreeStructure.Models;

public interface INode
{
    int Id { get; set; }
    int DisplayIndex { get; set; }
    string Name { get; set; }
    int? ParentId { get; set; }
    string ToString();
}