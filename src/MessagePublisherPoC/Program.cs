using SolaceSystems.Solclient.Messaging;

namespace MessagePublisherPoC;

public class Program
{
    private const string Host = "tcp://localhost:55554";
    private const string VPNName = "default";
    private const string UserName = "default";
    private const string Password  = "default";
    private const string QueueName = "testQueue";
    private const string SampleTransactionPath = "../../../../sampleTransaction.json";
    private const int NumberOfMessageToProduce = 100_000;

    public static void Main(string[] args)
    {
        // Initialize Solace Systems Messaging API with logging to console at Warning level
        ContextFactoryProperties cfp = new ContextFactoryProperties()
        {
            SolClientLogLevel = SolLogLevel.Warning
        };
        cfp.LogToConsoleError();
        ContextFactory.Instance.Init(cfp);

        try
        {
            // Context must be created first
            using (IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null))
            {
                // Create the application
                QueueProducer queueProducer = new QueueProducer()
                {
                    VPNName = VPNName,
                    UserName = UserName,
                    Password = Password,
                    QueueName = QueueName,
                };

                var messageContent = File.ReadAllText(SampleTransactionPath);

                // Run the application within the context and against the host
                queueProducer.Run(context, Host, NumberOfMessageToProduce, messageContent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception thrown: {0}", ex.Message);
        }
        finally
        {
            // Dispose Solace Systems Messaging API
            ContextFactory.Instance.Cleanup();
        }
        Console.WriteLine("Finished.");
    }
}
