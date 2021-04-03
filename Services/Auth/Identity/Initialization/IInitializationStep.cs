namespace Auth.Identity.Initialization
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IInitializationStep
    {
        public Task Initialize(CancellationToken cancellationToken);
    }
}