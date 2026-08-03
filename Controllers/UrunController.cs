using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_store.Controllers;

public class UrunController : Controller
{
    // Dependecy Injection => DI
    private readonly DataContext _context;
    public UrunController(DataContext context)
    {
        _context = context;
    }

    public ActionResult Index()
    {
        var urunler = _context.Urunler.ToList();
        return View(urunler);
    }


    // http://localhost:5162/urunler/telefon?q=apple
    //route params: url => value
    //query string: q   => value
    public ActionResult List(string url, string q)
    {
        var query = _context.Urunler.Where(i=> i.Aktif); //Queryable

        if (!string.IsNullOrEmpty(url))
        {
            //filtreleme
            query = query.Where(i => i.Kategori.Url == url);
        };

        if (!string.IsNullOrEmpty(q))
        {
            //filtreleme
            query = query.Where(i => i.UrunAdi.ToLower().Contains(q.ToLower()));

            ViewData["q"] = q;
        }

        return View(query.ToList());
    }

    public ActionResult Details(int id)
    {
        var urun = _context.Urunler.FirstOrDefault(i => i.Id == id);
        // var urun = _context.Urunler.Find(id); 
        if (urun == null)
        {
            return RedirectToAction("Index","Home");
        }
        ViewData["BenzerUrunler"] = _context.Urunler
                                            .Where(i => i.Aktif && i.KategoriId == urun.KategoriId && i.Id != urun.Id)
                                            .Take(4)
                                            .ToList();
        return View(urun);
    }
}