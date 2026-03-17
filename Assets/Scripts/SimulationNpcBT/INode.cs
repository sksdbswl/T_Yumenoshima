using AI.BT.Runtime;

/// <summary>
/// Node의 상태와 노드가 어떤 상태인지를 반환하는 인터페이스
/// </summary>
public interface INode
{
    public string Guid { get; }
    
    public ENodeState Evaluate();
}