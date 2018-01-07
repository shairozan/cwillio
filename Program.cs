using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace cwillio
{
    class Program
    {

        static void Main(string[] args)
        {

            Console.WriteLine("Welcome to our shitty pseudo twillio chat client");
            Console.WriteLine("At any point, you may hit the \"R\" key to interrupt and send a response to the last message ");
            Console.WriteLine();
            Console.WriteLine();

            string AccountSid = Environment.GetEnvironmentVariable("ACCOUNT_SID");
            string AuthToken = Environment.GetEnvironmentVariable("AUTH_TOKEN");
            string ListeningNumber = Environment.GetEnvironmentVariable("LISTENING_NUMBER");

            List<MessageResource> processed = new List<MessageResource>();

            if(AccountSid == null || AuthToken == null || ListeningNumber == null ){
                Console.WriteLine("Required environmental values not present. Please make sure both ACCOUNT_SID and AUTH_TOKEN are configured");
                Environment.Exit(-1);
            }
            
            //Init the client
            TwilioClient.Init(AccountSid,AuthToken);

            while(true){

                //Listen for CTRL+R to respond
                while (!Console.KeyAvailable){
                    var currentMessage = MessageResource.Read(
                    to: new PhoneNumber(ListeningNumber)
                    ).First();

                    if(! processed.Any(x => x.Sid == currentMessage.Sid)){
                        Console.WriteLine($"From {currentMessage.From}:  {currentMessage.Body}" );
                    }
                    

                    processed.Add(currentMessage); //Add it to the list for later evaluations

                    Task.Delay(1000).Wait();
                }
                
                switch(Console.ReadKey(true).Key.ToString().ToLower()){
                    case "r" :
                        Console.WriteLine();
                        Console.WriteLine("\tPlease type your response followed by enter");
                        string response = Console.ReadLine();
                        Console.WriteLine();
                        SendMessage(processed.Last().From.ToString(),response); 
                        break;
                    case "s" :
                        Console.WriteLine();
                        Console.WriteLine("\tPlease enter the phone number you wish to message");
                        string to = NormalizePhoneString(Console.ReadLine());
                        Console.WriteLine("\tPlease type your message below");
                        string body = Console.ReadLine();
                        SendMessage(to,body);
                        break;
                }
            }

            
        }

        private static MessageResource SendMessage(string To, string body){
            string AccountSid = Environment.GetEnvironmentVariable("ACCOUNT_SID");
            string AuthToken = Environment.GetEnvironmentVariable("AUTH_TOKEN");
            string ListeningNumber = Environment.GetEnvironmentVariable("LISTENING_NUMBER");

            TwilioClient.Init(AccountSid,AuthToken);

            return MessageResource.Create(
                to: GetPhoneNumber(To),
                from: GetPhoneNumber(ListeningNumber),
                body: body
            );
        }

        private static string NormalizePhoneString(string number){
            number = number.Replace(" ",string.Empty);
            if(number.First() ==  '+'){
                return number;
            } else {
                return number.Insert(0,"+1");
            }
        }

        private static PhoneNumber GetPhoneNumber(string number){
            try{
                return new PhoneNumber(number);
            } catch(Exception e){
                Console.WriteLine();
                Error(e.Message);
                return null;
            }

        }

        public static void Error(string message){
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
