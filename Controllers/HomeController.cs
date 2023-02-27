using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Diagnostics;
using Task1.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using System.Drawing;

namespace Task1.Controllers
{

    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public string setIP()
        {
            string IP = Response.HttpContext.Connection.RemoteIpAddress.ToString();
            return IP;

        }
        public string setDate()
        {
            string Date = DateTime.Now.ToString();
            return Date;
        }
        public string AdddateTimeIP(string Date, string IP)
        {

            MongoClient database_Client = new MongoClient("mongodb://localhost:27017");
            var db_user = database_Client.GetDatabase("InfoCollection");
            var collect = db_user.GetCollection<BsonDocument>("Info");
            var doc = new BsonDocument { { "Date_Time", Date }, { "IP", IP } };
            collect.InsertOne(doc);
            return "abcd";
        }
        public IActionResult Index()
        {
            string Date = setDate();
            string IP = setIP();
            ViewData[" "] = AdddateTimeIP(Date, IP);
            return View();

        }
        public string GetUserData()
        {
            MongoClient database_Client = new MongoClient("mongodb://localhost:27017");
            var db_user = database_Client.GetDatabase("InfoCollection");
            var collect = db_user.GetCollection<BsonDocument>("Info");

            var databaselist = collect.Find(new BsonDocument()).ToList();
            BsonDocument doc2 = new BsonDocument();
            foreach (var item in databaselist)
            {
                doc2 = item;
            }
            return $"Date :{doc2["Date_Time"].ToString()}, Ip Address :{doc2["IP"].ToString()}";
        }

        public IActionResult UserInfo()
        {
            return View();
        }
        [HttpPost]
        public IActionResult UserInfo(Home User)
        {
            MongoClient client_db = new MongoClient("mongodb://localhost:27017");

            var user_db = client_db.GetDatabase("User_Data");
            var collection = user_db.GetCollection<BsonDocument>("User");
            var document = new BsonDocument { { "_id", User.Id }, { "Name", User.Name }, { "Country", User.Country } };
            collection.InsertOne(document);

            return Redirect("/");
        }
        public ActionResult User()
        {
            MongoClient Client = new MongoClient("mongodb://localhost:27017");
            var db = Client.GetDatabase("User_Data");
            var collection = db.GetCollection<Home>("User").Find(new BsonDocument()).ToList();

            return View(collection);
        }

        public IActionResult Privacy()
        {
            ViewData["abcd"] = GetUserData();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public string DisplayCountry(string Country)
        {
            return ("The Country is " + Country);
        }
        public IActionResult AddImage()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddImage(UploadImage img)
        {
            MongoClient Client = new MongoClient("mongodb://localhost:27017");
            var db = Client.GetDatabase("ImageDB");
            var collect = db.GetCollection<BsonDocument>("ImageData");
            GridFSBucket bucket = new GridFSBucket(db);
            var options = new GridFSUploadOptions
            {
                ChunkSizeBytes = 64512, // 63KB
                Metadata = new BsonDocument
    {
        { "resolution", "1080P" },
        { "copyrighted", true }
    }
            };
            
            using var stream =  bucket.OpenUploadStream(img.Title, options);
            var id = stream.Id;
            img.Image.CopyTo(stream);
            stream.Close();
            var doc = new BsonDocument { { "Title", img.Title }, { "Description", img.Description }, { "Id", id } };
            collect.InsertOne(doc);

            return View("Index");

        }
        public async Task<ActionResult> DisplayImage()
        {
            MongoClient Client = new MongoClient("mongodb://localhost:27017");
            var db = Client.GetDatabase("ImageDB");
            GridFSBucket bucket = new GridFSBucket(db);
            var collect = db.GetCollection<BsonDocument>("ImageData");
            var databaselist = collect.Find(new BsonDocument()).ToList();
            BsonDocument doc = new BsonDocument();
            List<ViewImage> list = new List<ViewImage>();
            foreach (var item in databaselist)
            {
                doc = item;
                var byteImage = await bucket.DownloadAsBytesAsync(doc["Id"]);
                string im = Convert.ToBase64String(byteImage);
                string url = string.Format("data:image/png;base64,{0}", im);
                list.Add(new ViewImage() { Id =(ObjectId)doc["Id"],url = url,Description = doc["Description"].ToString()  });
            }
            return View(list);
        }
       
        [HttpPost]
        public IActionResult EditImage(string Id, UploadImage img)
        {
            MongoClient Client = new MongoClient("mongodb://localhost:27017");
            var db = Client.GetDatabase("ImageDB");
            var collect = db.GetCollection<BsonDocument>("ImageData");
            GridFSBucket bucket = new GridFSBucket(db);
            var options = new GridFSUploadOptions
            {
                ChunkSizeBytes = 255 * 1024,
                Metadata = new BsonDocument
    {
        { "resolution", "1080P" },
        { "copyrighted", true }
    }
            };
            ObjectId.TryParse(Id, out ObjectId oid);
            var filter = Builders<BsonDocument>.Filter.Eq("Id", oid);
            var doc = collect.Find(filter).FirstOrDefault();
            string description = doc["Description"].ToString();
            string title = doc["Title"].ToString();
            ObjectId _id = (ObjectId)doc["_id"];
            bucket.Delete(oid);
            collect.DeleteOne(filter);
            img.Title = title;
            using var stream = bucket.OpenUploadStream(img.Title, options);
            var id = stream.Id;
            img.Image.CopyTo(stream);
            stream.Close();
            var doc1 = new BsonDocument { { "Title", title }, { "Description", description }, { "Id", id } };
            collect.InsertOne(doc1);
            return Redirect("/");


        }
        
        [HttpPost]
        public IActionResult EditDescription(string Id, string Description)
        {
            MongoClient Client = new MongoClient("mongodb://localhost:27017");
            var db = Client.GetDatabase("ImageDB");
            var collect = db.GetCollection<BsonDocument>("ImageData");
            GridFSBucket bucket = new GridFSBucket(db);
            var options = new GridFSUploadOptions
            {
                ChunkSizeBytes = 255 * 1024,
                Metadata = new BsonDocument
    {
        { "resolution", "1080P" },
        { "copyrighted", true }
    }
            };
            ObjectId.TryParse(Id, out ObjectId oid);
            var filter = Builders<BsonDocument>.Filter.Eq("Id", oid);
            var doc = collect.Find(filter).FirstOrDefault();
            string description = Description;
            string title = doc["Title"].ToString();
            collect.DeleteOne(filter);     
            var doc1 = new BsonDocument { { "Title", title }, { "Description", description }, { "Id", oid } };
            collect.InsertOne(doc1);
            return Redirect("/");

        }





    }
}