using MongoDB.Bson;

namespace Task1.Models
{
    public class ViewImage
    {
        public ObjectId Id { get; set; }
        public string url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
       
    }
}
