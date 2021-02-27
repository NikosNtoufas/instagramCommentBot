using System.Collections.Generic;

namespace TestINsta
{
    internal class InstagramBotUser
    {

        public int InstagramUserId { get; set; }
        public string InstagramUserName { get; set; }
        public List<InstagramMedia> media { get; set; } = new List<InstagramMedia>();
       
        public InstagramBotUser()
        {
            
        }

        public InstagramBotUser(int userId,string name)
        {
            this.InstagramUserId = userId;
            this.InstagramUserName = name;
            this.media = new List<InstagramMedia>();
        }
    }
}