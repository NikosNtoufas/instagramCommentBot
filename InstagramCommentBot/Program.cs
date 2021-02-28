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

namespace InstagramCommentBot
{
    class Program
    {

        static readonly HttpClient client = new HttpClient();
        
        static async Task Main(string[] args)
        {
            string username;
            string password;
            string mediaUrl;

            //check if the version of the app is the latest
            if (!TokenHandler.AppIsUpToDate(client))
            {
                Console.Write("\nSomething went wrong!Your version of instaCommentBot is not app to date!\n");
                Console.Write("\nPress any key to exit...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }

            //initialize firebaseDBhandler
            FirebaseDbHandler firebaseDbHandler = new FirebaseDbHandler();

            if (firebaseDbHandler.fclient == null)
            {
                //connection failed
                Console.Write("\nPress any key to exit...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }

   
            InstagramApiHandler instgramApiHandler = new InstagramApiHandler(firebaseDbHandler);

            //set user 
            while (true)
            {
                Console.WriteLine("Give username:");
                username = Console.ReadLine();
                if (username.Equals("exit"))
                    Environment.Exit(0);

                Console.WriteLine("Give password:");
                password = GetPassword();
                Console.WriteLine("\n");

                await instgramApiHandler.SetUser(username, password);

                if (instgramApiHandler.userSuccessfullyLoggedIn())
                {
                    await instgramApiHandler.LoadCloseFriendsAsync();
                    break;
                }
                Console.WriteLine("Wrong credentials! Try again, press exit if you want to exit application\n");

            }

            Console.WriteLine("Give the media's url(Copy the url of the photo from browser and paste it here)");
            mediaUrl = Console.ReadLine();

            //set media
            while (true)
            {
                await instgramApiHandler.InitializeMedia(mediaUrl);
                if (instgramApiHandler.MediaInitializedSuccessfully())
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
                        //start commenting
                        await instgramApiHandler.CommentCurrMedia(number);
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
                    int count = await instgramApiHandler.GetNumberrOfYourComments();
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


            instgramApiHandler.endAppAndWriteLogs();


           

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
