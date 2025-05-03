using ADM_music.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NAudio.Wave;
using System.Data;

namespace ADM_music.Controllers
{
    public class AdminController : Controller
    {
        private ADMDataContext db = new ADMDataContext("Data Source=DESKTOP-LM7KOTA\\ASDASD;Initial Catalog=ADM;Integrated Security=True;Encrypt=False");
        Random random = new Random();
        // GET: Admin
        public ActionResult Index()
        {
            Session["SL_DH"] = db.AlbumPurchases.Count().ToString();
            Session["Tong_Thang"] = db.AlbumPurchases.Where(t=>t.PurchaseDate.Value.Month == DateTime.Now.Month).Sum(t=>t.Total_Price).GetValueOrDefault(0).ToString();
            Session["Tong_Ngay"] = db.AlbumPurchases.Where(t => t.PurchaseDate.Value.Day == DateTime.Now.Day).Sum(t => t.Total_Price).GetValueOrDefault(0).ToString();
            Session["Top3Artist"] = db.Top3ArtistsByLikes.ToList();
            Session["Top3Album"] = db.Top3AlbumsByLikes.ToList();
            Session["name"] = db.CountTracksInPlaylists().Select(t => t.PlaylistName).ToList();
            Session["total"] = db.CountTracksInPlaylists().Select(t => t.TotalTracks).ToList();
            CountNewCustomersResult a = db.CountNewCustomers().FirstOrDefault();
            Session["SL_NewUser"] = a.NewCustomers.ToString();
            return View();
        }

        //---------------------------------------------------------------------------------
        //Quản lý tracks
        public ActionResult QL_Tracks()
        {
            var tracks = db.Get_ALL_Tracks().ToList();

            return View(tracks);
        }
        public ActionResult Add_Track()
        {
            Session["at"] = db.Artists.ToList();
            Session["ab"] = db.Albums.ToList();
            return View();
        }
        [HttpPost] //tải NAudio trong NuGet
        public ActionResult AddTrack(Track Track, HttpPostedFileBase fileInput)
        {
            try
            {
                int idtrack = db.Tracks.Count() + 1;
                Track.TrackID = "T" + idtrack;

                if (ModelState.IsValid)
                {
                    if (fileInput != null && fileInput.ContentLength > 0)
                    {
                        string fileExtension = Path.GetExtension(fileInput.FileName).ToLower();
                        if (fileExtension == ".mp3")
                        {
                            string folderPath = Server.MapPath("~/Content/Music");
                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }

                            string filePath = Path.Combine(folderPath, Path.GetFileName(fileInput.FileName));
                            fileInput.SaveAs(filePath);
                            Track._FILE = Path.GetFileName(fileInput.FileName);

                            // Lấy độ dài file audio
                            TimeSpan duration = GetAudioDuration(filePath);

                            // Chuyển TimeSpan thành dạng thời gian cho SQL
                            TimeSpan? durationSql = duration;

                            Track.Duration = durationSql; // Lưu vào trường Duration có kiểu 'TIME' trong SQL

                            // Lưu vào cơ sở dữ liệu
                            int ID = db.ExecuteQuery<int>(
                                $"EXEC Add_new_Track '{Track.TrackID}', '{Track.Name}', '{Track._FILE}', '{Track.ArtistID}', '{Track.AlbumID}', '{Track.Duration}', '{Track.Img}'"
                            ).FirstOrDefault();

                            return RedirectToAction("QL_Tracks");
                        }
                        else
                        {
                            ViewBag.mess = "Chỉ hỗ trợ định dạng file .mp3.";
                            return View("Add_Track");
                        }
                    }
                    else
                    {
                        ViewBag.mess = "Vui lòng chọn một file để upload.";
                        return View("Add_Track");
                    }
                }
                else
                {
                    ViewBag.mess = "Chưa Điền Đủ thông tin.";
                    return View("Add_Track");
                }
            }
            catch (Exception ex)
            {

                ViewBag.mess = ""+ex.Message;
                return View("Add_Track");
            }
            
        }
        public TimeSpan GetAudioDuration(string filePath)
        {
            using (var reader = new Mp3FileReader(filePath))
            {
                return reader.TotalTime;
            }
        }
        public ActionResult Edit_Track(int id)
        {
            //lấy danh mục
            Session["at"] = db.Artists.ToList();
            Session["ab"] = db.Albums.ToList();

            //lấy thông tin sản phẩm 
            Track track_edit = db.Tracks.FirstOrDefault(s => s.ID == id);
            return View(track_edit);
        }
        [HttpPost]
        public ActionResult EditTrack(Track track)
        {
            if (ModelState.IsValid)
            {
                Track tr = db.Tracks.Where(t => t.ID == track.ID).Select(c => c).FirstOrDefault();

                tr.Name = track.Name;
                tr.Img= track.Img;
                tr.ArtistID = track.ArtistID;
                tr.AlbumID = track.AlbumID;
                db.SubmitChanges();
                return RedirectToAction("QL_Tracks");
            }
            else
            {
                ViewBag.mess = "Chưa Điền Đủ thông tin ";
                return View("Edit_Track", new {id = track.ID});
            }
        }
        public ActionResult Delete_Track(int id)
        {
            Track tr_delete = db.Tracks.FirstOrDefault(t => t.ID == id);
            string imagePath = Server.MapPath("~/Content/Music/" + tr_delete._FILE);

            // Xóa file hình ảnh từ thư mục nếu tồn tại
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            db.Tracks.DeleteOnSubmit(tr_delete);
            db.SubmitChanges();
            return RedirectToAction("QL_Tracks");
        }
        //end Quản lý tracks
        //---------------------------------------------------------------------------------
        //Quản lý Articts
        public ActionResult QL_Articts()
        {
            List<Artist> y = db.ExecuteQuery<Artist>("Exec Get_All_Artist_DSQL").ToList();
            return View(y);
        }
        public ActionResult View_Artict(string ArtistID)
        {
            Artist artist = db.Artists.FirstOrDefault(t=>t.ArtistID == ArtistID);
            Session["song"] = db.Tracks.Where(t=>t.ArtistID==ArtistID).ToList();
            return View(artist);
        }
        public ActionResult Add_Artict() 
        {

            return View();
        }
        [HttpPost]
        public ActionResult AddArtict(Artist at) 
        {

            int count = db.Artists.Count() + 1;
            Artist temp = new Artist();
            temp.ArtistID = "A" + count;
            temp.Name = at.Name;
            temp.Genre = at.Genre;
            temp.DateOfBirth = at.DateOfBirth;
            temp.LikeNum = 0;
            temp.Img = at.Img;
            db.Artists.InsertOnSubmit(temp);
            db.SubmitChanges();

            return RedirectToAction("QL_Articts");
        }
        public ActionResult Edit_Artict(string ArtistID) 
        {
            Artist art = db.Artists.Where(t => t.ArtistID == ArtistID).FirstOrDefault();
            return View(art);
        }
        [HttpPost]
        public ActionResult EditArtict(Artist at) 
        {
            try
            {
                // Câu lệnh SQL
                string sql = @"
                DECLARE @Result INT;
                EXEC UpdateArtistInfo 
                @ArtistID = {0}, 
                @NewName = {1}, 
                @NewGenre = {2}, 
                @NewDateOfBirth = {3}, 
                @NewImg= {4},
                @Result = @Result OUTPUT;
            SELECT @Result";


                // Thực thi truy vấn
                var result = db.ExecuteQuery<int>(
                    sql,
                    at.ArtistID.Trim(),
                    at.Name,
                    at.Genre,
                    at.DateOfBirth,
                    at.Img
                   
                ).FirstOrDefault();

                // Kiểm tra kết quả
                if (result == 1)
                {
                    Console.WriteLine("Cập nhật thành công");
                }
                else
                {
                    Console.WriteLine("Cập nhật thất bại");
                }

                return RedirectToAction("QL_Articts");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Lỗi: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return View("Error"); // Chuyển hướng đến trang lỗi
            }
            
        }
        public ActionResult Delete_Artict(string ArtistID) 
        {
            Artist at = db.Artists.FirstOrDefault(t => t.ArtistID == ArtistID);
            db.Artists.DeleteOnSubmit(at);
            db.SubmitChanges();
            return RedirectToAction("QL_Articts");
        }
        //end Quản lý Articts      
        //---------------------------------------------------------------------------------
        //Quản lý Albums
        public ActionResult QL_Albums()
        {
            var album = db.GetAlbumInfo().ToList();
            return View(album);
        }
        public ActionResult View_Albums(string AlbumID)
        {
            Album album = db.Albums.Where(t => t.AlbumID == AlbumID).FirstOrDefault();
            Session["lst_song_ab"] = db.GetSongsInAlbum(AlbumID).ToList();
            return View(album);
        }
        public ActionResult Add_Albums()
        {
            List<Get_Artist> ab = db.Get_Artists.ToList();
            return View(ab);
        }
        [HttpPost]
        public ActionResult AddAlbums(Album ab) 
        {
            int count = db.Albums.Count()+1;
            Album temp = new Album();
            temp.AlbumID = "Ab"+ count;
            temp.Name = ab.Name;
            temp.Genre = ab.Genre;
            temp.ArtistID = ab.ArtistID;
            temp.Price = ab.Price;
            temp.ReleaseDate = DateTime.Now.Date;
            temp.LikeNum = 0;
            temp.Img = ab.Img;
            temp.Amount = ab.Amount;
            db.Albums.InsertOnSubmit(temp);
            db.SubmitChanges();

            return RedirectToAction("QL_Albums");
        }
        public ActionResult Edit_Albums(string AlbumID) 
        {
            Session["art"] = db.Get_Artists.ToList();
            Album alb = db.Albums.Where(t=>t.AlbumID == AlbumID).FirstOrDefault();
            return View(alb);
        }
        [HttpPost]
        public ActionResult EditAlbums(Album ALB)
        {
            try
            {
                // Câu lệnh SQL
                string sql = @"
            DECLARE @Result INT;
            EXEC UpdateAlbumInfoCursor 
                @AlbumID = {0}, 
                @TenAlbum = {1}, 
                @Genre = {2}, 
                @Price = {3}, 
                @Img = {4}, 
                @Result = @Result OUTPUT;
            SELECT @Result";

                // Thực thi truy vấn
                var result = db.ExecuteQuery<int>(
                    sql,
                    ALB.AlbumID,
                    ALB.Name,
                    ALB.Genre,
                    ALB.Price,
                    ALB.Img
                ).FirstOrDefault();

                // Kiểm tra kết quả
                if (result == 1)
                {
                    Console.WriteLine("Cập nhật thành công");
                }
                else
                {
                    Console.WriteLine("Cập nhật thất bại");
                }

                return RedirectToAction("QL_Albums");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Lỗi: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return View("Error"); // Chuyển hướng đến trang lỗi
            }
        }

        public ActionResult Delete_Albums(string AlbumID) 
        {
            Album ab = db.Albums.FirstOrDefault(t => t.AlbumID == AlbumID);
            db.Albums.DeleteOnSubmit(ab);
            db.SubmitChanges();
            return RedirectToAction("QL_Albums");
        }
        //end Quản lý Albums
        //---------------------------------------------------------------------------------
        //Quản lý DH
        public ActionResult QL_DH(string trangthai)
        {
            Session["in"] = false;
            List<AlbumPurchase> AlbumPurchasess;
            Session["TrangThai"] = db.TrangThaiDHs.Select(t=>t).ToList();
            if (trangthai == string.Empty || trangthai == null)
            {
                AlbumPurchasess = db.AlbumPurchases.ToList();
            }
            else
            {
                AlbumPurchasess = db.AlbumPurchases.Where(t=>t.MaTrangThai.Trim() == trangthai.Trim()).ToList();
            }
            return View(AlbumPurchasess);
        }
        [HttpPost]
        public ActionResult Edit_DH(string id, string trangthai)
        {
            var order = db.AlbumPurchases.Where(t => t.PurchaseID == id.Trim()).FirstOrDefault(); 
            if (order != null)
            {
                order.MaTrangThai = trangthai;
                db.SubmitChanges();
                if (order.MaTrangThai.Trim() == "TT02")
                {
                    Session["in"] = true;
                }
                else
                {
                    Session["in"] = false;
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        public ActionResult View_Purchase(string PurchaseID)
        {
            
            Session["DH"] = db.ExecuteQuery<AlbumPurchase>("Exec GetAlbumPurchase '" + PurchaseID + "'");
            List<AlbumDetail> lst_CTDH = db.AlbumDetails.Where(t => t.PurchaseID == PurchaseID).Select(t => t).ToList();
            return View(lst_CTDH);
        }
        // tải nuget HtmlRenderer 
        //public ActionResult print_Purchase(string PurchaseID)
        //{
        //    // Đường dẫn wkhtmltoimage từ NuGet (nếu đã cài)
        //    string wkhtmlPath = Server.MapPath("~/bin/wkhtmltoimage.exe");

        //    if (!System.IO.File.Exists(wkhtmlPath))
        //    {
        //        return Content("wkhtmltoimage.exe không tồn tại trong thư mục bin.");
        //    }

        //    // URL của View cần render
        //    string viewUrl = Url.Action("View_Purchase", "Admin", new {PurchaseID}, Request.Url.Scheme);

        //    // Đường dẫn cố định lưu file ảnh
        //    string outputPath = Server.MapPath("~/HoaDon_Albums/HoaDon.png");

        //    // Kiểm tra thư mục tồn tại, nếu không thì tạo
        //    string outputDirectory = Path.GetDirectoryName(outputPath);
        //    if (!Directory.Exists(outputDirectory))
        //    {
        //        Directory.CreateDirectory(outputDirectory);
        //    }

        //    // Lệnh gọi wkhtmltoimage
        //    var process = new System.Diagnostics.Process
        //    {
        //        StartInfo = new System.Diagnostics.ProcessStartInfo
        //        {
        //            FileName = wkhtmlPath,
        //            Arguments = $"--width 800 \"{viewUrl}\" \"{outputPath}\"",
        //            RedirectStandardOutput = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = true
        //        }
        //    };

        //    try
        //    {
        //        process.Start();
        //        process.WaitForExit();

        //        // Kiểm tra quá trình có thành công hay không
        //        if (process.ExitCode != 0)
        //        {
        //            throw new Exception($"Lỗi khi chạy wkhtmltoimage. Mã lỗi: {process.ExitCode}");
        //        }

        //        return File(outputPath, "image/png", "HoaDon_"+PurchaseID+".png");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content($"Lỗi: {ex.Message}");
        //    }
        //}
        public ActionResult Delete_DH(string PurchaseID)
        {
            AlbumPurchase DH_delete = db.AlbumPurchases.FirstOrDefault(t => t.PurchaseID == PurchaseID);
            List<AlbumDetail> lst_DH = db.AlbumDetails.Where(t=>t.PurchaseID == PurchaseID).ToList();
            db.AlbumDetails.DeleteAllOnSubmit(lst_DH);
            db.AlbumPurchases.DeleteOnSubmit(DH_delete);
            db.SubmitChanges();
            return RedirectToAction("QL_DH");
        }
        //end Quản lý DH
        //---------------------------------------------------------------------------------
        //Quản lý ND
        public ActionResult QL_ND()
        {
            List<User> y = db.Users.ToList();
            return View(y);
        }
        //end Quản lý Articts
        ////---------------------------------------------------------------------------------
        public ActionResult Chart1()
        {
            List<GetMonthlyTotalResult> chart1 = db.GetMonthlyTotal(DateTime.Now.Year).ToList();
            return View(chart1);
        }
        public ActionResult Chart2()
        {
            Session["name_chart2"] = db.CountTracksInPlaylists().Select(t=>t.PlaylistName).ToList();
            List<int?> a = db.CountTracksInPlaylists().Select(t => t.TotalTracks).ToList();
            int? tong = a.Sum();
            int[] kq = new int[6];
            int i = 0;
            foreach (var item in a)
            {
                int t;
                if (item !=0)
                {
                    t = (int)((float)item / tong * 100);

                }
                else
                {
                    t = 25;
                }
                kq[i]= t;
                i++;
            }
            return View(kq);
        }
    }
}