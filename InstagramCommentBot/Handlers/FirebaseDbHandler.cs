using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Linq;
using System.Net;

namespace InstagramCommentBot
{
    internal class FirebaseDbHandler
    {

        public IFirebaseClient fclient;

        public FirebaseDbHandler()
        {

            IFirebaseConfig config = new FirebaseConfig()
            {
                BasePath = "https://instagramsmartbot-default-rtdb.firebaseio.com/",
                AuthSecret = "i8DPDoHbqda0tUJTOK26rrNskD24KCxlJXkddO8L",
            };

            //connect to firebase

            fclient = new FirebaseClient(config);


         

        }

        public void AddUser(long userId,string userName)
        {
            string date = DateTime.UtcNow.ToString("dd/MM/yyyy");
            
            if (!UserExistsInDb(userId))
            {
                InstagramBotUser newUser = new InstagramBotUser(userId, userName,date);

                FirebaseResponse response = fclient.Set("Users/" + userId,newUser);
                var x = response.StatusCode;
                if(!(response.StatusCode == HttpStatusCode.OK))
                {
                    Console.Write("\nError with firebase! Press any key to exit...");
                    Console.ReadKey(true);
                    Environment.Exit(0);
                }

            }
        }

        public void AddComments(long userId,string mediaId,long mediaProfileId,string mediaProfileName,string mediaUrl, int newComments)
        {
            FirebaseResponse response = fclient.Get("Users/" + userId);
            InstagramBotUser userInDb = response.ResultAs<InstagramBotUser>(); //The response will contain the data being retreived
            
            if (userInDb != null)
            {
                //new media
                InstagramMedia mediaUserAction = userInDb.Media.FirstOrDefault(c => c != null && c.MediaId == mediaId);

                if (mediaUserAction == null)
                {
                    userInDb.Media.Add(new InstagramMedia(mediaId, mediaProfileId, mediaProfileName, mediaUrl, newComments));

                    response = fclient.Update("Users/" + userId, userInDb);

                }
                else
                {
                    mediaUserAction.Comments += newComments;
                    response = fclient.Update("Users/" + userId, userInDb);

                }
            }
        

        }



        public bool UserExistsInDb(long userId)
        { 
            FirebaseResponse response = fclient.Get("Users/"+userId);
            InstagramBotUser userInDb = response.ResultAs<InstagramBotUser>(); //The response will contain the data being retreived

            if(userInDb == null)
                return false;

            return true;
        }


        public int GetNumberOfComments(long userId,string mediaId)
        {
            FirebaseResponse response = fclient.Get("Users/" + userId);
            InstagramBotUser userInDb = response.ResultAs<InstagramBotUser>(); //The response will contain the data being retreived

            //new media
            InstagramMedia mediaUserAction = userInDb.Media.FirstOrDefault(c => c != null && c.MediaId == mediaId);
            if (mediaUserAction != null)
            {
                return mediaUserAction.Comments;
            }

            return 0;
        }


    }
}