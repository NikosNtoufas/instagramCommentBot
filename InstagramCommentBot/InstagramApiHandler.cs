using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestINsta
{
    class InstagramApiHandler
    {
        //const string stateFile = "state.bin";
        private InstagramApiSharp.Classes.IResult<string> Media { get; set; }
        private int commentsAdded { get; set; } = 0;
        private UserSessionData SessionData { get; set; }

        private Friends friends { get; set; } = new Friends();


        IInstaApi _instaApi;

        public InstagramApiHandler()
        {
        }

        public async Task InitializeMedia(String url)
        {

            try
            {
                Media = await _instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));

            }
            catch (Exception)
            {
            }

        }

        //check if media initialized successfully
        public bool MediaInitializedSuccessfully()
        {
            if (Media == null || !Media.Succeeded)
                return false;

            return true;
        }

        public int getNumberOfComments()
        {
            return commentsAdded;
        }

        public bool userSuccessfullyLoggedIn()
        {
            return _instaApi.IsUserAuthenticated;
        }



        public async Task initializeApi(string userName, string password)
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
            }
        }

        //This function spam a media with comments. Each comment contains two friend tags.
        public async Task CommentCurrMedia(int number)
        {
            if (MediaInitializedSuccessfully())
            {
                SleepVariables sleep = new SleepVariables();


                string comment = "";


                Stopwatch stop = Stopwatch.StartNew();
                Stopwatch stopTotal = Stopwatch.StartNew();


                while (true)
                {
                    int friendsInComment = 0;
                    foreach (var user in friends.closeFriends)
                    {


                        comment += "@" + user + " ";
                        friendsInComment++;
                        if (friendsInComment == number)
                        {
                            try
                            {

                                if (commentsAdded != 0 && commentsAdded % 100 == 0)
                                {
                                    Console.WriteLine("Sleeping for one hour...");
                                    await LoadCloseFriendsAsync();
                                    await Task.Delay(3600000);

                                    sleep.HourSleeperCounter = 0;
                                    sleep.minuteSleeperCounter = 0;
                                    sleep.TenminuteSleeperCounter = 0;

                                }
                                if (sleep.minuteSleeperCounter == 9)
                                {
                                    Console.WriteLine("Sleeping for one minute...\n");
                                    await LoadCloseFriendsAsync();
                                    await Task.Delay(60000);
                                    sleep.minuteSleeperCounter = 0;
                                }

                                var commentResult = await _instaApi.CommentProcessor.CommentMediaAsync(Media.Value, comment);
                                sleep.minuteSleeperCounter++;
                                if (commentResult.Succeeded)
                                {
                                    commentsAdded++;
                                    Console.WriteLine("Number of total comments:" + commentsAdded + "....\n");
                                }
                                else
                                {
                                    var time = stop.Elapsed.TotalSeconds.ToString();

                                    Console.WriteLine("Something went wrong on comment #" + (commentsAdded + 1).ToString() + "!\n");
                                    Console.WriteLine("Reason: " + commentResult.Info.Message + "\n");


                                    if (commentResult.Info.ResponseType == ResponseType.Spam)
                                    {

                                        string msg =
                                            "Spam response after " + stopTotal.Elapsed.TotalSeconds + "s, " + time + "s after the last spam response.\n"
                                            + "Total comments sended:" + commentsAdded + ".\n";
                                        Console.WriteLine(msg);

                                        writeMessageToSpamLog(msg + "\n----------------------------" + "\n");

                                        sleep.TenminuteSleeperCounter++;
                                        if (sleep.TenminuteSleeperCounter == 5)
                                        {
                                            Console.WriteLine("Sleeping for ten minutes...");
                                            await LoadCloseFriendsAsync();
                                            await Task.Delay(600000);

                                            sleep.TenminuteSleeperCounter = 0;
                                        }


                                        sleep.HourSleeperCounter++;
                                        if (sleep.HourSleeperCounter == 10)
                                        {
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

                                comment = "";
                                friendsInComment = 0;
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
                var mediaA = await _instaApi.CommentProcessor.GetMediaCommentsAsync(Media.Value, PaginationParameters.MaxPagesToLoad(100000));

                if (!mediaA.Succeeded)
                    return 0;
                string username = _instaApi.GetLoggedUser().UserName;

                int usersComments = mediaA.Value.Comments.Where(c => c.User.UserName.Equals(username)) != null ?
                     mediaA.Value.Comments.Where(c => c.User.UserName.Equals(username)).Count() : 0;
                return usersComments;
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
