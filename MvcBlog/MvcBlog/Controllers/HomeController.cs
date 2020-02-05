using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBlog.Models;
using PagedList;
using PagedList.Mvc;

namespace MvcBlog.Controllers
{
    public class HomeController : Controller
    {

        MvcBlogDb db = new MvcBlogDb();
        // GET: Home
        public ActionResult Index(int? Page)
        {
            int _sayfaNo = Page ?? 1;
            var makales = db.Makales.OrderByDescending(x => x.MakaleId).ToPagedList(_sayfaNo, 5);

            if (makales == null)
            {
                return HttpNotFound();
            }

            return View(makales);
        }

        public ActionResult BlogAra(string Ara =null)
        {
            var aranan = db.Makales.Where(x => x.Baslik.Contains(Ara)).ToList();
            return View(aranan.OrderByDescending(x=>x.Tarih));

        }

        public ActionResult SonYorumlar()
        {
            return View(db.Yorums.OrderByDescending(x => x.YorumId).Take(5));
        }

        public ActionResult EncokOkunan()
        {
            return View(db.Makales.OrderByDescending(x => x.Okunma).Take(5));
        }


        public ActionResult KategoriMakale(int id)
        {
            //int _sayfaNo = Page ?? 1;
            var makaleler = db.Makales.Where(x => x.Kategori.KategoriID == id).ToList();
            //var makales = db.Makales.OrderByDescending(x => x.MakaleId).ToPagedList(_sayfaNo, 5);
            return View(makaleler);
        }






        public ActionResult MakaleDetay(int id)
        {
            var makale = db.Makales.Where(x => x.MakaleId == id).SingleOrDefault();
            if (makale == null)
            {
                return HttpNotFound();
            }
            return View(makale);
        }

        public ActionResult Hakkimizda()
        {
            return View();
        }

        public ActionResult Iletisim()
        {
            return View();
        }

        public ActionResult KategoriPartial()
        {
            return View(db.Kategoris.ToList());
        }


        public JsonResult Yorum(int Makaleid, string yorum)
        {
            var uyeid = Session["uyeid"];
            if (yorum != null)
            {
                db.Yorums.Add((new Yorum
                {
                    UyeId = Convert.ToInt32(uyeid)
                    ,
                    MakaleId = Makaleid
                    ,
                    Icerik = yorum
                    ,
                    Tarih = DateTime.Now
                }));
            }

            db.SaveChanges();

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult YorumSil(int id)
        {
            var uyeid = Session["uyeid"];
            var yorum = db.Yorums.Where(x => x.YorumId == id).SingleOrDefault();
            var makale = db.Makales.Where(x => x.MakaleId == yorum.MakaleId).SingleOrDefault();
            if (yorum.UyeId == Convert.ToInt32(uyeid))
            {
                db.Yorums.Remove(yorum);
                db.SaveChanges();
                return RedirectToAction("MakaleDetay", "Home", new { id = makale.MakaleId });
            }
            else
            {
                return HttpNotFound();
            }

          
        }

        public ActionResult YorumSayisi(int Makaleid)
        {
            var makale = db.Makales.Where(x => x.MakaleId == Makaleid).SingleOrDefault();
            makale.Okunma++;
            db.SaveChanges();
            return View();
        }


    }
}