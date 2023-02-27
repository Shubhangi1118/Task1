using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Task1.Models
{
    public class Home
    {
       
        [BsonId]
        public  ObjectId Id { get; set; }
        //Required(ErrorMessage = "Name is required")];
        public string Name { get; set; }
        public string Country { get; set; }
    }
}
