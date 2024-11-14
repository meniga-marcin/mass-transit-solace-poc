using System;
using System.Text;
using Amqp;
using Amqp.Framing;

namespace Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            Address address = new Address("amqp://localhost:5674/");
            Connection connection = new Connection(address);
            Session session = new Session(connection);

            while(true){
                var message = new Message() { BodySection = new Data() { Binary = Encoding.UTF8.GetBytes(File.ReadAllText("./../sampleTransaction.json")) } };

                SenderLink sender = new SenderLink(session, "sender-link", "test-queue");
                sender.Send(message, );
                Console.WriteLine("Sent Import transaction message!");
                sender.Close();
            }
          
            session.Close();
            connection.Close();
        }
    }
}