namespace Dealership.Contracts
{
    public interface IChainableCommandProcessor : ICommandProcessor
    {
        ICommandProcessor Successor { get; set; }
    }
}
