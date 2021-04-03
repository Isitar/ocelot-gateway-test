namespace Auth.Identity.Initialization
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Initializer
    {
        private readonly IEnumerable<IInitializationStep> initializationSteps;


        public Initializer(IEnumerable<IInitializationStep> initializationSteps)
        {
            this.initializationSteps = initializationSteps;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var initializationStep in initializationSteps)
            {
                await initializationStep.Initialize(cancellationToken);
            }
        }
    }
}