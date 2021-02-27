using FireSharp;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Linq;
using System.Net;

namespace TestINsta
{
    internal class FirebaseDbHandler
    {

        public IFirebaseClient fclient;

        public FirebaseDbHandler(IFirebaseConfig config)
        {
            fclient = new FirebaseClient(config);
        }

        public void addUser(int userId,string userName)
        {
            if (!userExistsInDb(userId))
            {
                InstagramBotUser newUser = new InstagramBotUser(userId, userName);

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

        public void updateComments(int userId,int mediaId,int newComments)
        {
            FirebaseResponse response = fclient.Get("Users/" + userId);
            InstagramBotUser userInDb = response.ResultAs<InstagramBotUser>(); //The response will contain the data being retreived

            //new media
            InstagramMedia mediaUserAction = userInDb.media.FirstOrDefault(c => c.mediaId == mediaId);

            if(mediaUserAction == null)
            {
                userInDb.media.Add(new InstagramMedia()
                {
                    mediaId = mediaId,
                    comments = newComments
                });

                response = fclient.Update("Users/" + userId,userInDb);

            }
            else
            {
                mediaUserAction.comments += newComments;
                response = fclient.Update("Users/" + userId, userInDb);
                
            }

        }



        public bool userExistsInDb(int userId)
        { 
            FirebaseResponse response = fclient.Get("Users/"+userId);
            InstagramBotUser userInDb = response.ResultAs<InstagramBotUser>(); //The response will contain the data being retreived

            if(userInDb == null)
                return false;

            return true;
        }


    }
}