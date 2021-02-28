using System;
using System.Collections.Generic;

namespace InstagramCommentBot
{
    internal class InstagramBotUser
    {

        public long InstagramUserId { get; set; }
        public string InstagramUserName { get; set; }
        public string DateOfSubscription { get; set; }
        public List<InstagramMedia> Media { get; set; } = new List<InstagramMedia>();
       
        public InstagramBotUser()
        {
            
        }

        public InstagramBotUser(long userId,string name,string date)
        {
            this.InstagramUserId = userId;
            this.InstagramUserName = name;
            this.Media = new List<InstagramMedia>();
            this.DateOfSubscription = date;
        }
    }
}