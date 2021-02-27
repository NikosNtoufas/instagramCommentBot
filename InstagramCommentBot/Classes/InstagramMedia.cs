namespace TestINsta
{
    internal class InstagramMedia
    {
        public int mediaId { get; set; }
        public int comments { get; set; } = 0;

        public InstagramMedia()
        {
           
        }

        public InstagramMedia(int id)
        {
            this.mediaId = id;
        }
    }
}