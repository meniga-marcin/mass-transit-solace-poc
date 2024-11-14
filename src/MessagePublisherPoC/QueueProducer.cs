using System.Text;
using SolaceSystems.Solclient.Messaging;

namespace MessagePublisherPoC;

/// <summary>
/// Demonstrates how to use Solace Systems Messaging API for sending and receiving a guaranteed delivery message
/// </summary>
public class QueueProducer
{
    public string VPNName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string QueueName { get; set; }

    const int DefaultReconnectRetries = 3;

    public void Run(IContext context, string host, int numberOfMessageToProduce, string messageContent)
    {
        // Validate parameters
        if (context == null)
        {
            throw new ArgumentException("Solace Systems API context Router must be not null.", "context");
        }

        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("Solace Messaging Router host name must be non-empty.", "host");
        }

        if (string.IsNullOrWhiteSpace(VPNName))
        {
            throw new InvalidOperationException("VPN name must be non-empty.");
        }

        if (string.IsNullOrWhiteSpace(UserName))
        {
            throw new InvalidOperationException("Client username must be non-empty.");
        }

        // Create session properties
        SessionProperties sessionProps = new SessionProperties()
        {
            Host = host,
            VPNName = VPNName,
            UserName = UserName,
            Password = Password,
            ReconnectRetries = DefaultReconnectRetries
        };

        // Connect to the Solace messaging router
        Console.WriteLine("Connecting as {0}@{1} on {2}...", UserName, VPNName, host);
        using (ISession session = context.CreateSession(sessionProps, null, null))
        {
            ReturnCode returnCode = session.Connect();
            if (returnCode == ReturnCode.SOLCLIENT_OK)
            {
                Console.WriteLine("Session successfully connected.");
                ProduceMessage(session, numberOfMessageToProduce, messageContent);
            }
            else
            {
                Console.WriteLine("Error connecting, return code: {0}", returnCode);
            }
        }
    }

    private void ProduceMessage(ISession session, int numberOfMessageToProduce, string messageContent)
    {
        // Provision the queue

        Console.WriteLine("Attempting to provision the queue '{0}'...", QueueName);

        // Create the queue
        using (IQueue queue = ContextFactory.Instance.CreateQueue(QueueName))
        {
            // Set queue permissions to "consume" and access-type to "exclusive"
            EndpointProperties endpointProps = new EndpointProperties()
            {
                Permission = EndpointProperties.EndpointPermission.Consume,
                AccessType = EndpointProperties.EndpointAccessType.Exclusive
            };
            // Provision it, and do not fail if it already exists
            session.Provision(
                queue,
                endpointProps,
                ProvisionFlag.IgnoreErrorIfEndpointAlreadyExists | ProvisionFlag.WaitForConfirm,
                null);
            Console.WriteLine("Queue '{0}' has been created and provisioned.", QueueName);

            // Create the message
            using (IMessage message = ContextFactory.Instance.CreateMessage())
            {
                // Message's destination is the queue and the message is persistent
                message.Destination = queue;
                message.DeliveryMode = MessageDeliveryMode.Persistent;
                // Create the message content as a binary attachment
                message.BinaryAttachment = Encoding.ASCII.GetBytes(messageContent);

                for (int i = 0; i < numberOfMessageToProduce; i++)
                {
                    // Send the message to the queue on the Solace messaging router
                    Console.WriteLine("Sending message to queue {0}...", QueueName);
                    ReturnCode returnCode = session.Send(message);
                    if (returnCode == ReturnCode.SOLCLIENT_OK)
                    {
                        // Delivery not yet confirmed. See ConfirmedPublish.cs
                        Console.WriteLine("Done.");
                    }
                    else
                    {
                        Console.WriteLine("Sending failed, return code: {0}", returnCode);
                    }
                }
            }
        }
    }
}
