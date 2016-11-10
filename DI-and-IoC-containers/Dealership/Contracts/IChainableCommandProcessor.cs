namespace Dealership.Contracts
{
    public interface IChainableCommandProcessor : ICommandProcessor
    {
        IChainableCommandProcessor Successor { get; set; }
    }
}
