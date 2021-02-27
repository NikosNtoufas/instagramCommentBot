using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace TestINsta
{
    class Program
    {

        static readonly HttpClient client = new HttpClient();
        
        static async Task Main(string[] args)
        {
            IFirebaseConfig config = new FirebaseConfig()
            {
                BasePath = "https://instagramsmartbot-default-rtdb.firebaseio.com/",
                AuthSecret = "i8DPDoHbqda0tUJTOK26rrNskD24KCxlJXkddO8L",
            };

            
            
           


            FirebaseDbHandler firebaseDbHandler = new FirebaseDbHandler(config);

            if (firebaseDbHandler.fclient == null)
            { 
                Console.Write("\nPress any key to exit...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }

            firebaseDbHandler.addUser(5, "nestoras");
            firebaseDbHandler.updateComments(5, 1,2);
            string username;
            string password;
            string mediaUrl;



            HttpResponseMessage response = client.GetAsync("https://tokeninstabot.herokuapp.com/token").Result;  // Blocking call! 
            string token = "";
            if (response.IsSuccessStatusCode)
            {
               
                // Get the response
                token = await response.Content.ReadAsStringAsync();
            }

            if(token=="")
            {
                Console.Write("\nSomething went wrong! Check your internet connection\n");
                Console.Write("\nPress any key to exit...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }
            if(token!="oAaaTsBYbE9Y2xFNuh3n")
            {
                Console.Write("\nSomething went wrong!Your version of instaCommentBot is not app to date!\n");
                Console.Write("\nPress any key to exit...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }
 
      

            InstagramApiHandler handler = new InstagramApiHandler();


            while (true)
            {
                Console.WriteLine("Give username:");
                username = Console.ReadLine();
                if (username.Equals("exit"))
                    Environment.Exit(0);

                Console.WriteLine("Give password:");
                password = GetPassword();
                Console.WriteLine("\n");

                await handler.initializeApi(username, password);
                if (handler.userSuccessfullyLoggedIn())
                {
                    await handler.LoadCloseFriendsAsync();
                    break;
                }
                Console.WriteLine("Wrong credentials! Try again, press exit if you want to exit application\n");

            }

            Console.WriteLine("Give the media's url(Copy the url of the photo from browser and paste it here)");
            mediaUrl = Console.ReadLine();

            while (true)
            {
                await handler.InitializeMedia(mediaUrl);
                if (handler.MediaInitializedSuccessfully())
                    break;

                //wrong url,try again or exit
                Console.WriteLine("Wrong Url!\n");
                Console.WriteLine("Give the media's url, press exit if you want to exit application\n");

                mediaUrl = Console.ReadLine();
                if (mediaUrl.Equals("exit"))
                    Environment.Exit(0);

            }

            Console.Write("\n--------------------------------------------------------------\n");

            while(true)
            {
                Console.Write("\nPress 1 to start commenting\nPress 2 to see the number of your comments in this media\n" +
               "press exit if you want to exit application\n");
                var go = Console.ReadLine();
                if (go.Equals("1"))
                {
                    try
                    {
                        int number = 0;
                        while(true)
                        {
                            Console.Write("\nThe insta autocommenter will tag friends from your close friends list.\nHow many friends do you want to tag on each comment?\n");
                            string numOfFriends = Console.ReadLine();
                            int n;
                            bool isNumeric = int.TryParse(numOfFriends, out n);
                            if (isNumeric)
                            {
                                number = n;
                                break;
                            }
                            else
                            {
                                Console.Write("\nEnter a valid number\n");
                            }

                        }
                        await handler.CommentCurrMedia(number);
                    }
                    catch (Exception e)
                    {
                        Console.Write("\nSomething went wrong!");
                        Console.Write("\nPress any key to exit...");
                        Console.ReadKey(true);
                        Environment.Exit(0);
                    }
                }
                else if (go.Equals("2"))
                {
                    int count = await handler.GetNumberrOfYourComments();
                    Console.Write("\nΥou have made "+count +" comments in the current media!\n");

                }
                else if(go.Equals("exit"))
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.Write("Select one of the options to continue!");
                }

               
            }
           



            Console.Write("\nPress any key to exit...");
            Console.ReadKey(true);


            handler.endAppAndWriteLogs();


           

        }




        static string GetPassword()
        {
            var pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    //pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            return pass;
        }


    }

    
}
