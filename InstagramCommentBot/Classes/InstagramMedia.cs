namespace InstagramCommentBot
{
    internal class InstagramMedia
    {
        public string MediaId { get; set; }
        public long ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string MediaUrl { get; set; }
        public int Comments { get; set; } = 0;

        public InstagramMedia()
        {
           
        }

        public InstagramMedia(string mediaId,long profileId,string profileName="",string mediaUrl="",int newComments=0)
        {
            this.MediaId = mediaId;
            this.ProfileId = profileId;
            this.ProfileName = profileName;
            this.MediaUrl = mediaUrl;
            this.Comments = newComments;
        }

    }
}