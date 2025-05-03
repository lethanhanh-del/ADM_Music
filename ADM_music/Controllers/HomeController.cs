using ADM_music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ADM_music.Controllers
{
    public class HomeController : Controller
    {
        ADMDataContext db = new ADMDataContext("Data Source=DESKTOP-LM7KOTA\\ASDASD;Initial Catalog=ADM;Integrated Security=True;Encrypt=False");
        
        public ActionResult Index(string trackID)
        {
            if (string.IsNullOrEmpty(trackID))
            {
                trackID = "T01"; // Gán trackID mặc định, ví dụ "T01"
            }

            // Lấy bài hát theo TrackID
            var music = db.Tracks.FirstOrDefault(t => t.TrackID == trackID);
            // Kiểm tra nếu không tìm thấy bài hát
            if (music == null)
            {
                return HttpNotFound(); // Trả về lỗi 404 nếu không tìm thấy bài hát
            }
            Session["list_BaiHat"] = db.ExecuteQuery<Track>("EXEC GetTop10RandomSongs").ToList();
            Session["list_Album"] = db.ExecuteQuery<Album>("EXEC GetTop8RandomAlbums").ToList();
            Session["list_Artist"] = db.ExecuteQuery<Artist>("EXEC GetTop3ArtistLike").ToList();
            Session["list_AlbumTRoiXanh"] = db.ExecuteQuery<Album>("EXEC Get5AlbumTroiXanh").ToList();
            Session["list_AlbumLike"] = db.ExecuteQuery<Album>("EXEC Get5AlbumLike").ToList();
            Session["list_TrackTamTrang"] = db.ExecuteQuery<Track>("EXEC Get5TrackTamTrang").ToList();
            return View(music);
        }
        public ActionResult Like(string tendangnhap,string TrackID , string ArtictID )
        {
            if (!string.IsNullOrEmpty(TrackID))
            {
                TrackLike tl = new TrackLike();
                tl.UsersID = tendangnhap;
                tl.TrackID = TrackID;
                db.TrackLikes.InsertOnSubmit(tl);
                db.SubmitChanges();
            }
            if (!string.IsNullOrEmpty(ArtictID))
            {
                ArtistLike artl = new ArtistLike();
                artl.UsersID = tendangnhap;
                artl.ArtistID = ArtictID;
                db.ArtistLikes.InsertOnSubmit(artl);
                db.SubmitChanges();
            }
            return RedirectToAction("Index");  
        }

        public ActionResult unLike(string tendangnhap, string TrackID, string ArtictID)
        {
            if (!string.IsNullOrEmpty(TrackID))
            {
                TrackLike tl = new TrackLike();
                tl.UsersID = tendangnhap;
                tl.TrackID = TrackID;
                db.TrackLikes.InsertOnSubmit(tl);
                db.SubmitChanges();
            }
            if (!string.IsNullOrEmpty(ArtictID))
            {
                ArtistLike artl = new ArtistLike();
                artl.UsersID = tendangnhap;
                artl.ArtistID = ArtictID;
                db.ArtistLikes.InsertOnSubmit(artl);
                db.SubmitChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Profile()
        {
            return View();
        }
        public ActionResult Premium()
        {
            return View();
        }
        public ActionResult Library()
        {
            return View();
        }
        public ActionResult Category()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        //[HttpGet]
        //public ActionResult Play_Music(string trackID)
        //{
        //    if (string.IsNullOrEmpty(trackID))
        //    {
        //        trackID = "T01"; // Gán trackID mặc định, ví dụ "T01"
        //    }

        //    // Lấy bài hát theo TrackID
        //    var music = db.Tracks.FirstOrDefault(t => t.TrackID == trackID);
        //    // Kiểm tra nếu không tìm thấy bài hát
        //    if (music == null)
        //    {
        //        return HttpNotFound(); // Trả về lỗi 404 nếu không tìm thấy bài hát
        //    }

        //    return PartialView("play_music",music); // Trả về view với model là bài hát
        //}
    }
}