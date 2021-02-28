using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InstagramCommentBot
{
    class InstagramApiHandler
    {
        //const string stateFile = "state.bin";

        private long Userid { get; set; }
        private string MediaUrl { get; set; }
        private string MediaId { get; set; }
        private InstaMedia Media { get; set; }
        private int CommentsAdded { get; set; } = 0;
        private UserSessionData SessionData { get; set; }

        private Friends friends { get; set; } = new Friends();

        IInstaApi _instaApi;
        private FirebaseDbHandler DbHandler;




        public InstagramApiHandler()
        {
        }
        public InstagramApiHandler(FirebaseDbHandler dbHandler)
        {
            this.DbHandler = dbHandler;
        }

        public async Task InitializeMedia(String url)
        {

            try
            {
                this.MediaUrl = url; 
                var response = await _instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));
                MediaId = response.Value;
                var response2 = await _instaApi.MediaProcessor.GetMediaByIdAsync(MediaId);
                Media = response2.Value;

            }
            catch (Exception)
            {
            }

        }

        //check if media initialized successfully
        public bool MediaInitializedSuccessfully()
        {
            if (Media == null/* || !Media.Succeeded*/)
                return false;

            return true;
        }

        public int getNumberOfComments()
        {
            return CommentsAdded;
        }

        public bool userSuccessfullyLoggedIn()
        {
            return _instaApi.IsUserAuthenticated;
        }
     

        //initialize
        public async Task SetUser(string userName, string password)
        {
            SessionData = new UserSessionData
            {
                UserName = userName,
                Password = password
            };

            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(SessionData)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .Build();

            await LoginUser();

            // save session in file
            //var state = _instaApi.GetStateDataAsStream();
            // in .net core or uwp apps don't use GetStateDataAsStream.
            // use this one:
            // var state = _instaApi.GetStateDataAsString();
            // this returns you session as json string.
            //using (var fileStream = File.Create(stateFile))
            //{
            //    state.Seek(0, SeekOrigin.Begin);
            //    state.CopyTo(fileStream);
            //}

        }

        public async Task LoginUser()
        {
            //  try
            //{
            //    // load session file if exists
            //    if (File.Exists(stateFile))
            //    {
            //        Console.WriteLine("Loading state from file");
            //        using (var fs = File.OpenRead(stateFile))
            //        {
            //            _instaApi.LoadStateDataFromStream(fs);
            //            // in .net core or uwp apps don't use LoadStateDataFromStream
            //            // use this one:
            //            // _instaApi.LoadStateDataFromString(new StreamReader(fs).ReadToEnd());
            //            // you should pass json string as parameter to this function.
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}

            if (!_instaApi.IsUserAuthenticated)
            {
                // login
                Console.WriteLine($"Logging in as {SessionData.UserName}");
                var logInResult = await _instaApi.LoginAsync();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    return;
                }
                else
                {
                    var instaUser = _instaApi.GetLoggedUser();
                    this.Userid = instaUser.LoggedInUser.Pk;
                    DbHandler.AddUser(instaUser.LoggedInUser.Pk, instaUser.LoggedInUser.UserName);
                    //DbHandler.addUser()
                }
            }
     
        }

        //This function spam a media with comments. Each comment contains two friend tags.
        public async Task CommentCurrMedia(int number)
        {
          

            if (MediaInitializedSuccessfully())
            {
                SleepVariables sleep = new SleepVariables();

                string commentText = "";
                int newComments = 0;

                Stopwatch stop = Stopwatch.StartNew();
                Stopwatch stopTotal = Stopwatch.StartNew();

                int numberOfFriendsInComment = 0;

                while (true)
                {
                    foreach (var user in friends.closeFriends)
                    {
                       
                        commentText += "@" + user + " ";
                        numberOfFriendsInComment++;

                        if (numberOfFriendsInComment==number)
                        {
                            try
                            {

                                if (CommentsAdded !=0 && CommentsAdded % 100 == 0)
                                {
                                    if (newComments > 0)
                                    {
                                        DbHandler.AddComments(Userid, MediaId, Media.User.Pk, Media.User.UserName, MediaUrl, newComments);
                                        newComments = 0;
                                    }
                                    Console.WriteLine("Sleeping for one hour...");
                                    await LoadCloseFriendsAsync();
                                    await Task.Delay(3600000);

                                    sleep.HourSleeperCounter = 0;
                                    sleep.minuteSleeperCounter = 0;
                                    sleep.TenminuteSleeperCounter = 0;

                                }
                                //save every 50 comments to db
                                if (newComments>50)
                                {
                                    DbHandler.AddComments(Userid, MediaId, Media.User.Pk, Media.User.UserName, MediaUrl, newComments);
                                    newComments = 0;
                                }

                                if (sleep.minuteSleeperCounter == 9)
                                {
                                    Console.WriteLine("Sleeping for one minute...\n");
                                    await LoadCloseFriendsAsync();
                                    await Task.Delay(60000);
                                    sleep.minuteSleeperCounter = 0;
                                }

                                var commentResult = await _instaApi.CommentProcessor.CommentMediaAsync(MediaId, commentText);
                                sleep.minuteSleeperCounter++;
                                if (commentResult.Succeeded)
                                {
                                    CommentsAdded++;
                                    newComments++;
                                    Console.WriteLine("Number of total comments:" + CommentsAdded + "....\n");
                                }
                                else
                                {
                                    var time = stop.Elapsed.TotalSeconds.ToString();

                                    Console.WriteLine("Something went wrong on comment #" + (CommentsAdded + 1).ToString() + "!\n");
                                    Console.WriteLine("Reason: " + commentResult.Info.Message + "\n");


                                    if (commentResult.Info.ResponseType == ResponseType.Spam)
                                    {

                                        string msg =
                                            "Spam response after " + stopTotal.Elapsed.TotalSeconds + "s, " + time + "s after the last spam response.\n"
                                            + "Total comments sended:" + CommentsAdded + ".\n";
                                        Console.WriteLine(msg);

                                        writeMessageToSpamLog(msg + "\n----------------------------" + "\n");

                                        sleep.TenminuteSleeperCounter++;
                                        if (sleep.TenminuteSleeperCounter == 5)
                                        {
                                            if(newComments>0)
                                            {
                                                DbHandler.AddComments(Userid, MediaId, Media.User.Pk, Media.User.UserName, MediaUrl, newComments);
                                                newComments = 0;
                                            }

                                            Console.WriteLine("Sleeping for ten minutes...");
                                            await LoadCloseFriendsAsync();
                                            await Task.Delay(600000);

                                            sleep.TenminuteSleeperCounter = 0;
                                        }


                                        sleep.HourSleeperCounter++;
                                        if (sleep.HourSleeperCounter == 6)
                                        {
                                            if (newComments > 0)
                                            {
                                                DbHandler.AddComments(Userid, MediaId, Media.User.Pk, Media.User.UserName, MediaUrl, newComments);
                                                newComments = 0;
                                            }
                                            Console.WriteLine("Sleeping for one hour...");
                                            await LoadCloseFriendsAsync();
                                            await Task.Delay(3600000);

                                            sleep.HourSleeperCounter = 0;
                                        }
                                        //Console.WriteLine("\nPress exit if you want to end the proccess.Press any key to contnue..");
                                        //if (Console.ReadLine().Equals("exit"))
                                        //{
                                        //    endAppAndWriteLogs();

                                        //}
                                        stop = new Stopwatch();

                                    }

                                }
                                
                                commentText = "";
                                numberOfFriendsInComment = 0;
                                //Console.Write("\nMediaId:" + media.Value);
                            }
                            catch (Exception e)
                            {
                                var a = e.Message;
                                throw;
                            }
                        }
                    }


                }
            }
           


        }

        public async Task<int> GetNumberrOfYourComments()
        {
            try
            {
                return DbHandler.GetNumberOfComments(Userid, MediaId);

                //var mediaA = await _instaApi.CommentProcessor.GetMediaCommentsAsync(MediaId, PaginationParameters.MaxPagesToLoad(100000));

                //if(!mediaA.Succeeded)
                //    return 0;
                //string username = _instaApi.GetLoggedUser().UserName;

                //int usersComments = mediaA.Value.Comments.Where(c => c.User.UserName.Equals(username))!=null ?
                //     mediaA.Value.Comments.Where(c => c.User.UserName.Equals(username)).Count() : 0;
                //return usersComments;
            }
            catch (Exception)
            {
                return 0;
            }
          
        }

        public async Task LoadCloseFriendsAsync()
        {
            Console.Write("Loading Data.....\n");

            //var userFollowings = await _instaApi.UserProcessor
            //            .GetUserFollowingAsync(_instaApi.GetLoggedUser().UserName, PaginationParameters.MaxPagesToLoad(10));

            //friends.followings = userFollowings.Value.Select(c => c.UserName).ToList();


            var userBestFriends = await _instaApi.UserProcessor.GetBestFriendsAsync(PaginationParameters.MaxPagesToLoad(10));
            friends.closeFriends = userBestFriends.Value.Select(c => c.UserName).ToList();

        }


       
        public void endAppAndWriteLogs()
        {

            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\InstagramCommentBotLog.txt";

            string text = DateTimeOffset.UtcNow + ": number of comments added: " + getNumberOfComments() + ".\n";

            File.AppendAllText(path, text + Environment.NewLine);

            Environment.Exit(0);
        }

        private void writeMessageToSpamLog(string message)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\InstagramCommentBotSpamLog.txt";


            File.AppendAllText(path, message + Environment.NewLine);


        }


        public class Friends
        {
            public List<string> followers = new List<string>();
            public List<string> followings = new List<string>();
            public List<string> closeFriends = new List<string>();

            public Friends() { }
        }

        public class SleepVariables
        {
            public int minuteSleeperCounter = 0;
            public int TenminuteSleeperCounter = 0;
            public int HourSleeperCounter = 0;

        }

    }
}
